using FrameworksXD.DialogueXD.Editor.GraphEditor;
using FrameworksXD.DialogueXD.Editor.GraphEditor.Elements;
using FrameworksXD.DialogueXD.Editor.Save;
using FrameworksXD.DialogueXD.Data;
using FrameworksXD.DialogueXD.ScriptableObjects;
using FrameworksXD.DialogueXD.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace FrameworksXD.DialogueXD.Editor.Utilities
{
    public static class DialogueIOUtility
    {
        private static string GraphFileName;
        private static string ContainerFolderPath;
        private static DialogueGraphView DialogueGraphView;
        private static List<DialogueGroup> Groups;
        private static List<DialogueNode> Nodes;

        private static Dictionary<string, DialogueGroupSO> CreatedDialogueGroup;
        private static Dictionary<string, DialogueSO> CreatedDialogues;

        private static Dictionary<string, DialogueGroup> LoadedGroups;
        private static Dictionary<string, DialogueNode> LoadedNodes;

        public static void Initialize(DialogueGraphView dialogueGraphView, string graphName)
        {
            GraphFileName = graphName;
            ContainerFolderPath = $"Assets/DialogueSystem/Dialogues/{GraphFileName}";
            DialogueGraphView = dialogueGraphView;

            Groups = new List<DialogueGroup>();
            Nodes = new List<DialogueNode>();

            CreatedDialogueGroup = new Dictionary<string, DialogueGroupSO>();
            CreatedDialogues = new Dictionary<string, DialogueSO>();

            LoadedGroups = new Dictionary<string, DialogueGroup>();
            LoadedNodes = new Dictionary<string, DialogueNode>();
        }

        #region Saving
        public static void Save()
        {
            CreateStaticFolders();

            GetElementsFromGraphView();

            DialogueGraphSaveDataSO graphData = CreateAsset<DialogueGraphSaveDataSO>("Assets/Editor/DialogueSystem/Graphs", $"{GraphFileName}Graph");
            graphData.Initialize(GraphFileName);

            DialogueContainerSO dialogueContainer = CreateAsset<DialogueContainerSO>(ContainerFolderPath, GraphFileName);
            dialogueContainer.Initialize(GraphFileName);

            SaveGroups(graphData, dialogueContainer);
            SaveNodes(graphData, dialogueContainer);

            SaveAsset(graphData);
            SaveAsset(dialogueContainer);
        }

        private static void SaveNodes(DialogueGraphSaveDataSO graphData, DialogueContainerSO dialogueContainer)
        {
            SerializableDictionary<string, List<string>> groupedNodeNames = new SerializableDictionary<string, List<string>>();
            List<string> ungroupedNodeNames = new List<string>();
            foreach (DialogueNode node in Nodes)
            {
                SaveNodeToGraph(node, graphData);
                SaveNodeToScriptableObject(node, dialogueContainer);

                if (node.Group != null)
                {
                    groupedNodeNames.AddItem(node.Group.title, node.DialogueName);
                    continue;
                }
                ungroupedNodeNames.Add(node.DialogueName);
            }
            UpdateDialoguesChoicesConnections();
            UpdateOldGroupedNodes(groupedNodeNames, graphData);
            UpdateOldUngroupedNodes(ungroupedNodeNames, graphData);
        }

        private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> currentGroupedNodeNames, DialogueGraphSaveDataSO graphData)
        {
            if (graphData.OldGroupedNodeNames != null && graphData.OldGroupedNodeNames.Count != 0)
            {
                foreach (KeyValuePair<string, List<string>> oldGroupedNode in graphData.OldGroupedNodeNames)
                {
                    List<string> nodesToRemove = new List<string>();
                    if (currentGroupedNodeNames.ContainsKey(oldGroupedNode.Key))
                    {
                        nodesToRemove = oldGroupedNode.Value.Except(currentGroupedNodeNames[oldGroupedNode.Key]).ToList();

                        foreach (string nodeToRemove in nodesToRemove)
                        {
                            RemoveAsset($"{ContainerFolderPath}/Groups/{oldGroupedNode.Key}/Dialogues", nodeToRemove);
                        }
                    }
                }
            }

            graphData.OldGroupedNodeNames = new SerializableDictionary<string, List<string>>(currentGroupedNodeNames);
        }

        private static void UpdateOldUngroupedNodes(List<string> currentUngroupedNodeNames, DialogueGraphSaveDataSO graphData)
        {
            if (graphData.OldUngroupedNodedNames != null && graphData.OldUngroupedNodedNames.Count != 0)
            {
                List<string> nodesToRemove = graphData.OldUngroupedNodedNames.Except(currentUngroupedNodeNames).ToList();
                foreach (string nodeToRemove in nodesToRemove)
                {
                    RemoveAsset($"{ContainerFolderPath}/Global/Dialogues", nodeToRemove);
                }
            }
            graphData.OldUngroupedNodedNames = new List<string>(currentUngroupedNodeNames);
        }

        private static void UpdateDialoguesChoicesConnections()
        {
            foreach (DialogueNode node in Nodes)
            {
                DialogueSO dialogue = CreatedDialogues[node.Id];
                for (int i = 0; i < node.Choices.Count; ++i)
                {
                    DialogueChoiceSaveData nodeChoice = node.Choices[i];
                    if (string.IsNullOrEmpty(nodeChoice.NodeId))
                        continue;

                    dialogue.Choices[i].NextDialogue = CreatedDialogues[nodeChoice.NodeId];
                    SaveAsset(dialogue);
                }
            }
        }

        private static void SaveNodeToScriptableObject(DialogueNode node, DialogueContainerSO dialogueContainer)
        {
            DialogueSO dialogue;
            if (node.Group != null)
            {
                dialogue = CreateAsset<DialogueSO>($"{ContainerFolderPath}/Groups/{node.Group.title}/Dialogues", node.DialogueName);
                dialogueContainer.DialogueGroups.AddItem(CreatedDialogueGroup[node.Group.Id], dialogue);
            }
            else
            {
                dialogue = CreateAsset<DialogueSO>($"{ContainerFolderPath}/Global/Dialogues", node.DialogueName);
                dialogueContainer.UngroupedDialogues.Add(dialogue);
            }
            dialogue.Initialize(node.DialogueName, node.Text, ConvertNodeChoicesToDialogueChoices(node.Choices), node.DialogueType, node.IsStartingNode());
            CreatedDialogues.Add(node.Id, dialogue);
            SaveAsset(dialogue);
        }

        private static List<DialogueChoiceData> ConvertNodeChoicesToDialogueChoices(List<DialogueChoiceSaveData> nodeChoices)
        {
            List<DialogueChoiceData> dialogueChoices = new List<DialogueChoiceData>();
            foreach (var nodeChoice in nodeChoices)
            {
                DialogueChoiceData choiceData = new DialogueChoiceData()
                {
                    Text = nodeChoice.Text
                };
                dialogueChoices.Add(choiceData);
            }
            return dialogueChoices;
        }

        private static void SaveNodeToGraph(DialogueNode node, DialogueGraphSaveDataSO graphData)
        {
            List<DialogueChoiceSaveData> choices = CloneNodeChoices(node.Choices);
            DialogueNodeSaveData nodeData = new DialogueNodeSaveData()
            {
                Id = node.Id,
                Name = node.DialogueName,
                Choices = choices,
                Text = node.Text,
                GroupId = node.Group?.Id,
                DialogueType = node.DialogueType,
                Position = node.GetPosition().position
            };

            graphData.Nodes.Add(nodeData);
        }

        private static void SaveGroups(DialogueGraphSaveDataSO graphData, DialogueContainerSO dialogueContainer)
        {
            List<string> groupNames = new List<string>();
            foreach (DialogueGroup group in Groups)
            {
                SaveGroupToGraph(group, graphData);
                SaveGroupSoScriptableObject(group, dialogueContainer);
                groupNames.Add(group.title);
            }
            UpdateOldGroups(graphData, groupNames);
        }

        private static void UpdateOldGroups(DialogueGraphSaveDataSO graphData, List<string> currentGroupNames)
        {
            if (graphData.OldGroupNames != null && graphData.OldGroupNames.Count != 0)
            {
                List<string> groupsToRemove = graphData.OldGroupNames.Except(currentGroupNames).ToList();
                foreach (string groupToRemove in groupsToRemove)
                {
                    RemoveFolder($"{ContainerFolderPath}/Groups/{groupToRemove}");
                }
            }

            graphData.OldGroupNames = new List<string>(currentGroupNames);
        }

        private static void SaveGroupSoScriptableObject(DialogueGroup group, DialogueContainerSO dialogueContainer)
        {
            string groupName = group.title;
            CreateFolder($"{ContainerFolderPath}/Groups", groupName);
            CreateFolder($"{ContainerFolderPath}/Groups/{groupName}", "Dialogues");

            DialogueGroupSO dialogueGroup = CreateAsset<DialogueGroupSO>($"{ContainerFolderPath}/Groups/{groupName}", groupName);
            dialogueGroup.Initialize(groupName);
            CreatedDialogueGroup.Add(group.Id, dialogueGroup);
            dialogueContainer.DialogueGroups.Add(dialogueGroup, new List<DialogueSO>());

            SaveAsset(dialogueGroup);
        }

        private static void SaveGroupToGraph(DialogueGroup group, DialogueGraphSaveDataSO graphData)
        {
            DialogueGroupSaveData groupData = new DialogueGroupSaveData()
            {
                Id = group.Id,
                Name = group.title,
                Position = group.GetPosition().position
            };
            graphData.Groups.Add(groupData);
        }

        private static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            T asset = LoadAsset<T>(path, assetName);
            if (asset == null)
            {
                string fullPath = $"{path}/{assetName}.asset";
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, fullPath);
            }
            return asset;
        }

        private static void RemoveAsset(string path, string assetName)
        {
            AssetDatabase.DeleteAsset($"{path}/{assetName}.asset");
        }

        private static void GetElementsFromGraphView()
        {
            Type groupType = typeof(DialogueGroup);
            DialogueGraphView.graphElements.ForEach(graphElement => 
            {
                if (graphElement is DialogueNode node)
                {
                    Nodes.Add(node);
                    return;
                }

                if (graphElement.GetType() == groupType)
                {
                    DialogueGroup group = (DialogueGroup)graphElement;
                    Groups.Add(group);
                    return;
                }

            });
        }

        private static void CreateStaticFolders()
        {
            CreateFolder("Assets/Editor", "DialogueSystem");
            CreateFolder("Assets/Editor/DialogueSystem", "Graphs");
            CreateFolder("Assets", "DialogueSystem");
            CreateFolder("Assets/DialogueSystem", "Dialogues");

            CreateFolder("Assets/DialogueSystem/Dialogues", GraphFileName);
            CreateFolder(ContainerFolderPath, "Global");
            CreateFolder(ContainerFolderPath, "Groups");
            CreateFolder($"{ContainerFolderPath}/Global", "Dialogues");
        }

        private static void CreateFolder(string path, string folderName)
        {
            if (AssetDatabase.IsValidFolder($"{path}/{folderName}"))
            {
                return;
            }
            AssetDatabase.CreateFolder(path, folderName);
        }

        private static void RemoveFolder(string path)
        {
            FileUtil.DeleteFileOrDirectory($"{path}.meta");
            FileUtil.DeleteFileOrDirectory($"{path}/");
        }

        private static void SaveAsset(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        #endregion

        #region Loading
        public static void Load()
        {
            string path = "Assets/Editor/DialogueSystem/Graphs";
            DialogueGraphSaveDataSO graphData = LoadAsset<DialogueGraphSaveDataSO>(path, GraphFileName);
            if (graphData == null)
            {
                EditorUtility.DisplayDialog
                (
                    "Couldn't load the file",
                    $"The file at the following path could not be found:\n\n{path}/{GraphFileName}",
                    "Ok"
                );
                return;
            }

            DialogueGraphWindow.UpdateFileName(graphData.FileName);
            LoadGroups(graphData.Groups);
            LoadNodes(graphData.Nodes);
            LoadNodesConnections();
        }

        private static void LoadNodesConnections()
        {
            foreach (KeyValuePair<string, DialogueNode> loadedNode in LoadedNodes)
            {
                foreach (Port choicePort in loadedNode.Value.outputContainer.Children())
                {
                    DialogueChoiceSaveData choiceData = (DialogueChoiceSaveData)choicePort.userData;

                    if (string.IsNullOrEmpty(choiceData.NodeId))
                        continue;

                    DialogueNode nextNode = LoadedNodes[choiceData.NodeId];
                    Port nextNodeInputPort = (Port)nextNode.inputContainer.Children().First();

                    Edge edge = choicePort.ConnectTo(nextNodeInputPort);
                    DialogueGraphView.AddElement(edge);
                    loadedNode.Value.RefreshPorts();
                }
            }
        }

        private static void LoadNodes(List<DialogueNodeSaveData> nodes)
        {
            foreach (DialogueNodeSaveData nodeData in nodes)
            {
                DialogueNode node = DialogueGraphView.CreateNode(nodeData.DialogueType, nodeData.Position, nodeData.Name, false);
                node.Id = nodeData.Id;
                node.Choices = CloneNodeChoices(nodeData.Choices);
                node.Text = nodeData.Text;

                node.Draw();

                DialogueGraphView.AddElement(node);

                LoadedNodes.Add(node.Id, node);

                if (string.IsNullOrEmpty(nodeData.GroupId))
                    continue;

                DialogueGroup group = LoadedGroups[nodeData.GroupId];
                node.Group = group;
                group.AddElement(node);
            }
        }

        private static void LoadGroups(List<DialogueGroupSaveData> groups)
        {
            foreach (DialogueGroupSaveData groupData in groups)
            {
                DialogueGroup group = DialogueGraphView.CreateGroup(groupData.Name, groupData.Position);
                group.Id = groupData.Id;
                LoadedGroups.Add(group.Id, group);
            }
        }

        private static T LoadAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";
            return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }
        #endregion


        private static List<DialogueChoiceSaveData> CloneNodeChoices(List<DialogueChoiceSaveData> nodeChoices)
        {
            List<DialogueChoiceSaveData> choices = new List<DialogueChoiceSaveData>();
            foreach (DialogueChoiceSaveData choice in nodeChoices)
            {
                DialogueChoiceSaveData choiceData = new DialogueChoiceSaveData()
                {
                    Text = choice.Text,
                    NodeId = choice.NodeId
                };
                choices.Add(choiceData);
            }

            return choices;
        }
    }
}

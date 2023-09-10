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
        private const string EditorFolderPath = "Assets/Editor";
        private const string GraphsFolder = "Assets/Editor/Dialogues";
        private const string DialoguesSOPath = "Assets/Dialogues";
        private const string DialogueFolderName = "Dialogues";
        private const string GlobalFolderName = "Global";
        private const string SpeakerFolderName = "Speakers";
        private const string GraphSOSuffix = "Graph";

        private static string GetGroupFolderPath(string groupName) => $"{ContainerFolderPath}/Groups/{groupName}";
        private static string GetGroupDialoguesFolderPath(string groupName) => $"{GetGroupFolderPath(groupName)}/{DialogueFolderName}";
        private static string GetGlobalDialoguesPath() => $"{ContainerFolderPath}/{GlobalFolderName}/{DialogueFolderName}";
        private static string GetDialogueSpeakersPath() => $"{ContainerFolderPath}/{SpeakerFolderName}";

        private static string GraphFileName;
        private static string ContainerFolderPath;
        private static DialogueGraphView DialogueGraphView;
        private static DialogueGraphWindow DialogueGraphWindow;
        private static List<DialogueGroup> Groups;
        private static List<DialogueNode> Nodes;
        private static List<StickyNote> StickyNotes;

        private static Dictionary<string, DialogueGroupSO> CreatedDialogueGroup;
        private static Dictionary<string, DialogueSO> CreatedDialogues;
        private static Dictionary<string, DialogueSpeakerSO> CreatedSpeakers;

        private static Dictionary<string, DialogueGroup> LoadedGroups;
        private static Dictionary<string, DialogueNode> LoadedNodes;
        private static DialogueGraphSettingsData LoadedGraphSettings;

        public static void Initialize(DialogueGraphView dialogueGraphView, DialogueGraphWindow dialogueGraphWindow, string graphName)
        {
            GraphFileName = graphName;
            ContainerFolderPath = $"{DialoguesSOPath}/{GraphFileName}";
            DialogueGraphView = dialogueGraphView;
            DialogueGraphWindow = dialogueGraphWindow;

            Groups = new List<DialogueGroup>();
            Nodes = new List<DialogueNode>();
            StickyNotes = new List<StickyNote>();

            CreatedDialogueGroup = new Dictionary<string, DialogueGroupSO>();
            CreatedDialogues = new Dictionary<string, DialogueSO>();
            CreatedSpeakers = new Dictionary<string, DialogueSpeakerSO>();

            LoadedGroups = new Dictionary<string, DialogueGroup>();
            LoadedNodes = new Dictionary<string, DialogueNode>();

            LoadedGraphSettings = null;
        }

        public static void Save()
        {
            CreateStaticFolders();

            GetElementsFromGraphView();

            DialogueGraphSaveDataSO graphData = CreateAsset<DialogueGraphSaveDataSO>(GraphsFolder, $"{GraphFileName}{GraphSOSuffix}");
            graphData.Initialize(GraphFileName);

            DialogueContainerSO dialogueContainer = CreateAsset<DialogueContainerSO>(ContainerFolderPath, GraphFileName);
            dialogueContainer.Initialize(GraphFileName);

            SaveDialogueGraphSettings(graphData);
            SaveDialogueSpeakers(graphData);
            SaveGroups(graphData, dialogueContainer);
            SaveNodes(graphData, dialogueContainer);
            SaveStickyNotes(graphData);

            SaveAsset(graphData);
            SaveAsset(dialogueContainer);
            SaveGroupsToAssets();
            SaveSpeakersToAssets();

            CommitAssetsToFiles();

            ClearCachedData();
        }

        private static void SaveSpeakersToAssets()
        {
            foreach (var speaker in CreatedSpeakers)
            {
                SaveAsset(speaker.Value);
            }
        }

        private static void SaveDialogueSpeakers(DialogueGraphSaveDataSO graphData)
        {
            var speakers = graphData.Speakers;

            var path = GetDialogueSpeakersPath();
            List<string> currentSpeakers = new List<string>();
            foreach (var speaker in speakers)
            {
                if (string.IsNullOrEmpty(speaker.Id))
                    continue;
                DialogueSpeakerSO speakerSO = CreateAsset<DialogueSpeakerSO>(path, speaker.Name);
                speakerSO.Initialize(speaker.Id, speaker.Name);
                CreatedSpeakers.Add(speaker.Id, speakerSO);
                currentSpeakers.Add(speaker.Name);
            }

            UpdateOldDialogueSpeakers(currentSpeakers, graphData);
        }
        private static void UpdateOldDialogueSpeakers(List<string> currentDialogueSpeakers, DialogueGraphSaveDataSO graphData)
        {
            if (graphData.OldSpeakerNames != null && graphData.OldSpeakerNames.Count != 0)
            {
                List<string> speakersToRemove = graphData.OldSpeakerNames.Except(currentDialogueSpeakers).ToList();
                foreach (string speakerToRemove in speakersToRemove)
                {
                    RemoveAsset(GetDialogueSpeakersPath(), speakerToRemove);
                }
            }
            graphData.OldSpeakerNames = new List<string>(currentDialogueSpeakers);
        }

        public static void Load()
        {
            string path = GraphsFolder;
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
            LoadDialogueSettings(graphData);
            LoadNodes(graphData.Nodes);
            LoadNodesConnections();
            LoadStickyNotes(graphData.StickyNotes);

            ClearCachedData();
        }

        #region Saving helpers

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
                            RemoveAsset(GetGroupDialoguesFolderPath(oldGroupedNode.Key), nodeToRemove);
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
                    RemoveAsset(GetGlobalDialoguesPath(), nodeToRemove);
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
                dialogue = CreateAsset<DialogueSO>(GetGroupDialoguesFolderPath(node.Group.title), node.DialogueName);
                string groupID = node.Group.Id;
                DialogueGroupSO groupSO = CreatedDialogueGroup[groupID];
                groupSO.Dialogues.Add(node.Id, dialogue);
            }
            else
            {
                dialogue = CreateAsset<DialogueSO>(GetGlobalDialoguesPath(), node.DialogueName);
                dialogueContainer.UngroupedDialogues.Add(node.Id, dialogue);
            }

            DialogueSpeakerSO speakerSO = string.IsNullOrEmpty(node.GetSelectedSpeaker().Id) ? null : CreatedSpeakers[node.GetSelectedSpeaker().Id];
            dialogue.Initialize(node.Id, node.DialogueName, node.Text, speakerSO, ConvertNodeChoicesToDialogueChoices(node.Choices), node.DialogueType, node.IsStartingNode());
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
                Position = node.GetPosition().position,
                SpeakerId = node.GetSelectedSpeaker().Id,
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
                    RemoveFolder(GetGroupFolderPath(groupToRemove));
                }
            }

            graphData.OldGroupNames = new List<string>(currentGroupNames);
        }

        private static void SaveGroupSoScriptableObject(DialogueGroup group, DialogueContainerSO dialogueContainer)
        {
            string groupName = group.title;
            CreateFolder($"{ContainerFolderPath}/Groups", groupName);
            CreateFolder(GetGroupFolderPath(groupName), DialogueFolderName);

            DialogueGroupSO dialogueGroup = CreateAsset<DialogueGroupSO>(GetGroupFolderPath(groupName), groupName);
            dialogueGroup.Initialize(group.Id, groupName);
            CreatedDialogueGroup.Add(group.Id, dialogueGroup);
            dialogueContainer.DialogueGroups.Add(dialogueGroup.Id, dialogueGroup);
        }

        private static void SaveGroupsToAssets()
        {
            foreach (var group in CreatedDialogueGroup)
            {
                SaveAsset(group.Value);
            }
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

        private static void SaveStickyNotes(DialogueGraphSaveDataSO graphData)
        {
            foreach (StickyNote stickyNote in StickyNotes)
            {
                DialogueStickyNoteSaveData data = new DialogueStickyNoteSaveData()
                {
                    Title = stickyNote.title,
                    Content = stickyNote.contents,
                    Position = stickyNote.GetPosition().position,
                    Theme = stickyNote.theme,
                    FontSize = stickyNote.fontSize
                };
                graphData.StickyNotes.Add(data);
            }
        }

        private static void SaveDialogueGraphSettings(DialogueGraphSaveDataSO graphData)
        {
            List<DialogueSpeakerData> speakerData = new List<DialogueSpeakerData>();
            foreach (var speaker in DialogueGraphWindow.DialogueGraphSettings.DialogueSpeakerData)
            {
                speakerData.Add(new DialogueSpeakerData(speaker.Name) { Id = speaker.Id });
            }
            graphData.Speakers = speakerData;
        }

        private static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            T asset = LoadAsset<T>(path, assetName);
            if (asset == null)
            {
                string fullPath = MakeAssetPath(path, assetName);
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, fullPath);
            }
            return asset;
        }

        private static void RemoveAsset(string path, string assetName)
        {
            AssetDatabase.DeleteAsset(MakeAssetPath(path, assetName));
        }

        private static void GetElementsFromGraphView()
        {
            Type groupType = typeof(DialogueGroup);
            Type stickyNoteType = typeof(StickyNote);
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

                if (graphElement.GetType() == stickyNoteType)
                {
                    StickyNote stickyNote = (StickyNote)graphElement;
                    StickyNotes.Add(stickyNote); 
                }

            });
        }

        private static void CreateStaticFolders()
        {
            CreateFolder("Assets", "Editor");
            CreateFolder(EditorFolderPath, DialogueFolderName);
            CreateFolder("Assets", DialogueFolderName);
            CreateFolder("Assets", DialogueFolderName);

            CreateFolder(DialoguesSOPath, GraphFileName);
            CreateFolder(ContainerFolderPath, GlobalFolderName);
            CreateFolder(ContainerFolderPath, SpeakerFolderName);
            CreateFolder(ContainerFolderPath, "Groups");
            CreateFolder($"{ContainerFolderPath}/{GlobalFolderName}", DialogueFolderName);
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
        }

        private static void CommitAssetsToFiles()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        #endregion

        #region Loading helpers

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
                node.SpeakerIndex = LoadedGraphSettings.GetSpeakerIndex(nodeData.SpeakerId);

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
            string fullPath = MakeAssetPath(path, assetName);
            return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }


        private static void LoadStickyNotes(List<DialogueStickyNoteSaveData> stickyNotesData)
        {
            foreach (var stickyNoteData in stickyNotesData)
            {
                DialogueGraphView.CreateStickyNote
                (
                    stickyNoteData.Title, 
                    stickyNoteData.Content, 
                    stickyNoteData.Position,
                    stickyNoteData.Theme,
                    stickyNoteData.FontSize
                );
            }
        }

        private static void LoadDialogueSettings(DialogueGraphSaveDataSO graphData)
        {
            DialogueGraphSettingsData settingsData = new DialogueGraphSettingsData();
            settingsData.Clear();
            foreach (var speaker in graphData.Speakers)
            {
                settingsData.DialogueSpeakerData.Add(new DialogueSpeakerData()
                {
                    Id = speaker.Id,
                    Name = speaker.Name
                });
            }
            LoadedGraphSettings = settingsData;
            DialogueGraphWindow.DialogueGraphSettings = settingsData;
        }

        #endregion

        #region Generic
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

        private static void ClearCachedData()
        {
            GraphFileName = null;
            ContainerFolderPath = null;
            DialogueGraphView = null;

            Groups = null;
            Nodes = null;
            StickyNotes = null;

            CreatedDialogueGroup = null;
            CreatedDialogues = null;
            CreatedSpeakers = null;

            LoadedGroups = null;
            LoadedNodes = null;

            LoadedGraphSettings = null;
        }

        private static string MakeAssetPath(string path, string assetName)
        {
            return $"{path}/{assetName}.asset";
        }
        #endregion
    }
}

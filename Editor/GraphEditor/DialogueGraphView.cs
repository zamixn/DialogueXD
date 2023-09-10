using FrameworksXD.DialogueXD.Data;
using FrameworksXD.DialogueXD.Editor.Error;
using FrameworksXD.DialogueXD.Editor.GraphEditor.Elements;
using FrameworksXD.DialogueXD.Editor.Save;
using FrameworksXD.DialogueXD.Editor.Utilities;
using FrameworksXD.DialogueXD.Utilities;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrameworksXD.DialogueXD.Editor.GraphEditor
{
    public class DialogueGraphView : GraphView
    {
        private DialogueGraphSearchWindow DialogueGraphSearchWindow;
        private DialogueGraphWindow DialogueGraphWindow;
        private MiniMap MiniMap;

        private SerializableDictionary<string, DialogueNodeErrorData> UngroupedNodes;
        private SerializableDictionary<string, DialogueGroupErrorData> Groups;
        private SerializableDictionary<Group, SerializableDictionary<string, DialogueNodeErrorData>> GroupedNodes;
        private List<StickyNote> StickyNotes;

        public int RepeatedNameErrorCount { get; set; }
        public int EmptyDialogueNames { get; set; }
        public object DialogueChoiceSaveData { get; private set; }

        public DialogueGraphView(DialogueGraphWindow dialogueGraphWindow)
        {
            DialogueGraphWindow = dialogueGraphWindow;
            UngroupedNodes = new SerializableDictionary<string, DialogueNodeErrorData>();
            Groups = new SerializableDictionary<string, DialogueGroupErrorData>();
            GroupedNodes = new SerializableDictionary<Group, SerializableDictionary<string, DialogueNodeErrorData>>();
            StickyNotes = new List<StickyNote>();

            AddManipulators();
            AddSearchWindow();
            AddGridBackgroung();
            AddMiniMap();

            OnElementsDeleted();
            OnGroupElementsAdded();
            OnGroupElementsRemoved();
            OnGroupRenamed();
            OnGraphViewChanged();

            AddStyles();
            AddMiniMapStyles();
        }

        private void AddSearchWindow()
        {
            if (DialogueGraphSearchWindow == null)
            {
                DialogueGraphSearchWindow = ScriptableObject.CreateInstance<DialogueGraphSearchWindow>();
                DialogueGraphSearchWindow.Initialize(this);
            }

            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), DialogueGraphSearchWindow);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort == port)
                    return;
                if (startPort.node == port.node)
                    return;
                if (startPort.direction == port.direction)
                    return;

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(CreateNodeContextualMenu("Add Node (Single Choice)", DialogueType.SingleChoice));
            this.AddManipulator(CreateNodeContextualMenu("Add Node (Multiple Choice)", DialogueType.MultipleChoice));
            this.AddManipulator(CreateGroupContextualMenu());
            this.AddManipulator(CreateStickyNoteContextualMenu());
        }

        private IManipulator CreateNodeContextualMenu(string actionTitle, DialogueType dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, 
                    actionEvent => AddElement(CreateNode(dialogueType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
                )
            );

            return contextualMenuManipulator;
        }

        private Type GetTypeOfDialogue(DialogueType dialogueType)
        {
            switch (dialogueType)
            {
                case DialogueType.SingleChoice:
                    return typeof(DialogueNodeSingleChoice);
                case DialogueType.MultipleChoice:
                    return typeof(DialogueNodeMultipleChoice);
                default:
                    throw new Exception($"DialogueType: {dialogueType} is not implemented");
            }
        }

        public List<DialogueSpeakerData> GetAvailableSpeakers()
        {
            return DialogueGraphWindow.GetAvailableSpeakers();
        }

        public DialogueNode CreateNode(DialogueType dialogueType, Vector2 position, string nodeName = "DialogueName", bool shouldDraw = true)
        {
            Type nodeType = GetTypeOfDialogue(dialogueType);
            DialogueNode node = (DialogueNode)Activator.CreateInstance(nodeType);
            node.Initialize(nodeName, this, position);
            if(shouldDraw)
                node.Draw();

            AddUngroupedNode(node);

            return node;
        }

        public void AddUngroupedNode(DialogueNode node)
        {
            string nodeName = node.DialogueName.ToLower();
            if (!UngroupedNodes.ContainsKey(nodeName))
            {
                DialogueNodeErrorData nodeErrorData = new DialogueNodeErrorData();
                nodeErrorData.Nodes.Add(node);

                UngroupedNodes.Add(nodeName, nodeErrorData);
                return;
            }

            DialogueNodeErrorData data = UngroupedNodes[nodeName];
            data.Nodes.Add(node);

            Color errorColor = data.ErrorData.Color;
            node.SetErrorStyle(errorColor);

            if (data.Nodes.Count == 2)
            {
                ++RepeatedNameErrorCount;
                if(!string.IsNullOrEmpty(data.Nodes[0].GetDialogueNameTextFieldValue()))
                    data.Nodes[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveUngroupedNode(DialogueNode node)
        {
            string nodeName = node.DialogueName.ToLower();
            UngroupedNodes[nodeName].Nodes.Remove(node);
            if (!string.IsNullOrEmpty(node.GetDialogueNameTextFieldValue()))
                node.ResetStyle();

            var nodeList = UngroupedNodes[nodeName].Nodes;
            if (nodeList.Count == 1)
            {
                --RepeatedNameErrorCount;
                if (!string.IsNullOrEmpty(nodeList[0].GetDialogueNameTextFieldValue()))
                    nodeList[0].ResetStyle();                
            }
            else if (nodeList.Count == 0)
                UngroupedNodes.Remove(nodeName);
        }

        public void AddGroupedNode(DialogueNode node, DialogueGroup group)
        {
            string nodeName = node.DialogueName.ToLower();
            node.Group = group;
            if (!GroupedNodes.ContainsKey(group))
            {
                GroupedNodes.Add(group, new SerializableDictionary<string, DialogueNodeErrorData>());
            }

            if (!GroupedNodes[group].ContainsKey(nodeName))
            {
                DialogueNodeErrorData nodeErrorData = new DialogueNodeErrorData();
                nodeErrorData.Nodes.Add(node);
                GroupedNodes[group].Add(nodeName, nodeErrorData);
                return;
            }

            var groupedNodesList = GroupedNodes[group][nodeName].Nodes;
            groupedNodesList.Add(node);
            Color errorColor = GroupedNodes[group][nodeName].ErrorData.Color;
            node.SetErrorStyle(errorColor);

            if (groupedNodesList.Count == 2)
            {
                if (!string.IsNullOrEmpty(groupedNodesList[0].GetDialogueNameTextFieldValue()))
                    groupedNodesList[0].SetErrorStyle(errorColor);
                ++RepeatedNameErrorCount;
            }
        }

        public void RemoveGroupedNode(DialogueNode node, Group group)
        {
            string nodeName = node.DialogueName.ToLower();
            node.Group = null;
            List<DialogueNode> groupedNodesList = GroupedNodes[group][nodeName].Nodes;

            groupedNodesList.Remove(node);
            if(string.IsNullOrEmpty(node.GetDialogueNameTextFieldValue()))
                node.ResetStyle();

            if (groupedNodesList.Count == 1)
            {
                --RepeatedNameErrorCount;
                if (!string.IsNullOrEmpty(groupedNodesList[0].GetDialogueNameTextFieldValue()))
                    groupedNodesList[0].ResetStyle();
            }
            else if (groupedNodesList.Count == 0)
            {
                GroupedNodes[group].Remove(nodeName);

                if (GroupedNodes[group].Count == 0)
                    GroupedNodes.Remove(group);
            }
        }

        private void AddGroup(DialogueGroup group)
        {
            string groupName = group.title.ToLower();
            if (!Groups.ContainsKey(groupName))
            {
                DialogueGroupErrorData groupErrorData = new DialogueGroupErrorData();
                groupErrorData.Groups.Add(group);
                Groups.Add(groupName, groupErrorData);
                return;
            }

            var groupsList = Groups[groupName].Groups;
            groupsList.Add(group);
            Color errorColor = Groups[groupName].ErrorData.Color;
            group.SetErrorStyle(errorColor);

            if (groupsList.Count == 2)
            {
                ++RepeatedNameErrorCount;
                groupsList[0].SetErrorStyle(errorColor);
            }
        }

        private void RemoveGroup(DialogueGroup group)
        {
            string oldGroupName = group.OldTitle.ToLower();
            var groupsList = Groups[oldGroupName].Groups;
            groupsList.Remove(group);
            group.ResetStyle();

            if (groupsList.Count == 1)
            {
                --RepeatedNameErrorCount;
                groupsList[0].ResetStyle();
            }

            else if (groupsList.Count == 0)
                Groups.Remove(oldGroupName);
        }

        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group",
                    actionEvent => CreateGroup("DialogueGroup", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))
                )
            );

            return contextualMenuManipulator;
        }

        public DialogueGroup CreateGroup(string groupName, Vector2 position)
        {
            DialogueGroup group = new DialogueGroup(groupName, position);
            AddGroup(group);

            AddElement(group);

            foreach (GraphElement selectedElement in selection)
            {
                if (!(selectedElement is DialogueNode))
                    continue;

                DialogueNode node = (DialogueNode)selectedElement;
                group.AddElement(node);
            }

            return group;
        }

        private IManipulator CreateStickyNoteContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Comment",
                    actionEvent => CreateStickyNote("Title", "Comment...", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))
                )
            );

            return contextualMenuManipulator;
        }
        public StickyNote CreateStickyNote(string title, string text, Vector2 position, StickyNoteTheme theme = StickyNoteTheme.Black, StickyNoteFontSize fontSize = StickyNoteFontSize.Small)
        {
            StickyNote stickyNote = new StickyNote(position)
            {
                title = title,
                contents = text,
                fontSize = fontSize,
                theme = theme,
            };

            AddElement(stickyNote);
            StickyNotes.Add(stickyNote);
            return stickyNote;
        }

        public bool HasDuplicateName(DialogueNode node, out Color color)
        {
            string name = node.DialogueName.ToLower();

            if (node.Group != null)
            {
                var errorData = GroupedNodes[node.Group][name];
                if (errorData.Nodes.Count > 1)
                {
                    color = errorData.ErrorData.Color;
                    return true;
                }
                color = default(Color);
                return false;
            }

            if (UngroupedNodes[name].Nodes.Count > 1)
            {
                color = UngroupedNodes[name].ErrorData.Color;
                return true;
            }
            color = default(Color);
            return false;
        }

        private void AddStyles()
        {
            this.AddStyleSheets("DialogueGraphStyles.uss", "DialogueNodeStyles.uss");
        }

        private void AddMiniMapStyles()
        {
            StyleColor backgroundColor = new StyleColor(new Color32(29, 29, 30, 255));
            StyleColor borderColor = new StyleColor(new Color32(51, 51, 51, 255));
            MiniMap.style.backgroundColor = backgroundColor;
            MiniMap.style.borderTopColor = borderColor;
            MiniMap.style.borderRightColor = borderColor;
            MiniMap.style.borderBottomColor = borderColor;
            MiniMap.style.borderLeftColor = borderColor;
        }

        public void ToggleMiniMap()
        {
            MiniMap.visible = !MiniMap.visible;
        }

        private void AddGridBackgroung()
        {
            GridBackground bg = new GridBackground();
            bg.StretchToParentSize();

            Insert(0, bg);
        }

        private void AddMiniMap()
        {
            MiniMap = new MiniMap()
            {
                anchored = true
            };

            MiniMap.SetPosition(new Rect(15, 50, 200, 180));
            Add(MiniMap);
            ToggleMiniMap();
        }

        private void AddBlackBoardItemRequested(Blackboard blackboard)
        {

            var gm = new GenericMenu();
            gm.AddItem(new GUIContent("Vector1"), false, () => Debug.Log("request"));
            gm.ShowAsContext();
        }

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;

            if (isSearchWindow)
            {
                worldMousePosition -= DialogueGraphWindow.position.position;
            }

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);

            return localMousePosition;
        }

        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                Type groupType = typeof(DialogueGroup);
                Type edgeType = typeof(Edge);
                Type stickyNoteType = typeof(StickyNote);
                List<DialogueGroup> groupsToDelete = new List<DialogueGroup>();
                List<Edge> edgesToDelete = new List<Edge>();
                List<DialogueNode> nodesToDelete = new List<DialogueNode>();
                List<StickyNote> stickyNotesToDelete = new List<StickyNote>();
                foreach (GraphElement element in selection)
                {
                    if (element is DialogueNode node)
                        nodesToDelete.Add(node);

                    if (element.GetType() == edgeType)
                    {
                        Edge edge = (Edge)element;
                        edgesToDelete.Add(edge);
                        continue;
                    }

                    if (element.GetType() == stickyNoteType)
                    {
                        stickyNotesToDelete.Add((StickyNote)element);
                        continue;
                    }

                    if (element.GetType() != groupType)
                        continue;

                    DialogueGroup group = (DialogueGroup)element;
                    groupsToDelete.Add(group);
                }

                foreach (var group in groupsToDelete)
                {
                    List<DialogueNode> groupNodesToDelete = new List<DialogueNode>();

                    foreach (GraphElement groupElement in group.containedElements)
                    {
                        if (!(groupElement is DialogueNode))
                            continue;

                        DialogueNode groupNode = (DialogueNode)groupElement;
                        groupNodesToDelete.Add(groupNode);
                    }

                    group.RemoveElements(groupNodesToDelete);
                    RemoveGroup(group);
                    RemoveElement(group);
                }

                DeleteElements(edgesToDelete);

                foreach (var node in nodesToDelete)
                {
                    if (node.Group != null)
                        node.Group.RemoveElement(node);

                    RemoveUngroupedNode(node);
                    node.DisconnectAllPorts();
                    RemoveElement(node);
                }

                DeleteElements(stickyNotesToDelete);
            };
        }

        private void OnGroupElementsAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is DialogueNode))
                        continue;

                    DialogueGroup nodeGroup = (DialogueGroup)group;
                    DialogueNode node = (DialogueNode)element;
                    RemoveUngroupedNode(node);
                    AddGroupedNode(node, nodeGroup);
                }
            };
        }

        private void OnGroupElementsRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is DialogueNode))
                        continue;

                    DialogueNode node = (DialogueNode)element;
                    RemoveGroupedNode(node, group);
                    AddUngroupedNode(node);
                }
            };
        }

        private void OnGroupRenamed()
        {
            groupTitleChanged = (group, newTitle) =>
            {
                DialogueGroup dGroup = (DialogueGroup)group;
                dGroup.title = newTitle.RemoveWhitespaces().RemoveSpecialCharacters();


                if (string.IsNullOrEmpty(dGroup.title))
                {
                    if (!string.IsNullOrEmpty(dGroup.OldTitle))
                        ++RepeatedNameErrorCount;
                }
                else
                {
                    if (string.IsNullOrEmpty(dGroup.OldTitle))
                        --RepeatedNameErrorCount;
                }

                RemoveGroup(dGroup);
                dGroup.OldTitle = dGroup.title;

                AddGroup(dGroup);
            };
        }

        private void OnGraphViewChanged()
        {
            graphViewChanged = changes =>
            {
                if (changes.edgesToCreate != null)
                {
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        DialogueNode nextNode = (DialogueNode)edge.input.node;

                        DialogueChoiceSaveData choiceData = (DialogueChoiceSaveData)edge.output.userData;
                        choiceData.NodeId = nextNode.Id;
                    }
                }

                if (changes.elementsToRemove != null)
                {
                    Type edgeType = typeof(Edge);
                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element.GetType() != edgeType)
                            continue;

                        Edge edge = (Edge)element;
                        DialogueChoiceSaveData choiceData = (DialogueChoiceSaveData)edge.output.userData;
                        choiceData.NodeId = "";
                    }
                }

                return changes;
            };
        }

        public void ClearGraph()
        {

            graphElements.ForEach(graphElement => RemoveElement(graphElement));

            Groups.Clear();
            GroupedNodes.Clear();
            UngroupedNodes.Clear();
            StickyNotes.Clear();

            RepeatedNameErrorCount = 0;
            EmptyDialogueNames = 0;
        }
    }
}

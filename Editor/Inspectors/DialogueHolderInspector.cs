using FrameworksXD.DialogueXD.Editor.GraphEditor;
using FrameworksXD.DialogueXD.Editor.Utilities;
using FrameworksXD.DialogueXD.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FrameworksXD.DialogueXD.Editor.Inspectors
{
    [CustomEditor(typeof(DialogueHolder), true)]
    public class DialogueHolderInspector : UnityEditor.Editor
    {
        private const int SpacingValue = 10;

        private SerializedProperty DialogueContainerProperty;
        private SerializedProperty StartingDialogueGroupIDProperty;
        private SerializedProperty StartingDialogueIDProperty;

        private Dictionary<string, DialogueSO> AvailableDialogues;
        private string[] AvailableDialogueNames;
        private int SelectedDialogueIndex;

        private Dictionary<string, DialogueGroupSO> AvailableDialogueGroups;
        private string[] AvailableDialogueGroupNames;
        private int SelectedDialogueGroupIndex;

        private void OnEnable()
        {
            DialogueContainerProperty = serializedObject.FindProperty("DialogueGraph");
            StartingDialogueGroupIDProperty = serializedObject.FindProperty("StartingDialogueGroupID");
            StartingDialogueIDProperty = serializedObject.FindProperty("StartingDialogueID");

            if (IsDialogueContainerValid())
            {
                UpdateAvailableDialogueGroups();
                UpdateAvailableDialogues();
                RefreshSelectedDialogueGroupIndex();
                RefreshSelectedDialogueIndex();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawClassField();
            DrawDialogueSettings();
            DialogueInspectorUtilities.DrawSpace(SpacingValue);
            DialogueInspectorUtilities.DrawSpace(SpacingValue);
            DrawChildClassInspector();

            DrawOpenGraphEditor();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawOpenGraphEditor()
        {
            if (GUILayout.Button("Open Graph Editor"))
            {
                var graphSO = (DialogueContainerSO)DialogueContainerProperty.objectReferenceValue;
                if (graphSO)
                    DialogueGraphWindow.OpenWithGraph(graphSO);
                else
                    DialogueGraphWindow.Open();
            }
        }

        private void DrawClassField()
        {
            GUI.enabled = false;
            MonoScript lineScriptAsset = MonoScript.FromMonoBehaviour(target as MonoBehaviour);
            EditorGUILayout.ObjectField("Script", lineScriptAsset, typeof(MonoScript), true);
            GUI.enabled = true;
        }

        private void DrawChildClassInspector()
        {
            if (target.GetType() != typeof(DialogueHolder))
            {
                DialogueInspectorUtilities.DrawSpace(SpacingValue);
                DialogueInspectorUtilities.DrawSpace(SpacingValue);
                DialogueInspectorUtilities.DrawHeader($"{target.GetType()} inspector");
                MemberInfo[] infos = target.GetType().GetMembers(
                                    BindingFlags.DeclaredOnly |
                                    BindingFlags.NonPublic |
                                    BindingFlags.Public |
                                    BindingFlags.Static |
                                    BindingFlags.Instance);
                foreach (var info in infos)
                {
                    SerializedProperty property = serializedObject.FindProperty(info.Name);
                    if (property == null)
                        continue;
                    EditorGUILayout.PropertyField(property);
                }
            }
        }

        private void DrawDialogueSettings()
        {
            DialogueInspectorUtilities.DrawHeader("Dialogue Settings");

            var prevContainerRef = DialogueContainerProperty.objectReferenceValue;
            DialogueContainerProperty.DrawPropertyField();
            if (!IsDialogueContainerValid())
            {
                DialogueInspectorUtilities.DrawHelpBox("Assign dialogue graph");
                return;
            }

            DialogueInspectorUtilities.DrawSpace(SpacingValue);

            var prevGroupId = StartingDialogueGroupIDProperty.stringValue;
            if (prevContainerRef != DialogueContainerProperty.objectReferenceValue)
            {
                StartingDialogueGroupIDProperty.stringValue = "";
                StartingDialogueIDProperty.stringValue = "";
                UpdateAvailableDialogueGroups();
                UpdateAvailableDialogues();
                RefreshSelectedDialogueGroupIndex();
                RefreshSelectedDialogueIndex();
            }
            DrawDialogueGroup();

            if (prevGroupId != StartingDialogueGroupIDProperty.stringValue)
            {
                UpdateAvailableDialogues();
                RefreshSelectedDialogueIndex();
            }

            DialogueInspectorUtilities.DrawSpace(SpacingValue);

            DrawDialogue();
        }

        private bool IsDialogueContainerValid()
        {
            var container = (DialogueContainerSO)DialogueContainerProperty.objectReferenceValue;
            return container != null;
        }

        private void UpdateAvailableDialogueGroups()
        {
            DialogueContainerSO dialogueContainer = (DialogueContainerSO)DialogueContainerProperty.objectReferenceValue;
            List<DialogueGroupSO> allGroups = dialogueContainer.GetAllDialogueGroups();

            AvailableDialogueGroups = new Dictionary<string, DialogueGroupSO>(allGroups.Count);
            AvailableDialogueGroupNames = new string[allGroups.Count + 1];
            AvailableDialogueGroups.Add("None", null);
            AvailableDialogueGroupNames[0] = "None";

            for (int i = 1; i <= allGroups.Count; i++)
            {
                var group = allGroups[i - 1];
                AvailableDialogueGroups.Add(group.GroupName, group);
                AvailableDialogueGroupNames[i] = group.GroupName;
            }
        }

        private void RefreshSelectedDialogueGroupIndex()
        {
            DialogueContainerSO dialogueContainer = (DialogueContainerSO)DialogueContainerProperty.objectReferenceValue;
            DialogueGroupSO selectedGroup = dialogueContainer.GetDialogueGroup(StartingDialogueGroupIDProperty.stringValue);
            if (selectedGroup == null)
            {
                SelectedDialogueGroupIndex = 0;
                return;
            }
            List<DialogueGroupSO> allGroups = dialogueContainer.GetAllDialogueGroups();
            for (int i = 0; i < allGroups.Count; i++)
            {
                var group = allGroups[i];
                if (selectedGroup.Id == group.Id) 
                { 
                    SelectedDialogueGroupIndex = i + 1;
                    return;
                }
            }
            SelectedDialogueGroupIndex = 0;
        }

        private void UpdateAvailableDialogues()
        {
            DialogueContainerSO dialogueContainer = (DialogueContainerSO)DialogueContainerProperty.objectReferenceValue;

            DialogueSO selectedDialogue = dialogueContainer.GetDialogue(StartingDialogueIDProperty.stringValue, StartingDialogueGroupIDProperty.stringValue);
            string groupID = StartingDialogueGroupIDProperty.stringValue;
            List<DialogueSO> availableDialogues = dialogueContainer.GetAllAvailableDialogues(groupID);
            availableDialogues = availableDialogues.Where(d => d.IsStartingDialogue).ToList();

            AvailableDialogues = new Dictionary<string, DialogueSO>(availableDialogues.Count + 1);
            AvailableDialogueNames = new string[availableDialogues.Count + 1];
            AvailableDialogues.Add("None", null);
            AvailableDialogueNames[0] = "None";

            for (int i = 1; i <= availableDialogues.Count; i++)
            {
                var dialogue = availableDialogues[i - 1];
                AvailableDialogues.Add(dialogue.DialogueName, dialogue);
                AvailableDialogueNames[i] = dialogue.DialogueName;
            }
        }

        private void RefreshSelectedDialogueIndex()
        {
            DialogueContainerSO dialogueContainer = (DialogueContainerSO)DialogueContainerProperty.objectReferenceValue;

            DialogueSO selectedDialogue = dialogueContainer.GetDialogue(StartingDialogueIDProperty.stringValue, StartingDialogueGroupIDProperty.stringValue);
            if (selectedDialogue == null)
            {
                SelectedDialogueIndex = 0;
                return;
            }
            string groupID = StartingDialogueGroupIDProperty.stringValue;
            List<DialogueSO> availableDialogues = dialogueContainer.GetAllAvailableDialogues(groupID);
            availableDialogues = availableDialogues.Where(d => d.IsStartingDialogue).ToList();

            for (int i = 0; i < availableDialogues.Count; i++)
            {
                var dialogue = availableDialogues[i];
                if (selectedDialogue.Id == dialogue.Id)
                {
                    SelectedDialogueIndex = i + 1;
                    return;
                }
            }
            SelectedDialogueIndex = 0;
        }

        private void DrawDialogueGroup()
        {
            if (SelectedDialogueGroupIndex > 0 && AvailableDialogueGroups[AvailableDialogueGroupNames[SelectedDialogueGroupIndex]] == null)
                UpdateAvailableDialogueGroups();
            var dialogueGroup = DrawDialogueGroupHelperField();

            if (dialogueGroup != null)
                StartingDialogueGroupIDProperty.stringValue = dialogueGroup.Id;
            else
                StartingDialogueGroupIDProperty.stringValue = "";

            StartingDialogueGroupIDProperty.DrawDisabledPropertyField();

            if (dialogueGroup == null && SelectedDialogueGroupIndex != 0)
                DialogueInspectorUtilities.DrawHelpBox("Invalid dialogue group assigned", MessageType.Error);
        }

        private DialogueGroupSO DrawDialogueGroupHelperField()
        {
            SelectedDialogueGroupIndex = DialogueInspectorUtilities.DrawPopup("Starting Dialogue Group", SelectedDialogueGroupIndex, AvailableDialogueGroupNames);
            return AvailableDialogueGroups[AvailableDialogueGroupNames[SelectedDialogueGroupIndex]];
        }

        private void DrawDialogue()
        {
            if (SelectedDialogueIndex > 0 && AvailableDialogues[AvailableDialogueNames[SelectedDialogueIndex]] == null)
                UpdateAvailableDialogues();
            var dialogue = DrawDialogueHelperField();

            if (dialogue != null)
                StartingDialogueIDProperty.stringValue = dialogue.Id;
            else
                StartingDialogueIDProperty.stringValue = "";

            StartingDialogueIDProperty.DrawDisabledPropertyField();

            if (dialogue == null)
                DialogueInspectorUtilities.DrawHelpBox("Invalid dialogue assigned", MessageType.Error);
        }
        private DialogueSO DrawDialogueHelperField()
        {
            SelectedDialogueIndex = DialogueInspectorUtilities.DrawPopup("Starting Dialogue", SelectedDialogueIndex, AvailableDialogueNames);
            return AvailableDialogues[AvailableDialogueNames[SelectedDialogueIndex]];
        }
    }
}

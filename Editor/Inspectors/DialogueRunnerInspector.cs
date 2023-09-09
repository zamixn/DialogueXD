using FrameworksXD.DialogueXD.Editor.Utilities;
using FrameworksXD.DialogueXD.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FrameworksXD.DialogueXD.Editor.Inspectors
{
    [CustomEditor(typeof(DialogueRunner))]
    public class DialogueRunnerInspector : UnityEditor.Editor
    {
        private const int SpacingValue = 10;

        private SerializedProperty DialogueContainerProperty;
        private SerializedProperty DialogueGroupIDProperty;
        private SerializedProperty StartingDialogueIDProperty;
        private SerializedProperty DialogueVisualizerProperty;

        private Dictionary<string, DialogueSO> AvailableDialogues;
        private string[] AvailableDialogueNames;
        private int SelectedDialogueIndex;

        private Dictionary<string, DialogueGroupSO> AvailableDialogueGroups;
        private string[] AvailableDialogueGroupNames;
        private int SelectedDialogueGroupIndex;

        private void OnEnable()
        {
            DialogueContainerProperty = serializedObject.FindProperty("DialogueGraph");
            DialogueGroupIDProperty = serializedObject.FindProperty("GroupID");
            StartingDialogueIDProperty = serializedObject.FindProperty("StartingDialogueID");
            DialogueVisualizerProperty = serializedObject.FindProperty("DialogueVisualizer");

            if (IsDialogueContainerValid())
            {
                UpdateAvailableDialogueGroups();
                UpdateAvailableDialogues();
            }

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDialogueSettings();
            DialogueInspectorUtilities.DrawSpace(SpacingValue);
            DialogueInspectorUtilities.DrawSpace(SpacingValue);
            DrawVisualizerSettings();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawVisualizerSettings()
        {
            DialogueInspectorUtilities.DrawHeader("Visualizer Settings");
            DialogueVisualizerProperty.DrawPropertyField();
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

            var prevGroupId = DialogueGroupIDProperty.stringValue;
            if (prevContainerRef != DialogueContainerProperty.objectReferenceValue)
            {
                UpdateAvailableDialogueGroups();
                UpdateAvailableDialogues();
            }
            DrawDialogueGroup();

            if (prevGroupId != DialogueGroupIDProperty.stringValue)
                UpdateAvailableDialogues();

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

            DialogueGroupSO selectedGroup = dialogueContainer.GetDialogueGroup(DialogueGroupIDProperty.stringValue);
            AvailableDialogueGroups = new Dictionary<string, DialogueGroupSO>(allGroups.Count);
            AvailableDialogueGroupNames = new string[allGroups.Count + 1];
            AvailableDialogueGroups.Add("No Group", null);
            AvailableDialogueGroupNames[0] = "No Group";

            for (int i = 1; i <= allGroups.Count; i++)
            {
                var group = allGroups[i - 1];
                AvailableDialogueGroups.Add(group.GroupName, group);
                AvailableDialogueGroupNames[i] = group.GroupName;
                if (selectedGroup != null && selectedGroup.Id == group.Id)
                    SelectedDialogueGroupIndex = i;
            }

            if (selectedGroup == null)
                SelectedDialogueGroupIndex = 0;
        }

        private void UpdateAvailableDialogues()
        {
            DialogueContainerSO dialogueContainer = (DialogueContainerSO)DialogueContainerProperty.objectReferenceValue;

            DialogueSO selectedDialogue = dialogueContainer.GetDialogue(StartingDialogueIDProperty.stringValue, DialogueGroupIDProperty.stringValue);
            string groupID = DialogueGroupIDProperty.stringValue;
            List<DialogueSO> allDialogues = dialogueContainer.GetAllAvailableDialogues(groupID);
            List<DialogueSO> startingDialogues = allDialogues.Where(d => d.IsStartingDialogue).ToList();

            AvailableDialogues = new Dictionary<string, DialogueSO>(startingDialogues.Count);
            AvailableDialogueNames = new string[startingDialogues.Count];
            for (int i = 0; i < startingDialogues.Count; i++)
            {
                var dialogue = startingDialogues[i];
                AvailableDialogues.Add(dialogue.DialogueName, dialogue);
                AvailableDialogueNames[i] = dialogue.DialogueName;
                if (selectedDialogue != null && selectedDialogue.Id == dialogue.Id)
                    SelectedDialogueIndex = i;
            }

            if(selectedDialogue == null)
                SelectedDialogueIndex = 0;
        }

        private void DrawDialogueGroup()
        {
            var dialogueGroup = DrawDialogueGroupHelperField();

            if (dialogueGroup != null)
                DialogueGroupIDProperty.stringValue = dialogueGroup.Id;
            else
                DialogueGroupIDProperty.stringValue = "";

            DialogueGroupIDProperty.DrawDisabledPropertyField();

            if (dialogueGroup == null && SelectedDialogueGroupIndex != 0)
                DialogueInspectorUtilities.DrawHelpBox("Invalid dialogue group assigned", MessageType.Error);
        }

        private DialogueGroupSO DrawDialogueGroupHelperField()
        {
            SelectedDialogueGroupIndex = DialogueInspectorUtilities.DrawPopup("Dialogue Group", SelectedDialogueGroupIndex, AvailableDialogueGroupNames);
            return AvailableDialogueGroups[AvailableDialogueGroupNames[SelectedDialogueGroupIndex]];
        }

        private void DrawDialogue()
        {
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
            SelectedDialogueIndex = DialogueInspectorUtilities.DrawPopup("Dialogue", SelectedDialogueIndex, AvailableDialogueNames);
            return AvailableDialogues[AvailableDialogueNames[SelectedDialogueIndex]];
        }
    }
}

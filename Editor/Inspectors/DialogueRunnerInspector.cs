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
        private SerializedProperty GroupIDProperty;
        private SerializedProperty StartingDialogueIDProperty;
        private SerializedProperty DialogueVisualizerProperty;

        private Dictionary<string, DialogueSO> AvailableDialogues;
        private string[] AvailableDialogueNames;
        private int SelectedDialogueIndex;

        private void OnEnable()
        {
            DialogueContainerProperty = serializedObject.FindProperty("DialogueGraph");
            GroupIDProperty = serializedObject.FindProperty("GroupID");
            StartingDialogueIDProperty = serializedObject.FindProperty("StartingDialogueID");
            DialogueVisualizerProperty = serializedObject.FindProperty("DialogueVisualizer");

            if (DialogueContainerProperty.objectReferenceValue != null)
                UpdateAvailableDialogues();

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
            if (prevContainerRef != DialogueContainerProperty.objectReferenceValue)
                UpdateAvailableDialogues();
            if (DialogueContainerProperty.objectReferenceValue == null)
            {
                DialogueInspectorUtilities.DrawHelpBox("Assign dialogue graph");
                return;
            }
            DialogueInspectorUtilities.DrawSpace(SpacingValue);

            DrawDialogue();
        }

        private void UpdateAvailableDialogues()
        {
            DialogueContainerSO dialogueContainer = (DialogueContainerSO)DialogueContainerProperty.objectReferenceValue;
            if (dialogueContainer == null)
                return;

            DialogueSO selectedDialogue = dialogueContainer.GetDialogue(StartingDialogueIDProperty.stringValue);
            List<DialogueSO> allDialogues = dialogueContainer.GetAllDialogues();
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

        private void DrawDialogue()
        {
            var dialogueId = StartingDialogueIDProperty.stringValue;
            var dialogueContainerSO = (DialogueContainerSO)DialogueContainerProperty.objectReferenceValue;

            var dialogue = dialogueContainerSO.GetDialogue(dialogueId);

            dialogue = DrawDialogueHelperField(dialogue);

            if (dialogue != null)
                StartingDialogueIDProperty.stringValue = dialogue.Id;
            else
                StartingDialogueIDProperty.stringValue = "";

            StartingDialogueIDProperty.DrawDisabledPropertyField();

            if (dialogue == null)
                DialogueInspectorUtilities.DrawHelpBox("Invalid dialogue assigned", MessageType.Error);
        }
        private DialogueSO DrawDialogueHelperField(DialogueSO dialogue)
        {
            SelectedDialogueIndex = DialogueInspectorUtilities.DrawPopup("Dialogue", SelectedDialogueIndex, AvailableDialogueNames);
            return AvailableDialogues[AvailableDialogueNames[SelectedDialogueIndex]];
        }
    }
}

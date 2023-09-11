using FrameworksXD.DialogueXD.Data;
using FrameworksXD.DialogueXD.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FrameworksXD.DialogueXD
{

    public class DialogueRunner : MonoBehaviour
    {
        [SerializeField] private DialogueContainerSO DialogueGraph;
        [SerializeField] private string DialogueGroupID;
        [SerializeField] private string StartingDialogueID;
        [SerializeField] private DialogueVisualizer DialogueVisualizer;

        public UnityEvent<DialogueSO> OnDialogueSequenceStarted;
        public UnityEvent<DialogueSO> OnDialogueShown;
        public UnityEvent<DialogueChoiceData> OnDialogueChoiceSelected;
        public UnityEvent OnDialogueSequenceFinished;

        private DialogueSO StartingDialogue;
        private DialogueSO CurrentDialogue;

        public DialogueSO GetStartingDialogue()
        { 
            if(StartingDialogue == null)
                StartingDialogue = DialogueGraph.GetDialogue(StartingDialogueID, DialogueGroupID);
            return StartingDialogue;
        }

        public DialogueSO GetCurrentDialogue()
        {
            return CurrentDialogue;
        }

        public void SetDialogueVisualizer(DialogueVisualizer dialogueVisualizer)
        {
            DialogueVisualizer = dialogueVisualizer;
        }

        public virtual void StartDialogue()
        {
            GetStartingDialogue();
            OnDialogueSequenceStarted.Invoke(StartingDialogue);
            ShowDialogue(StartingDialogue);
        }

        public virtual void StartDialogue(DialogueSO dialogue)
        {
            OnDialogueSequenceStarted.Invoke(dialogue);
            ShowDialogue(dialogue);
        }

        public virtual void ShowDialogue(DialogueSO dialogue)
        {
            CurrentDialogue = dialogue;
            DialogueVisualizer.ShowDialogue(CurrentDialogue, OnChoiceSelected);
            OnDialogueShown.Invoke(CurrentDialogue);
        }

        protected void OnChoiceSelected(DialogueChoiceData choice)
        {
            OnDialogueChoiceSelected.Invoke(choice);
            if (choice.NextDialogue == null)
                FinishDialogueSequence();
            else
                ShowDialogue(choice.NextDialogue);
        }

        protected virtual void FinishDialogueSequence()
        {
            DialogueVisualizer.CloseDialogue();
            OnDialogueSequenceFinished.Invoke();
        }
    }
}

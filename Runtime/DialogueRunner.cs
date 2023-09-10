using FrameworksXD.DialogueXD.Data;
using FrameworksXD.DialogueXD.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FrameworksXD.DialogueXD
{

    public class DialogueRunner : MonoBehaviour
    {
        [SerializeField] private DialogueContainerSO DialogueGraph;
        [SerializeField] private string DialogueGroupID;
        [SerializeField] private string StartingDialogueID;
        [SerializeField] private DialogueVisualizer DialogueVisualizer;

        private DialogueSO StartingDialogue;
        private DialogueSO CurrentDialogue;

        private void Start()
        {
            StartingDialogue = DialogueGraph.GetDialogue(StartingDialogueID, DialogueGroupID);
            StartDialogue();
        }

        public void StartDialogue()
        {
            NextDialogue(StartingDialogue);
        }

        private void NextDialogue(DialogueSO nextDialogue)
        {
            CurrentDialogue = nextDialogue;
            DialogueVisualizer.ShowDialogue(CurrentDialogue, OnDialogueShown);
        }

        private void OnDialogueShown(DialogueChoiceData choice)
        {
            if (choice.NextDialogue == null)
            {
                Debug.LogError("Dialogue finished");
            }
            else
                NextDialogue(choice.NextDialogue);
        }
    }
}

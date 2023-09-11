using FrameworksXD.DialogueXD.ScriptableObjects;
using System;
using UnityEngine;

namespace FrameworksXD.DialogueXD
{
    public abstract class DialogueVisualizer : MonoBehaviour
    {
        protected Action<DialogueChoiceData> OnChoiceSelected;
        protected DialogueSO CurrentDialogue;

        public void ShowDialogue(DialogueSO dialogue, Action<DialogueChoiceData> onChoiceSelected)
        {
            OnChoiceSelected = onChoiceSelected;
            CurrentDialogue = dialogue;
            ShowDialogue();
        }

        protected abstract void ShowDialogue();

        public virtual void CloseDialogue() 
        {
            OnChoiceSelected = null;
            CurrentDialogue = null;
        }
    }
}

using FrameworksXD.DialogueXD.Data;
using FrameworksXD.DialogueXD.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FrameworksXD.DialogueXD
{
    public class DialogueVisualizer : MonoBehaviour
    {
        private Action<DialogueChoiceData> OnDialogueShown;
        private DialogueSO CurrentDialogue;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                OnDialogueShown?.Invoke(GetSelectedChoice());
        }

        private DialogueChoiceData GetSelectedChoice()
        {
            List<DialogueChoiceData> choices = CurrentDialogue.Choices;
            DialogueChoiceData choice = null;
            switch (CurrentDialogue.DialogueType)
            {
                case DialogueType.SingleChoice:
                    choice = choices[0];
                    break;
                case DialogueType.MultipleChoice:
                    choice = choices[UnityEngine.Random.Range(0, choices.Count)];
                    break;
                default:
                    Debug.LogError($"Invalid dialogue type: {CurrentDialogue.DialogueType}");
                    break;
            }
            Debug.LogError($"Selecting: {choice.Text}");
            return choice;
        }

        public void ShowDialogue(DialogueSO dialogue, Action<DialogueChoiceData> onShown)
        {
            OnDialogueShown = onShown;
            CurrentDialogue = dialogue;
            Debug.LogError($"Showing dialogue ({dialogue.GetSpeakerName()}): {dialogue.GetDialogueText()}");
        }
    }
}

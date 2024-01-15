using System.Collections.Generic;
using UnityEngine;

namespace FrameworksXD.DialogueXD.ScriptableObjects
{
    using System;

    public class DialogueSO : ScriptableObject
    {
        [field: SerializeField] public string Id { get; set; }
        [field: SerializeField] public string DialogueName { get; set; }
        [field: SerializeField] [field: TextArea()] public string Text { get; set; }
        [field: SerializeField] public DialogueSpeakerSO Speaker { get; set; }
        [field: SerializeField] public List<DialogueChoiceData> Choices { get; set; }
        [field: SerializeField] public DialogueType DialogueType { get; set; }
        [field: SerializeField] public bool IsStartingDialogue { get; set; }

        public void Initialize(string id, string dialogueName, string text, DialogueSpeakerSO speaker, List<DialogueChoiceData> choices, DialogueType dialogueType, bool isStartingDialogue)
        {
            Id = id;
            DialogueName = dialogueName;
            Text = text;
            Speaker = speaker;
            Choices = choices;
            DialogueType = dialogueType;
            IsStartingDialogue = isStartingDialogue;
        }

        public string GetDialogueText()
        {
            return Text;
        }

        public string GetSpeakerName()
        {
            return Speaker == null ? "None" : Speaker.SpeakerName;
        }

        public bool HasNextDialogue()
        {
            return Choices != null && Choices.Count > 0 && Choices[0].NextDialogue != null;
        }
    }
}

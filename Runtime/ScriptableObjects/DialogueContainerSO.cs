using FrameworksXD.DialogueXD.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrameworksXD.DialogueXD.ScriptableObjects
{
    public class DialogueContainerSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public SerializableDictionary<string, DialogueGroupSO> DialogueGroups { get; set; }
        [field: SerializeField] public SerializableDictionary<string, DialogueSO> UngroupedDialogues { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;
            DialogueGroups = new SerializableDictionary<string, DialogueGroupSO>();
            UngroupedDialogues = new SerializableDictionary<string, DialogueSO>();
        }

        public DialogueSO GetDialogue(string dialogueId, string groupID = null)
        {
            if (string.IsNullOrEmpty(groupID)) 
            {
                if (UngroupedDialogues.ContainsKey(dialogueId))
                    return UngroupedDialogues[dialogueId];
                return null;
            }

            if (DialogueGroups.ContainsKey(groupID))
                return DialogueGroups[groupID].GetDialogue(dialogueId);

            return null;
        }

        public DialogueGroupSO GetDialogueGroup(string groupID)
        {
            if (DialogueGroups.ContainsKey(groupID))
                return DialogueGroups[groupID];
            return null;
        }

        public List<DialogueSO> GetAllDialogues()
        {
            List<DialogueSO> dialogues = new List<DialogueSO>();
            foreach (var dialogue in UngroupedDialogues)
                dialogues.Add(dialogue.Value);

            foreach (var group in DialogueGroups)
                foreach (var dialogue in group.Value.GetAllDialogues())
                    dialogues.Add(dialogue);

            return dialogues;
        }
        public List<DialogueSO> GetAllAvailableDialogues(string groupID = null)
        {
            if (string.IsNullOrEmpty(groupID))
            {
                List<DialogueSO> dialogues = new List<DialogueSO>();
                foreach (var dialogue in UngroupedDialogues)
                    dialogues.Add(dialogue.Value);
                return dialogues;
            }

            return GetAllDialoguesInGroup(groupID);
        }
        public List<DialogueSO> GetAllDialoguesInGroup(string groupID)
        {
            if (DialogueGroups.ContainsKey(groupID))
                return DialogueGroups[groupID].GetAllDialogues();

            Debug.LogError($"Group not found in DialogueGraph: {FileName}");
            return null;
        }

        public List<DialogueGroupSO> GetAllDialogueGroups()
        {
            List<DialogueGroupSO> dialogueGroups = new List<DialogueGroupSO>();
            foreach (var group in DialogueGroups)
                dialogueGroups.Add(group.Value);
            return dialogueGroups;
        }
    }
}

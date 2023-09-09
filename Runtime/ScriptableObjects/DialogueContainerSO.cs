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
                DialogueGroups[groupID].GetDialogue(dialogueId);

            return null;
        }
    }
}

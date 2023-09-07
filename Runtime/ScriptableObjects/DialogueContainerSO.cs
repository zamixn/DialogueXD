using FrameworksXD.DialogueXD.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace FrameworksXD.DialogueXD.ScriptableObjects
{
    public class DialogueContainerSO : ScriptableObject
    {
        [field:SerializeField] public string FileName { get; set; }
        [field: SerializeField] public SerializableDictionary<DialogueGroupSO, List<DialogueSO>> DialogueGroups { get; set; }
        [field: SerializeField] public List<DialogueSO> UngroupedDialogues { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;
            DialogueGroups = new SerializableDictionary<DialogueGroupSO, List<DialogueSO>>();
            UngroupedDialogues = new List<DialogueSO>();
        }
    }
}

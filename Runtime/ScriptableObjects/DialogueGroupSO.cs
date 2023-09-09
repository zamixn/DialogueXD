using FrameworksXD.DialogueXD.Utilities;
using UnityEngine;

namespace FrameworksXD.DialogueXD.ScriptableObjects
{
    public class DialogueGroupSO : ScriptableObject
    {
        [field: SerializeField] public string Id { get; set; }
        [field: SerializeField] public string GroupName { get; set; }
        [field: SerializeField] public SerializableDictionary<string, DialogueSO> Dialogues { get; set; }

        public void Initialize(string id, string groupName)
        {
            Id = id;
            GroupName = groupName;
            Dialogues = new SerializableDictionary<string, DialogueSO>();
        }


        public DialogueSO GetDialogue(string id)
        {
            if (Dialogues.ContainsKey(id))
                return Dialogues[id];
            return null;
        }
    }
}

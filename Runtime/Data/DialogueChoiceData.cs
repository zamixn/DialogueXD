using FrameworksXD.DialogueXD.ScriptableObjects;
using UnityEngine;

namespace FrameworksXD.DialogueXD.Data
{

    [System.Serializable]
    public class DialogueChoiceData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public DialogueSO NextDialogue { get; set; }
    }
}

using UnityEngine;

namespace FrameworksXD.DialogueXD.Editor.Save
{
    [System.Serializable]
    public class DialogueChoiceSaveData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public string NodeId { get; set; }
    }
}

using FrameworksXD.DialogueXD;
using System.Collections.Generic;
using UnityEngine;

namespace FrameworksXD.DialogueXD.Editor.Save
{
    [System.Serializable]
    public class DialogueNodeSaveData
    {
        [field: SerializeField] public string Id { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public List<DialogueChoiceSaveData> Choices { get; set; }
        [field: SerializeField] public string GroupId { get; set; }
        [field: SerializeField] public DialogueType DialogueType { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
    }
}

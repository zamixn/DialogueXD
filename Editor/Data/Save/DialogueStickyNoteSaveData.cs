using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace FrameworksXD.DialogueXD.Editor.Save
{
    [System.Serializable]
    public class DialogueStickyNoteSaveData
    {
        [field: SerializeField] public string Title { get; set; }
        [field: SerializeField] public string Content { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
        [field: SerializeField] public StickyNoteTheme Theme { get; set; }
        [field: SerializeField] public StickyNoteFontSize FontSize { get; set; }
    }
}

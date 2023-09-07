using UnityEngine;

namespace FrameworksXD.DialogueXD.Editor.Save
{
    [System.Serializable]
    public class DialogueGroupSaveData
    {
        [field: SerializeField] public string Id { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }

    }
}

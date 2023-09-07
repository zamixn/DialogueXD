using FrameworksXD.DialogueXD.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace FrameworksXD.DialogueXD.Editor.Save
{
    public class DialogueGraphSaveDataSO : ScriptableObject
    {
        [field: SerializeField] public string FileName;
        [field: SerializeField] public List<DialogueGroupSaveData> Groups { get; set; }
        [field: SerializeField] public List<DialogueNodeSaveData> Nodes { get; set; }
        [field: SerializeField] public List<string> OldGroupNames { get; set; }
        [field: SerializeField] public List<string> OldUngroupedNodedNames { get; set; }
        [field: SerializeField] public SerializableDictionary<string, List<string>> OldGroupedNodeNames { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;

            Groups = new List<DialogueGroupSaveData>();
            Nodes = new List<DialogueNodeSaveData>();
        }
    }
}

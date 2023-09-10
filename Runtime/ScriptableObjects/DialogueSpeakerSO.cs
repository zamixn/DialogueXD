using FrameworksXD.DialogueXD.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace FrameworksXD.DialogueXD.ScriptableObjects
{
    public class DialogueSpeakerSO : ScriptableObject
    {
        [field: SerializeField] public string Id { get; set; }
        [field: SerializeField] public string SpeakerName { get; set; }

        public void Initialize(string id, string speakerName)
        {
            Id = id;
            SpeakerName = speakerName;
        }
    }
}

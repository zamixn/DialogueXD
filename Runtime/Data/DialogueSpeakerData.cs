using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FrameworksXD.DialogueXD.Data
{
    [System.Serializable]
    public class DialogueSpeakerData
    {
        [field: SerializeField] public string Id { get; set; }
        [field: SerializeField] public string Name { get; set; }

        public DialogueSpeakerData(string name = "")
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
        }
    }
}

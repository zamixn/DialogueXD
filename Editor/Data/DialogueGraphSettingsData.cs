using System.Collections.Generic;
using UnityEngine;

namespace FrameworksXD.DialogueXD.Editor
{

    [System.Serializable]
    public class DialogueGraphSettingsData
    {
        [field: SerializeField] public List<DialogueSpeakerData> DialogueSpeakerData { get; set; }

        public DialogueGraphSettingsData()
        {
            DialogueSpeakerData = new List<DialogueSpeakerData>() { new DialogueSpeakerData("None") { Id = "" } };
        }

        public void Clear()
        {
            DialogueSpeakerData.Clear();
        }

        public int GetSpeakerIndex(string speakerId)
        {
            for (int i = 0; i < DialogueSpeakerData.Count; i++)
            {
                if (speakerId == DialogueSpeakerData[i].Id)
                    return i;
            }
            return 0;
        }
    }
}

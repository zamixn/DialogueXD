using UnityEngine;

namespace FrameworksXD.DialogueXD.Editor.Error
{
    public class DialogueErrorData
    {
        public Color Color { get; set; }

        public DialogueErrorData()
        {
            GenerateRandomColor();
        }

        private void GenerateRandomColor()
        {
            Color = new Color32(
                (byte)Random.Range(165, 256),
                (byte)Random.Range(150, 256),
                (byte)Random.Range(150, 256),
                255
            );
        }
    }
}

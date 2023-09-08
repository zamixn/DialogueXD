using FrameworksXD.DialogueXD.Editor.Utilities;
using UnityEngine;

namespace FrameworksXD.DialogueXD.Editor.Error
{
    public class DialogueErrorData
    {
        public Color Color { get; set; }

        public DialogueErrorData()
        {
            Color = ColorUtilities.RandomErrorColor();
        }
    }
}

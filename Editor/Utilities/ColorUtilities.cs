using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FrameworksXD.DialogueXD.Editor.Utilities
{
    public static class ColorUtilities
    {
        public static Color RandomErrorColor()
        {
            return new Color32(
                (byte)Random.Range(165, 256),
                (byte)Random.Range(150, 256),
                (byte)Random.Range(150, 256),
                255
            );
        }
    }
}

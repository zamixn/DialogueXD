using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace FrameworksXD.DialogueXD.Editor.GraphEditor.Elements
{
    public class DialogueGroup : Group
    {
        public string Id { get; set; }
        private Color DefaultBorderColor;
        private float DefaultBorderWidth;

        public string OldTitle { get; set; }

        public DialogueGroup(string groupTitle, Vector2 position)
        {
            Id = Guid.NewGuid().ToString();
            title = groupTitle;
            OldTitle = groupTitle;
            SetPosition(new Rect(position, Vector2.zero));

            DefaultBorderColor = contentContainer.style.borderBottomColor.value;
            DefaultBorderWidth = contentContainer.style.borderBottomWidth.value;
        }

        public void SetErrorStyle(Color color)
        {
            contentContainer.style.borderBottomColor = color;
            contentContainer.style.borderBottomWidth = 2f;
        }

        public void ResetStyle()
        {
            contentContainer.style.borderBottomColor = DefaultBorderColor;
            contentContainer.style.borderBottomWidth = DefaultBorderWidth;
        }
    }
}

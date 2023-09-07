using FrameworksXD.DialogueXD.Editor.GraphEditor.Elements;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace FrameworksXD.DialogueXD.Editor.Utilities
{
    public static class DialogueUIElementUtilities
    {
        public static TextField CreateTextField(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textField = new TextField() { value = value, label = label };
            if (onValueChanged != null)
                textField.RegisterCallback(onValueChanged);
            return textField;
        }

        public static TextField CreateTextArea(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textField = CreateTextField(value, label, onValueChanged);
            textField.multiline = true;
            return textField;
        }

        public static Foldout CreateFoldout(string title, bool collapsed = false)
        {
            Foldout foldout = new Foldout() { text = title, value = !collapsed };
            return foldout;
        }

        public static Button CreateButton(string text, Action onClick = null)
        {
            Button button = new Button(onClick) { text = text };
            return button;
        }

        public static Port CreatePort(this DialogueNode node, string name = "", Orientation orientation = Orientation.Horizontal, Direction direction = Direction.Output, Port.Capacity capacity = Port.Capacity.Single)
        {
            Port port = node.InstantiatePort(orientation, direction, capacity, typeof(bool));
            port.portName = name;
            return port;
        }
    }
}

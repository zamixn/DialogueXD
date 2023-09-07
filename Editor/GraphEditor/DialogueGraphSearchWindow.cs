using FrameworksXD.DialogueXD.Editor.GraphEditor.Elements;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace FrameworksXD.DialogueXD.Editor.GraphEditor
{
    public class DialogueGraphSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogueGraphView DialogueGraphView;
        private Texture2D IdentationIcon;
        public void Initialize(DialogueGraphView dialogueGraphView)
        {
            DialogueGraphView = dialogueGraphView;

            IdentationIcon = new Texture2D(1, 1);
            IdentationIcon.SetPixel(0, 0, Color.clear);
            IdentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Element"), 0),
                new SearchTreeGroupEntry(new GUIContent("Dialogue Node"), 1),
                new SearchTreeEntry(new GUIContent("Single Choice", IdentationIcon)) { level=2, userData= DialogueType.SingleChoice},
                new SearchTreeEntry(new GUIContent("Multiple Choice", IdentationIcon)) { level=2, userData= DialogueType.MultipleChoice},
                new SearchTreeGroupEntry(new GUIContent("Dialogue Group"), 1),
                new SearchTreeEntry(new GUIContent("Single Group", IdentationIcon)) { level = 2, userData = new Group()},
            };
            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            Vector2 localMousePosition = DialogueGraphView.GetLocalMousePosition(context.screenMousePosition, true);
            switch (searchTreeEntry.userData)
            {
                case DialogueType.SingleChoice:
                    {
                        DialogueNode node = DialogueGraphView.CreateNode(DialogueType.SingleChoice, localMousePosition);
                        DialogueGraphView.AddElement(node);
                        return true;
                    }
                case DialogueType.MultipleChoice:
                    {
                        DialogueNode node = DialogueGraphView.CreateNode(DialogueType.MultipleChoice, localMousePosition);
                        DialogueGraphView.AddElement(node);
                        return true;
                    }
                case Group _:
                    {
                        DialogueGraphView.CreateGroup("Dialogue Group", localMousePosition);
                        return true;
                    }
            }
            return false;
        }
    }
}

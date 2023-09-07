using FrameworksXD.DialogueXD.Editor.Save;
using FrameworksXD.DialogueXD.Editor.Utilities;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace FrameworksXD.DialogueXD.Editor.GraphEditor.Elements
{
    public class DialogueNodeSingleChoice : DialogueNode
    {
        public override void Initialize(string nodeName, DialogueGraphView dialogueGraphView, Vector2 position)
        {
            base.Initialize(nodeName, dialogueGraphView, position);
            DialogueType = DialogueType.SingleChoice;

            Choices.Add(new DialogueChoiceSaveData() { Text = "Next Dialogue" });
        }

        public override void Draw()
        {
            base.Draw();

            foreach (var choice in Choices)
            {
                Port choicePort = this.CreatePort(choice.Text, Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
                choicePort.userData = choice;
                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }
    }
}

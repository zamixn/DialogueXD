using FrameworksXD.DialogueXD.Editor.Save;
using FrameworksXD.DialogueXD.Editor.Utilities;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrameworksXD.DialogueXD.Editor.GraphEditor.Elements
{
    public class DialogueNodeMultipleChoice : DialogueNode
    {
        public override void Initialize(string nodeName, DialogueGraphView dialogueGraphView, Vector2 position)
        {
            base.Initialize(nodeName, dialogueGraphView, position);
            DialogueType = DialogueType.MultipleChoice;

            Choices.Add(new DialogueChoiceSaveData() { Text = "New Choice" });
        }

        public override void Draw()
        {
            base.Draw();

            Button addChoiceButton = DialogueUIElementUtilities.CreateButton("Add Choice", () =>
            {
                var choiceData = new DialogueChoiceSaveData() { Text = "New Choice" };
                Choices.Add(choiceData);
                CreateNewPort(choiceData);
            });
            addChoiceButton.AddClasses("d-node__button");
            mainContainer.Insert(1, addChoiceButton);

            foreach (var choice in Choices)
            {
                CreateNewPort(choice);
            }

            RefreshExpandedState();
        }

        private void CreateNewPort(object userData)
        {
            Port choicePort = this.CreatePort("", Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
            choicePort.userData = userData;

            DialogueChoiceSaveData choiceData = (DialogueChoiceSaveData)userData;

            Button deleteChoiceButton = DialogueUIElementUtilities.CreateButton("X", () => 
            {
                if (Choices.Count == 1)
                    return;

                if (choicePort.connected)
                {
                    DialogueGraphView.DeleteElements(choicePort.connections);
                }

                Choices.Remove(choiceData);
                DialogueGraphView.RemoveElement(choicePort);
            });
            deleteChoiceButton.AddClasses("d-node-button");
            TextField choiceTextField = DialogueUIElementUtilities.CreateTextField(choiceData.Text, null, callback =>
            {
                choiceData.Text = callback.newValue;
            });
            choiceTextField.AddClasses("d-node__textfield", "d-node__choice-textfield", "d-node__textfield__hidden");

            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);

            outputContainer.Add(choicePort);
        }
    }
}


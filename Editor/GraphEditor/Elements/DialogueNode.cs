using FrameworksXD.DialogueXD.Data;
using FrameworksXD.DialogueXD.Editor.Save;
using FrameworksXD.DialogueXD.Editor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrameworksXD.DialogueXD.Editor.GraphEditor.Elements
{
    public class DialogueNode : Node
    {
        protected DialogueGraphView DialogueGraphView;

        public string Id { get; set; }
        public string DialogueName { get; set; }
        public List<DialogueChoiceSaveData> Choices { get; set; }
        public string Text { get; set; }
        public DialogueType DialogueType { get; set; }

        private Color DefaultBackgroundColor;
        private Color DefaultTitleBackgroundColor;
        public DialogueGroup Group { get; set; }

        public int SpeakerIndex { get; set; }


        private TextField DialogueNameTextField;
        private PopupField<DialogueSpeakerData> SpeakerPopup;

        public virtual void Initialize(string nodeName, DialogueGraphView dialogueGraphView, Vector2 position)
        {
            Id = Guid.NewGuid().ToString();
            DialogueName = nodeName;
            Choices = new List<DialogueChoiceSaveData>();
            Text = "placeholder";

            DefaultBackgroundColor = new Color(29f / 255f, 29 / 255f, 30 / 255f);
            DefaultTitleBackgroundColor = titleContainer.style.borderTopColor.value;
            DialogueGraphView = dialogueGraphView;

            SetPosition(new Rect(position, Vector2.zero));

            mainContainer.AddClasses("d-node__main-container");
            extensionContainer.AddClasses("d-node__extension-container");
        }

        public virtual void Draw()
        {
            DialogueNameTextField = DialogueUIElementUtilities.CreateTextField(DialogueName, null, callback => 
            {
                TextField target = (TextField)callback.target;
                target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();


                bool wasTitleEmpty = string.IsNullOrEmpty(DialogueName); 

                if (Group == null)
                {
                    DialogueGraphView.RemoveUngroupedNode(this);
                    DialogueName = target.value;
                    DialogueGraphView.AddUngroupedNode(this);
                }
                else
                {
                    DialogueGroup currentGroup = Group;
                    DialogueGraphView.RemoveGroupedNode(this, currentGroup);
                    DialogueName = target.value;
                    DialogueGraphView.AddGroupedNode(this, currentGroup);
                }

                if (string.IsNullOrEmpty(target.value))
                {
                    if (!wasTitleEmpty)
                    {
                        ++DialogueGraphView.EmptyDialogueNames;
                        SetErrorStyle(ColorUtilities.RandomErrorColor());
                    }
                }
                else
                {
                    if (wasTitleEmpty)
                    {
                        --DialogueGraphView.EmptyDialogueNames;
                        if (DialogueGraphView.HasDuplicateName(this, out Color c))
                            SetErrorStyle(c);
                        else
                            ResetStyle();
                    }
                }
            });
            titleContainer.Insert(0, DialogueNameTextField);
            DialogueNameTextField.AddClasses("d-node__textfield", "d-node__filename-textfield", "d-node__textfield__hidden");

            Port inputPort = this.CreatePort("Previous Conenction", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(inputPort);

            VisualElement customDataContainer = new VisualElement();
            customDataContainer.AddClasses("d-node__custom-data-container");
            Foldout textFoldout = DialogueUIElementUtilities.CreateFoldout("Dialogue Text");
            TextField textTextField = DialogueUIElementUtilities.CreateTextField(Text, null, callback => 
            {
                Text = callback.newValue;
            });
            textTextField.AddClasses("d-node__textfield", "d-node__quote-textfield");
            textFoldout.Add(textTextField);

            Foldout additionalSettingsFoldout = DialogueUIElementUtilities.CreateFoldout("Additional settings");
            SpeakerPopup = new PopupField<DialogueSpeakerData>("Speaker", DialogueGraphView.GetAvailableSpeakers(), SpeakerIndex, GetSpeakerPopupValue, GetSpeakerPopupValue);
            SpeakerPopup.RegisterCallback(new EventCallback<MouseDownEvent>(OnSpeakerPopupMouseDown), TrickleDown.TrickleDown);
            additionalSettingsFoldout.Add(SpeakerPopup);

            customDataContainer.Add(textFoldout);
            customDataContainer.Add(additionalSettingsFoldout);

            extensionContainer.Add(customDataContainer);
        }

        private void OnSpeakerPopupMouseDown(MouseDownEvent evt)
        {
            RefreshSpeakers();
        }

        private string GetSpeakerPopupValue(DialogueSpeakerData speaker)
        {
            return speaker.Name;
        }

        public void RefreshSpeakers()
        {
            SpeakerPopup.choices = DialogueGraphView.GetAvailableSpeakers();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Inputs", _ => DisconnectPorts(inputContainer));
            evt.menu.AppendAction("Disconnect Outputs", _ => DisconnectPorts(outputContainer));
            base.BuildContextualMenu(evt);
        }

        public void DisconnectAllPorts()
        {
            DisconnectPorts(inputContainer);
            DisconnectPorts(outputContainer);
        }

        private void DisconnectPorts(VisualElement container)
        {
            foreach (Port port in container.Children())
            {
                if (!port.connected)
                    continue;

                DialogueGraphView.DeleteElements(port.connections);
            }
        }

        public bool IsStartingNode()
        {
            Port inputPort = (Port)inputContainer.Children().First();
            return !inputPort.connected;
        }

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
            titleContainer.style.borderTopColor = color;
            titleContainer.style.borderTopWidth = 2f;
        }

        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = DefaultBackgroundColor;
            titleContainer.style.borderTopColor = DefaultTitleBackgroundColor;
            titleContainer.style.borderTopWidth = 0f;
        }

        public string GetDialogueNameTextFieldValue() => DialogueNameTextField.value;

        public DialogueSpeakerData GetSelectedSpeaker()
        {
            return SpeakerPopup.value;
        }
    }
}

using FrameworksXD.DialogueXD.Editor.GraphEditor.Elements;
using System.Collections.Generic;

namespace FrameworksXD.DialogueXD.Editor.Error
{
    public class DialogueGroupErrorData
    {
        public DialogueErrorData ErrorData { get; set; }
        public List<DialogueGroup> Groups { get; set; }

        public DialogueGroupErrorData()
        {
            ErrorData = new DialogueErrorData();
            Groups = new List<DialogueGroup>();
        }


    }
}

using FrameworksXD.DialogueXD.ScriptableObjects;
using UnityEngine;

namespace FrameworksXD.DialogueXD
{
    public class DialogueHolder : MonoBehaviour
    {
        [SerializeField] protected DialogueContainerSO DialogueGraph;
        [SerializeField] protected string DialogueGroupID;
        [SerializeField] protected string StartingDialogueID;

        protected DialogueSO StartingDialogue;

        protected void Awake()
        {
            StartingDialogue = DialogueGraph.GetDialogue(StartingDialogueID, DialogueGroupID);
        }

        public DialogueSO GetStartingDialogue()
        { 
            return StartingDialogue;
        }

        public string GetSpeakerName()
        {
            return StartingDialogue.GetSpeakerName();
        }
    }
}

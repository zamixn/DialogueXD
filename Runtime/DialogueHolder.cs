using FrameworksXD.DialogueXD.ScriptableObjects;
using UnityEngine;

namespace FrameworksXD.DialogueXD
{
    public class DialogueHolder : MonoBehaviour
    {
        [SerializeField] protected DialogueContainerSO DialogueGraph;
        [SerializeField] protected string StartingDialogueGroupID;
        [SerializeField] protected string StartingDialogueID;

        protected DialogueSO StartingDialogue;

        protected void Awake()
        {
            StartingDialogue = DialogueGraph.GetDialogue(StartingDialogueID, StartingDialogueGroupID);
        }

        public DialogueSO GetStartingDialogue()
        { 
            return StartingDialogue;
        }

        public string GetSpeakerName()
        {
            return StartingDialogue.GetSpeakerName();
        }

        public DialogueContainerSO GetDialogueGraph()
        {
            return DialogueGraph;
        }

        public DialogueSO GetDialogue(string id)
        {
            return DialogueGraph.GetDialogueFromAnyGroup(id);
        }

        public DialogueSO GetDialogue(string dialogueId, string groupId)
        {
            return DialogueGraph.GetDialogue(dialogueId, groupId);
        }
    }
}

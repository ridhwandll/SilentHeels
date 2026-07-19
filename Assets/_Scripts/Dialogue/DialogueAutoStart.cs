using UnityEngine;

public class DialogueAutoStart : MonoBehaviour
{
    [Header("The Story")]
    public DialogueLine[] Conversation;

    [Header("Next Destination")]
    [Tooltip("The exact name of the Scene to load when the dialogue finishes.")]
    public string NextSceneName;

    private void Start()
    {
        DialogueManager.Instance.StartDialogue(Conversation, NextSceneName);
    }
}
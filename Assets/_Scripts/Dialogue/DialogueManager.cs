using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct DialogueLine
{
    public string SpeakerName;
    [TextArea(3, 10)]
    public string Sentence;
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Elements")]
    public GameObject DialoguePanel;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI DialogueText;

    [Header("Settings")]
    public float TypingSpeed = 0.02f;
    public KeyCode AdvanceKey1 = KeyCode.Space;
    public KeyCode AdvanceKey2 = KeyCode.E;
    public KeyCode SkipKey = KeyCode.Return;

    private Queue<DialogueLine> _sentences = new Queue<DialogueLine>();
    private bool _isTyping = false;
    private string _currentSentence = "";
    private string _sceneToLoadAfter = "";

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void StartDialogue(DialogueLine[] dialogueLines, string nextSceneName)
    {
        DialoguePanel.SetActive(true);
        _sentences.Clear();
        _sceneToLoadAfter = nextSceneName;

        // Load all lines into the queue
        foreach (DialogueLine line in dialogueLines)
            _sentences.Enqueue(line);

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (_isTyping)
        {
            StopAllCoroutines();
            DialogueText.text = _currentSentence;
            _isTyping = false;
            return;
        }

        if (_sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = _sentences.Dequeue();
        NameText.text = currentLine.SpeakerName;
        _currentSentence = currentLine.Sentence;

        StopAllCoroutines();
        StartCoroutine(TypeSentence(_currentSentence));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        _isTyping = true;
        DialogueText.text = "";

        // Loop through each character to create the Typewriter Effect
        foreach (char letter in sentence.ToCharArray())
        {
            DialogueText.text += letter;
            yield return new WaitForSeconds(TypingSpeed);
        }

        _isTyping = false;
    }

    // New method to handle skipping the entire sequence
    public void SkipEntireDialogue()
    {
        StopAllCoroutines();
        _sentences.Clear();
        _isTyping = false;
        EndDialogue();
    }

    private void EndDialogue()
    {
        DialoguePanel.SetActive(false);

        // Load the gameplay level if a name was provided
        if (!string.IsNullOrEmpty(_sceneToLoadAfter))
            SceneManager.LoadScene(_sceneToLoadAfter);
    }

    private void Update()
    {
        if (DialoguePanel.activeSelf)
        {
            if (Input.GetKeyDown(SkipKey))
            {
                SkipEntireDialogue();
            }
            else if (Input.GetKeyDown(AdvanceKey1) || Input.GetKeyDown(AdvanceKey2))
            {
                DisplayNextSentence();
            }
        }
    }
}
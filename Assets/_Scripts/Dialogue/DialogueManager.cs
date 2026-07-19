using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// The custom struct to hold our data in the Inspector
[System.Serializable]
public struct DialogueLine
{
    public string SpeakerName;
    [TextArea(3, 10)] // Makes the text box bigger in the Inspector
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

    private void EndDialogue()
    {
        DialoguePanel.SetActive(false);

        // Load the gameplay level if a name was provided
        if (!string.IsNullOrEmpty(_sceneToLoadAfter))
            SceneManager.LoadScene(_sceneToLoadAfter);
    }

    private void Update()
    {
        if (DialoguePanel.activeSelf && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E)))
            DisplayNextSentence();
    }
}
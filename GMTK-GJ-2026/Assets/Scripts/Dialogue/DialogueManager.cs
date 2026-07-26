using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // new Input System package
using TMPro; // remove this line and swap TMP_Text -> Text if not using TextMeshPro
using System;

// most of this is claude vibe code



/// <summary>
/// The four speaking characters. Values match the border Animator's Int parameter:
/// Turtle = 0, Rat = 1, Player = 2, Shopkeeper = 3
/// </summary>
public enum Character
{
    Dial = 0,
    King = 1,
    Player = 2,
    Shopkeeper = 3,
    YogSlothoth = 4,
    Unknown = 5

}

/// <summary>One line of dialogue: who's speaking + what they say.</summary>
[System.Serializable]
public class DialogueLine
{
    public Character speaker;
    [TextArea(2, 5)]
    public string text;
}

/// <summary>
/// Drives the Dialogue Box UI for all four hardcoded characters.
/// Attach to your DialogueBox root and wire up the references below.
/// Names and portraits are set once in the Inspector (index 0-3 = Turtle, Rat, Player, Shopkeeper).
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialogueBoxRoot;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Animator borderAnimator;
    [SerializeField] private string borderIntParam = "CharacterIndex";

    [Header("Character Data (index: 0=Turtle, 1=Rat, 2=Player, 3=Shopkeeper, 4=Yog-Slothoth, 5=Unknown)")]
    [SerializeField]
    private string[] characterNames = new string[6]
    {
        "Dial",
        "King",
        "Player",
        "Shopkeeper",
        "Yog-Slothoth",
        "???"
    };

    [SerializeField]
    private Sprite[] characterPortraits = new Sprite[6];

    [Header("Typing")]
    [SerializeField] private float secondsPerChar = 0.02f;
    [SerializeField] private bool useTypewriterEffect = true;

    private DialogueLine[] currentLines;
    private int currentIndex;
    private Coroutine typingRoutine;
    private bool isTyping;
    private int dialogueStartFrame = -1; // guards against the same click that opened dialogue also advancing it

    public event Action OnDialogEnd;

    public bool IsDialogueActive => dialogueBoxRoot != null && dialogueBoxRoot.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (dialogueBoxRoot != null)
            dialogueBoxRoot.SetActive(false);
    }

    private void Update()
    {
        if (!IsDialogueActive) return;

        // Ignore input on the same frame dialogue started, so the click/interact
        // that opened the box doesn't also immediately advance it.
        if (Time.frameCount == dialogueStartFrame) return;

        bool advancePressed = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            || (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
            || (Keyboard.current != null && Keyboard.current.numpadEnterKey.wasPressedThisFrame);

        if (advancePressed)
            AdvanceDialogue();
    }

    /// <summary>Starts a conversation. Example call:
    /// DialogueManager.Instance.StartDialogue(new DialogueLine[] {
    ///     new DialogueLine { speaker = Character.Shopkeeper, text = "Welcome, traveler!" },
    ///     new DialogueLine { speaker = Character.Player, text = "Got anything good?" }
    /// });</summary>
    public void StartDialogue(DialogueLine[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("DialogueManager: tried to start an empty dialogue.");
            return;
        }

        currentLines = lines;
        currentIndex = 0;
        dialogueStartFrame = Time.frameCount;

        if (dialogueBoxRoot != null)
            dialogueBoxRoot.SetActive(true);

        DisplayLine(currentLines[currentIndex]);
    }

    /// <summary>Call on player input (click / Submit) to progress dialogue.</summary>
    public void AdvanceDialogue()
    {
        if (currentLines == null) return;

        if (isTyping)
        {
            CompleteTypingImmediately();
            return;
        }

        currentIndex++;

        if (currentIndex >= currentLines.Length)
        {
            EndDialogue();
            return;
        }

        DisplayLine(currentLines[currentIndex]);
    }

    public void EndDialogue()
    {
        currentLines = null;
        currentIndex = 0;
        isTyping = false;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        if (dialogueBoxRoot != null)
            dialogueBoxRoot.SetActive(false);

        OnDialogEnd?.Invoke();
    }

    private void DisplayLine(DialogueLine line)
    {
        int i = (int)line.speaker;

        if (nameText != null)
            nameText.text = characterNames[i];

        if (portraitImage != null)
        {
            portraitImage.sprite = characterPortraits[i];
            portraitImage.enabled = characterPortraits[i] != null;
        }

        if (borderAnimator != null)
            borderAnimator.SetInteger(borderIntParam, i);

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        if (useTypewriterEffect)
            typingRoutine = StartCoroutine(TypeLine(line.text));
        else
            dialogueText.text = line.text;
    }

    private IEnumerator TypeLine(string fullText)
    {
        isTyping = true;
        dialogueText.text = string.Empty;

        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(secondsPerChar);
        }

        isTyping = false;
    }

    private void CompleteTypingImmediately()
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        if (currentLines != null && currentIndex < currentLines.Length)
            dialogueText.text = currentLines[currentIndex].text;

        isTyping = false;
    }
}
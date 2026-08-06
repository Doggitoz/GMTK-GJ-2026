using System;
using UnityEngine;

/// <summary>
/// Put this on each question/dialogue GameObject (one per question set).
/// Your GameManager activates the GameObject to run the encounter.
///
/// Flow:
///   Intro dialogue (optional) -> Question dialogue -> player picks an answer
///     -> WRONG:   play incorrect-response dialogue -> done
///     -> CORRECT: play correct-response dialogue -> currency awarded
///                 -> player picks a topic (Yourself / The World / Extra-dimensional Meta Secrets)
///                 -> next unused lore entry for that topic plays (then is discarded) -> done
///
/// No changes to DialogueManager or ChoiceUIManager required - this only uses
/// their existing public API (StartDialogue, OnDialogEnd, ShowChoices).
/// </summary>
public class QuestionDialogueTrigger : MonoBehaviour
{
    [Header("Intro (optional, plays before the question)")]
    [SerializeField] private DialogueLine[] introLines;

    [Header("Question (asked via the Dialogue Box, e.g. by the Rat)")]
    [SerializeField] private DialogueLine[] questionLines;

    [Header("Choices (exactly 3, in display order)")]
    [SerializeField] private string[] choices = new string[3];
    [SerializeField] private int correctChoiceIndex;

    [Header("Follow-up Dialogue (played after the player answers)")]
    [SerializeField] private DialogueLine[] correctResponseLines;
    [SerializeField] private DialogueLine[] incorrectResponseLines;

    [Header("Reward")]
    [SerializeField] private int currencyReward = 10;

    [Header("Behavior")]
    [Tooltip("If true, automatically begins the sequence when this GameObject is enabled. " +
             "Leave this OFF if the object stays active in the scene and should only start on player interaction.")]
    [SerializeField] private bool autoStartOnEnable = false;

    // Labels for the post-correct-answer topic choice. Index matches LoreTopic enum values.
    private static readonly string[] TopicChoiceLabels =
    {
        "Yourself",
        "The World",
        "Extra-dimensional Meta Secrets"
    };

    /// Fires on ANY correct answer across all questions, with the reward amount.
    /// Subscribe your currency system to this once (e.g. GameManager.Awake / OnEnable).
    public static event Action<int> OnCorrectAnswer;

    /// Fires when THIS question is fully resolved (all dialogue/topic steps done), correct or not.
    /// Subscribe here if your GameManager wants to manage the question pool itself.
    public event Action<QuestionDialogueTrigger, bool> OnAnswered;

    private Action pendingDialogueCallback;

    private void OnEnable()
    {
        if (autoStartOnEnable)
            BeginQuestion();
    }

    private void OnDisable()
    {
        // Safety: don't leave a dangling subscription if this object gets
        // disabled mid-sequence (e.g. GameManager force-closes it).
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnDialogEnd -= HandleGenericDialogueFinished;

        pendingDialogueCallback = null;
    }

    public void BeginQuestion()
    {
        PlayDialogueThen(introLines, PlayQuestion);
    }

    private void PlayQuestion()
    {
        PlayDialogueThen(questionLines, ShowAnswerChoices);
    }

    private void ShowAnswerChoices()
    {
        if (ChoiceUIManager.Instance == null)
        {
            Debug.LogError("QuestionDialogueTrigger: No ChoiceUIManager found in scene.");
            return;
        }

        ChoiceUIManager.Instance.ShowChoices(choices, HandleAnswerSelected);
    }

    private void HandleAnswerSelected(int selectedIndex)
    {
        bool correct = selectedIndex == correctChoiceIndex;

        if (!correct)
        {
            PlayDialogueThen(incorrectResponseLines, () => FinishQuestion(false));
            return;
        }

        OnCorrectAnswer?.Invoke(currencyReward);
        Services.Game?.SaveGame();
        PlayDialogueThen(correctResponseLines, ShowTopicChoice);
    }

    private void ShowTopicChoice()
    {
        if (ChoiceUIManager.Instance == null)
        {
            Debug.LogError("QuestionDialogueTrigger: No ChoiceUIManager found in scene.");
            FinishQuestion(true);
            return;
        }

        ChoiceUIManager.Instance.ShowChoices(TopicChoiceLabels, HandleTopicSelected);
    }

    private void HandleTopicSelected(int topicIndex)
    {
        LoreTopic topic = (LoreTopic)topicIndex;

        DialogueLine[] loreLines = LoreRevealManager.Instance != null
            ? LoreRevealManager.Instance.GetNextAndRemove(topic)
            : null;

        // If that topic's pool is empty, this just skips straight to finishing.
        PlayDialogueThen(loreLines, () => FinishQuestion(true));
    }

    private void FinishQuestion(bool correct)
    {
        OnAnswered?.Invoke(this, correct);

        // This encounter is done either way - GameManager decides whether to
        // destroy/remove it from the pool (typically only on correct) via OnAnswered above.
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Plays a dialogue block (if any lines are provided) and calls onFinished
    /// once it ends. If lines is null/empty, skips straight to onFinished.
    /// </summary>
    private void PlayDialogueThen(DialogueLine[] lines, Action onFinished)
    {
        if (lines != null && lines.Length > 0 && DialogueManager.Instance != null)
        {
            pendingDialogueCallback = onFinished;
            DialogueManager.Instance.OnDialogEnd += HandleGenericDialogueFinished;
            DialogueManager.Instance.StartDialogue(lines);
        }
        else
        {
            onFinished?.Invoke();
        }
    }

    private void HandleGenericDialogueFinished()
    {
        DialogueManager.Instance.OnDialogEnd -= HandleGenericDialogueFinished;

        Action callback = pendingDialogueCallback;
        pendingDialogueCallback = null;
        callback?.Invoke();
    }
}
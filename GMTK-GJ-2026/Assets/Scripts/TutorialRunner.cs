using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class TutorialRunner : MonoBehaviour
{
    [SerializeField]
    bool TutorialDisabled;
    [SerializeField]
    Narration.Canvas NarrationCanvas;
    [SerializeField]
    Narration.Script TutorialScript;
    [SerializeField]
    GameObject _tutorialNPC;

    [SerializeField]
    private Animator _playerAnimator;

    [SerializeField]
    private Clock.Hand _hourHand;

    [SerializeField]
    private Clock.Hand _minuteHand;

    [SerializeField]
    private Clock.Hand _secondHand;

    [SerializeField]
    private DialogueLine[] _introDialogue;

    [SerializeField]
    private DialogueLine[] _hourHandDialogue;

    [SerializeField]
    private DialogueLine[] _minuteHandDialogue;

    [SerializeField]
    private DialogueLine[] _secondHandDialogue;

    [SerializeField]
    private CinemachineCamera _centerZoomCam;
    GameManager _gameManager => GameManager.Instance;
    void Start()
    {
        _gameManager.OnTutorialStart += RunTutorial;
    }

    private void OnDestroy()
    {
        _gameManager.OnTutorialStart -= RunTutorial;
    }

    private void RunTutorial()
    {
        StartCoroutine(RunTutorialCoroutine());
    }

    private IEnumerator RunTutorialCoroutine()
    {
        if (TutorialDisabled)
            yield break;

        // Enable black canvas
        NarrationCanvas.SetActive(true);

        // Make sure game manager states are correct
        _gameManager.StopGame();
        _gameManager.SetPlayerActive(false);

        yield return new WaitForSeconds(1f);

        // Teleport player to correct space in scene
        // Vector3(0, 1, -10)
        _gameManager.SetPlayerActive(false);

        // Set player animation to sleep
        _playerAnimator.SetBool("IsAsleep", true);

        // Play the narration and wait until its done
        yield return NarrationCanvas.PlayScript(TutorialScript);

        // Disable black canvas
        NarrationCanvas.SetActive(false);
        yield return new WaitForSeconds(3f);

        // Player animation to get up
        _playerAnimator.SetBool("IsAsleep", false);
        yield return WaitForAnimation("Standup");
        yield return new WaitForSeconds(2f);

        yield return PlayDialogue(
            new DialogueLine[]
            {
                CreatePlayerDialogue("A voice whispers to you, screaming in from all sides."),
                CreatePlayerDialogue("From every face, from every indicator…"),
                CreatePlayerDialogue("And somehow, from the cogs and tickers themselves."),
                CreatePlayerDialogue("It’s vibration, ancient, unknowable and crying out like a newborn babe."),
                CreatePlayerDialogue("Somehow… you know it,"),
                CreatePlayerDialogue("As the slithering…"),
                CreatePlayerDialogue("Velvety…"),
                CreatePlayerDialogue("Articulation of the Watcher…"),
                CreatePlayerDialogue("Yog-Sothoth.")
            }
        );

        // Enable NPC
        _tutorialNPC.SetActive(true);

        yield return PlayDialogue(
            new DialogueLine[]
            {
                CreateYogSlothothDialogue("Greetings… Ward…"),
                CreatePlayerDialogue("…Is it a time here?"),
                CreateYogSlothothDialogue("It is… now, and never, and forever."),
                CreateYogSlothothDialogue("The season of your final ordeal begins now."),
                CreateYogSlothothDialogue("We need not delay your suffering...")
            }
        );

        // Yield Dialogue: "This is the hour hand. It has to make a full rotation for you to survive."
        yield return _hourHand.TutorialSpin();
        yield return _minuteHand.TutorialSpin();
        yield return _secondHand.TutorialSpin();

        _gameManager.SetPlayerActive(true);

        // "The clock will deteriorate over time. You must maintain it"
        // "You may interact with the environment to make repairs."

        // "Its urgent that you tend to the winding of the clock throughout the day.
        // enable wind up task

        // Focus on wind up center of clock
        _centerZoomCam.Priority = 100;
        yield return new WaitForSeconds(2f);


        // "Be weary of rust buildup."
        // spawn in a single rust spot
        // "It is important that you clean it up before it overruns the clock."
        // yield until user cleans up rust

        // "You may run into unfamiliar errors that even I don't know. I trust you to figure it out"

        // "Pay attention to the state of the clock. Bad things will happen if it does not get maintained.
        // "The clock is not nice to those who are new. Good luck.

        _centerZoomCam.Priority = -100;

        // Disable NPC
        _tutorialNPC.SetActive(false);

        // insert scripted hard loss. Everything is way too difficult for this run. player is forced to die. Only rust and wind up
    }

    private IEnumerator WaitForAnimation(string stateName)
    {
        // Wait until the animator enters the animation state
        while (!_playerAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            yield return null;
        }

        // Wait until the animation finishes
        while (_playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }
    }

    DialogueLine CreatePlayerDialogue(string text)
    {
        return new DialogueLine
        {
            speaker = Character.Player,
            text = text
        };
    }

    DialogueLine CreateYogSlothothDialogue(string text)
    {
        return new DialogueLine
        {
            speaker = Character.Rat,
            text = text
        };
    }

    DialogueLine CreateMysteryDialogue(string text)
    {
        return new DialogueLine
        {
            speaker = Character.Turtle,
            text = text
        };
    }

    private IEnumerator PlayDialogue(DialogueLine[] dialogue)
    {
        bool dialogueFinished = false;

        void OnDialogueFinished()
        {
            dialogueFinished = true;
        }

        DialogueManager.Instance.OnDialogEnd += OnDialogueFinished;

        DialogueManager.Instance.StartDialogue(dialogue);

        while (!dialogueFinished)
        {
            yield return null;
        }

        DialogueManager.Instance.OnDialogEnd -= OnDialogueFinished;
    }
}

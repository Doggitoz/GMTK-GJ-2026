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


        // Introduction to YogSlothoth
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

        // Introduction to the world challenge
        yield return PlayDialogue(
            new DialogueLine[]
            {
                CreateYogSlothothDialogue("You shall be tried in increments according to your earthly progression."),
                CreateYogSlothothDialogue("Survive the revolution of time, and in your final hour, you may yet be saved."),
                CreateYogSlothothDialogue("Your death, however, cannot be helped if you don’t maintain your pace."),
                CreateYogSlothothDialogue("This clock is subject to the degradation of time, as all mortal constructs are."),
                CreateYogSlothothDialogue("But, if your will is strong, may yet persevere."),
            }
        );

        // Rust introduction
        yield return PlayDialogue(
            new DialogueLine[]
            {
                CreateYogSlothothDialogue("Purge the rust from the bones of this time piece.")
            }
        );
        // Zoom into rust
        yield return PlayDialogue(
            new DialogueLine[]
            {
                CreateMysteryDialogue("Clean the rust with a click!"),
                CreateMysteryDialogue("Don’t let it accumulate")
            }
        );

        // Wind up introduction
        yield return PlayDialogue(
            new DialogueLine[]
            {
                CreateYogSlothothDialogue("Wind it and keep pace, with the hands that seek to strike you down.")
            }
        );
        //_centerZoomCam.Priority = 100;
        yield return PlayDialogue(
            new DialogueLine[]
            {
                CreateMysteryDialogue("Keep the clock tightly wound."),
                CreateMysteryDialogue("Or become unwound yourself.")
            }
        );
        //_centerZoomCam.Priority = -100;

        // Clock hand introduction
        yield return PlayDialogue(
            new DialogueLine[]
            {
                CreateYogSlothothDialogue("And leap through time, or go mad trying…"),
                CreateMysteryDialogue("Jump over the minute and second hand."),
                CreateMysteryDialogue("Survive until the hour hand strikes 12.")
            }
        );
        yield return _hourHand.TutorialSpin();

        // Final remarks
        yield return PlayDialogue(
            new DialogueLine[]
            {
                CreateYogSlothothDialogue("And don’t forget… to go    i n s a n e"),
                CreateYogSlothothDialogue("I’d love to watch a mortal squirm…")
            }
        );

        yield return new WaitForSeconds(4f);

        _gameManager.SetPlayerActive(true);

        _gameManager.StartGame();
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
            speaker = Character.Dial,
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

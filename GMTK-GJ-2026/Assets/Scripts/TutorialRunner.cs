using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class TutorialRunner : MonoBehaviour
{
    [SerializeField]
    Narration.Canvas NarrationCanvas;
    [SerializeField]
    Narration.Script TutorialScript;
    [SerializeField]
    GameObject _yogiBear;

    [SerializeField]
    private Animator _playerAnimator;

    [SerializeField]
    private Clock.Hand _hourHand;

    [SerializeField]
    private Clock.Hand _minuteHand;

    [SerializeField]
    private Clock.Hand _secondHand;

    [SerializeField]
    private CinemachineCamera _zoomedOutCam;

    [SerializeField]
    private CinemachineCamera _centerZoomCam;

    [SerializeField]
    private CinemachineCamera _yogiBearCam;
    GameManager _gameManager => GameManager.Instance;
    void Awake()
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
        // Enable black canvas
        NarrationCanvas.SetActive(true);

        // Make sure game manager states are correct
        _gameManager.SetPlayerActive(false);

        yield return new WaitForSeconds(1f);

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
                CreateMysteryDialogue("A voice whispers to you, screaming in from all sides."),
                CreateMysteryDialogue("From every face, from every indicator…"),
                CreateMysteryDialogue("And somehow, from the cogs and tickers themselves."),
                CreateMysteryDialogue("It’s vibration, ancient, unknowable and crying out like a newborn babe."),
                CreateMysteryDialogue("Somehow… you know it,"),
                CreateMysteryDialogue("As the slithering…"),
                CreateMysteryDialogue("Velvety…"),
                CreateMysteryDialogue("Articulation of the Watcher…"),
                CreateMysteryDialogue("Yog-Sothoth.")
            }
        );

        // Spawn yogi in
        _yogiBear.SetActive(true);

        yield return new WaitForSeconds(2f);

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

        _zoomedOutCam.Priority = 100;
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
        _zoomedOutCam.Priority = -100;

        yield return new WaitForSeconds(3f);

        _centerZoomCam.Priority = 100;
        // Wind up introduction
        yield return PlayDialogue(
            new DialogueLine[]
            {
                CreateYogSlothothDialogue("Wind it and keep pace, with the hands that seek to strike you down.")
            }
        );
        yield return PlayDialogue(
            new DialogueLine[]
            {
                CreateMysteryDialogue("Keep the clock tightly wound."),
                CreateMysteryDialogue("Or become unwound yourself.")
            }
        );
        _centerZoomCam.Priority = -100;

        yield return new WaitForSeconds(3f);

        _zoomedOutCam.Priority = 100;
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
        yield return new WaitForSeconds(1f);
        _zoomedOutCam.Priority = -100;

        yield return new WaitForSeconds(3f);

        // Zoom in on yogi bear
        _yogiBearCam.Priority = 100;
        // Final remarks
        yield return PlayDialogue(
            new DialogueLine[]
            {
                CreateYogSlothothDialogue("And don’t forget… to go    i n s a n e"),
                CreateYogSlothothDialogue("I’d love to watch a mortal squirm…")
            }
        );
        _yogiBearCam.Priority = -100;
        _yogiBear.SetActive(false);

        yield return new WaitForSeconds(2f);

        Save.Manager.Instance.CompleteTutorial();
        GameEvents.RequestPlayerTeleport(GameManager.HubSpawnLocation);
        _gameManager.EndTutorial();
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
            speaker = Character.YogSlothoth,
            text = text
        };
    }


    DialogueLine CreateMysteryDialogue(string text)
    {
        return new DialogueLine
        {
            speaker = Character.Unknown,
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

using UnityEngine;
using System.Collections;

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

        // Enable NPC
        _tutorialNPC.SetActive(true);

        // Disable black canvas
        NarrationCanvas.SetActive(false);
        yield return new WaitForSeconds(3f);

        // Player animation to get up
        _playerAnimator.SetBool("IsAsleep", false);
        yield return WaitForAnimation("Standup");
        yield return new WaitForSeconds(2f);

        // NPC dialogue automatically triggered for introduction
        /* smthn smthn leslie real script here
         * "welcome to clock land bucko
         * im sure youre confused about whats happening
         * here ill teach ya"
         */

        // Yield Dialogue: "This is the hour hand. It has to make a full rotation
        yield return _hourHand.TutorialSpin();

        // Yield Dialogue: "This is the minute hand. It creates an impassable barrier"
        yield return _minuteHand.TutorialSpin();

        // Yield Dialogue: "This is the second hand. It can be hopped for your convenience"
        yield return _secondHand.TutorialSpin();

        _gameManager.SetPlayerActive(true);

        // "The clock will deteriorate over time. You must maintain it"
        // "You may interact with the environment to make repairs."

        // "Its urgent that you tend to the winding of the clock throughout the day.
        // enable wind up task

        // "Be weary of rust buildup."
        // spawn in a single rust spot
        // "It is important that you clean it up before it overruns the clock."
        // yield until user cleans up rust

        // "You may run into unfamiliar errors that even I don't know. I trust you to figure it out"

        // "Pay attention to the state of the clock. Bad things will happen if it does not get maintained.
        // "The clock is not nice to those who are new. Good luck.

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
}

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

        _gameManager.StopGame();
        _gameManager.SetPlayerActive(false);

        NarrationCanvas.SetActive(true);

        // Wait until the narration has completely finished.
        yield return NarrationCanvas.PlayScript(TutorialScript);

        // Enable a black canvas by default

        // Run through basic dialogue for entry

        // Fade blackness away

        // Player animation to get up

        NarrationCanvas.SetActive(false);
        _gameManager.SetPlayerActive(true);
    }
}

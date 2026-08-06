using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public string SceneToLoad;

    [SerializeField]
    GameObject _continueButton;

    private void Start()
    {
        _continueButton.SetActive(Services.Save.HasSave());
    }

    public void ContinueGame()
    {
        LoadScene();
    }

    public void NewGame()
    {
        Services.Game.NewGame();
        LoadScene();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void LoadScene()
    {
        SceneManager.LoadScene(SceneToLoad);
    }
}
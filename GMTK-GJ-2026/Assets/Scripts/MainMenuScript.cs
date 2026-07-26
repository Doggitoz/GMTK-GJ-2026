using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField]
    private string _mainMenuSceneName;

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(_mainMenuSceneName);
    }
}
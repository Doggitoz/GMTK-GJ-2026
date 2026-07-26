using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneHelper : MonoBehaviour
{
    public string SceneToLoad;
    public void LoadScene()
    {
        SceneManager.LoadScene(SceneToLoad);
    }
}

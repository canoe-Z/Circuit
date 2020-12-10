using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScript : MonoBehaviour
{
    public void OnStartGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void OnCloseGame()
    {
        Application.Quit();
    }
}

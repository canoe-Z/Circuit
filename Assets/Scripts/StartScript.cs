using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScript : MonoBehaviour
{
	public void OnStartGame(string sceneName)
	{
		Cursor.visible = false;
		SceneManager.LoadScene(sceneName);
	}

	public void OnCloseGame()
	{
		Application.Quit();
	}
}

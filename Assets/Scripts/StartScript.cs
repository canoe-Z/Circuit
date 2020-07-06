using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


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

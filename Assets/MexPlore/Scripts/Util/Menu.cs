using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void ButtonPlay()
    {
        SceneManager.LoadSceneAsync( 1, LoadSceneMode.Single );

		foreach ( var but in FindObjectsOfType<Button>() )
		{
            but.interactable = false;
		}
    }
}

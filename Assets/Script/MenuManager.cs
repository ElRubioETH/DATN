using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void Continue()
    {

    }
    public void StartNewGame()
    {
        SceneManager.LoadScene(1);
    }
    public void Setting()
    {

    }
    public void Exit()
    {
        Application.Quit();
    }
}

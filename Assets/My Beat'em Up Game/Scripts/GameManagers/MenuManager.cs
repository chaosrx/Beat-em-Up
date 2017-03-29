using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public void GameStart()
    {
        GamePlayManager.LoadLevel(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

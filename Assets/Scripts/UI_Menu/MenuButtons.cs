using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    [Header("Nome da cena de jogo")]
    public string gameSceneName = "Game";

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
using UnityEngine;

public class MenuButtons : MonoBehaviour
{
    [Header("Nome da cena de jogo")]
    public string gameSceneName = "Game";

    public void StartGame()
    {
        // Inicia o loading
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.StartLoading(gameSceneName);
        }
        else
        {
            Debug.LogError("LoadingScreenManager não encontrado!");
        }
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
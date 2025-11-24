using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Menu Scene Name")]
    public string menuSceneName = "MainMenu";

    [Header("Config")]
    public bool preventCreditsOnReturn = true; // impede tocar créditos ao voltar

    private bool isReturningToMenu = false;

    private void Awake()
    {
        // Singleton (persiste entre cenas)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        // Pressionar ESC para voltar ao menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMenu();
        }
    }

    public void ReturnToMenu()
    {
        if (isReturningToMenu) return;
        isReturningToMenu = true;

        StartCoroutine(ReturnToMenuRoutine());
    }

    private System.Collections.IEnumerator ReturnToMenuRoutine()
    {
        // Garante cursor ativo
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Carrega a cena do menu
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(menuSceneName);
        while (!loadOp.isDone)
            yield return null;

        // Espera um frame pra garantir que tudo da cena carregou
        yield return null;

        // Reativa o EventSystem se necessário
        var ev = EventSystem.current;
        if (ev == null)
        {
            Debug.LogWarning("[GameManager] Nenhum EventSystem encontrado — criando um novo.");
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
        else if (!ev.enabled)
            ev.enabled = true;

        // Reativa input no Canvas (caso tenha CanvasGroup principal)
        CanvasGroup[] groups = GameObject.FindObjectsOfType<CanvasGroup>(true);
        foreach (var cg in groups)
        {
            if (cg.CompareTag("MainMenuCanvas")) // usa uma tag pra identificar o canvas principal
            {
                cg.alpha = 1;
                cg.interactable = true;
                cg.blocksRaycasts = true;
                cg.gameObject.SetActive(true);
            }
        }

        // Impede que os créditos rodem novamente
        if (preventCreditsOnReturn)
        {
            var credits = GameObject.FindObjectOfType<CreditsManager>();
            if (credits != null)
                credits.enabled = false;
        }

        isReturningToMenu = false;
    }
}

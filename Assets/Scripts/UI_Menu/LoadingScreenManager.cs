using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance { get; private set; }

    [Header("Referências")]
    public GameObject loadingScreen;
    public Animator loadingAnimator; 
    public string slideInTrigger = "Open"; 
    public string slideOutTrigger = "Close"; 

    [Header("Configurações")]
    public float preLoadDelay = 1f;
    public float postLoadDelay = 2f;

    private bool isLoading = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("LoadingScreenManager persistido.");
        }
        else
        {
            Destroy(gameObject);
        }

        // Garante que o Canvas comece desativado
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }

    // Método para iniciar o loading (chamado do MenuButtons)
    public void StartLoading(string sceneName)
    {
        if (isLoading) return;
        isLoading = true;

        // Ativa a tela e toca slide in
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
            Debug.Log("[Loading] Canvas ativado.");
        }
        else
        {
            Debug.LogError("[Loading] Canvas não atribuído!");
        }

        loadingAnimator.SetTrigger(slideInTrigger);
        Debug.Log("[Loading] Slide in iniciado.");

        // Carrega a cena
        StartCoroutine(LoadSceneAsync(sceneName));
    }


    private System.Collections.IEnumerator LoadSceneAsync(string sceneName)
    {
        // Aguarda o delay antes de começar a carregar
        yield return new WaitForSeconds(preLoadDelay);
        Debug.Log("[Loading] Delay pré-carregamento concluído, iniciando carregamento.");

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName);
        loadOp.allowSceneActivation = false; // Não ativa a cena ainda

        // Aguarda o carregamento (progresso até 0.9)
        while (loadOp.progress < 0.9f)
        {
            yield return null;
        }

        // Ativa a cena
        loadOp.allowSceneActivation = true;

        // Aguarda a cena carregar completamente
        while (!loadOp.isDone)
        {
            yield return null;
        }

        // Aguarda o delay pós-carregamento
        yield return new WaitForSeconds(postLoadDelay);
        Debug.Log("[Loading] Delay pós-carregamento concluído.");

        // Toca slide out e desativa a tela
        loadingAnimator.SetTrigger(slideOutTrigger);
        Debug.Log("[Loading] Slide out iniciado.");

        // Aguarda o fim da animação (ajuste baseado na duração da animação de slide out)
        yield return new WaitForSeconds(0.5f); // Tempo estimado da animação

        loadingScreen.SetActive(false);
        isLoading = false;
        Debug.Log("[Loading] Tela desativada.");
    }
}

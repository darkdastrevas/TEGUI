using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    [Header("Roots")]
    public GameObject titleScreenRoot;
    public GameObject mainMenuRoot;

    [Header("Animator")]
    public Animator animator;
    public string exitTrigger = "Exit";

    [Header("Config")]
    public bool anyKeyToStart = true;
    public KeyCode startKey = KeyCode.Mouse0;

    private bool isExiting = false;

    // verificaar se ja ocorreu uma vez
    private static bool hasStarted = false;

    void Start()
    {
        if (mainMenuRoot != null)
            mainMenuRoot.SetActive(true);

        if (hasStarted)
        {
            if (titleScreenRoot != null)
                titleScreenRoot.SetActive(false);
            return;
        }

        hasStarted = true;

        if (titleScreenRoot != null)
            titleScreenRoot.SetActive(true);
    }

    void Update()
    {
        if (isExiting) return;
        if (titleScreenRoot == null || !titleScreenRoot.activeSelf) return;

        if (anyKeyToStart && Input.anyKeyDown)
            StartExit();
        else if (!anyKeyToStart && Input.GetKeyDown(startKey))
            StartExit();
    }

    void StartExit()
    {
        isExiting = true;

        if (animator != null)
            animator.SetTrigger(exitTrigger);
        else
            FinishExit(); // fallback caso não tenha animator
    }

    public void OnExitAnimationFinished()
    {
        FinishExit();
    }

    void FinishExit()
    {
        if (titleScreenRoot != null)
            titleScreenRoot.SetActive(false);

        if (mainMenuRoot != null)
            mainMenuRoot.SetActive(true);
    }
}

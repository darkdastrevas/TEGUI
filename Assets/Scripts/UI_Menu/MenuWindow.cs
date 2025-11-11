using UnityEngine;

public class MenuWindow : MonoBehaviour
{
    [Header("Objetos da janela")]
    public GameObject panel;
    public GameObject background;

    [Header("Animação")]
    public bool useAnimations = true;
    public Animator panelAnimator;
    public Animator backgroundAnimator;

    public string openTrigger = "Open";
    public string closeTrigger = "Close";
    public string fadeInTrigger = "FadeIn";
    public string fadeOutTrigger = "FadeOut";

    [Header("Configuração")]
    public float disableDelay = 0.6f;
    public KeyCode closeKey = KeyCode.Escape;

    [Header("Estrutura")]
    public MenuWindow parentMenu;
    public MenuWindow[] subMenus;

    [Header("Ocultar elementos do menu pai quando este submenu abrir")]
    public GameObject[] hideWhenSubmenuOpens;

    private bool isOpen = false;

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        panel.SetActive(true);
        background.SetActive(true);

        if (useAnimations)
        {
            if (panelAnimator) panelAnimator.SetTrigger(openTrigger);
            if (backgroundAnimator) backgroundAnimator.SetTrigger(fadeInTrigger);
        }

        // Se este menu tem pai, escondemos os elementos dele
        if (parentMenu != null)
        {
            foreach (var obj in parentMenu.hideWhenSubmenuOpens)
                if (obj) obj.SetActive(false);
        }
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        if (useAnimations)
        {
            if (panelAnimator) panelAnimator.SetTrigger(closeTrigger);
            if (backgroundAnimator) backgroundAnimator.SetTrigger(fadeOutTrigger);
            StartCoroutine(WaitToDisable());
        }
        else
        {
            panel.SetActive(false);
            background.SetActive(false);
        }

        // Quando o submenu fecha, reativa os objetos ocultos no menu pai
        if (parentMenu != null)
        {
            foreach (var obj in parentMenu.hideWhenSubmenuOpens)
                if (obj) obj.SetActive(true);
        }
    }

    private System.Collections.IEnumerator WaitToDisable()
    {
        yield return new WaitForSeconds(disableDelay);

        panel.SetActive(false);
        background.SetActive(false);
    }

    public void OpenSubmenu(int index)
    {
        if (index >= 0 && index < subMenus.Length)
            MenuManager.Instance.OpenMenu(subMenus[index]);
    }

    public void CloseToParent()
    {
        MenuManager.Instance.CloseCurrentMenu();
    }

    private void Update()
    {
        if (isOpen && Input.GetKeyDown(closeKey))
            CloseToParent();
    }
}

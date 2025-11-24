using UnityEngine;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    private Stack<MenuWindow> menuStack = new Stack<MenuWindow>();



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OpenMenu(MenuWindow menu)
    {
        // Agora não fecha o menu anterior
        menuStack.Push(menu);
        menu.Open();
    }

    public void CloseCurrentMenu()
    {
        if (menuStack.Count == 0)
            return;

        // Fecha o menu atual
        MenuWindow current = menuStack.Pop();
        current.Close();

        // O anterior continua ativo automaticamente
    }

    public void CloseAllMenus()
    {
        while (menuStack.Count > 0)
        {
            menuStack.Pop().Close();
        }
    }
}

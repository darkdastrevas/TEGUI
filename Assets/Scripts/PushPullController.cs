using Fusion;
using UnityEngine;

public class PushPullController : NetworkBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform cam;
    [SerializeField] private float interactDist = 8f; // distância máxima do Raycast
    [SerializeField] private float pushSpeed = 2.5f;  // velocidade reduzida no modo push

    private PushableObject current; // referência do objeto empurrável
    private bool pushing;           // se o jogador está no modo push
    private CharacterController cc; // componente de movimento do player

    void Start()
    {
        cc = GetComponent<CharacterController>();

        // Caso o campo 'cam' esteja vazio, tenta pegar a câmera principal
        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;
    }

    void Update()
    {
        // Garante que só o jogador local controla seu próprio personagem
        if (!Object.HasInputAuthority)
            return;

        // Pressiona E para tentar entrar ou sair do modo push
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (pushing)
                ExitPush();
            else
                TryPush();
        }

        // Se estiver empurrando, controla o movimento
        if (pushing && current != null)
            HandlePush();
    }

    void TryPush()
    {
        Debug.DrawRay(transform.position, transform.forward * interactDist, Color.red, 2f);
        Debug.Log("Tentando interagir...");

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactDist))
        {
            Debug.Log("Raycast atingiu: " + hit.collider.name);

            var obj = hit.collider.GetComponent<PushableObject>();
            if (obj != null)
            {
                Debug.Log("Encontrou objeto empurrável!");
                current = obj;
                pushing = true;
            }
            else
            {
                Debug.Log("O objeto atingido não tem PushableObject!");
            }
        }
        else
        {
            Debug.Log("Raycast não atingiu nada.");
        }
    }

    void ExitPush()
    {
        pushing = false;
        current = null;
        Debug.Log("Saiu do modo push.");
    }

    void HandlePush()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 dir = new Vector3(h, 0, v);
        dir = cam.TransformDirection(dir);
        dir.y = 0;

        if (dir.magnitude < 0.1f) return;

        cc.Move(dir * pushSpeed * Time.deltaTime);

        if (Object.HasStateAuthority)
        {
            current.Move(dir);
        }
        else
        {
            RPC_Move(dir);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_Move(Vector3 dir)
    {
        current?.Move(dir);
    }
}

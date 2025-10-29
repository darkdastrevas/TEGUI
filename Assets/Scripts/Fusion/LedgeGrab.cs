using UnityEngine;
using Fusion;

[RequireComponent(typeof(CharacterController))]
public class LedgeGrab : NetworkBehaviour
{
    [Header("Referências")]
    private CharacterController controller;
    //private Animator animator;

    [Header("Configurações")]
    public float climbUpHeight = 1.5f;    // Altura que ele sobe
    public float climbUpSpeed = 3f;       // Velocidade da subida
    public LayerMask ledgeLayer;          // Layer "Ledge"

    private bool isGrabbing;
    private Vector3 ledgePosition;

    public override void Spawned()
    {
        controller = GetComponent<CharacterController>();
        //animator = GetComponentInChildren<Animator>();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isGrabbing) return;

        // Se encostou em algo com tag "Ledge" ou layer correspondente
        if (hit.collider.CompareTag("Ledge") || ((1 << hit.gameObject.layer) & ledgeLayer) != 0)
        {
            // Armazena o ponto da borda
            ledgePosition = hit.point;
            StartGrab();
        }
    }

    private void StartGrab()
    {
        isGrabbing = true;

        // Trava movimento
        controller.enabled = false;

        // Posiciona o player um pouco abaixo da borda
        Vector3 grabPos = ledgePosition - transform.forward * 0.2f;
        grabPos.y -= 0.5f;
        transform.position = grabPos;

       // if (animator) animator.SetBool("isGrabbing", true);
    }

    private void Update()
    {
        if (!Object.HasInputAuthority) return;

        if (isGrabbing)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                Runner.StartCoroutine(ClimbUp());
        }
    }

    private System.Collections.IEnumerator ClimbUp()
    {
        //if (animator) animator.SetTrigger("ClimbUp");

        float t = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * climbUpHeight + transform.forward * 0.5f;

        while (t < 1f)
        {
            t += Time.deltaTime * climbUpSpeed;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // Libera movimento
        controller.enabled = true;
        isGrabbing = false;

        //if (animator)
        //{
        //    animator.SetBool("isGrabbing", false);
        //    animator.ResetTrigger("ClimbUp");
        //}
    }
}

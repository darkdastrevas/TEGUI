using UnityEngine;
using Fusion;

namespace SmallHedge.SoundManager
{
    public class PlaySoundEnter : StateMachineBehaviour
    {
        [SerializeField] private SoundType sound;
        [SerializeField, Range(0, 1)] private float volume = 1;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var networkObject = animator.GetComponentInParent<NetworkObject>();

            // Só quem tem autoridade local toca o áudio
            if (networkObject && networkObject.HasInputAuthority)
            {
                SoundManager.PlaySound(sound, null, volume);
            }
        }
    }
}

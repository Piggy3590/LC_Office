using GameNetcodeStuff;
using System.Collections;
using UnityEngine;

namespace LCOffice.Components
{
    public class MetalDetector : MonoBehaviour
    {
        public bool powered = true;
        private bool shocking = false;

        public ParticleSystem particles;
        public AudioSource source;
        public AudioClip zapSound;
        public Light redLight;

        private Coroutine shockSequence;

        public void OnTriggerStay(Collider other)
        {
            if (!powered) return;

            if (other.CompareTag("Player") && other.gameObject.TryGetComponent(out PlayerControllerB player) && !player.isPlayerDead)
            {
                if (shocking)
                {
                    player.KillPlayer(Vector3.up, true, CauseOfDeath.Electrocution, 3);
                }
                else
                {
                    foreach (GrabbableObject item in player.ItemSlots)
                    {
                        if (item == null) continue;
                        if (item.itemProperties == null) continue;
                        if (item.itemProperties.isConductiveMetal)
                        {
                            shockSequence = StartCoroutine(ShockSequence());
                            break;
                        }
                    }
                }
            }
        }

        public void PowerDown()
        {
            powered = false;
            StopShocking();
        }

        private IEnumerator ShockSequence()
        {
            redLight.enabled = shocking = true;

            particles.Play();
            source.PlayOneShot(zapSound);

            yield return new WaitForSeconds(1f);

            StopShocking();
        }

        private void StopShocking()
        {
            if (shockSequence != null)
            {
                StopCoroutine(shockSequence);
                shockSequence = null;
            }

            redLight.enabled = shocking = false;
        }
    }
}

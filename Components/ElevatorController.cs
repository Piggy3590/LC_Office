using DunGen;
using JLL.Components;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LCOffice.Components
{
    public class ElevatorController : MonoBehaviour, IDungeonLoadListener
    {
        public Animator animator;
        public Animator doorAnim;

        public AudioSource doorSource;
        public AudioClip openClip;
        public AudioClip closeClip;

        public AudioSource source;
        public AudioClip upClip;
        public AudioClip upShortClip;
        public AudioClip downClip;
        public AudioClip downShortClip;

        public Animator screenDoorAnim;

        public List<Light> lights;
        public Transform storagePos;
        [HideInInspector] public Vector3 startPos;

        public ElevatorMusic music;

        [Header("Notice Board")]
        public Doorway[] topSockets;
        public GameObject topBlocker;
        public Doorway[] midSockets;
        public GameObject midBlocker;
        public Doorway[] bottomSockets;
        public GameObject bottomBlocker;

        public GameObject[] floorIndicators;

        private void Start()
        {
            startPos = transform.position;
            doorAnim.SetFloat("speed", 1);
            OpenDoor(1);
        }

        public void PostDungeonGeneration()
        {
            ElevatorSystem.Spawn(transform);

            topBlocker.SetActive(!DoorwayConnected(topSockets));
            midBlocker.SetActive(!DoorwayConnected(midSockets));
            bottomBlocker.SetActive(!DoorwayConnected(bottomSockets));
        }

        private bool DoorwayConnected(Doorway[] sockets)
        {
            foreach (Doorway socket in sockets)
            {
                if (socket.ConnectedDoorway != null)
                {
                    return true;
                }
            }
            return false;
        }

        public void OpenDoor(int floor)
        {
            doorSource.PlayOneShot(openClip);
            doorAnim.SetBool("closed", true);

            if (floor == 1)
            {
                screenDoorAnim.SetBool("open", true);
            }
        }

        public void CloseDoor()
        {
            doorSource.PlayOneShot(closeClip);
            doorAnim.SetBool("closed", false);
            screenDoorAnim.SetBool("open", false);
        }

        public void ToggleLights(bool on)
        {
            foreach (Light light in lights)
            {
                light.enabled = on;
            }
        }
    }
}

using UnityEngine;

namespace LCOffice.Components
{
    public class CameraCulling : MonoBehaviour
    {
        private Transform player;
        private GameObject[] playerObjects;

        public Camera camera;
        public Transform cullPos;

        public float followDistance = -1f;
        public float renderDistance = 10f;

        private float fps = 15;
        private bool disabled;
        private float elapsed;

        void Start()
        {
            player = StartOfRound.Instance.localPlayerController.transform;
            playerObjects = StartOfRound.Instance.allPlayerObjects;
            camera = GetComponent<Camera>();
            fps = Plugin.cameraFrameSpeed.Value;
            disabled = Plugin.cameraDisable.Value || fps == 0;
            camera.enabled = false;
            camera.Render();
            elapsed = Random.Range(0f, 1f);
        }

        void Update()
        {
            if (disabled || player == null) return;

            float distance = Vector3.Distance(cullPos.position, player.position);
            if (distance <= renderDistance)
            {
                if (fps > 0)
                {
                    elapsed += Time.deltaTime;
                    if (elapsed > 1f / fps)
                    {
                        elapsed = 0f;
                        camera.Render();
                    }
                }
                else
                {
                    camera.enabled = true;
                }
            }
            else camera.enabled = false;
        }

        void LateUpdate()
        {
            if (disabled || followDistance <= 0) return;

            Transform lookAtTarget = null;
            float targetDistance = Mathf.Infinity;

            foreach (GameObject player in playerObjects)
            {
                float playerDistance = Vector3.Distance(transform.position, player.transform.position);
                if (playerDistance < followDistance && playerDistance < targetDistance)
                {
                    lookAtTarget = player.transform;
                    targetDistance = playerDistance;
                }
            }
            if (lookAtTarget != null)
            {
                Vector3 lookDirection = lookAtTarget.position - transform.position;
                lookDirection.Normalize();

                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), 5 * Time.deltaTime);
            }
        }
    }
}

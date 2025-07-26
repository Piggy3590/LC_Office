using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace LCOffice.Components
{
    public class ElevatorPannel : ElevatorBehavior
    {
        public Animator animator;
        public Animator lightAnimator;
        public InteractTrigger UpButton;
        public InteractTrigger MidButton;
        public InteractTrigger DownButton;
        [FormerlySerializedAs("panelLabel")]
        public TextMeshProUGUI display;

        public void SetInteractable(bool value)
            => UpButton.interactable = MidButton.interactable = DownButton.interactable = value;

        private static void SubscribeListener(InteractTrigger trigger, int floor)
            => trigger.onInteract.AddListener((player) => ElevatorSystem.System.ElevatorTriggerServerRpc(floor));

        public override void Setup()
        {
            ElevatorSystem.System.controlPannels.Add(this);
            SubscribeListener(UpButton, 2);
            SubscribeListener(MidButton, 1);
            SubscribeListener(DownButton, 0);
            display.font = ElevatorSystem.System.textFont;
            display.material = ElevatorSystem.System.textMaterial;
            display.color = ElevatorSystem.System.textColor;
            display.text = "Idle";
        }
    }

    public class ElevatorStorage : ElevatorBehavior
    {
        public Transform target;
        public override void Setup()
        {
            target = ElevatorSystem.System.controller.storagePos;
            transform.rotation = target.rotation;
        }

        public void LateUpdate()
        {
            if (InSystem) transform.position = target.position;
        }
    }

    public abstract class ElevatorBehavior : MonoBehaviour
    {
        public bool InSystem { get; private set; } = false;

        public virtual void Start()
        {
            StartCoroutine(AddToSystem());
        }

        public abstract void Setup();

        IEnumerator AddToSystem()
        {
            yield return new WaitUntil(() => ElevatorSystem.System != null);
            Setup();
            InSystem = true;
            yield break;
        }
    }
}

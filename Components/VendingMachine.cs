using GameNetcodeStuff;
using JLL.API;
using JLL.API.Compatability;
using JLL.Components;
using Unity.Netcode;
using UnityEngine;

namespace LCOffice.Components
{
    public class VendingMachine : NetworkBehaviour
    {
        [System.Serializable]
        public class DrinkOption
        {
            public Item item;
            public InteractTrigger trigger;
            public Animator anim;
        }

        public DrinkOption[] Drinks = [];
        private int previousDrink = 0;
        public string currencyTag = "Office Coin";

        public AudioSource source;
        public AudioClip[] pressClip;

        public Transform spawnPos;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            for (int i = 0; i < Drinks.Length; i++)
            {
                int value = i;
                Drinks[i].trigger.onInteract.AddListener((player) => SelectDrinkServerRpc(player.Index(), value));
            }
            if (IsOwner)
            {
                SelectDrinkClientRpc(Random.Range(0, Drinks.Length));
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SelectDrinkServerRpc(int playerId, int index)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerId];

            if (player.currentlyHeldObjectServer != null && player.currentlyHeldObjectServer.itemProperties != null && LLLHelper.ItemHasTag(player.currentlyHeldObjectServer.itemProperties, currencyTag))
            {
                Plugin.mls.LogInfo($"Spawning Drink: {index} / {Drinks.Length} for {player.playerUsername}");
                Item item = Drinks[index].item;
                ItemSpawner.SpawnItem(item, spawnPos.position, RoundManager.Instance.spawnedScrapContainer, spawnPos.rotation, Random.Range(item.minValue, item.maxValue), rotation: RotationType.ObjectRotation);
                SelectDrinkClientRpc(index, playerId);
            }
            else SelectDrinkClientRpc(index, -1);
        }

        [ClientRpc]
        private void SelectDrinkClientRpc(int index, int playerId = -1)
        {
            Drinks[previousDrink].anim.ResetTrigger("Button");
            DrinkOption drink = Drinks[index];
            drink.anim.SetTrigger("Button");

            source.PlayOneShot(pressClip[Random.Range(0, pressClip.Length)]);
            previousDrink = index;

            if (playerId != -1)
            {
                PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerId];
                player.DestroyItemInSlotAndSync(player.currentItemSlot);
            }
        }
    }
}

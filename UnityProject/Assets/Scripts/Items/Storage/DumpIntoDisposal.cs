using System.Collections;
using Objects.Disposals;
using UnityEngine;

/// <summary>
/// This script is used to allow bags to empty all of their contents into a disposal with just one click
/// Should work with any item that has itemStorage, do not use this script on an object that doesn't
/// </summary>
[RequireComponent(typeof(ItemStorage))]
public class DumpIntoDisposal : MonoBehaviour, ICheckedInteractable<PositionalHandApply>
{
	private bool allowedToInteract = false;

	private ItemStorage ItemStorage => itemStorage;

	private ItemStorage itemStorage;

	// Prevents Auto Clickers
	// (Code Taken from InteractableStorage.cs)
	private void OnEnable()
	{
		allowedToInteract = false;
		itemStorage = GetComponent<ItemStorage>();
		StartCoroutine(SpawnCoolDown());
	}

	IEnumerator SpawnCoolDown()
	{
		yield return WaitFor.Seconds(0.5f);
		allowedToInteract = true;
	}

	public bool WillInteract(PositionalHandApply interaction, NetworkSide side)
	{
		// Spam Protection
		if (!allowedToInteract) return false;

		// Use default interaction checks
		if (!DefaultWillInteract.Default(interaction, side)) return false;

		if (interaction.TargetObject == gameObject) return false; // If we are targeting out self
		if (interaction.HandObject == null) return false; // If the bag is not in our hand
		if (interaction.TargetObject == null) return false; // If we are not targeting something, return false

		if (interaction.TargetObject.GetComponent<DisposalBin>())
		{
			Chat.AddLocalMsgToChat("We passed the client side check", gameObject);
			return true;
		}

		return false;
	}

	public void ServerPerformInteraction(PositionalHandApply interaction)
	{
		var disposalBin = interaction.TargetObject.GetComponent<DisposalBin>();

		if (disposalBin != null)
		{
			Chat.AddLocalMsgToChat("We passed the server side check, the bag has this many slots: " + itemStorage.ItemStorageCapacity, gameObject);
			disposalBin.BagDump(ItemStorage);
			var storage = interaction.HandObject.GetComponent<InteractableStorage>();
			storage.ItemStorage.ServerDropAll();
		}
	}
}
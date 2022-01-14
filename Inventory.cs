using System;
using FeelFreeGames.Evaluation.Extensions;
using FeelFreeGames.Evaluation.Utils;
using UnityEngine;

namespace FeelFreeGames.Evaluation.UI
{
	public class Inventory : IInventory, IInventoryEvents
	{
		#region Events
		event Action<IItem> IInventoryEvents.ItemSelected
		{
			add => ItemSelected += value;
			remove => ItemSelected -= value;
		}

		event Action IInventoryEvents.ItemPickedUp
		{
			add => ItemPickedUp += value;
			remove => ItemPickedUp -= value;
		}

		event Action IInventoryEvents.ItemDeleted
		{
			add => ItemDeleted += value;
			remove => ItemDeleted -= value;
		}

		event Action IInventoryEvents.NewItemsDrawn
		{
			add => NewItemsDrawn += value;
			remove => NewItemsDrawn -= value;
		}

		event Action IInventoryEvents.ItemDropped
		{
			add => ItemDropped += value;
			remove => ItemDropped -= value;
		}

		event Action IInventoryEvents.ItemSwapped
		{
			add => ItemSwapped += value;
			remove => ItemSwapped -= value;
		}

		event Action IInventoryEvents.ItemPickUpCancelled
		{
			add => ItemPickUpCancelled += value;
			remove => ItemPickUpCancelled -= value;
		}
		
		event Action IInventoryEvents.SelectionMoved
		{
			add => SelectionMoved += value;
			remove => SelectionMoved -= value;
		}
		
		private event Action<IItem> ItemSelected;
		private event Action ItemPickedUp;
		private event Action ItemDeleted;
		private event Action NewItemsDrawn;
		private event Action ItemDropped;
		private event Action ItemSwapped;
		private event Action ItemPickUpCancelled;
		private event Action SelectionMoved;
		#endregion Events
		
		private IInventorySlot SelectedSlot => _slots[_selectedSlotIndex];
		
		private int _selectedSlotIndex;
		
		private IItem _pickedUpItem;
		private IInventorySlot _originSlot;

		private readonly IInventorySlot[] _slots;
		private readonly IItem[] _availableItems;
		private readonly Vector2Int _size;
		private readonly int _slotCount;
		private readonly int _itemDrawCount;

		public Inventory(Vector2Int size, IItem[] availableItems, int itemDrawCount)
		{
			_slotCount = size.x * size.y;
			_slots = new IInventorySlot[_slotCount];
			_availableItems = availableItems;
			_size = size;
			_itemDrawCount = itemDrawCount;
		}

		public IInventorySlotEvents[] CreateSlots()
		{
			var slotEvents = new IInventorySlotEvents[_slotCount];

			for (var i = 0; i < _slotCount; i++)
			{
				var slot = new InventorySlot();
				_slots[i] = slot;
				slotEvents[i] = slot;
			}
			
			return slotEvents;
		}
		
		public void Initialize()
		{
			DrawNewItems(_itemDrawCount);
			SelectSlot(0);
		}

		void IInventory.MoveSelection(Vector2Int direction)
		{
			var desiredCoord = MathUtils.IndexToCoords(_selectedSlotIndex, _size.x) + direction;
			var clampedCoord = MathUtils.ClampCoords(desiredCoord, _size);
			var desiredIndex = MathUtils.CoordsToIndex(clampedCoord, _size.x);
			var fits = desiredIndex != _selectedSlotIndex;

			if (!fits)
			{
				return;
			}

			SelectSlot(desiredIndex);
			SelectionMoved?.Invoke();
		}

		void IInventory.PickUpItem()
		{
			if (SelectedSlot.CurrentItem == null)
			{
				Debug.Log("Trying to pick up an item, but the selected slot is empty!");
				return;
			}
			
			_originSlot = SelectedSlot;
			_pickedUpItem = _originSlot.CurrentItem;

			_originSlot.PickUpItem();

			ItemSelected?.Invoke(SelectedSlot.CurrentItem);
			ItemPickedUp?.Invoke();
		}

		void IInventory.DropItem()
		{
			if (_pickedUpItem == null)
			{
				Debug.LogError("Trying to drop an item, but no item has been picked up!");
				return;
			}
			
			if (SelectedSlot.CurrentItem != null)
			{
				var originalItem = SelectedSlot.CurrentItem;
				
				SelectedSlot.DropInItem(_pickedUpItem);
				_originSlot.SetItem(originalItem);
				
				ResetOriginSlot();
				ItemSwapped?.Invoke();
			}
			else
			{
				SelectedSlot.DropInItem(_pickedUpItem);
				
				ResetOriginSlot();
				ItemDropped?.Invoke();
			}
			
			ItemSelected?.Invoke(SelectedSlot.CurrentItem);
		}

		void IInventory.CancelPickUp()
		{
			if (_pickedUpItem == null)
			{
				Debug.LogError("Trying to cancel picking up an item, but no item has been picked up!");
				return;
			}
			
			_originSlot.CancelPickUpItem(_pickedUpItem);
			ResetOriginSlot();
			
			ItemPickUpCancelled?.Invoke();
		}

		void IInventory.DeleteItem()
		{
			_originSlot.DeleteItem();
			ResetOriginSlot();
			
			ItemDeleted?.Invoke();
		}
		
		void IInventory.DrawNewItems()
		{
			DrawNewItems(_itemDrawCount);
			NewItemsDrawn?.Invoke();
		}

		private void DrawNewItems(int itemCount)
		{
			foreach (var inventorySlot in _slots)
			{
				inventorySlot.ClearSlot();
			}

			var randomSlots = _slots.SelectionSample(itemCount);
			var randomItems = _availableItems.SelectionSample(itemCount);

			for (var i = 0; i < itemCount; i++)
			{
				randomSlots[i].SetItem(randomItems[i]);
			}
			
			ItemSelected?.Invoke(SelectedSlot.CurrentItem);
		}

		private void SelectSlot(int index)
		{
			_selectedSlotIndex = index;
			SelectedSlot.Select();
			
			ItemSelected?.Invoke(SelectedSlot.CurrentItem);
		}

		private void ResetOriginSlot()
		{
			_originSlot = null;
			_pickedUpItem = null;
		}
	}
}
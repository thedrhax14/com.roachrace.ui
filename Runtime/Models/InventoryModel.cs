using System;
using RoachRace.Data;
using RoachRace.UI.Core;
using UnityEngine;

namespace RoachRace.UI.Models
{
    /// <summary>
    /// ScriptableObject model backing inventory UI.
    /// 
    /// Setup:
    /// - Create the asset via Create > RoachRace > Models > InventoryModel.
    /// - Assign this asset to one or more InventoryView components.
    /// - NetworkPlayerInventory (on the local player) is expected to push data into this model.
    /// 
    /// Notes:
    /// - Slots is an array snapshot of the current inventory (not a SyncList reference).
    /// - SelectedSlotIndex is the currently selected slot index.
    /// - IsVisible can be used to hide/show an inventory layout without disabling GameObjects.
    /// </summary>
    [CreateAssetMenu(fileName = "InventoryModel", menuName = "RoachRace/Models/InventoryModel")]
    public sealed class InventoryModel : UIModel
    {
        public readonly Observable<InventorySlotState[]> Slots =
            new Observable<InventorySlotState[]>(Array.Empty<InventorySlotState>());
        public readonly Observable<int> SelectedSlotIndex = new Observable<int>(0);
        public readonly Observable<bool> IsVisible = new Observable<bool>(true);

        public void SetVisible(bool visible) => IsVisible.Value = visible;

        public void SetInventory(InventorySlotState[] slots, int selectedSlotIndex)
        {
            Slots.Value = slots ?? Array.Empty<InventorySlotState>();
            SelectedSlotIndex.Value = selectedSlotIndex;
        }

        public void NotifyInventoryChanged()
        {
            Slots.Notify(Slots.Value);
            SelectedSlotIndex.Notify(SelectedSlotIndex.Value);
        }
    }
}

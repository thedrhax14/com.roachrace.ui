using RoachRace.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoachRace.UI.Components.Inventory
{
    /// <summary>
    /// Visual widget for a single inventory slot.
    /// 
    /// Setup:
    /// - Assign iconImage (Image) to show item icon.
    /// - Assign countText (TMP_Text) to show stack count (hidden for 0/1).
    /// - Assign selectedIndicator (any GameObject) to show selection highlight.
    /// - Set slotIndex to map this widget to a specific inventory slot.
    /// </summary>
    public sealed class InventorySlotWidget : MonoBehaviour
    {
        [Header("UI")]
        [Tooltip("Image used to display the item icon.")]
        [SerializeField] private Image iconImage;
        [Tooltip("Text used to display stack count. Hidden when count <= 1.")]
        [SerializeField] private TMP_Text countText;
        [Tooltip("Shown/hidden to indicate selected slot.")]
        [SerializeField] private GameObject selectedIndicator;

        [Header("Slot")]
        [Tooltip("Which inventory slot index this widget represents.")]
        [SerializeField, Range(0, 9)] private int slotIndex;

        /// <summary>
        /// Gets the inventory slot index rendered by this widget.<br>
        /// Typical usage: pair this with a manually positioned inventory slot layout or pie menu slot.<br>
        /// </summary>
        public int SlotIndex => slotIndex;

        /// <summary>
        /// Updates the slot index used by this widget.<br>
        /// Typical usage: call this from editor-time layout code when widgets are auto-assigned to inventory slots.<br>
        /// </summary>
        /// <param name="index">Zero-based inventory slot index.</param>
        public void SetSlotIndex(int index)
        {
            slotIndex = index;
        }

        /// <summary>
        /// Renders the visual state for this inventory slot.<br>
        /// Typical usage: call after resolving the slot snapshot and icon for the current inventory model.<br>
        /// </summary>
        /// <param name="slot">Snapshot of the inventory slot state.</param>
        /// <param name="icon">Resolved icon for the slot, or null when empty/unresolved.</param>
        /// <param name="selected">Whether this slot should display its selected indicator.</param>
        public void Render(InventorySlotState slot, Sprite icon, bool selected)
        {
            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = icon != null;
            }
            else
            {
                Debug.LogWarning($"[{nameof(InventorySlotWidget)}] Slot {SlotIndex}: iconImage is not assigned.", this);
            }

            if (countText != null)
            {
                if (slot.Count <= 1)
                {
                    countText.text = string.Empty;
                }
                else
                {
                    countText.text = slot.Count.ToString();
                }
            }

            if (selectedIndicator != null) selectedIndicator.SetActive(selected);
            else Debug.LogWarning($"[{nameof(InventorySlotWidget)}] Slot {SlotIndex}: selectedIndicator is not assigned.", this);
        }
    }
}

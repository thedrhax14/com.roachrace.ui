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
        [SerializeField, Range(0, 8)] private int slotIndex;

        public int SlotIndex => slotIndex;

        public void SetSlotIndex(int index)
        {
            slotIndex = index;
        }

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

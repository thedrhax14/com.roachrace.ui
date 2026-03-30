using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RoachRace.UI.Components.Inventory.PieMenu
{
    /// <summary>
    /// Bridges UI hover events to an InventoryPieMenuView for a specific slot index.<br>
    /// Typical usage: add this component to a UI element that should be hoverable (e.g., the slot widget root or an
    /// invisible Image with raycast target enabled), set slotIndex in the inspector, and ensure the element is under
    /// the same hierarchy as an InventoryPieMenuView.<br>
    /// Configuration notes: this uses Unity's EventSystem pointer callbacks; ensure a GraphicRaycaster exists on the
    /// canvas and an EventSystem exists in the scene.
    /// </summary>
    public sealed class InventoryPieMenuSlotHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Tooltip("Slot index that this hover target represents (0..9).")]
        [SerializeField, Range(0, 9)] private int slotIndex;

        [Tooltip("Optional explicit pie menu view reference. If null, the handler searches parent hierarchy on Awake.")]
        [SerializeField] private InventoryPieMenuView pieMenuView;

        private bool _hovering;

        /// <summary>
        /// Validates dependencies and resolves the InventoryPieMenuView reference.<br>
        /// Typical usage: Unity calls this automatically.
        /// </summary>
        private void Awake()
        {
            if (pieMenuView == null)
            {
                pieMenuView = GetComponentInParent<InventoryPieMenuView>();
            }

            if (pieMenuView == null)
            {
                Debug.LogError($"[{nameof(InventoryPieMenuSlotHoverHandler)}] Missing {nameof(InventoryPieMenuView)} on '{gameObject.name}'. This handler must be under a pie menu view hierarchy.", gameObject);
                throw new InvalidOperationException($"[{nameof(InventoryPieMenuSlotHoverHandler)}] Missing {nameof(InventoryPieMenuView)} on '{gameObject.name}'.");
            }
        }

        /// <summary>
        /// Pointer enter callback used to mark hover and request selection through the view.<br>
        /// Typical usage: driven by Unity UI raycasts.
        /// </summary>
        /// <param name="eventData">Event data provided by the EventSystem.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_hovering)
            {
                return;
            }

            _hovering = true;
            pieMenuView.SetHoveredSlotIndex(slotIndex);
            pieMenuView.TrySelectSlot(slotIndex);
        }

        /// <summary>
        /// Pointer exit callback used to clear hover.
        /// </summary>
        /// <param name="eventData">Event data provided by the EventSystem.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_hovering)
            {
                return;
            }

            _hovering = false;
            pieMenuView.ClearHoveredSlotIndex(slotIndex);
        }
    }
}

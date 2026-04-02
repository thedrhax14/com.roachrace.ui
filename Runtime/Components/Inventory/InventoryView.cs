using RoachRace.Data;
using RoachRace.Interaction;
using RoachRace.UI.Core;
using RoachRace.UI.Models;
using UnityEngine;

namespace RoachRace.UI.Components.Inventory
{
    /// <summary>
    /// Generic inventory renderer.
    /// 
    /// Layout is scene-driven: this view does not position widgets. It simply renders into whatever
    /// arrangement you build (horizontal bar, pie menu, left/right columns, etc).
    /// 
    /// Scene/prefab setup:
    /// - Assign inventoryModel (ScriptableObject).
    /// - Assign itemDatabase (ScriptableObject) so item ids can be resolved to icons.
    /// - Assign slots with one InventorySlotWidget per visible slot, and set slotIndex on each widget.
    /// - Assign a CanvasGroup if you want show/hide without disabling the GameObject (recommended).
    /// </summary>
    public sealed class InventoryView : MonoBehaviour
    {
        [Header("Data")]
        [Tooltip("InventoryModel asset to observe for slot/selection/visibility updates.")]
        [SerializeField] private InventoryModel inventoryModel;

        [Tooltip("ItemDatabase asset used to resolve item ids into UI icons.")]
        [SerializeField] private ItemDatabase itemDatabase;

        [Header("UI")]
        [Tooltip("Optional. If assigned, visibility is controlled via CanvasGroup alpha/interactable. If not assigned, GameObject active state is used.")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Tooltip("Slot widgets to render into. These can be arranged in any layout; slotIndex determines which inventory slot each widget represents.")]
        [SerializeField] private InventorySlotWidget[] slots;

        private IObserver<InventorySlotState[]> _slotsObserver;
        private IObserver<int> _selectedObserver;
        private IObserver<bool> _visibleObserver;

        private void Awake()
        {
            if (inventoryModel == null)
            {
                Debug.LogError($"[{nameof(InventoryView)}] inventoryModel is not assigned on '{gameObject.name}'. This view cannot render without an InventoryModel.", gameObject);
                throw new System.NullReferenceException($"[{nameof(InventoryView)}] inventoryModel is null on '{gameObject.name}'.");
            }

            if (itemDatabase == null)
            {
                Debug.LogWarning($"[{nameof(InventoryView)}] itemDatabase is not assigned on '{gameObject.name}'. Inventory icons cannot be resolved.", gameObject);
            }

            _slotsObserver = new ActionObserver<InventorySlotState[]>(RenderSlots);
            _selectedObserver = new ActionObserver<int>(_ => RenderSlots(inventoryModel != null ? inventoryModel.Slots.Value : null));
            _visibleObserver = new ActionObserver<bool>(SetVisible);
        }

        private void OnEnable()
        {
            Debug.Log("InventoryView: OnEnable subscribing to model.", this);
            if (inventoryModel == null) return;

            inventoryModel.Slots.Attach(_slotsObserver);
            inventoryModel.SelectedSlotIndex.Attach(_selectedObserver);
            inventoryModel.IsVisible.Attach(_visibleObserver);

            SetVisible(inventoryModel.IsVisible.Value);
            RenderSlots(inventoryModel.Slots.Value);
        }

        private void OnDisable()
        {
            if (inventoryModel == null) return;

            inventoryModel.Slots.Detach(_slotsObserver);
            inventoryModel.SelectedSlotIndex.Detach(_selectedObserver);
            inventoryModel.IsVisible.Detach(_visibleObserver);
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup == null)
            {
                gameObject.SetActive(visible);
                return;
            }

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        private void RenderSlots(InventorySlotState[] slotStates)
        {
            if (inventoryModel == null) return;
            if (slots == null) return;

            slotStates ??= System.Array.Empty<InventorySlotState>();
            int selected = inventoryModel.SelectedSlotIndex.Value;

            for (int i = 0; i < slots.Length; i++)
            {
                var widget = slots[i];
                if (widget == null) continue;

                int slotIndex = widget.SlotIndex;
                InventorySlotState state = (slotIndex >= 0 && slotIndex < slotStates.Length) ? slotStates[slotIndex] : default;

                Sprite icon = null;
                if (state.ItemId == 0)
                {
                    // Allow a UI placeholder definition for empty slots (ItemDefinition id 0).
                    if (itemDatabase == null)
                    {
                        Debug.LogWarning($"[{nameof(InventoryView)}] Slot {slotIndex}: slot is empty and itemDatabase is not assigned (cannot resolve placeholder itemId 0).", this);
                    }
                    else
                    {
                        bool foundEmpty = itemDatabase.TryGet(0, out var emptyDef);
                        if (!foundEmpty)
                        {
                            Debug.LogWarning($"[{nameof(InventoryView)}] Slot {slotIndex}: slot is empty and ItemDatabase has no ItemDefinition for itemId 0 (optional placeholder).", this);
                        }
                        else if (emptyDef == null)
                        {
                            Debug.LogWarning($"[{nameof(InventoryView)}] Slot {slotIndex}: slot is empty and ItemDatabase returned a null ItemDefinition for itemId 0.", this);
                        }
                        else
                        {
                            icon = emptyDef.icon;

                            if (icon == null)
                            {
                                Debug.LogWarning($"[{nameof(InventoryView)}] Slot {slotIndex}: empty-slot ItemDefinition '{emptyDef.name}' (id 0) has no icon assigned.", this);
                            }
                        }
                    }
                }
                else if (itemDatabase == null)
                {
                    Debug.LogWarning($"[{nameof(InventoryView)}] Slot {slotIndex} {state}: itemDatabase is not assigned, cannot resolve icon for itemId {state.ItemId}.", this);
                }
                else
                {
                    bool found = itemDatabase.TryGet(state.ItemId, out var def);
                    if (!found)
                    {
                        Debug.LogWarning($"[{nameof(InventoryView)}] Slot {slotIndex} {state}: ItemDatabase has no ItemDefinition for itemId {state.ItemId}.", this);
                    }
                    else if (def == null)
                    {
                        Debug.LogWarning($"[{nameof(InventoryView)}] Slot {slotIndex} {state}: ItemDatabase returned a null ItemDefinition for itemId {state.ItemId} (missing asset reference in database list?).", this);
                    }
                    else
                    {
                        // Choose icon by role, with survivor fallback.
                        icon = def.icon;

                        if (icon == null)
                        {
                            Debug.LogWarning($"[{nameof(InventoryView)}] Slot {slotIndex} {state}: ItemDefinition '{def.name}' (id {def.id}) has no icon assigned (ghostIcon/survivorIcon are null).", this);
                        }
                    }
                }

                widget.Render(state, icon, selected == slotIndex);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (slots == null) return;

            // If user forgets to set indices, auto-assign sequentially.
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null)
                    slots[i].SetSlotIndex(i);
            }
        }
#endif
    }
}

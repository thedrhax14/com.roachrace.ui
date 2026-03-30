using System;
using RoachRace.Controls;
using RoachRace.Data;
using RoachRace.Interaction;
using RoachRace.UI.Core;
using RoachRace.UI.Models;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoachRace.UI.Components.Inventory.PieMenu
{
    /// <summary>
    /// Renders a ten-slot inventory pie menu UI from an observed InventoryModel.<br>
    /// Typical usage: place this on the pie menu root RectTransform, assign ten manually positioned InventorySlotWidget
    /// instances, connect the shared InventoryModel, and wire a hold-to-open action so the menu can be opened/closed.<br>
    /// Configuration notes: slot 0 is the center slot and slots 1-9 are outer slots. This component intentionally does
    /// not implement directional slot selection; selection/hover behavior should be implemented elsewhere.
    /// </summary>
    public sealed class InventoryPieMenuView : MonoBehaviour
    {
        private const int SlotCount = 10;

        [Header("Data")]
        [Tooltip("InventoryModel asset to observe for slot snapshots and visibility.")]
        [SerializeField] private InventoryModel inventoryModel;

        [Tooltip("Input action that opens the pie menu while held. This is required; without it the menu has no open/close control.")]
        [SerializeField] private InputActionReference openAction;

        [Tooltip("Input action that would confirm a selection. Selection is intentionally disabled; this is kept for wiring/testing.")]
        [SerializeField] private InputActionReference selectAction;

        [Tooltip("ItemDatabase asset used to resolve item ids into UI icons.")]
        [SerializeField] private ItemDatabase itemDatabase;

        [Header("UI")]
        [Tooltip("CanvasGroup used to show/hide the pie menu without disabling the GameObject.")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Tooltip("Ten slot widgets arranged manually in the scene. Slot 0 is the center slot; slots 1-9 form the pie.\nEditor note: slot indices are auto-assigned from this array order (0..9) to avoid duplicate-index setup errors.")]
        [SerializeField] private InventorySlotWidget[] slots;

        [Tooltip("Optional camera used for screen-to-UI pointer conversion. Leave null for Screen Space Overlay canvases.")]
        [SerializeField] private Camera uiCamera;

        private readonly InventorySlotWidget[] _slotsByIndex = new InventorySlotWidget[SlotCount];
        private readonly RectTransform[] _slotRectsByIndex = new RectTransform[SlotCount];

        private RectTransform _rootRectTransform;
        private IPlayerInventory _inventory;

        private Core.IObserver<InventorySlotState[]> _slotsObserver;
        private Core.IObserver<int> _selectedObserver;
        private Core.IObserver<bool> _visibleObserver;

        private InventorySlotState[] _latestSlots = Array.Empty<InventorySlotState>();
        private int _hoveredSlotIndex = -1;
        private bool _modelVisible = true;
        private bool _openHeld;
        private bool _visible;

        private bool _selectActionEnabledByThisComponent;
        private bool _openActionEnabledByThisComponent;
        private bool _loggedSelectionDisabled;
        private bool _cursorOverridden;

        /// <summary>
        /// Validates dependencies, caches slot widgets, and prepares observers.<br>
        /// Typical usage: let Unity call this automatically; the component is not safe to run with missing references.
        /// </summary>
        private void Awake()
        {
            if (!TryGetComponent(out _rootRectTransform))
            {
                Debug.LogError($"[{nameof(InventoryPieMenuView)}] Missing RectTransform on '{gameObject.name}'. This component must live on a UI object.", gameObject);
                throw new InvalidOperationException($"[{nameof(InventoryPieMenuView)}] Missing RectTransform on '{gameObject.name}'.");
            }

            if (inventoryModel == null)
            {
                Debug.LogError($"[{nameof(InventoryPieMenuView)}] inventoryModel is not assigned on '{gameObject.name}'.", gameObject);
                throw new NullReferenceException($"[{nameof(InventoryPieMenuView)}] inventoryModel is null on '{gameObject.name}'.");
            }

            if (openAction == null || openAction.action == null)
            {
                Debug.LogError($"[{nameof(InventoryPieMenuView)}] openAction is not assigned on '{gameObject.name}'. This menu requires a hold-to-open action.", gameObject);
                throw new NullReferenceException($"[{nameof(InventoryPieMenuView)}] openAction is null on '{gameObject.name}'.");
            }

            if (selectAction == null || selectAction.action == null)
            {
                Debug.LogError($"[{nameof(InventoryPieMenuView)}] selectAction is not assigned on '{gameObject.name}'.", gameObject);
                throw new NullReferenceException($"[{nameof(InventoryPieMenuView)}] selectAction is null on '{gameObject.name}'.");
            }

            if (canvasGroup == null)
            {
                Debug.LogError($"[{nameof(InventoryPieMenuView)}] canvasGroup is not assigned on '{gameObject.name}'. The pie menu must stay active in-scene and use CanvasGroup visibility.", gameObject);
                throw new NullReferenceException($"[{nameof(InventoryPieMenuView)}] canvasGroup is null on '{gameObject.name}'.");
            }

            if (itemDatabase == null)
            {
                Debug.LogWarning($"[{nameof(InventoryPieMenuView)}] itemDatabase is not assigned on '{gameObject.name}'. Inventory icons cannot be resolved.", gameObject);
            }

            // Default to closed until the hold-to-open action is pressed.
            _openHeld = false;

            CacheSlots();

            _slotsObserver = new ActionObserver<InventorySlotState[]>(OnSlotsChanged);
            _selectedObserver = new ActionObserver<int>(_ => RenderSlots());
            _visibleObserver = new ActionObserver<bool>(SetVisible);
        }

        /// <summary>
        /// Subscribes to model updates, enables input actions, and renders the current menu state.<br>
        /// Typical usage: Unity calls this when the UI becomes active.
        /// </summary>
        private void OnEnable()
        {
            inventoryModel.Slots.Attach(_slotsObserver);
            inventoryModel.SelectedSlotIndex.Attach(_selectedObserver);
            inventoryModel.IsVisible.Attach(_visibleObserver);

            EnableSelectAction();
            EnableOpenAction();

            SetVisible(inventoryModel.IsVisible.Value);

            _latestSlots = inventoryModel.Slots.Value ?? Array.Empty<InventorySlotState>();
            RenderSlots();
        }

        /// <summary>
        /// Keeps the cursor in the expected state while the menu is visible.<br>
        /// Typical usage: Unity calls this every frame.
        /// </summary>
        private void Update()
        {
            if (!_visible)
            {
                return;
            }

            EnsureCursorVisibleAndUnlockedWhileOpen();
        }

        /// <summary>
        /// Detaches observers and disables input actions if this component enabled them.<br>
        /// Typical usage: Unity calls this when the menu is hidden or destroyed.
        /// </summary>
        private void OnDisable()
        {
            if (inventoryModel != null)
            {
                inventoryModel.Slots.Detach(_slotsObserver);
                inventoryModel.SelectedSlotIndex.Detach(_selectedObserver);
                inventoryModel.IsVisible.Detach(_visibleObserver);
            }

            DisableSelectAction();
            DisableOpenAction();

            HideAndCenterCursorIfOverridden();
        }

        /// <summary>
        /// Confirm callback. Selection is intentionally disabled in this component.<br>
        /// Typical usage: keeps the action wired while preventing accidental selection until a new implementation is provided.
        /// </summary>
        /// <param name="context">Input system callback context for the confirm action.</param>
        private void OnSelectPerformed(InputAction.CallbackContext context)
        {
            if (!_visible)
            {
                return;
            }

            if (_loggedSelectionDisabled)
            {
                return;
            }

            _loggedSelectionDisabled = true;
            Debug.LogWarning($"[{nameof(InventoryPieMenuView)}] Selection is intentionally disabled on '{gameObject.name}'.", gameObject);
        }

        /// <summary>
        /// Sets which slot index should be rendered as hovered.<br>
        /// Typical usage: called by a UI hover component (pointer enter/move) that knows its slot index.
        /// </summary>
        /// <param name="slotIndex">Zero-based slot index (0..9).</param>
        public void SetHoveredSlotIndex(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SlotCount)
            {
                Debug.LogWarning($"[{nameof(InventoryPieMenuView)}] Ignoring hovered slot index {slotIndex} on '{gameObject.name}'. Expected 0..{SlotCount - 1}.", gameObject);
                return;
            }

            if (_hoveredSlotIndex == slotIndex)
            {
                return;
            }

            _hoveredSlotIndex = slotIndex;
            RenderSlots();
        }

        /// <summary>
        /// Clears the currently hovered slot if it matches the provided slot index.<br>
        /// Typical usage: called by a UI hover component on pointer exit.
        /// </summary>
        /// <param name="slotIndex">Slot index that is exiting hover.</param>
        public void ClearHoveredSlotIndex(int slotIndex)
        {
            if (_hoveredSlotIndex != slotIndex)
            {
                return;
            }

            _hoveredSlotIndex = -1;
            RenderSlots();
        }

        /// <summary>
        /// Attempts to select a slot through the currently bound inventory command target.<br>
        /// Typical usage: called by a hover/click component that wants to drive selection without directional math.
        /// </summary>
        /// <param name="slotIndex">Zero-based slot index (0..9).</param>
        /// <returns>True when the selection request was accepted by the inventory target.</returns>
        public bool TrySelectSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SlotCount)
            {
                Debug.LogWarning($"[{nameof(InventoryPieMenuView)}] Cannot select slot {slotIndex} on '{gameObject.name}'. Expected 0..{SlotCount - 1}.", gameObject);
                return false;
            }

            if (_inventory == null)
            {
                Debug.LogWarning($"[{nameof(InventoryPieMenuView)}] Cannot select slot {slotIndex} because no inventoryTarget is bound on '{gameObject.name}'.", gameObject);
                return false;
            }

            if (_inventory.TrySelectSlot(slotIndex))
            {
                return true;
            }

            Debug.LogWarning($"[{nameof(InventoryPieMenuView)}] Failed to select slot {slotIndex} on '{gameObject.name}'.", gameObject);
            return false;
        }

        /// <summary>
        /// Sets the current inventory command target. This is retained so ownership-based binding can continue to work.<br>
        /// Typical usage: a local-owner NetworkPlayerInventory assigns itself when ownership is gained.
        /// </summary>
        /// <param name="newInventoryTarget">IPlayerInventory implementing IPlayerInventory (typically NetworkPlayerInventory).</param>
        public void SetInventoryTarget(IPlayerInventory newInventoryTarget)
        {
            if (newInventoryTarget == null)
            {
                ClearInventoryTarget(expectedInventoryTarget: null);
                return;
            }

            _inventory = newInventoryTarget;
            Debug.Log($"[{nameof(InventoryPieMenuView)}] Bound inventoryTarget to '{newInventoryTarget.GetType().Name}' on '{gameObject.name}'.", gameObject);
        }

        /// <summary>
        /// Clears the currently bound inventory command target.<br>
        /// Typical usage: called when local ownership is lost or the owning object despawns.
        /// </summary>
        /// <param name="expectedInventoryTarget">Optional expected target; when provided, clearing is skipped if the current target differs.</param>
        public void ClearInventoryTarget(IPlayerInventory expectedInventoryTarget)
        {
            if (expectedInventoryTarget != null && _inventory != expectedInventoryTarget)
            {
                return;
            }

            _inventory = null;
            Debug.Log($"[{nameof(InventoryPieMenuView)}] Cleared inventoryTarget on '{gameObject.name}'.", gameObject);
        }

        /// <summary>
        /// Updates the cached inventory snapshot and rerenders the menu.<br>
        /// Typical usage: internal observer callback for InventoryModel.Slots.
        /// </summary>
        /// <param name="slotStates">Latest slot snapshot from the inventory model.</param>
        private void OnSlotsChanged(InventorySlotState[] slotStates)
        {
            _latestSlots = slotStates ?? Array.Empty<InventorySlotState>();
            RenderSlots();
        }

        /// <summary>
        /// Applies the visibility state using CanvasGroup.<br>
        /// Typical usage: internal observer callback for InventoryModel.IsVisible.
        /// </summary>
        /// <param name="visible">True to show the menu, false to hide it.</param>
        private void SetVisible(bool visible)
        {
            _modelVisible = visible;
            ApplyEffectiveVisibility();
        }

        /// <summary>
        /// Applies the combined visibility state (model visibility AND hold-to-open input) to the CanvasGroup.<br>
        /// Typical usage: internal helper so the menu is gated by both model visibility and open/hold input.
        /// </summary>
        private void ApplyEffectiveVisibility()
        {
            bool wasVisible = _visible;
            _visible = _modelVisible && _openHeld;

            canvasGroup.alpha = _visible ? 1f : 0f;
            canvasGroup.interactable = _visible;
            canvasGroup.blocksRaycasts = _visible;

            if (_visible && !wasVisible)
            {
                _loggedSelectionDisabled = false;
                _hoveredSlotIndex = -1;
                CaptureAndOverrideCursor();
            }
            else if (!_visible && wasVisible)
            {
                _hoveredSlotIndex = -1;
                HideAndCenterCursorIfOverridden();
            }
        }

        /// <summary>
        /// Captures and overrides the cursor for radial menu interaction.<br>
        /// Typical usage: called when the pie menu becomes visible; makes the cursor visible and unlocked.
        /// </summary>
        private void CaptureAndOverrideCursor()
        {
            if (_cursorOverridden)
            {
                return;
            }

            _cursorOverridden = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        /// <summary>
        /// Ensures the cursor remains visible and unlocked while the pie menu is open, even if other scripts modify it.<br>
        /// Typical usage: called each frame while the menu is visible.
        /// </summary>
        private void EnsureCursorVisibleAndUnlockedWhileOpen()
        {
            if (!_visible)
            {
                return;
            }

            if (!_cursorOverridden)
            {
                CaptureAndOverrideCursor();
                return;
            }

            if (!Cursor.visible)
            {
                Cursor.visible = true;
            }

            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        /// <summary>
        /// Hides and locks the cursor, and warps it to screen center when possible.<br>
        /// Typical usage: called when the pie menu closes or disables.
        /// </summary>
        private void HideAndCenterCursorIfOverridden()
        {
            if (!_cursorOverridden)
            {
                return;
            }

            if (Mouse.current != null)
            {
                Mouse.current.WarpCursorPosition(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
            }

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            _cursorOverridden = false;
        }

        /// <summary>
        /// Input callback used to open the pie menu while the action is held.<br>
        /// Typical usage: bound to openAction started/performed.
        /// </summary>
        /// <param name="context">Input system callback context for the open action.</param>
        private void OnOpenStarted(InputAction.CallbackContext context)
        {
            _openHeld = true;
            ApplyEffectiveVisibility();
            RenderSlots();
        }

        /// <summary>
        /// Input callback used to close the pie menu when the hold action is released.<br>
        /// Typical usage: bound to openAction canceled.
        /// </summary>
        /// <param name="context">Input system callback context for the open action.</param>
        private void OnOpenCanceled(InputAction.CallbackContext context)
        {
            _openHeld = false;
            ApplyEffectiveVisibility();
        }

        /// <summary>
        /// Renders all cached slot widgets using the latest inventory snapshot.<br>
        /// Typical usage: called when the model changes or when the menu is opened.
        /// </summary>
        private void RenderSlots()
        {
            if (!_visible)
            {
                return;
            }

            for (int i = 0; i < SlotCount; i++)
            {
                InventorySlotWidget widget = _slotsByIndex[i];
                if (widget == null)
                {
                    continue;
                }

                InventorySlotState state = i >= 0 && i < _latestSlots.Length ? _latestSlots[i] : default;
                Sprite icon = ResolveIcon(state);
                widget.Render(state, icon, _hoveredSlotIndex == i);
            }
        }

        /// <summary>
        /// Resolves the icon for a slot from the configured item database.<br>
        /// Typical usage: internal render helper for both empty and occupied slots.
        /// </summary>
        /// <param name="slot">Slot snapshot to render.</param>
        /// <returns>The resolved icon, or null when none is available.</returns>
        private Sprite ResolveIcon(InventorySlotState slot)
        {
            if (itemDatabase == null)
            {
                return null;
            }

            ushort itemId = slot.IsEmpty ? (ushort)0 : slot.ItemId;
            if (!itemDatabase.TryGet(itemId, out var definition) || definition == null)
            {
                return null;
            }

            return definition.icon;
        }

        /// <summary>
        /// Enables the confirm action and subscribes to its performed callback.<br>
        /// Typical usage: called when the component becomes active.
        /// </summary>
        private void EnableSelectAction()
        {
            InputAction action = selectAction.action;
            action.performed += OnSelectPerformed;

            _selectActionEnabledByThisComponent = !action.enabled;
            if (_selectActionEnabledByThisComponent)
            {
                action.Enable();
            }
        }

        /// <summary>
        /// Enables the open/hold action and subscribes to its callbacks.<br>
        /// Typical usage: called when the component becomes active.
        /// </summary>
        private void EnableOpenAction()
        {
            InputAction action = openAction.action;
            action.started += OnOpenStarted;
            action.performed += OnOpenStarted;
            action.canceled += OnOpenCanceled;

            _openActionEnabledByThisComponent = !action.enabled;
            if (_openActionEnabledByThisComponent)
            {
                action.Enable();
            }

            _openHeld = action.IsPressed();
            ApplyEffectiveVisibility();
        }

        /// <summary>
        /// Unsubscribes from the confirm action and restores its disabled state if this component enabled it.<br>
        /// Typical usage: called when the component becomes inactive.
        /// </summary>
        private void DisableSelectAction()
        {
            if (selectAction == null || selectAction.action == null)
            {
                return;
            }

            InputAction action = selectAction.action;
            action.performed -= OnSelectPerformed;

            if (_selectActionEnabledByThisComponent && action.enabled)
            {
                action.Disable();
            }

            _selectActionEnabledByThisComponent = false;
        }

        /// <summary>
        /// Unsubscribes from the open/hold action and restores its disabled state if this component enabled it.<br>
        /// Typical usage: called when the component becomes inactive.
        /// </summary>
        private void DisableOpenAction()
        {
            if (openAction == null || openAction.action == null)
            {
                return;
            }

            InputAction action = openAction.action;
            action.started -= OnOpenStarted;
            action.performed -= OnOpenStarted;
            action.canceled -= OnOpenCanceled;

            if (_openActionEnabledByThisComponent && action.enabled)
            {
                action.Disable();
            }

            _openActionEnabledByThisComponent = false;
        }

        /// <summary>
        /// Caches the ten slot widgets by index and validates that the pie menu is fully configured.<br>
        /// Typical usage: called once during Awake.
        /// </summary>
        private void CacheSlots()
        {
            if (slots == null || slots.Length != SlotCount)
            {
                Debug.LogError($"[{nameof(InventoryPieMenuView)}] '{gameObject.name}' must be configured with exactly {SlotCount} slot widgets.", gameObject);
                throw new InvalidOperationException($"[{nameof(InventoryPieMenuView)}] '{gameObject.name}' must be configured with exactly {SlotCount} slot widgets.");
            }

            Array.Clear(_slotsByIndex, 0, _slotsByIndex.Length);
            Array.Clear(_slotRectsByIndex, 0, _slotRectsByIndex.Length);

            for (int i = 0; i < slots.Length; i++)
            {
                InventorySlotWidget widget = slots[i];
                if (widget == null)
                {
                    Debug.LogError($"[{nameof(InventoryPieMenuView)}] Slot widget entry {i} is null on '{gameObject.name}'.", gameObject);
                    throw new NullReferenceException($"[{nameof(InventoryPieMenuView)}] Slot widget entry {i} is null on '{gameObject.name}'.");
                }

                if (!widget.TryGetComponent(out RectTransform slotRect))
                {
                    Debug.LogError($"[{nameof(InventoryPieMenuView)}] Slot widget '{widget.name}' on '{gameObject.name}' is missing a RectTransform.", gameObject);
                    throw new InvalidOperationException($"[{nameof(InventoryPieMenuView)}] Slot widget '{widget.name}' on '{gameObject.name}' is missing a RectTransform.");
                }

                int slotIndex = widget.SlotIndex;
                if (slotIndex < 0 || slotIndex >= SlotCount)
                {
                    Debug.LogError($"[{nameof(InventoryPieMenuView)}] Slot widget '{widget.name}' on '{gameObject.name}' has invalid slot index {slotIndex}. Expected 0..{SlotCount - 1}.", gameObject);
                    throw new InvalidOperationException($"[{nameof(InventoryPieMenuView)}] Slot widget '{widget.name}' on '{gameObject.name}' has invalid slot index {slotIndex}.");
                }

                if (_slotsByIndex[slotIndex] != null)
                {
                    InventorySlotWidget existing = _slotsByIndex[slotIndex];
                    Debug.LogError($"[{nameof(InventoryPieMenuView)}] Duplicate slot index {slotIndex} on '{gameObject.name}'. Widgets: '{existing.name}' and '{widget.name}'.", gameObject);
                    throw new InvalidOperationException($"[{nameof(InventoryPieMenuView)}] Duplicate slot index {slotIndex} on '{gameObject.name}'. Widgets: '{existing.name}' and '{widget.name}'.");
                }

                _slotsByIndex[slotIndex] = widget;
                _slotRectsByIndex[slotIndex] = slotRect;
            }

            for (int i = 0; i < SlotCount; i++)
            {
                if (_slotsByIndex[i] != null)
                {
                    continue;
                }

                Debug.LogError($"[{nameof(InventoryPieMenuView)}] Missing slot widget for slot {i} on '{gameObject.name}'.", gameObject);
                throw new InvalidOperationException($"[{nameof(InventoryPieMenuView)}] Missing slot widget for slot {i} on '{gameObject.name}'.");
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only validation hook to keep slot indices consistent with the configured array order.<br>
        /// Typical usage: prevents runtime exceptions caused by duplicate or unset InventorySlotWidget.SlotIndex values.
        /// </summary>
        private void OnValidate()
        {
            if (slots == null || slots.Length != SlotCount)
            {
                return;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null)
                {
                    continue;
                }

                slots[i].SetSlotIndex(i);
            }
        }
#endif
    }
}

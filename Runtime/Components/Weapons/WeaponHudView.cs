using RoachRace.Data;
using RoachRace.UI.Core;
using RoachRace.UI.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoachRace.UI.Components.Weapons
{
    /// <summary>
    /// Minimal weapon HUD renderer: weapon icon + ammo counter.
    ///
    /// Setup:
    /// - Assign weaponHudModel.
    /// - Assign inventoryModel (used to compute reserve ammo from slots).
    /// - Assign iconImage and ammoText.
    /// </summary>
    public sealed class WeaponHudView : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private WeaponHudModel weaponHudModel;
        [SerializeField] private InventoryModel inventoryModel;

        [Header("UI")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text ammoText;

        private IObserver<Sprite> _iconObserver;
        private IObserver<int> _ammoObserver;
        private IObserver<ushort> _ammoItemObserver;
        private IObserver<InventorySlotState[]> _invSlotsObserver;

        private void Awake()
        {
            if (weaponHudModel == null)
            {
                Debug.LogError($"[{nameof(WeaponHudView)}] weaponHudModel is not assigned on '{gameObject.name}'.", gameObject);
                throw new System.NullReferenceException($"[{nameof(WeaponHudView)}] weaponHudModel is null on '{gameObject.name}'.");
            }

            if (inventoryModel == null)
            {
                Debug.LogError($"[{nameof(WeaponHudView)}] inventoryModel is not assigned on '{gameObject.name}'.", gameObject);
                throw new System.NullReferenceException($"[{nameof(WeaponHudView)}] inventoryModel is null on '{gameObject.name}'.");
            }

            _iconObserver = new ActionObserver<Sprite>(RenderIcon);
            _ammoObserver = new ActionObserver<int>(_ => RenderAmmo());
            _ammoItemObserver = new ActionObserver<ushort>(_ => RenderAmmo());
            _invSlotsObserver = new ActionObserver<InventorySlotState[]>(_ => RenderAmmo());
        }

        private void OnEnable()
        {
            weaponHudModel.WeaponIcon.Attach(_iconObserver);
            weaponHudModel.AmmoInMag.Attach(_ammoObserver);
            weaponHudModel.AmmoItemId.Attach(_ammoItemObserver);

            inventoryModel.Slots.Attach(_invSlotsObserver);

            RenderIcon(weaponHudModel.WeaponIcon.Value);
            RenderAmmo();
        }

        private void OnDisable()
        {
            weaponHudModel.WeaponIcon.Detach(_iconObserver);
            weaponHudModel.AmmoInMag.Detach(_ammoObserver);
            weaponHudModel.AmmoItemId.Detach(_ammoItemObserver);

            inventoryModel.Slots.Detach(_invSlotsObserver);
        }

        private void RenderIcon(Sprite icon)
        {
            if (iconImage == null)
                return;

            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        private void RenderAmmo()
        {
            if (ammoText == null)
                return;

            int mag = weaponHudModel.AmmoInMag.Value;
            ushort ammoId = weaponHudModel.AmmoItemId.Value;

            int reserve = 0;
            var slots = inventoryModel.Slots.Value ?? System.Array.Empty<InventorySlotState>();
            if (ammoId != 0)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    var s = slots[i];
                    if (s.IsEmpty) continue;
                    if (s.ItemId != ammoId) continue;
                    reserve += s.Count;
                }
            }

            ammoText.text = $"{mag}/{reserve}";
        }
    }
}

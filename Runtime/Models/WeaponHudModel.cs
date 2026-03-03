using RoachRace.UI.Core;
using UnityEngine;

namespace RoachRace.UI.Models
{
    /// <summary>
    /// ScriptableObject model backing the weapon HUD (icon + ammo counter).
    ///
    /// Notes:
    /// - Updated by networking/gameplay code for the local owner.
    /// - Reserve ammo is typically computed from InventoryModel (UI-side).
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponHudModel", menuName = "RoachRace/Models/WeaponHudModel")]
    public sealed class WeaponHudModel : UIModel
    {
        public readonly Observable<Sprite> WeaponIcon = new Observable<Sprite>(null);
        public readonly Observable<ushort> AmmoItemId = new Observable<ushort>(0);
        public readonly Observable<int> MagazineSize = new Observable<int>(0);
        public readonly Observable<int> AmmoInMag = new Observable<int>(0);
        public readonly Observable<bool> IsReloading = new Observable<bool>(false);

        public void SetWeapon(Sprite icon, ushort ammoItemId, int magazineSize)
        {
            WeaponIcon.Value = icon;
            AmmoItemId.Value = ammoItemId;
            MagazineSize.Value = magazineSize;
        }

        public void SetAmmoInMag(int ammoInMag) => AmmoInMag.Value = ammoInMag;
        public void SetReloading(bool isReloading) => IsReloading.Value = isReloading;

        public void NotifyAll()
        {
            WeaponIcon.Notify(WeaponIcon.Value);
            AmmoItemId.Notify(AmmoItemId.Value);
            MagazineSize.Notify(MagazineSize.Value);
            AmmoInMag.Notify(AmmoInMag.Value);
            IsReloading.Notify(IsReloading.Value);
        }
    }
}

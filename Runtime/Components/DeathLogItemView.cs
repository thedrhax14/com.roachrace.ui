using RoachRace.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoachRace.UI.Components
{
    public sealed class DeathLogItemView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text attackerNameText;
        [SerializeField] private TMP_Text victimNameText;
        [SerializeField] private TMP_Text weaponText;

        [SerializeField] private Image attackerAvatarImage;
        [SerializeField] private Image victimAvatarImage;

        [Tooltip("Used when we cannot resolve avatars yet.")]
        [SerializeField] private Sprite defaultAvatar;

        public float ExpireAtTime { get; private set; }

        public void Show(DeathLogEntry entry, float expireAtTime)
        {
            ExpireAtTime = expireAtTime;

            if (attackerNameText != null)
                attackerNameText.text = string.IsNullOrWhiteSpace(entry.Attacker.Name) ? "?" : entry.Attacker.Name;

            if (victimNameText != null)
                victimNameText.text = string.IsNullOrWhiteSpace(entry.Victim.Name) ? "?" : entry.Victim.Name;

            if (weaponText != null)
            {
                if (!string.IsNullOrWhiteSpace(entry.WeaponIconKey))
                    weaponText.text = entry.WeaponIconKey;
                else
                    weaponText.text = entry.DamageType.ToString();
            }

            ApplyAvatar(attackerAvatarImage, entry.Attacker.AvatarUrl);
            ApplyAvatar(victimAvatarImage, entry.Victim.AvatarUrl);

            SetVisible(true);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void ApplyAvatar(Image image, string avatarUrl)
        {
            if (image == null) return;

            // Placeholder implementation.
            // TODO: resolve avatarUrl to a Sprite (remote download, addressables, or cache).
            image.sprite = defaultAvatar;
            image.enabled = image.sprite != null;
        }
    }
}

using RoachRace.UI.Core;
using UnityEngine;

namespace RoachRace.UI.Models
{
    /// <summary>
    /// ScriptableObject model backing a single on-screen "action prompt" widget.
    ///
    /// Intended usage:
    /// - Create up to 8 ActionPromptModel assets (one per widget slot).
    /// - Each UI widget observes exactly one ActionPromptModel instance.
    /// - Gameplay/input forwarders update a specific ActionPromptModel to drive the HUD.
    ///
    /// Fields supported:
    /// - Key icon + optional key text
    /// - Action name
    /// - Hold progress (0..1)
    /// - Uses left (optional; set to -1 to hide)
    /// - Visibility
    /// </summary>
    [CreateAssetMenu(fileName = "ActionPromptModel", menuName = "RoachRace/Models/ActionPromptModel")]
    public sealed class ActionPromptModel : UIModel
    {
        public readonly Observable<bool> IsVisible = new(false);
        public readonly Observable<Sprite> KeyIcon = new(null);
        public readonly Observable<string> KeyText = new(string.Empty);
        public readonly Observable<string> ActionName = new(string.Empty);
        public readonly Observable<float> HoldProgress01 = new(0f);
        public readonly Observable<int> UsesLeft = new(-1);

        public void SetVisible(bool visible) => IsVisible.Value = visible;

        public void SetKey(Sprite icon, string text)
        {
            KeyIcon.Value = icon;
            KeyText.Value = text ?? string.Empty;
        }

        public void SetActionName(string name)
        {
            ActionName.Value = name ?? string.Empty;
        }

        public void SetHoldProgress(float progress01)
        {
            HoldProgress01.Value = Mathf.Clamp01(progress01);
        }

        /// <summary>
        /// Set to -1 to hide uses-left UI.
        /// </summary>
        public void SetUsesLeft(int usesLeft)
        {
            UsesLeft.Value = usesLeft;
        }

        public void Clear()
        {
            IsVisible.Value = false;
            KeyIcon.Value = null;
            KeyText.Value = string.Empty;
            ActionName.Value = string.Empty;
            HoldProgress01.Value = 0f;
            UsesLeft.Value = -1;
        }
    }
}

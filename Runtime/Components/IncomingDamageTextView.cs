using RoachRace.UI.Core;
using RoachRace.UI.Models;
using TMPro;
using UnityEngine;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// Displays the latest incoming damage event as a short-lived text indicator.<br/>
    /// Typical usage: place this on a HUD object, assign <see cref="IncomingDamageModel"/>, and connect a <see cref="TextMeshProUGUI"/> plus optional <see cref="CanvasGroup"/> so the local player sees recent damage without coupling networking code to a specific window.<br/>
    /// Configuration/context: this is an owner-local presentation component only; it reacts to event-like model updates and ignores the initial default observable value.
    /// </summary>
    public sealed class IncomingDamageTextView : MonoBehaviour, IObserver<IncomingDamageEntry>
    {
        [Header("Model")]
        [SerializeField]
        [Tooltip("Owner-local incoming damage model published by the networking bridge.")]
        private IncomingDamageModel incomingDamageModel;

        [Header("UI")]
        [SerializeField]
        [Tooltip("Text element used to render the latest incoming damage amount.")]
        private TextMeshProUGUI damageText;

        [SerializeField]
        [Tooltip("Optional canvas group used for fading. If empty, the component tries to resolve one on the same GameObject.")]
        private CanvasGroup canvasGroup;

        [Header("Behavior")]
        [SerializeField, Min(0.1f)]
        [Tooltip("Total time in seconds that the damage text remains visible after a hit.")]
        private float displayDurationSeconds = 0.9f;

        [SerializeField, Min(0f)]
        [Tooltip("How many seconds at the end of the display window are used to fade the text out.")]
        private float fadeOutDurationSeconds = 0.35f;

        [SerializeField]
        [Tooltip("If true, include the weapon/effect key before the damage amount when attribution is available.")]
        private bool showWeaponIconKey;

        private float visibleUntilTime;

        /// <summary>
        /// Resolves optional UI dependencies and hides the indicator until the first damage event arrives.<br/>
        /// Typical usage: Unity invokes this during component initialization; external callers should not call it directly.<br/>
        /// Configuration/context: the text starts hidden so the HUD does not flash placeholder content on scene load.
        /// </summary>
        private void Awake()
        {
            if (canvasGroup == null)
                TryGetComponent(out canvasGroup);

            ApplyVisibility(0f);
        }

        /// <summary>
        /// Subscribes to incoming damage events when the component becomes active.<br/>
        /// Typical usage: Unity invokes this on the owning client's HUD object; the current observable value is pushed immediately, so default payloads are filtered in <see cref="OnNotify"/>.<br/>
        /// Configuration/context: missing model or text references are logged and the component remains inactive.
        /// </summary>
        private void OnEnable()
        {
            if (incomingDamageModel == null)
            {
                Debug.LogError($"[{nameof(IncomingDamageTextView)}] Missing required reference on '{gameObject.name}': {nameof(incomingDamageModel)}.", gameObject);
                return;
            }

            if (damageText == null)
            {
                Debug.LogError($"[{nameof(IncomingDamageTextView)}] Missing required reference on '{gameObject.name}': {nameof(damageText)}.", gameObject);
                return;
            }

            incomingDamageModel.LatestEntry.Attach(this);
        }

        /// <summary>
        /// Unsubscribes from incoming damage events when the component is disabled.<br/>
        /// Typical usage: Unity invokes this during HUD teardown or deactivation to avoid stale model subscriptions.<br/>
        /// Configuration/context: safe to call even if subscription was never established.
        /// </summary>
        private void OnDisable()
        {
            if (incomingDamageModel != null)
                incomingDamageModel.LatestEntry.Detach(this);

            ApplyVisibility(0f);
        }

        /// <summary>
        /// Updates the damage indicator fade over time after a hit has been shown.<br/>
        /// Typical usage: Unity invokes this each frame while the HUD is active.<br/>
        /// Configuration/context: uses unscaled time so pause/menu time scaling does not freeze the indicator mid-fade.
        /// </summary>
        private void Update()
        {
            if (visibleUntilTime <= 0f)
                return;

            float remaining = visibleUntilTime - Time.unscaledTime;
            if (remaining <= 0f)
            {
                visibleUntilTime = 0f;
                ApplyVisibility(0f);
                return;
            }

            float fadeDuration = Mathf.Min(displayDurationSeconds, Mathf.Max(0f, fadeOutDurationSeconds));
            if (fadeDuration <= 0f || remaining > fadeDuration)
            {
                ApplyVisibility(1f);
                return;
            }

            ApplyVisibility(Mathf.Clamp01(remaining / fadeDuration));
        }

        /// <summary>
        /// Receives a newly published incoming damage event and updates the text indicator.<br/>
        /// Typical usage: invoked by <see cref="IncomingDamageModel"/> whenever the local player takes damage.<br/>
        /// Configuration/context: ignores the initial default payload pushed by observable attachment so the widget does not show a bogus zero-damage hit.
        /// </summary>
        /// <param name="entry">The incoming damage event to display.</param>
        public void OnNotify(IncomingDamageEntry entry)
        {
            if (entry.DamageAmount <= 0 && entry.PreviousHealth == 0 && entry.CurrentHealth == 0 && string.IsNullOrEmpty(entry.WeaponIconKey))
                return;

            if (damageText == null)
                return;

            damageText.text = BuildDisplayText(entry);
            visibleUntilTime = Time.unscaledTime + Mathf.Max(0.1f, displayDurationSeconds);
            ApplyVisibility(1f);
        }

        /// <summary>
        /// Builds the on-screen text for an incoming damage event.<br/>
        /// Typical usage: called when a new hit arrives so the latest damage can be rendered consistently.<br/>
        /// Configuration/context: optional weapon/effect attribution is included only when enabled and available.
        /// </summary>
        /// <param name="entry">The incoming damage event to format.</param>
        /// <returns>The formatted damage text for display.</returns>
        private string BuildDisplayText(IncomingDamageEntry entry)
        {
            string amountText = $"-{entry.DamageAmount}";
            if (!showWeaponIconKey || string.IsNullOrWhiteSpace(entry.WeaponIconKey))
                return amountText;

            return $"{entry.WeaponIconKey} {amountText}";
        }

        /// <summary>
        /// Applies the current alpha/visibility to the configured text and canvas group.<br/>
        /// Typical usage: called during initialization, on new hits, and during fade updates.<br/>
        /// Configuration/context: when no canvas group is assigned, the text alpha is updated directly instead.
        /// </summary>
        /// <param name="alpha">Normalized alpha value between 0 and 1.</param>
        private void ApplyVisibility(float alpha)
        {
            alpha = Mathf.Clamp01(alpha);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
                canvasGroup.blocksRaycasts = alpha > 0f;
                canvasGroup.interactable = false;
            }

            if (damageText != null)
            {
                Color color = damageText.color;
                color.a = alpha;
                damageText.color = color;
            }
        }
    }
}
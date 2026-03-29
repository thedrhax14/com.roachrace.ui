using RoachRace.UI.Core;
using RoachRace.UI.Models;
using UnityEngine;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// Displays a camera-relative arc that points toward the source of the most recent incoming damage.<br/>
    /// Typical usage: place this near the center of the gameplay HUD, assign an <see cref="IncomingDamageModel"/>, and connect an arc-shaped RectTransform/Image so the local player sees where damage came from without showing raw damage numbers.<br/>
    /// Configuration/context: the indicator rotates relative to camera yaw and fades over time; it requires a source world position in the incoming damage payload.
    /// </summary>
    public sealed class IncomingDamageArcIndicatorView : MonoBehaviour, IObserver<IncomingDamageEntry>
    {
        [Header("Model")]
        [SerializeField] private IncomingDamageModel incomingDamageModel;

        [Header("UI")]
        [SerializeField] private RectTransform indicatorRect;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Camera")]
        [SerializeField] private Camera targetCamera;

        [Header("Behavior")]
        [SerializeField, Min(0.1f)] private float displayDurationSeconds = 0.9f;
        [SerializeField, Min(0f)] private float fadeOutDurationSeconds = 0.35f;
        [SerializeField] private bool flattenToHorizontalPlane = true;

        private bool hasActiveSource;
        private Vector3 sourceWorldPosition;
        private float visibleUntilTime;

        /// <summary>
        /// Resolves optional UI dependencies and hides the indicator until the first valid hit arrives.<br/>
        /// Typical usage: Unity invokes this during initialization before the HUD is enabled.<br/>
        /// Configuration/context: if no explicit <see cref="CanvasGroup"/> is assigned, the component tries to resolve one on the same GameObject.
        /// </summary>
        private void Awake()
        {
            if (indicatorRect == null)
                indicatorRect = transform as RectTransform;

            if (canvasGroup == null)
                TryGetComponent(out canvasGroup);

            ApplyVisibility(0f);
        }

        /// <summary>
        /// Subscribes to incoming damage events when the indicator becomes active.<br/>
        /// Typical usage: Unity invokes this when the gameplay HUD is shown.<br/>
        /// Configuration/context: the current observable value is pushed immediately, so default payloads are ignored in <see cref="OnNotify"/>.
        /// </summary>
        private void OnEnable()
        {
            if (incomingDamageModel == null)
            {
                Debug.LogError($"[{nameof(IncomingDamageArcIndicatorView)}] Missing required reference on '{gameObject.name}': {nameof(incomingDamageModel)}.", gameObject);
                return;
            }

            if (indicatorRect == null)
            {
                Debug.LogError($"[{nameof(IncomingDamageArcIndicatorView)}] Missing required reference on '{gameObject.name}': {nameof(indicatorRect)}.", gameObject);
                return;
            }

            incomingDamageModel.LatestEntry.Attach(this);
        }

        /// <summary>
        /// Unsubscribes from incoming damage events and hides the indicator.<br/>
        /// Typical usage: Unity invokes this when the HUD is hidden or destroyed.<br/>
        /// Configuration/context: safe to call even if the subscription was never established.
        /// </summary>
        private void OnDisable()
        {
            if (incomingDamageModel != null)
                incomingDamageModel.LatestEntry.Detach(this);

            hasActiveSource = false;
            visibleUntilTime = 0f;
            ApplyVisibility(0f);
        }

        /// <summary>
        /// Updates the indicator direction and fade each frame while active.<br/>
        /// Typical usage: Unity invokes this while the gameplay HUD is visible.<br/>
        /// Configuration/context: uses unscaled time so pause/menu time scaling does not freeze the fade.
        /// </summary>
        private void Update()
        {
            if (!hasActiveSource)
                return;

            float remaining = visibleUntilTime - Time.unscaledTime;
            if (remaining <= 0f)
            {
                hasActiveSource = false;
                visibleUntilTime = 0f;
                ApplyVisibility(0f);
                return;
            }

            Camera cameraToUse = targetCamera != null ? targetCamera : Camera.main;
            if (cameraToUse == null)
            {
                ApplyVisibility(0f);
                return;
            }

            Vector3 delta = sourceWorldPosition - cameraToUse.transform.position;
            Vector3 forward = cameraToUse.transform.forward;
            Vector3 right = cameraToUse.transform.right;

            if (flattenToHorizontalPlane)
            {
                delta.y = 0f;
                forward.y = 0f;
                right.y = 0f;
            }

            if (delta.sqrMagnitude <= 0.0001f || forward.sqrMagnitude <= 0.0001f || right.sqrMagnitude <= 0.0001f)
            {
                ApplyVisibility(0f);
                return;
            }

            forward.Normalize();
            right.Normalize();

            float x = Vector3.Dot(delta, right);
            float y = Vector3.Dot(delta, forward);
            float angle = Mathf.Atan2(x, y) * Mathf.Rad2Deg;
            indicatorRect.localRotation = Quaternion.Euler(0f, 0f, -angle);

            float fadeDuration = Mathf.Min(displayDurationSeconds, Mathf.Max(0f, fadeOutDurationSeconds));
            float alpha = fadeDuration <= 0f || remaining > fadeDuration
                ? 1f
                : Mathf.Clamp01(remaining / fadeDuration);
            ApplyVisibility(alpha);
        }

        /// <summary>
        /// Receives an incoming damage event and refreshes the arc direction when a source position is available.<br/>
        /// Typical usage: invoked by <see cref="IncomingDamageModel"/> whenever the local player takes damage.<br/>
        /// Configuration/context: default observable payloads and hits without source position are ignored.
        /// </summary>
        /// <param name="entry">The incoming damage event to visualize.</param>
        public void OnNotify(IncomingDamageEntry entry)
        {
            if (entry.DamageAmount <= 0 && entry.PreviousHealth == 0 && entry.CurrentHealth == 0 && !entry.HasSourceWorldPosition)
                return;

            if (!entry.HasSourceWorldPosition)
                return;

            sourceWorldPosition = entry.SourceWorldPosition;
            hasActiveSource = true;
            visibleUntilTime = Time.unscaledTime + Mathf.Max(0.1f, displayDurationSeconds);
            ApplyVisibility(1f);
        }

        /// <summary>
        /// Applies the current alpha and active state to the indicator.<br/>
        /// Typical usage: called during initialization, on new hits, and during fade updates.<br/>
        /// Configuration/context: updates <see cref="CanvasGroup"/> when present and otherwise toggles the indicator GameObject directly.
        /// </summary>
        /// <param name="alpha">Normalized alpha value between 0 and 1.</param>
        private void ApplyVisibility(float alpha)
        {
            alpha = Mathf.Clamp01(alpha);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            if (indicatorRect != null && indicatorRect.gameObject.activeSelf != (alpha > 0f))
                indicatorRect.gameObject.SetActive(alpha > 0f);
        }
    }
}
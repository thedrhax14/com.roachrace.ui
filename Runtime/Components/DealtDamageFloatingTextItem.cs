using TMPro;
using RoachRace.UI.Models;
using UnityEngine;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// Single pooled floating damage text item anchored to a world position projected into HUD space.<br/>
    /// Typical usage: managed by <see cref="DealtDamageFloatingTextView"/>, which reuses a small pool of these items for repeated hit feedback.<br/>
    /// Configuration/context: the item hides itself whenever the tracked world position is behind the camera, but it keeps updating so it can reappear if the camera turns before expiration.
    /// </summary>
    public sealed class DealtDamageFloatingTextItem : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private CanvasGroup canvasGroup;

        private Vector3 baseWorldPosition;
        private float spawnTime;
        private float expireAtTime;
        private float riseSpeedWorldUnits;

        /// <summary>
        /// Expiration timestamp for this pooled item in unscaled time.<br/>
        /// Typical usage: pooling views use this to determine the oldest active item when the pool is exhausted.
        /// </summary>
        public float ExpireAtTime => expireAtTime;

        /// <summary>
        /// Resolves optional UI references and hides the item until first use.<br/>
        /// Typical usage: Unity invokes this during prefab or pooled instance initialization.<br/>
        /// Configuration/context: if no explicit <see cref="RectTransform"/> is assigned, the component falls back to its own transform.
        /// </summary>
        private void Awake()
        {
            if (rectTransform == null)
                rectTransform = transform as RectTransform;

            if (canvasGroup == null)
                TryGetComponent(out canvasGroup);

            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
            }

            SetVisible(false, 0f);
        }

        /// <summary>
        /// Shows this pooled item for a single dealt-damage event.<br/>
        /// Typical usage: called by <see cref="DealtDamageFloatingTextView"/> whenever a new hit should display a floating number.<br/>
        /// Configuration/context: lifetime uses unscaled time so pause/menu time scaling does not affect the animation.
        /// </summary>
        /// <param name="entry">The dealt-damage event being displayed.</param>
        /// <param name="worldPosition">Starting world-space position for the floating text.</param>
        /// <param name="lifetimeSeconds">Total visible lifetime in seconds.</param>
        /// <param name="riseSpeedWorldUnits">World-space upward speed in units per second.</param>
        public void Show(DealtDamageEntry entry, Vector3 worldPosition, float lifetimeSeconds, float riseSpeedWorldUnits)
        {
            baseWorldPosition = worldPosition;
            spawnTime = Time.unscaledTime;
            expireAtTime = spawnTime + Mathf.Max(0.1f, lifetimeSeconds);
            this.riseSpeedWorldUnits = Mathf.Max(0f, riseSpeedWorldUnits);

            if (damageText != null)
            {
                string text = $"{entry.DamageAmount}";
                if (entry.IsFatal)
                    text = $"{text}!";

                damageText.text = text;
            }

            SetVisible(true, 1f);
        }

        /// <summary>
        /// Updates the pooled item position, dot-based visibility, and fade state for the current frame.<br/>
        /// Typical usage: called every frame by <see cref="DealtDamageFloatingTextView"/> while the item is active.<br/>
        /// Configuration/context: the item is hidden whenever its tracked world position is behind the camera according to the forward-vector dot test.
        /// </summary>
        /// <param name="targetCamera">Camera used to project the world position into screen space.</param>
        /// <param name="fadeOutDurationSeconds">How long the text fades at the end of its lifetime.</param>
        /// <returns><c>true</c> while the item remains alive; otherwise <c>false</c>.</returns>
        public bool Tick(Camera targetCamera, float fadeOutDurationSeconds)
        {
            float now = Time.unscaledTime;
            if (now >= expireAtTime)
            {
                SetVisible(false, 0f);
                return false;
            }

            if (targetCamera == null || rectTransform == null)
            {
                SetVisible(false, 0f);
                return true;
            }

            float elapsed = now - spawnTime;
            Vector3 worldPosition = baseWorldPosition + (Vector3.up * riseSpeedWorldUnits * elapsed);
            Vector3 toPoint = worldPosition - targetCamera.transform.position;
            if (toPoint.sqrMagnitude <= 0.0001f)
            {
                SetVisible(false, 0f);
                return true;
            }

            float dot = Vector3.Dot(targetCamera.transform.forward, toPoint.normalized);
            float remaining = expireAtTime - now;
            float fadeDuration = Mathf.Min(Mathf.Max(0f, fadeOutDurationSeconds), Mathf.Max(0.0001f, expireAtTime - spawnTime));
            float alpha = fadeDuration <= 0f || remaining > fadeDuration
                ? 1f
                : Mathf.Clamp01(remaining / fadeDuration);

            if (dot <= 0f)
            {
                SetVisible(false, 0f);
                return true;
            }

            rectTransform.position = targetCamera.WorldToScreenPoint(worldPosition);

            SetVisible(true, alpha);
            return true;
        }

        /// <summary>
        /// Applies visibility and alpha to the pooled item.<br/>
        /// Typical usage: called internally during show, tick, and recycle operations.<br/>
        /// Configuration/context: updates both <see cref="CanvasGroup"/> and the text alpha when available.
        /// </summary>
        /// <param name="visible">Whether the item should be visible.</param>
        /// <param name="alpha">Normalized alpha value to apply.</param>
        public void SetVisible(bool visible, float alpha)
        {
            alpha = Mathf.Clamp01(alpha);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? alpha : 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            if (damageText != null)
            {
                Color color = damageText.color;
                color.a = visible ? alpha : 0f;
                damageText.color = color;
            }

            if (gameObject.activeSelf != visible)
                gameObject.SetActive(visible);
        }
    }
}
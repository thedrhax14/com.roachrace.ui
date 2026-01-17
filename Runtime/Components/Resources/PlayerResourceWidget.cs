using RoachRace.Controls;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace RoachRace.UI.Components.Resources
{
    /// <summary>
    /// Renders a single PlayerResource (icon + optional fill + optional value text).
    /// 
    /// Prefab setup:
    /// - iconImage is optional
    /// - fillImage is optional (Image Type should be Filled)
    /// - valueText is optional
    /// </summary>
    public sealed class PlayerResourceWidget : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image fillImage;
        [SerializeField] private TMP_Text valueText;

        [Header("Animation")]
        [Tooltip("Duration (seconds) for value/fill to move linearly to the new target.")]
        [SerializeField, Min(0f)] private float animationDuration = 0.2f;
        [SerializeField, Min(0f)] private float animationDelay = 1f;

        [Tooltip("If true, when first bound the widget animates from 0 to the current value.")]
        [SerializeField] private bool animateFromZeroOnBind = true;

        private PlayerResource _resource;
        private Coroutine _animateRoutine;
        private float _displayedValue;
        private float _displayedFill;

        public void Bind(PlayerResource resource)
        {
            Unbind();

            _resource = resource;
            if (_resource != null)
                _resource.Changed += OnResourceChanged;

            // Apply static UI immediately.
            ApplyStaticUI();

            // Animate from 0 when first bound.
            float startValue = animateFromZeroOnBind ? 0f : (_resource != null ? _resource.Current : 0f);
            float startFill = animateFromZeroOnBind ? 0f : (_resource != null ? _resource.Normalized : 0f);
            _displayedValue = startValue;
            _displayedFill = startFill;
            ApplyDynamicUI(_displayedValue, _displayedFill);
            AnimateToTarget(applyDelay: true);
        }

        public void Unbind()
        {
            StopAnimation();
            if (_resource != null)
                _resource.Changed -= OnResourceChanged;
            _resource = null;
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void OnResourceChanged(PlayerResource _)
        {
            ApplyStaticUI();
            // Resource changes should animate immediately (delay is only for initial bind).
            AnimateToTarget(applyDelay: false);
        }

        private void StopAnimation()
        {
            if (_animateRoutine == null) return;
            StopCoroutine(_animateRoutine);
            _animateRoutine = null;
        }

        private void ApplyStaticUI()
        {
            if (_resource == null) return;

            if (iconImage != null)
            {
                iconImage.sprite = _resource.Icon;
                iconImage.enabled = _resource.Icon != null;
                iconImage.color = _resource.Color;
            }

            if (fillImage != null)
            {
                fillImage.color = _resource.Color;
            }
        }

        private void ApplyDynamicUI(float value, float fill)
        {
            if (fillImage != null)
                fillImage.fillAmount = fill;

            if (valueText != null)
                valueText.text = Mathf.CeilToInt(value).ToString();
        }

        private void AnimateToTarget(bool applyDelay)
        {
            if (_resource == null) return;

            float targetValue = _resource.Current;
            float targetFill = _resource.Normalized;

            StopAnimation();

            // If animation is disabled/instant, snap.
            if (animationDuration <= 0f)
            {
                _displayedValue = targetValue;
                _displayedFill = targetFill;
                ApplyDynamicUI(_displayedValue, _displayedFill);
                return;
            }

            _animateRoutine = StartCoroutine(AnimateRoutine(_displayedValue, targetValue, _displayedFill, targetFill, animationDuration, applyDelay));
        }

        private IEnumerator AnimateRoutine(float fromValue, float toValue, float fromFill, float toFill, float duration, bool applyDelay)
        {
            float t = 0f;
            if (applyDelay && animationDelay > 0f)
                yield return new WaitForSeconds(animationDelay);
            while (t < duration)
            {
                t += Time.deltaTime;
                float a = duration <= 0f ? 1f : Mathf.Clamp01(t / duration);

                _displayedValue = Mathf.Lerp(fromValue, toValue, a);
                _displayedFill = Mathf.Lerp(fromFill, toFill, a);
                ApplyDynamicUI(_displayedValue, _displayedFill);

                yield return null;
            }

            _displayedValue = toValue;
            _displayedFill = toFill;
            ApplyDynamicUI(_displayedValue, _displayedFill);
            _animateRoutine = null;
        }
    }
}

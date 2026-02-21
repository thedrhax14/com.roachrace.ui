using System.Collections;
using RoachRace.Data;
using TMPro;
using UnityEngine;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// Visual representation of a floating damage number that appears briefly and fades out.
    /// Tracks world position to stay anchored to damage location in 3D space.
    /// </summary>
    public class FloatingDamagePopup : MonoBehaviour
    {
        [Header("Text Component")]
        [SerializeField] private TextMeshProUGUI damageText;

        [Header("Animation Settings")]
        [SerializeField] private float duration = 1.5f;
        [SerializeField] private float floatDistance = 1.0f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.8f);

        [Header("Color")]
        [SerializeField] private Color normalDamageColor = Color.white;
        [SerializeField] private Color criticalDamageColor = Color.red;
        [SerializeField] private Gradient colorOverTime = new Gradient();

        private Vector3 _worldPosition;
        private Vector3 _startScreenPosition;
        private CanvasGroup _canvasGroup;
        private float _elapsedTime;

        private void Start()
        {
            _startScreenPosition = transform.position;
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            StartCoroutine(AnimatePopup());
        }

        private void Update()
        {
            // Update screen position based on world position (camera movement)
            if (Camera.main != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(_worldPosition);
                transform.position = screenPos;
            }
        }

        public void Initialize(DamageEventData damageEvent)
        {
            // Store world position for continuous tracking
            _worldPosition = damageEvent.DamagePosition;
            
            // Set damage text
            damageText.text = damageEvent.DamageInfo.Amount.ToString();

            // Color based on damage type or amount
            bool isCritical = damageEvent.DamageInfo.Amount >= 25; // Arbitrary threshold
            damageText.color = isCritical ? criticalDamageColor : normalDamageColor;
        }

        private IEnumerator AnimatePopup()
        {
            _elapsedTime = 0f;

            while (_elapsedTime < duration)
            {
                _elapsedTime += Time.deltaTime;
                float t = _elapsedTime / duration;

                // Fade out
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = fadeCurve.Evaluate(t);
                }

                // Float upward (offset in screen space)
                Vector3 screenOffset = Vector3.up * floatDistance * 100f * t; // Scale for screen space
                transform.position = _startScreenPosition + screenOffset;

                // Scale down
                float scale = scaleCurve.Evaluate(t);
                transform.localScale = Vector3.one * scale;

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}

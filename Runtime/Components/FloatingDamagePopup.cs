using System.Collections;
using RoachRace.Data;
using TMPro;
using UnityEngine;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// Visual representation of a floating damage number that appears briefly and fades out.
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

        private Vector3 _startPosition;
        private CanvasGroup _canvasGroup;
        private float _elapsedTime;

        private void Start()
        {
            _startPosition = transform.position;
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            StartCoroutine(AnimatePopup());
        }

        public void Initialize(DamageEventData damageEvent)
        {
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

                // Float upward
                Vector3 newPos = _startPosition + Vector3.up * floatDistance * t;
                transform.position = newPos;

                // Scale down
                float scale = scaleCurve.Evaluate(t);
                transform.localScale = Vector3.one * scale;

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}

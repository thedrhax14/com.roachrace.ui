using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoachRace.UI.Components.Radar
{
    /// <summary>
    /// One blip/icon on the radar UI.
    /// </summary>
    public sealed class RadarBlipUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI distanceText;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform == null)
            {
                Debug.LogError($"[{nameof(RadarBlipUI)}] RectTransform missing!", gameObject);
                throw new System.NullReferenceException($"[{nameof(RadarBlipUI)}] RectTransform is missing on '{gameObject.name}'.");
            }
        }

        public void ApplyDefinition(RadarPointOfInterestDefinition definition)
        {
            if (definition == null)
            {
                Debug.LogError($"[{nameof(RadarBlipUI)}] Definition is null.", gameObject);
                return;
            }

            if (iconImage != null)
            {
                iconImage.sprite = definition.Icon;
                iconImage.color = definition.Color;
                iconImage.enabled = definition.Icon != null;
            }
        }

        public void SetAnchoredPosition(Vector2 anchoredPosition)
        {
            _rectTransform.anchoredPosition = anchoredPosition;
        }

        public void SetDistanceVisible(bool visible)
        {
            if (distanceText == null) return;
            distanceText.gameObject.SetActive(visible);
        }

        public void SetDistanceMeters(float meters)
        {
            if (distanceText == null) return;
            distanceText.text = $"{Mathf.RoundToInt(meters)}m";
        }
    }
}

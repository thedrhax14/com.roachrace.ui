using System.Collections.Generic;
using UnityEngine;

namespace RoachRace.UI.Components.Radar
{
    /// <summary>
    /// Circular radar UI.
    /// 
    /// - Place this under the Game HUD (e.g., GameWindow) and anchor it top-left.
    /// - Add RadarPointOfInterest components in the scene.
    /// </summary>
    public sealed class RadarWidget : MonoBehaviour
    {
        [Header("UI")]
        [Tooltip("The RectTransform representing the radar circle area.")]
        [SerializeField] private RectTransform radarRect;

        [Tooltip("Parent container for blips. If not set, uses radarRect.")]
        [SerializeField] private RectTransform blipContainer;

        [Tooltip("Prefab for a radar blip (Image + optional distance TMP text).")]
        [SerializeField] private RadarBlipUI blipPrefab;

        [Header("Radar Settings")]
        [Min(0.1f)]
        [SerializeField] private float rangeMeters = 30f;

        [Tooltip("Padding inside the radar circle so icons don't clip.")]
        [Min(0f)]
        [SerializeField] private float edgePaddingPixels = 6f;

        [Tooltip("If true, radar rotates with camera yaw (camera forward = up on radar).")]
        [SerializeField] private bool rotateWithCamera = true;

        [Header("Target")]
        [Tooltip("Optional. If not assigned, uses Camera.main.")]
        [SerializeField] private Camera targetCamera;

        private readonly Dictionary<RadarPointOfInterest, RadarBlipUI> _blips = new();

        private void OnEnable()
        {
            ValidateDependencies();

            RadarPointOfInterestRegistry.Added += OnPoiAdded;
            RadarPointOfInterestRegistry.Removed += OnPoiRemoved;

            // Prime with already-active POIs.
            foreach (var poi in RadarPointOfInterestRegistry.Active)
                OnPoiAdded(poi);
        }

        private void OnDisable()
        {
            RadarPointOfInterestRegistry.Added -= OnPoiAdded;
            RadarPointOfInterestRegistry.Removed -= OnPoiRemoved;

            foreach (var kvp in _blips)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value.gameObject);
            }
            _blips.Clear();
        }

        void Update()
        {
            float radiusPixels = GetRadarRadiusPixels();
            Vector3 camPos = targetCamera.transform.position;

            Vector3 forward = targetCamera.transform.forward;
            Vector3 right = targetCamera.transform.right;
            forward.y = 0f;
            right.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
            if (right.sqrMagnitude < 0.0001f) right = Vector3.right;
            forward.Normalize();
            right.Normalize();

            foreach (var kvp in _blips)
            {
                RadarPointOfInterest poi = kvp.Key;
                RadarBlipUI blip = kvp.Value;
                if (poi == null || blip == null)
                    continue;

                Vector3 delta = poi.WorldPosition - camPos;
                delta.y = 0f;

                float distance = delta.magnitude;

                Vector2 local;
                if (rotateWithCamera)
                {
                    // Camera-relative: forward = up (Y), right = X
                    float x = Vector3.Dot(delta, right);
                    float y = Vector3.Dot(delta, forward);
                    local = new Vector2(x, y);
                }
                else
                {
                    // World-relative: +X right, +Z up
                    local = new Vector2(delta.x, delta.z);
                }

                bool inRange = distance <= rangeMeters;

                // Map meters -> pixels
                Vector2 normalized = local / Mathf.Max(0.0001f, rangeMeters);
                if (normalized.sqrMagnitude > 1f)
                    normalized = normalized.normalized;

                blip.SetAnchoredPosition(normalized * radiusPixels);

                // Distance text rule: only show if outside radar covering area.
                var def = poi.Definition;
                bool shouldShowDistance = !inRange && def != null && def.ShowDistanceWhenOutsideRange;
                blip.SetDistanceVisible(shouldShowDistance);
                if (shouldShowDistance)
                    blip.SetDistanceMeters(distance);
            }
        }

        private float GetRadarRadiusPixels()
        {
            float diameter = 0f;
            if (radarRect != null)
                diameter = Mathf.Min(radarRect.rect.width, radarRect.rect.height);

            float radius = Mathf.Max(0f, (diameter * 0.5f) - edgePaddingPixels);
            return radius;
        }

        private void OnPoiAdded(RadarPointOfInterest poi)
        {
            if (poi == null) return;
            if (_blips.ContainsKey(poi)) return;

            RadarBlipUI blip = Instantiate(blipPrefab, blipContainer != null ? blipContainer : radarRect);
            blip.ApplyDefinition(poi.Definition);
            blip.SetDistanceVisible(false);

            _blips.Add(poi, blip);
        }

        private void OnPoiRemoved(RadarPointOfInterest poi)
        {
            if (poi == null) return;
            if (!_blips.TryGetValue(poi, out RadarBlipUI blip)) return;

            if (blip != null)
                Destroy(blip.gameObject);

            _blips.Remove(poi);
        }

        private void ValidateDependencies()
        {
            if (radarRect == null)
            {
                Debug.LogError($"[{nameof(RadarWidget)}] RadarRect is not assigned!", gameObject);
                throw new System.NullReferenceException($"[{nameof(RadarWidget)}] radarRect is null on GameObject '{gameObject.name}'.");
            }

            if (blipPrefab == null)
            {
                Debug.LogError($"[{nameof(RadarWidget)}] BlipPrefab is not assigned!", gameObject);
                throw new System.NullReferenceException($"[{nameof(RadarWidget)}] blipPrefab is null on GameObject '{gameObject.name}'.");
            }
        }
    }
}

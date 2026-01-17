using UnityEngine;

namespace RoachRace.UI.Components.Radar
{
    /// <summary>
    /// World-space marker for the radar.
    /// Add this component to scene objects which should appear on the radar.
    /// </summary>
    public sealed class RadarPointOfInterest : MonoBehaviour
    {
        [SerializeField] private RadarPointOfInterestDefinition definition;

        [Tooltip("Optional. If assigned, the marker will track this transform instead of this GameObject's transform.")]
        [SerializeField] private Transform targetTransform;

        [Tooltip("Optional. Adds an offset to the tracked world position.")]
        [SerializeField] private Vector3 worldOffset;

        public RadarPointOfInterestDefinition Definition => definition;

        public Vector3 WorldPosition
        {
            get
            {
                Transform t = targetTransform != null ? targetTransform : transform;
                return t.position + worldOffset;
            }
        }

        private void OnEnable()
        {
            ValidateDefinitionAssigned();
            RadarPointOfInterestRegistry.Register(this);
        }

        private void OnDisable()
        {
            RadarPointOfInterestRegistry.Unregister(this);
        }

        private void ValidateDefinitionAssigned()
        {
            if (definition != null) return;

            Debug.LogError($"[{nameof(RadarPointOfInterest)}] Definition is not assigned! Please assign a {nameof(RadarPointOfInterestDefinition)} in the Inspector.", gameObject);
            throw new System.NullReferenceException(
                $"[{nameof(RadarPointOfInterest)}] Definition is null on GameObject '{gameObject.name}'. This component requires a RadarPointOfInterestDefinition to function.");
        }

        private void OnValidate()
        {
            if (targetTransform == transform)
                targetTransform = null;
        }
    }
}

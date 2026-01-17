using UnityEngine;

namespace RoachRace.UI.Components.Radar
{
    [CreateAssetMenu(fileName = "RadarPOI", menuName = "RoachRace/UI/Radar/Point Of Interest Definition")]
    public sealed class RadarPointOfInterestDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string displayName;

        [Header("UI")]
        [SerializeField] private Sprite icon;
        [SerializeField] private Color color = Color.white;

        [Header("Behavior")]
        [Tooltip("If true, show distance text when the POI is outside radar range.")]
        [SerializeField] private bool showDistanceWhenOutsideRange = true;

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public Sprite Icon => icon;
        public Color Color => color;
        public bool ShowDistanceWhenOutsideRange => showDistanceWhenOutsideRange;

        private void OnValidate()
        {
            if (displayName != null)
                displayName = displayName.Trim();
        }
    }
}

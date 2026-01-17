using System;
using System.Collections.Generic;

namespace RoachRace.UI.Components.Radar
{
    /// <summary>
    /// Global registry for active radar POIs.
    /// Uses OnEnable/OnDisable registration so it naturally tracks spawned/despawned or enabled/disabled POIs.
    /// </summary>
    public static class RadarPointOfInterestRegistry
    {
        private static readonly HashSet<RadarPointOfInterest> _active = new();

        public static event Action<RadarPointOfInterest> Added;
        public static event Action<RadarPointOfInterest> Removed;

        public static IReadOnlyCollection<RadarPointOfInterest> Active => _active;

        public static void Register(RadarPointOfInterest poi)
        {
            if (poi == null) return;
            if (!_active.Add(poi)) return;
            Added?.Invoke(poi);
        }

        public static void Unregister(RadarPointOfInterest poi)
        {
            if (poi == null) return;
            if (!_active.Remove(poi)) return;
            Removed?.Invoke(poi);
        }
    }
}

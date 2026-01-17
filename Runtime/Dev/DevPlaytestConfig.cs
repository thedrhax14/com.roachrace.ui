using RoachRace.Data;
using UnityEngine;

namespace RoachRace.UI.Dev
{
    /// <summary>
    /// Editor play-mode bootstrap configuration.
    /// 
    /// Intended usage:
    /// - Create one asset and reference it from a DevPlaytestBootstrap in your startup scene.
    /// - Choose a mode to skip UI clicks and start in lobby or in-progress with a desired team.
    /// </summary>
    [CreateAssetMenu(fileName = "DevPlaytestConfig", menuName = "RoachRace/Dev/Playtest Config")]
    public sealed class DevPlaytestConfig : ScriptableObject
    {
        public enum PlaytestMode
        {
            Disabled,
            HostLobby,
            HostInProgressAsSurvivor,
            HostInProgressAsGhost
        }

        [Header("Mode")]
        [SerializeField] private PlaytestMode mode = PlaytestMode.Disabled;

        [Header("Local Host")]
        [SerializeField] private bool useLocalHost = true;
        [SerializeField] private ushort localHostPort = 7777;

        [Header("Game Start")]
        [Tooltip("If true, skip the normal countdown and start immediately (editor only, best-effort).")]
        [SerializeField] private bool skipCountdown = true;

        public PlaytestMode Mode => mode;
        public bool UseLocalHost => useLocalHost;
        public ushort LocalHostPort => localHostPort;
        public bool SkipCountdown => skipCountdown;

        public Team DesiredTeam
        {
            get
            {
                return mode switch
                {
                    PlaytestMode.HostInProgressAsGhost => Team.Ghost,
                    PlaytestMode.HostInProgressAsSurvivor => Team.Survivor,
                    _ => Team.Survivor
                };
            }
        }

        public bool ShouldStartInProgress => mode == PlaytestMode.HostInProgressAsGhost || mode == PlaytestMode.HostInProgressAsSurvivor;
    }
}

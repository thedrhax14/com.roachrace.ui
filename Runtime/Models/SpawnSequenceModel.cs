using RoachRace.UI.Core;
using UnityEngine;

namespace RoachRace.UI.Models
{
    /// <summary>
    /// ScriptableObject UI model representing the match start "intro → spawn" sequence runtime state.<br></br>
    /// Purpose: provide a single observable source of truth for presentation systems (Timeline flyover, spawn pod UI) and
    /// for networking bridges to synchronize server-driven phases to clients.<br></br>
    /// Typical usage:<br></br>
    /// - Server/networking sets the phase and (optionally) the UTC start timestamp for the intro.<br></br>
    /// - Clients observe <see cref="Phase"/> and start/stop a looping Timeline accordingly.<br></br>
    /// - When intro is disabled by lobby game settings, the server can set phase directly to <see cref="SpawnSequencePhase.ControllersSpawned"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "SpawnSequenceModel", menuName = "RoachRace/UI/Spawn Sequence Model")]
    public sealed class SpawnSequenceModel : UIModel
    {
        /// <summary>
        /// High-level phases for the match start sequence.<br></br>
        /// Notes:<br></br>
        /// - This model does not prescribe server logic; it only communicates state.<br></br>
        /// - "Cinematic" is the looping flyover Timeline phase before pods/controllers appear.
        /// </summary>
        public enum SpawnSequencePhase
        {
            /// <summary>
            /// No active sequence / reset state.
            /// </summary>
            None = 0,

            /// <summary>
            /// Match started, waiting for the first MapGen chunk to be generated.
            /// </summary>
            WaitingForFirstChunk = 1,

            /// <summary>
            /// Intro cinematic is active (Timeline flyover loop).
            /// </summary>
            Cinematic = 2,

            /// <summary>
            /// Spawn pods exist and are playing their team animations.
            /// </summary>
            PodsActive = 3,

            /// <summary>
            /// Player controllers are spawned and the sequence is complete.
            /// </summary>
            ControllersSpawned = 4
        }

        [Header("Runtime")]
        [Tooltip("Current phase for the match start spawn sequence.")]
        public Observable<SpawnSequencePhase> Phase { get; } = new(SpawnSequencePhase.None);

        [Tooltip("Whether the intro cinematic is enabled for this match (as applied on match start).")]
        public Observable<bool> IntroEnabled { get; } = new(true);

        [Tooltip("Configured intro duration in seconds (as applied on match start).")]
        public Observable<float> IntroDurationSeconds { get; } = new(1f);

        [Tooltip("UTC timestamp in milliseconds when the intro cinematic started (0 means unset/unknown).")]
        public Observable<long> IntroStartTimestampMsUtc { get; } = new(0);

        /// <summary>
        /// Sets the current phase and notifies observers.<br></br>
        /// Typical usage: called by a networking bridge when the server advances the sequence.<br></br>
        /// </summary>
        /// <param name="phase">New phase.</param>
        public void SetPhase(SpawnSequencePhase phase)
        {
            Phase.Value = phase;
        }

        /// <summary>
        /// Applies intro configuration values (enabled + duration) and notifies observers.<br></br>
        /// Typical usage: called once on match start after server has resolved final settings from lobby configuration.
        /// </summary>
        /// <param name="enabled">Whether intro is enabled for this match.</param>
        /// <param name="durationSeconds">Intro duration in seconds. Values &lt;= 0 will be clamped to 0.</param>
        public void ApplyIntroConfig(bool enabled, float durationSeconds)
        {
            IntroEnabled.Value = enabled;
            IntroDurationSeconds.Value = Mathf.Max(0f, durationSeconds);
        }

        /// <summary>
        /// Sets the UTC timestamp (milliseconds) at which the cinematic started and notifies observers.<br></br>
        /// Typical usage: server sets this once when transitioning into <see cref="SpawnSequencePhase.Cinematic"/> so clients can
        /// align timers without relying on local Time.time.
        /// </summary>
        /// <param name="timestampMsUtc">UTC start timestamp in ms, or 0 to clear.</param>
        public void SetIntroStartTimestampMsUtc(long timestampMsUtc)
        {
            IntroStartTimestampMsUtc.Value = timestampMsUtc < 0 ? 0 : timestampMsUtc;
        }

        /// <summary>
        /// Resets the model back to defaults for returning to lobby or restarting a match.<br></br>
        /// Typical usage: called by menu/game state cleanup code when leaving gameplay.
        /// </summary>
        public void ResetToDefaults()
        {
            Phase.Value = SpawnSequencePhase.None;
            IntroEnabled.Value = true;
            IntroDurationSeconds.Value = 10f;
            IntroStartTimestampMsUtc.Value = 0;
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            // Ensure stable defaults when the ScriptableObject is enabled.
            ResetToDefaults();
        }
    }
}

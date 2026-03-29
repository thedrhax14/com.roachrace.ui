using UnityEngine;

namespace RoachRace.UI.Models
{
    /// <summary>
    /// Event-style UI payload describing one dealt-damage instance for the local player.<br/>
    /// Typical usage: networking bridges publish this into <see cref="DealtDamageModel"/>, and UI components consume it for hit markers, damage dealt popups, or confirmation widgets.<br/>
    /// Configuration/context: target identifiers may be -1 when damage was applied to an unowned or unknown target.
    /// </summary>
    public readonly struct DealtDamageEntry
    {
        /// <summary>
        /// Creates a UI payload for one dealt-damage notification.<br/>
        /// Typical usage: owner-local networking bridges construct this immediately before publishing to <see cref="DealtDamageModel"/>.<br/>
        /// Configuration/context: <paramref name="damageAmount"/> should be a positive magnitude.
        /// </summary>
        /// <param name="damageAmount">Positive magnitude of the applied damage.</param>
        /// <param name="weaponIconKey">Optional UI-facing weapon/effect key used for attribution. Empty when not applicable.</param>
        /// <param name="targetConnectionId">ClientId of the damaged target owner, or -1 when unknown/non-player.</param>
        /// <param name="targetObjectId">NetworkObjectId of the damaged target, or -1 when unknown.</param>
        /// <param name="targetHealthAfterHit">Resolved target health after the hit was applied.</param>
        /// <param name="targetWorldPosition">World-space position of the damaged target when the hit was applied.</param>
        /// <param name="isFatal">Whether the hit reduced the target to zero or below.</param>
        public DealtDamageEntry(int damageAmount, string weaponIconKey, int targetConnectionId, int targetObjectId, int targetHealthAfterHit, Vector3 targetWorldPosition, bool isFatal)
        {
            DamageAmount = damageAmount;
            WeaponIconKey = weaponIconKey;
            TargetConnectionId = targetConnectionId;
            TargetObjectId = targetObjectId;
            TargetHealthAfterHit = targetHealthAfterHit;
            TargetWorldPosition = targetWorldPosition;
            IsFatal = isFatal;
        }

        /// <summary>
        /// Positive magnitude of the applied damage.<br/>
        /// Typical usage: drive attacker hit markers or damage dealt text.
        /// </summary>
        public int DamageAmount { get; }

        /// <summary>
        /// Optional UI-facing weapon/effect attribution key.<br/>
        /// Typical usage: show which source produced the hit when multiple sources are possible.
        /// </summary>
        public string WeaponIconKey { get; }

        /// <summary>
        /// ClientId of the damaged target owner.<br/>
        /// Typical usage: correlate a hit with player/team registries if local UI wants to label the victim.
        /// </summary>
        public int TargetConnectionId { get; }

        /// <summary>
        /// NetworkObjectId of the damaged target.<br/>
        /// Typical usage: correlate a hit with a tracked target object if needed.
        /// </summary>
        public int TargetObjectId { get; }

        /// <summary>
        /// Resolved target health after the hit was applied.<br/>
        /// Typical usage: drive threshold-based attacker confirmation effects.
        /// </summary>
        public int TargetHealthAfterHit { get; }

        /// <summary>
        /// World-space position of the damaged target when the hit was applied.<br/>
        /// Typical usage: floating damage text can anchor and drift upward from this position.
        /// </summary>
        public Vector3 TargetWorldPosition { get; }

        /// <summary>
        /// Whether the hit reduced the target to zero or below.<br/>
        /// Typical usage: trigger stronger confirmation feedback for kills/final blows.
        /// </summary>
        public bool IsFatal { get; }
    }
}
namespace RoachRace.UI.Models
{
    /// <summary>
    /// Event-style UI payload describing one incoming damage instance for the local player.<br/>
    /// Typical usage: networking bridges publish this into <see cref="IncomingDamageModel"/>, and UI components consume it for hit indicators, directional damage widgets, or floating number systems.<br/>
    /// Configuration/context: instigator identifiers may be -1 when damage came from the environment or an unknown source.
    /// </summary>
    public readonly struct IncomingDamageEntry
    {
        /// <summary>
        /// Creates a UI payload for one incoming damage notification.<br/>
        /// Typical usage: owner-local networking bridges construct this immediately before publishing to <see cref="IncomingDamageModel"/>.<br/>
        /// Configuration/context: <paramref name="damageAmount"/> should be a positive magnitude.
        /// </summary>
        /// <param name="previousHealth">Health total before the damage was applied.</param>
        /// <param name="currentHealth">Health total after the damage was applied.</param>
        /// <param name="damageAmount">Positive magnitude of the applied damage.</param>
        /// <param name="weaponIconKey">Optional UI-facing weapon/effect key used for attribution. Empty when not applicable.</param>
        /// <param name="instigatorConnectionId">ClientId of the instigator connection, or -1 for environment/unknown.</param>
        /// <param name="instigatorObjectId">NetworkObjectId of the instigator object, or -1 for environment/unknown.</param>
        public IncomingDamageEntry(int previousHealth, int currentHealth, int damageAmount, string weaponIconKey, int instigatorConnectionId, int instigatorObjectId)
        {
            PreviousHealth = previousHealth;
            CurrentHealth = currentHealth;
            DamageAmount = damageAmount;
            WeaponIconKey = weaponIconKey;
            InstigatorConnectionId = instigatorConnectionId;
            InstigatorObjectId = instigatorObjectId;
        }

        /// <summary>
        /// Health total before damage was applied.<br/>
        /// Typical usage: compare against <see cref="CurrentHealth"/> for local animation thresholds or interpolation.
        /// </summary>
        public int PreviousHealth { get; }

        /// <summary>
        /// Health total after damage was applied.<br/>
        /// Typical usage: determine whether the hit was fatal or whether a low-health effect should start.
        /// </summary>
        public int CurrentHealth { get; }

        /// <summary>
        /// Positive magnitude of the applied damage.<br/>
        /// Typical usage: drive hit markers, damage-number magnitude, or damage accumulation widgets.
        /// </summary>
        public int DamageAmount { get; }

        /// <summary>
        /// Optional UI-facing weapon/effect attribution key.<br/>
        /// Typical usage: map into an icon or effect label in owner-side UI.
        /// </summary>
        public string WeaponIconKey { get; }

        /// <summary>
        /// ClientId of the instigator connection responsible for the hit.<br/>
        /// Typical usage: later owner-side systems can compare this against local/team registries if they need attacker identity.
        /// </summary>
        public int InstigatorConnectionId { get; }

        /// <summary>
        /// NetworkObjectId of the instigator object responsible for the hit.<br/>
        /// Typical usage: correlate incoming damage with a tracked projectile, trap, or actor if such a registry exists client-side.
        /// </summary>
        public int InstigatorObjectId { get; }

        /// <summary>
        /// Whether the hit reduced health to zero or below.<br/>
        /// Typical usage: distinguish generic hit feedback from fatal-hit UI behavior.
        /// </summary>
        public bool IsFatal => CurrentHealth <= 0;
    }
}
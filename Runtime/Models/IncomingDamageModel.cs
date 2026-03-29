using RoachRace.UI.Core;
using UnityEngine;

namespace RoachRace.UI.Models
{
    /// <summary>
    /// Publishes incoming damage notifications for owner-local UI. This model does not store history; it emits the latest event only.<br/>
    /// Typical usage: a networking bridge publishes authoritative incoming damage, and HUD/presentation components observe <see cref="LatestEntry"/> for transient reactions.<br/>
    /// Configuration/context: intended for the local player only; remote players should not write to or read from this model for gameplay decisions.
    /// </summary>
    [CreateAssetMenu(fileName = "IncomingDamageModel", menuName = "RoachRace/Models/Incoming Damage Model")]
    public sealed class IncomingDamageModel : UIModel
    {
        /// <summary>
        /// Latest incoming damage event published to the local UI.<br/>
        /// Typical usage: UI components attach to this observable and react whenever <see cref="Publish"/> is called.
        /// </summary>
        public readonly Observable<IncomingDamageEntry> LatestEntry = new Observable<IncomingDamageEntry>(default);

        /// <summary>
        /// Publishes a new incoming damage event to attached owner-local UI observers.<br/>
        /// Typical usage: networking bridges call this after receiving authoritative owner-only damage feedback from the server.<br/>
        /// Configuration/context: event-like behavior is intentional, so the payload is pushed via <see cref="Observable{T}.Notify(T)"/> rather than stored as durable state.
        /// </summary>
        /// <param name="entry">The incoming damage event to publish.</param>
        public void Publish(IncomingDamageEntry entry)
        {
            LatestEntry.Notify(entry);
        }
    }
}
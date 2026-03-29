using RoachRace.UI.Core;
using UnityEngine;

namespace RoachRace.UI.Models
{
    /// <summary>
    /// Publishes dealt-damage notifications for owner-local UI. This model does not store history; it emits the latest event only.<br/>
    /// Typical usage: a networking bridge publishes authoritative dealt-damage events, and HUD/presentation components observe <see cref="LatestEntry"/> for transient reactions.<br/>
    /// Configuration/context: intended for the local attacking player only; remote players should not write to or read from this model for gameplay decisions.
    /// </summary>
    [CreateAssetMenu(fileName = "DealtDamageModel", menuName = "RoachRace/Models/Dealt Damage Model")]
    public sealed class DealtDamageModel : UIModel
    {
        /// <summary>
        /// Latest dealt-damage event published to the local UI.<br/>
        /// Typical usage: UI components attach to this observable and react whenever <see cref="Publish"/> is called.
        /// </summary>
        public readonly Observable<DealtDamageEntry> LatestEntry = new Observable<DealtDamageEntry>(default);

        /// <summary>
        /// Publishes a new dealt-damage event to attached owner-local UI observers.<br/>
        /// Typical usage: networking bridges call this after receiving authoritative owner-only dealt-damage feedback from the server.<br/>
        /// Configuration/context: event-like behavior is intentional, so the payload is pushed via <see cref="Observable{T}.Notify(T)"/> rather than stored as durable state.
        /// </summary>
        /// <param name="entry">The dealt-damage event to publish.</param>
        public void Publish(DealtDamageEntry entry)
        {
            LatestEntry.Notify(entry);
        }
    }
}
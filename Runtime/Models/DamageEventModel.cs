using UnityEngine;
using RoachRace.Data;
using RoachRace.UI.Core;

namespace RoachRace.UI.Models
{
    /// <summary>
    /// Publishes damage events for UI visualization (floating damage numbers, etc).
    /// This model does not store history; it only emits events as they occur.
    /// UI components observe this to react to damage events.
    /// </summary>
    [CreateAssetMenu(fileName = "DamageEventModel", menuName = "RoachRace/Models/Damage Event Model")]
    public class DamageEventModel : UIModel
    {
        /// <summary>
        /// Observable that publishes damage events. UI components attach to this.
        /// </summary>
        public readonly Observable<DamageEventData> OnDamageEvent = new Observable<DamageEventData>(default);

        /// <summary>
        /// Called when damage occurs. Publishes the event to all observers.
        /// </summary>
        /// <param name="damageEvent">The damage event data to publish</param>
        public void PublishDamageEvent(DamageEventData damageEvent)
        {
            OnDamageEvent.Notify(damageEvent);
        }
    }
}

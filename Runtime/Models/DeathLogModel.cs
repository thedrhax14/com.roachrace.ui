using UnityEngine;
using RoachRace.Data;
using RoachRace.UI.Core;

namespace RoachRace.UI.Models
{
    /// <summary>
    /// Publishes death log entries for UI. This model does not store history; it only emits events.
    /// </summary>
    [CreateAssetMenu(fileName = "DeathLogModel", menuName = "RoachRace/Models/Death Log Model")]
    public class DeathLogModel : UIModel
    {
        public readonly Observable<DeathLogEntry> LatestEntry = new Observable<DeathLogEntry>(default);

        public void Publish(DeathLogEntry entry)
        {
            LatestEntry.Notify(entry);
        }
    }
}

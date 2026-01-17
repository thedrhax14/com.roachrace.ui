using RoachRace.UI.Core;
using UnityEngine;

namespace RoachRace.UI.Models
{
    [CreateAssetMenu(fileName = "PlayerStatsModel", menuName = "RoachRace/Models/PlayerStatsModel")]
    public class PlayerStatsModel : UIModel
    {
        public readonly Observable<int> Health = new Observable<int>();
        public readonly Observable<int> MaxHealth = new Observable<int>();
        public readonly Observable<float> Stamina = new Observable<float>();
        public readonly Observable<float> MaxStamina = new Observable<float>();

        public void SetHealth(int current, int max)
        {
            Health.Value = current;
            MaxHealth.Value = max;
        }

        public void SetStamina(float current, float max)
        {
            Stamina.Value = current;
            MaxStamina.Value = max;
        }
    }
}

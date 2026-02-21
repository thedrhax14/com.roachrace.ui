using RoachRace.Data;
using RoachRace.UI.Core;
using UnityEngine;
using FishNet;
using RoachRace.UI.Models;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// Observes damage events and spawns floating damage numbers.
    /// Only displays damage dealt by the local player to reduce visual clutter.
    /// </summary>
    public class FloatingDamagePopupSpawner : MonoBehaviour, IObserver<DamageEventData>
    {
        [Header("Dependencies")]
        [SerializeField] private DamageEventModel damageEventModel;

        [Header("Prefab")]
        [SerializeField] private FloatingDamagePopup damagePopupPrefab;

        [Header("Settings")]
        [Tooltip("If true, show damage from all players. If false, only show damage dealt by local player.")]
        [SerializeField] private bool showAllDamage = false;

        private int _localPlayerConnectionId = -1;

        private void Start()
        {
            ValidateDependencies();
            CacheLocalPlayerConnectionId();
            
            if (damageEventModel != null)
            {
                damageEventModel.OnDamageEvent.Attach(this);
            }
        }

        private void OnDestroy()
        {
            if (damageEventModel != null)
            {
                damageEventModel.OnDamageEvent.Detach(this);
            }
        }

        private void ValidateDependencies()
        {
            if (damageEventModel == null)
            {
                Debug.LogError("[FloatingDamagePopupSpawner] DamageEventModel is not assigned! Please assign it in the Inspector.", gameObject);
                throw new System.NullReferenceException($"[FloatingDamagePopupSpawner] DamageEventModel is null on GameObject '{gameObject.name}'.");
            }

            if (damagePopupPrefab == null)
            {
                Debug.LogError("[FloatingDamagePopupSpawner] DamagePopupPrefab is not assigned! Please assign it in the Inspector.", gameObject);
                throw new System.NullReferenceException($"[FloatingDamagePopupSpawner] DamagePopupPrefab is null on GameObject '{gameObject.name}'.");
            }
        }

        private void CacheLocalPlayerConnectionId()
        {
            // Get local player's connection ID for filtering damage
            var networkManager = InstanceFinder.NetworkManager;
            if (networkManager != null && networkManager.ClientManager != null && networkManager.ClientManager.Connection != null)
            {
                _localPlayerConnectionId = (int)networkManager.ClientManager.Connection.ClientId;
            }
        }

        public void OnNotify(DamageEventData damageEvent)
        {
            // Filter: only show damage if it's from local player or if showAllDamage is enabled
            if (!showAllDamage && damageEvent.DamageInfo.InstigatorId != _localPlayerConnectionId)
            {
                return;
            }

            SpawnDamagePopup(damageEvent);
        }

        private void SpawnDamagePopup(DamageEventData damageEvent)
        {
            var popup = Instantiate(damagePopupPrefab, damageEvent.DamagePosition, Quaternion.identity);
            popup.Initialize(damageEvent);
        }
    }
}

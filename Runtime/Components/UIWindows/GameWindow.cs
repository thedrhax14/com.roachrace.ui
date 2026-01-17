using RoachRace.UI.Core;
using RoachRace.UI.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// Main game HUD window - shown during gameplay
    /// </summary>
    public class GameWindow : UIWindow
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerStatsModel playerStatsModel;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI staminaText;

        private IObserver<int> _healthObserver;
        private IObserver<int> _maxHealthObserver;
        private IObserver<float> _staminaObserver;
        private IObserver<float> _maxStaminaObserver;

        protected override void Awake()
        {
            base.Awake();
            
            if (playerStatsModel == null)
            {
                Debug.LogError($"[GameWindow] PlayerStatsModel is missing!", gameObject);
                return;
            }

            // Create observers using ActionObserver to avoid boilerplate
            _healthObserver = new ActionObserver<int>(OnHealthChanged);
            _maxHealthObserver = new ActionObserver<int>(_ => OnHealthChanged(playerStatsModel.Health.Value));
            
            _staminaObserver = new ActionObserver<float>(OnStaminaChanged);
            _maxStaminaObserver = new ActionObserver<float>(_ => OnStaminaChanged(playerStatsModel.Stamina.Value));
        }

        protected override void OnShow()
        {
            base.OnShow();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // Subscribe to model updates
            if (playerStatsModel != null)
            {
                playerStatsModel.Health.Attach(_healthObserver);
                playerStatsModel.MaxHealth.Attach(_maxHealthObserver);
                
                playerStatsModel.Stamina.Attach(_staminaObserver);
                playerStatsModel.MaxStamina.Attach(_maxStaminaObserver);
                
                // Initial update
                OnHealthChanged(playerStatsModel.Health.Value);
                OnStaminaChanged(playerStatsModel.Stamina.Value);
            }
        }

        protected override void OnHide()
        {
            base.OnHide();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Unsubscribe
            if (playerStatsModel != null)
            {
                playerStatsModel.Health.Detach(_healthObserver);
                playerStatsModel.MaxHealth.Detach(_maxHealthObserver);
                
                playerStatsModel.Stamina.Detach(_staminaObserver);
                playerStatsModel.MaxStamina.Detach(_maxStaminaObserver);
            }
        }

        private void OnHealthChanged(int currentHealth)
        {
            int maxHealth = playerStatsModel.MaxHealth.Value;
            if (maxHealth <= 0) maxHealth = 100; // Prevent division by zero
            healthText.text = $"{currentHealth}";
        }

        private void OnStaminaChanged(float currentStamina)
        {
            float maxStamina = playerStatsModel.MaxStamina.Value;
            if (maxStamina <= 0) maxStamina = 100f;
            staminaText.text = $"{Mathf.CeilToInt(currentStamina)}";
        }
    }
}

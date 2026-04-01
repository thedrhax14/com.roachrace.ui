using RoachRace.UI.Core;
using RoachRace.UI.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// Main gameplay HUD window that observes <see cref="PlayerStatsModel"/> and renders health/stamina text.<br/>
    /// Typical usage: place this in the in-game UI, assign the player stats model, and wire the health/stamina text fields so the local HUD updates automatically as the model observables change.<br/>
    /// Configuration/context: this is owner-local presentation only; it subscribes to the ScriptableObject model and should not contain gameplay-authoritative logic.
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

        /// <summary>
        /// Resolves model observers used to keep the HUD text synchronized with <see cref="PlayerStatsModel"/>.<br/>
        /// Typical usage: Unity invokes this during initialization; the window caches lightweight observers so it can attach and detach cleanly when shown or hidden.<br/>
        /// Configuration/context: missing model references are logged and leave the window inert rather than throwing, matching the existing HUD window behavior.
        /// </summary>
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

        /// <summary>
        /// Subscribes to the player stats model and pushes the current values into the text fields when the HUD becomes visible.<br/>
        /// Typical usage: called by the window system when gameplay begins or the HUD is shown again after being hidden.<br/>
        /// Configuration/context: the observer attachment immediately triggers the latest model values, so the text always renders the current snapshot.
        /// </summary>
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

        /// <summary>
        /// Unsubscribes from the player stats model when the HUD is hidden.<br/>
        /// Typical usage: called by the window system when gameplay ends or another window takes over the screen.<br/>
        /// Configuration/context: releasing the model observers prevents stale callbacks while the HUD is not visible.
        /// </summary>
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

        /// <summary>
        /// Updates the health text from the latest model value.<br/>
        /// Typical usage: invoked whenever the health observable changes or when the HUD is first shown.<br/>
        /// Configuration/context: renders a current/max pair so the player can see both remaining and total health at a glance.
        /// </summary>
        /// <param name="currentHealth">The current health value from the model.</param>
        private void OnHealthChanged(int currentHealth)
        {
            if (healthText == null || playerStatsModel == null)
                return;

            int maxHealth = playerStatsModel.MaxHealth.Value;
            if (maxHealth <= 0)
                maxHealth = currentHealth > 0 ? currentHealth : 100;

            healthText.text = $"{currentHealth} / {maxHealth}";
        }

        /// <summary>
        /// Updates the stamina text from the latest model value.<br/>
        /// Typical usage: invoked whenever the stamina observable changes or when the HUD is first shown.<br/>
        /// Configuration/context: stamina is displayed as a whole-number current/max pair because the resource is tracked as inventory units.
        /// </summary>
        /// <param name="currentStamina">The current stamina value from the model.</param>
        private void OnStaminaChanged(float currentStamina)
        {
            if (staminaText == null || playerStatsModel == null)
                return;

            float maxStamina = playerStatsModel.MaxStamina.Value;
            if (maxStamina <= 0f)
                maxStamina = currentStamina > 0f ? currentStamina : 100f;

            staminaText.text = $"{Mathf.CeilToInt(currentStamina)} / {Mathf.CeilToInt(maxStamina)}";
        }
    }
}

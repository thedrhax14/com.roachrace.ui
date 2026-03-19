using RoachRace.UI.Core;
using RoachRace.UI.Models;
using System.Globalization;
using UnityEngine;

namespace RoachRace.UI.Components.Options
{
    /// <summary>
    /// Binds multiple OptionStepperWidget instances to a GameSettingsModel so options are populated dynamically and changes are synchronized.<br></br>
    /// Typical usage:<br></br>
    /// - Place this component on the Settings page GameObject.<br></br>
    /// - Assign a GameSettingsModel asset and the corresponding OptionStepperWidget references.<br></br>
    /// - The binder will build option lists from the model (presets, ranges) and keep widget selection in sync with model Observables.<br></br>
    /// Notes:<br></br>
    /// - This avoids dynamic scene searches; dependencies are explicit via serialized references.<br></br>
    /// - No UnityEvents or C# events are used; synchronization uses Observable&lt;T&gt; + IObserver&lt;T&gt;.
    /// </summary>
    public sealed class GameSettingsOptionSteppersBinder : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private GameSettingsModel model;

        [Header("Widgets")]
        [SerializeField] private OptionStepperWidget configStepper;
        [SerializeField] private OptionStepperWidget regenSpeedStepper;
        [SerializeField] private OptionStepperWidget regenDelayStepper;
        [SerializeField] private OptionStepperWidget friendlyFireStepper;
        [SerializeField] private OptionStepperWidget friendlyFireProgressionStepper;
        [SerializeField] private OptionStepperWidget survivorLivesStepper;
        [SerializeField] private OptionStepperWidget winnerCountStepper;
        [SerializeField] private OptionStepperWidget ectoplasmStepper;
        [SerializeField] private OptionStepperWidget roundTimeStepper;

        [Header("Optional Widgets")]
        [Tooltip("Optional: binds GameSettingsModel.IntroEnabled.")]
        [SerializeField] private OptionStepperWidget introEnabledStepper;

        [Tooltip("Optional: binds GameSettingsModel.IntroDurationSeconds.")]
        [SerializeField] private OptionStepperWidget introDurationStepper;

        private bool _suppressWidgetToModel;

        private IObserver<string> _configWidgetObserver;
        private IObserver<string> _regenSpeedWidgetObserver;
        private IObserver<string> _regenDelayWidgetObserver;
        private IObserver<string> _friendlyFireWidgetObserver;
        private IObserver<string> _friendlyFireProgressionWidgetObserver;
        private IObserver<string> _survivorLivesWidgetObserver;
        private IObserver<string> _winnerCountWidgetObserver;
        private IObserver<string> _ectoplasmWidgetObserver;
        private IObserver<string> _roundTimeWidgetObserver;
        private IObserver<string> _introEnabledWidgetObserver;
        private IObserver<string> _introDurationWidgetObserver;

        private IObserver<string> _configModelObserver;
        private IObserver<GameSettingsModel.RegenSpeed> _regenSpeedModelObserver;
        private IObserver<bool> _regenDelayModelObserver;
        private IObserver<bool> _friendlyFireModelObserver;
        private IObserver<bool> _friendlyFireProgressionModelObserver;
        private IObserver<int> _survivorLivesModelObserver;
        private IObserver<int> _winnerCountModelObserver;
        private IObserver<int> _ectoplasmModelObserver;
        private IObserver<int> _roundTimeModelObserver;
        private IObserver<bool> _introEnabledModelObserver;
        private IObserver<float> _introDurationModelObserver;

        private void OnValidate()
        {
            RenameWidgetGameObjects();
        }

        private void Awake()
        {
            if (model == null)
            {
                Debug.LogError($"[{nameof(GameSettingsOptionSteppersBinder)}] Missing required reference on '{gameObject.name}': model", gameObject);
                throw new System.InvalidOperationException($"[{nameof(GameSettingsOptionSteppersBinder)}] Missing required reference on '{gameObject.name}': model");
            }

            RenameWidgetGameObjects();

            ValidateWidget(nameof(configStepper), configStepper);
            ValidateWidget(nameof(regenSpeedStepper), regenSpeedStepper);
            ValidateWidget(nameof(regenDelayStepper), regenDelayStepper);
            ValidateWidget(nameof(friendlyFireStepper), friendlyFireStepper);
            ValidateWidget(nameof(friendlyFireProgressionStepper), friendlyFireProgressionStepper);
            ValidateWidget(nameof(survivorLivesStepper), survivorLivesStepper);
            ValidateWidget(nameof(winnerCountStepper), winnerCountStepper);
            ValidateWidget(nameof(ectoplasmStepper), ectoplasmStepper);
            ValidateWidget(nameof(roundTimeStepper), roundTimeStepper);

            _configWidgetObserver = new ActionObserver<string>(OnConfigSelected);
            _regenSpeedWidgetObserver = new ActionObserver<string>(OnRegenSpeedSelected);
            _regenDelayWidgetObserver = new ActionObserver<string>(value => OnBoolSelected(value, model.SetRegenDelayEnabled));
            _friendlyFireWidgetObserver = new ActionObserver<string>(value => OnBoolSelected(value, model.SetFriendlyFireEnabled));
            _friendlyFireProgressionWidgetObserver = new ActionObserver<string>(value => OnBoolSelected(value, model.SetFriendlyFireProgressionEnabled));
            _survivorLivesWidgetObserver = new ActionObserver<string>(value => OnIntSelected(value, model.SetSurvivorLives));
            _winnerCountWidgetObserver = new ActionObserver<string>(OnWinnerCountSelected);
            _ectoplasmWidgetObserver = new ActionObserver<string>(value => OnIntSelected(value, model.SetEctoplasm));
            _roundTimeWidgetObserver = new ActionObserver<string>(OnRoundTimeSelected);

            _introEnabledWidgetObserver = new ActionObserver<string>(value => OnBoolSelected(value, model.SetIntroEnabled));
            _introDurationWidgetObserver = new ActionObserver<string>(value => OnSecondsSelected(value, model.SetIntroDurationSeconds));

            _configModelObserver = new ActionObserver<string>(RenderConfigSelection);
            _regenSpeedModelObserver = new ActionObserver<GameSettingsModel.RegenSpeed>(RenderRegenSpeed);
            _regenDelayModelObserver = new ActionObserver<bool>(value => RenderOnOff(regenDelayStepper, value));
            _friendlyFireModelObserver = new ActionObserver<bool>(value => RenderOnOff(friendlyFireStepper, value));
            _friendlyFireProgressionModelObserver = new ActionObserver<bool>(value => RenderOnOff(friendlyFireProgressionStepper, value));
            _survivorLivesModelObserver = new ActionObserver<int>(value => RenderInt(survivorLivesStepper, value));
            _winnerCountModelObserver = new ActionObserver<int>(RenderWinnerCount);
            _ectoplasmModelObserver = new ActionObserver<int>(value => RenderInt(ectoplasmStepper, value));
            _roundTimeModelObserver = new ActionObserver<int>(RenderRoundTime);

            _introEnabledModelObserver = new ActionObserver<bool>(value =>
            {
                if (introEnabledStepper != null)
                    RenderOnOff(introEnabledStepper, value);
            });

            _introDurationModelObserver = new ActionObserver<float>(RenderIntroDuration);
        }

        /// <summary>
        /// Renames the referenced widget GameObjects to stable, readable names so the scene hierarchy stays clean.<br></br>
        /// This runs in the editor via OnValidate and once at runtime during Awake.
        /// </summary>
        private void RenameWidgetGameObjects()
        {
            RenameWidgetGameObject(configStepper, "ConfigStepper");
            RenameWidgetGameObject(regenSpeedStepper, "RegenSpeedStepper");
            RenameWidgetGameObject(regenDelayStepper, "RegenDelayStepper");
            RenameWidgetGameObject(friendlyFireStepper, "FriendlyFireStepper");
            RenameWidgetGameObject(friendlyFireProgressionStepper, "FriendlyFireProgressionStepper");
            RenameWidgetGameObject(survivorLivesStepper, "SurvivorLivesStepper");
            RenameWidgetGameObject(winnerCountStepper, "WinnerCountStepper");
            RenameWidgetGameObject(ectoplasmStepper, "EctoplasmStepper");
            RenameWidgetGameObject(roundTimeStepper, "RoundTimeStepper");
            RenameWidgetGameObject(introEnabledStepper, "IntroEnabledStepper");
            RenameWidgetGameObject(introDurationStepper, "IntroDurationStepper");
        }

        /// <summary>
        /// Renames a widget's underlying GameObject if it is assigned.
        /// </summary>
        /// <param name="widget">Widget reference.</param>
        /// <param name="name">Desired GameObject name.</param>
        private static void RenameWidgetGameObject(OptionStepperWidget widget, string name)
        {
            if (widget == null) return;
            if (string.IsNullOrWhiteSpace(name)) return;

            if (widget.gameObject != null && widget.gameObject.name != name)
            {
                widget.gameObject.name = name;
            }
        }

        private void Start()
        {
            ApplyLabels();
            BuildAndAssignOptions();

            // Widget -> model
            configStepper.SelectedOption.Attach(_configWidgetObserver);
            regenSpeedStepper.SelectedOption.Attach(_regenSpeedWidgetObserver);
            regenDelayStepper.SelectedOption.Attach(_regenDelayWidgetObserver);
            friendlyFireStepper.SelectedOption.Attach(_friendlyFireWidgetObserver);
            friendlyFireProgressionStepper.SelectedOption.Attach(_friendlyFireProgressionWidgetObserver);
            survivorLivesStepper.SelectedOption.Attach(_survivorLivesWidgetObserver);
            winnerCountStepper.SelectedOption.Attach(_winnerCountWidgetObserver);
            ectoplasmStepper.SelectedOption.Attach(_ectoplasmWidgetObserver);
            roundTimeStepper.SelectedOption.Attach(_roundTimeWidgetObserver);

            if (introEnabledStepper != null)
                introEnabledStepper.SelectedOption.Attach(_introEnabledWidgetObserver);

            if (introDurationStepper != null)
                introDurationStepper.SelectedOption.Attach(_introDurationWidgetObserver);

            // Model -> widget
            model.ConfigName.Attach(_configModelObserver);
            model.RegenSpeedSetting.Attach(_regenSpeedModelObserver);
            model.RegenDelayEnabled.Attach(_regenDelayModelObserver);
            model.FriendlyFireEnabled.Attach(_friendlyFireModelObserver);
            model.FriendlyFireProgressionEnabled.Attach(_friendlyFireProgressionModelObserver);
            model.SurvivorLives.Attach(_survivorLivesModelObserver);
            model.WinnerCount.Attach(_winnerCountModelObserver);
            model.Ectoplasm.Attach(_ectoplasmModelObserver);
            model.RoundTimeSeconds.Attach(_roundTimeModelObserver);

            model.IntroEnabled.Attach(_introEnabledModelObserver);
            model.IntroDurationSeconds.Attach(_introDurationModelObserver);

            // Initial render (Attach already notifies, but this ensures options were assigned first).
            RenderAll();
        }

        /// <summary>
        /// Assigns consistent label text to each widget so scene setup does not rely on manual per-widget label wiring.
        /// </summary>
        private void ApplyLabels()
        {
            configStepper.SetLabel("Config");
            regenSpeedStepper.SetLabel("Regen Speed");
            regenDelayStepper.SetLabel("Regen Delay");
            friendlyFireStepper.SetLabel("Friendly Fire");
            friendlyFireProgressionStepper.SetLabel("FF Progression");
            survivorLivesStepper.SetLabel("Survivor Lives");
            winnerCountStepper.SetLabel("One Winner");
            ectoplasmStepper.SetLabel("Ectoplasm");
            roundTimeStepper.SetLabel("Round Time");

            if (introEnabledStepper != null)
                introEnabledStepper.SetLabel("Intro");

            if (introDurationStepper != null)
                introDurationStepper.SetLabel("Intro Duration");
        }

        private void OnDestroy()
        {
            if (configStepper != null) configStepper.SelectedOption.Detach(_configWidgetObserver);
            if (regenSpeedStepper != null) regenSpeedStepper.SelectedOption.Detach(_regenSpeedWidgetObserver);
            if (regenDelayStepper != null) regenDelayStepper.SelectedOption.Detach(_regenDelayWidgetObserver);
            if (friendlyFireStepper != null) friendlyFireStepper.SelectedOption.Detach(_friendlyFireWidgetObserver);
            if (friendlyFireProgressionStepper != null) friendlyFireProgressionStepper.SelectedOption.Detach(_friendlyFireProgressionWidgetObserver);
            if (survivorLivesStepper != null) survivorLivesStepper.SelectedOption.Detach(_survivorLivesWidgetObserver);
            if (winnerCountStepper != null) winnerCountStepper.SelectedOption.Detach(_winnerCountWidgetObserver);
            if (ectoplasmStepper != null) ectoplasmStepper.SelectedOption.Detach(_ectoplasmWidgetObserver);
            if (roundTimeStepper != null) roundTimeStepper.SelectedOption.Detach(_roundTimeWidgetObserver);

            if (introEnabledStepper != null) introEnabledStepper.SelectedOption.Detach(_introEnabledWidgetObserver);
            if (introDurationStepper != null) introDurationStepper.SelectedOption.Detach(_introDurationWidgetObserver);

            if (model == null) return;

            model.ConfigName.Detach(_configModelObserver);
            model.RegenSpeedSetting.Detach(_regenSpeedModelObserver);
            model.RegenDelayEnabled.Detach(_regenDelayModelObserver);
            model.FriendlyFireEnabled.Detach(_friendlyFireModelObserver);
            model.FriendlyFireProgressionEnabled.Detach(_friendlyFireProgressionModelObserver);
            model.SurvivorLives.Detach(_survivorLivesModelObserver);
            model.WinnerCount.Detach(_winnerCountModelObserver);
            model.Ectoplasm.Detach(_ectoplasmModelObserver);
            model.RoundTimeSeconds.Detach(_roundTimeModelObserver);

            model.IntroEnabled.Detach(_introEnabledModelObserver);
            model.IntroDurationSeconds.Detach(_introDurationModelObserver);
        }

        private static void ValidateWidget(string fieldName, OptionStepperWidget widget)
        {
            if (widget != null) return;
            throw new System.InvalidOperationException($"[{nameof(GameSettingsOptionSteppersBinder)}] Missing required reference: {fieldName}");
        }

        private void BuildAndAssignOptions()
        {
            _suppressWidgetToModel = true;
            try
            {
                configStepper.SetOptions(model.BuildConfigOptions(), 0);

                regenSpeedStepper.SetOptions(new[] { "Slow", "Fast" }, 0);

                regenDelayStepper.SetOptions(new[] { "Off", "On" }, 0);
                friendlyFireStepper.SetOptions(new[] { "Off", "On" }, 0);
                friendlyFireProgressionStepper.SetOptions(new[] { "Off", "On" }, 0);

                survivorLivesStepper.SetOptions(model.BuildSurvivorLivesOptions(), 0);

                // Winner count is displayed as "On" for 1, then 2..N.
                string[] rawWinnerOptions = model.BuildWinnerCountOptions();
                string[] winnerOptions = new string[rawWinnerOptions.Length];
                for (int i = 0; i < rawWinnerOptions.Length; i++)
                {
                    winnerOptions[i] = rawWinnerOptions[i] == "1" ? "On" : rawWinnerOptions[i];
                }
                winnerCountStepper.SetOptions(winnerOptions, 0);

                ectoplasmStepper.SetOptions(model.BuildEctoplasmOptions(), 0);
                roundTimeStepper.SetOptions(model.BuildRoundTimeOptions(), 0);

                if (introEnabledStepper != null)
                    introEnabledStepper.SetOptions(new[] { "Off", "On" }, 0);

                if (introDurationStepper != null)
                    introDurationStepper.SetOptions(model.BuildIntroDurationOptions(), 0);
            }
            finally
            {
                _suppressWidgetToModel = false;
            }
        }

        private void RenderAll()
        {
            RenderConfigSelection(model.ConfigName.Value);
            RenderRegenSpeed(model.RegenSpeedSetting.Value);
            RenderOnOff(regenDelayStepper, model.RegenDelayEnabled.Value);
            RenderOnOff(friendlyFireStepper, model.FriendlyFireEnabled.Value);
            RenderOnOff(friendlyFireProgressionStepper, model.FriendlyFireProgressionEnabled.Value);
            RenderInt(survivorLivesStepper, model.SurvivorLives.Value);
            RenderWinnerCount(model.WinnerCount.Value);
            RenderInt(ectoplasmStepper, model.Ectoplasm.Value);
            RenderRoundTime(model.RoundTimeSeconds.Value);

            if (introEnabledStepper != null)
                RenderOnOff(introEnabledStepper, model.IntroEnabled.Value);

            if (introDurationStepper != null)
                RenderIntroDuration(model.IntroDurationSeconds.Value);
        }

        private void OnConfigSelected(string selected)
        {
            if (_suppressWidgetToModel) return;
            model.ApplyConfig(selected);
        }

        private void OnRegenSpeedSelected(string selected)
        {
            if (_suppressWidgetToModel) return;

            if (string.Equals(selected, "Fast", System.StringComparison.OrdinalIgnoreCase))
            {
                model.SetRegenSpeed(GameSettingsModel.RegenSpeed.Fast);
                return;
            }

            model.SetRegenSpeed(GameSettingsModel.RegenSpeed.Slow);
        }

        private void OnWinnerCountSelected(string selected)
        {
            if (_suppressWidgetToModel) return;

            if (string.Equals(selected, "On", System.StringComparison.OrdinalIgnoreCase))
            {
                model.SetWinnerCount(1);
                return;
            }

            OnIntSelected(selected, model.SetWinnerCount);
        }

        private void OnRoundTimeSelected(string selected)
        {
            if (_suppressWidgetToModel) return;

            if (string.Equals(selected, "Off/Infinite", System.StringComparison.OrdinalIgnoreCase))
            {
                model.SetRoundTimeSeconds(0);
                return;
            }

            OnIntSelected(selected, model.SetRoundTimeSeconds);
        }

        private void RenderConfigSelection(string configName)
        {
            SetWidgetSelection(configStepper, string.IsNullOrWhiteSpace(configName) ? GameSettingsModel.CustomConfigName : configName);
        }

        private void RenderRegenSpeed(GameSettingsModel.RegenSpeed speed)
        {
            SetWidgetSelection(regenSpeedStepper, speed == GameSettingsModel.RegenSpeed.Fast ? "Fast" : "Slow");
        }

        private void RenderWinnerCount(int count)
        {
            SetWidgetSelection(winnerCountStepper, count <= 1 ? "On" : count.ToString());
        }

        private void RenderRoundTime(int seconds)
        {
            SetWidgetSelection(roundTimeStepper, seconds <= 0 ? "Off/Infinite" : seconds.ToString());
        }

        private void RenderIntroDuration(float seconds)
        {
            if (introDurationStepper == null)
                return;

            int s = Mathf.Max(0, Mathf.RoundToInt(seconds));
            SetWidgetSelection(introDurationStepper, s.ToString());
        }

        private void RenderOnOff(OptionStepperWidget widget, bool enabled)
        {
            SetWidgetSelection(widget, enabled ? "On" : "Off");
        }

        private void RenderInt(OptionStepperWidget widget, int value)
        {
            SetWidgetSelection(widget, value.ToString());
        }

        private void SetWidgetSelection(OptionStepperWidget widget, string option)
        {
            _suppressWidgetToModel = true;
            try
            {
                if (!widget.TrySetSelectedOption(option))
                {
                    // If the target is not present (e.g., range changed), fall back safely.
                    widget.SetSelectedIndex(0);
                }
            }
            finally
            {
                _suppressWidgetToModel = false;
            }
        }

        private static void OnBoolSelected(string selected, System.Action<bool> setter)
        {
            if (setter == null) return;

            bool enabled = string.Equals(selected, "On", System.StringComparison.OrdinalIgnoreCase);
            setter(enabled);
        }

        private static void OnIntSelected(string selected, System.Action<int> setter)
        {
            if (setter == null) return;

            if (int.TryParse(selected, out int value))
            {
                setter(value);
            }
        }

        private static void OnSecondsSelected(string selected, System.Action<float> setter)
        {
            if (setter == null) return;

            if (string.IsNullOrWhiteSpace(selected))
                return;

            // Accept simple formats like "10" or "10s".
            string trimmed = selected.Trim();
            if (trimmed.EndsWith("s", System.StringComparison.OrdinalIgnoreCase))
                trimmed = trimmed.Substring(0, trimmed.Length - 1);

            if (float.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out float seconds))
            {
                setter(Mathf.Max(0f, seconds));
            }
        }
    }
}

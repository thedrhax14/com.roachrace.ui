using System;
using System.Collections.Generic;
using RoachRace.UI.Core;
using UnityEngine;

namespace RoachRace.UI.Models
{
    /// <summary>
    /// ScriptableObject UI model that holds match/game settings intended to be selected in UI and applied on the server.<br></br>
    /// Typical usage:<br></br>
    /// - Create one GameSettingsModel asset and reference it from the settings page binder and networking bridge.<br></br>
    /// - UI binds to the Observables to render current values and calls Set* methods to change settings.<br></br>
    /// - A selected Config preset can apply values for all other settings in one action.
    /// </summary>
    [CreateAssetMenu(fileName = "GameSettingsModel", menuName = "RoachRace/Models/GameSettingsModel")]
    public sealed class GameSettingsModel : UIModel
    {
        /// <summary>
        /// Regen speed options.
        /// </summary>
        public enum RegenSpeed
        {
            /// <summary>
            /// Slow regeneration.
            /// </summary>
            Slow = 0,

            /// <summary>
            /// Fast regeneration.
            /// </summary>
            Fast = 1
        }

        /// <summary>
        /// Serialized preset definition for the Config selector.<br></br>
        /// A preset is a named bundle of values for all other settings.
        /// </summary>
        [Serializable]
        public sealed class GameSettingsPreset
        {
            [SerializeField] private string name;

            [Header("Regen")]
            [SerializeField] private RegenSpeed regenSpeed = RegenSpeed.Slow;
            [SerializeField] private bool regenDelayEnabled = true;

            [Header("Friendly Fire")]
            [SerializeField] private bool friendlyFireEnabled;
            [SerializeField] private bool friendlyFireProgressionEnabled;

            [Header("Rounds")]
            [Range(0, 3)]
            [SerializeField] private int survivorLives = 3;
            [Range(1, 10)]
            [SerializeField] private int winnerCount = 1;
            [Range(0, 100)]
            [SerializeField] private int ectoplasm = 0;
            [Tooltip("0 means Off/Infinite. Otherwise time in seconds (e.g., 300, 600, 900, 1200).")]
            [SerializeField] private int roundTimeSeconds;

            [Header("Intro")]
            [SerializeField] private bool introEnabled = true;

            [Tooltip("Intro duration in seconds before pods spawn.")]
            [SerializeField] private float introDurationSeconds = 10f;

            /// <summary>
            /// Display name used by the Config selector.
            /// </summary>
            public string Name => name;

            /// <summary>
            /// Regen speed value for this preset.
            /// </summary>
            public RegenSpeed PresetRegenSpeed => regenSpeed;

            /// <summary>
            /// Whether regen delay is enabled in this preset.
            /// </summary>
            public bool PresetRegenDelayEnabled => regenDelayEnabled;

            /// <summary>
            /// Whether friendly fire is enabled in this preset.
            /// </summary>
            public bool PresetFriendlyFireEnabled => friendlyFireEnabled;

            /// <summary>
            /// Whether friendly fire progression is enabled in this preset.
            /// </summary>
            public bool PresetFriendlyFireProgressionEnabled => friendlyFireProgressionEnabled;

            /// <summary>
            /// Survivor lives (0..3) in this preset.
            /// </summary>
            public int PresetSurvivorLives => survivorLives;

            /// <summary>
            /// Winner count (1..10) in this preset.
            /// </summary>
            public int PresetWinnerCount => winnerCount;

            /// <summary>
            /// Ectoplasm amount (0..100) in this preset.
            /// </summary>
            public int PresetEctoplasm => ectoplasm;

            /// <summary>
            /// Round time in seconds, where 0 means Off/Infinite.
            /// </summary>
            public int PresetRoundTimeSeconds => roundTimeSeconds;

            /// <summary>
            /// Whether the match intro is enabled in this preset.
            /// </summary>
            public bool PresetIntroEnabled => introEnabled;

            /// <summary>
            /// Match intro duration in seconds in this preset.
            /// </summary>
            public float PresetIntroDurationSeconds => introDurationSeconds;
        }

        /// <summary>
        /// Display string used to represent "no preset" / custom configuration.
        /// </summary>
        public const string CustomConfigName = "Custom";

        [Header("Config Presets")]
        [Tooltip("List of Config presets. The Config selector will show these names plus 'Custom'.")]
        [SerializeField] private List<GameSettingsPreset> presets = new();

        [Header("Numeric Ranges")]
        [SerializeField] private int survivorLivesMin = 0;
        [SerializeField] private int survivorLivesMax = 3;
        [SerializeField] private int survivorLivesStep = 1;

        [SerializeField] private int winnerCountMin = 1;
        [SerializeField] private int winnerCountMax = 10;
        [SerializeField] private int winnerCountStep = 1;

        [SerializeField] private int ectoplasmMin = 0;
        [SerializeField] private int ectoplasmMax = 100;
        [SerializeField] private int ectoplasmStep = 1;

        [Header("Round Time Options")]
        [Tooltip("Available round times in seconds (excluding Off/Infinite). Typical values: 300, 600, 900, 1200.")]
        [SerializeField] private int[] roundTimeOptionsSeconds = { 300, 600, 900, 1200 };

        [Header("Intro Options")]
        [Tooltip("Available intro durations in seconds. Typical values: 0, 5, 10, 15.")]
        [SerializeField] private int[] introDurationOptionsSeconds = { 0, 5, 10, 15 };

        private bool _isApplyingPreset;

        /// <summary>
        /// Currently selected Config preset name, or 'Custom'.
        /// </summary>
        public Observable<string> ConfigName { get; } = new(CustomConfigName);

        /// <summary>
        /// Regen speed setting.
        /// </summary>
        public Observable<RegenSpeed> RegenSpeedSetting { get; } = new(RegenSpeed.Slow);

        /// <summary>
        /// Regen delay toggle setting.
        /// </summary>
        public Observable<bool> RegenDelayEnabled { get; } = new(true);

        /// <summary>
        /// Friendly fire toggle setting.
        /// </summary>
        public Observable<bool> FriendlyFireEnabled { get; } = new(false);

        /// <summary>
        /// Friendly fire progression toggle setting.
        /// </summary>
        public Observable<bool> FriendlyFireProgressionEnabled { get; } = new(false);

        /// <summary>
        /// Survivor lives (0..3 by default).
        /// </summary>
        public Observable<int> SurvivorLives { get; } = new(3);

        /// <summary>
        /// Winner count (1..10 by default).<br></br>
        /// UI may display 1 as "On" for "one winner" mode.
        /// </summary>
        public Observable<int> WinnerCount { get; } = new(1);

        /// <summary>
        /// Ectoplasm amount (0..100 by default).
        /// </summary>
        public Observable<int> Ectoplasm { get; } = new(0);

        /// <summary>
        /// Round time in seconds, where 0 means Off/Infinite.
        /// </summary>
        public Observable<int> RoundTimeSeconds { get; } = new(0);

        /// <summary>
        /// Match intro (cinematic + pods) toggle.<br></br>
        /// Purpose: host decides before starting the match whether to play the immersive intro sequence or skip directly to controllers.<br></br>
        /// Typical usage: UI binds to this observable and the server reads it at match start.
        /// </summary>
        public Observable<bool> IntroEnabled { get; } = new(true);

        /// <summary>
        /// Match intro duration in seconds (clamped to >= 0).<br></br>
        /// Purpose: controls how long the intro cinematic runs before pods are spawned.<br></br>
        /// Typical usage: UI binds to this observable and the server reads it at match start.
        /// </summary>
        public Observable<float> IntroDurationSeconds { get; } = new(10f);

        /// <summary>
        /// Returns the configured Config options list: 'Custom' + preset names.
        /// </summary>
        public IReadOnlyList<GameSettingsPreset> Presets => presets;

        /// <summary>
        /// Builds the Config selector options: 'Custom' + preset names (skips empty names).
        /// </summary>
        /// <returns>Array of option display strings.</returns>
        public string[] BuildConfigOptions()
        {
            List<string> options = new() { CustomConfigName };

            if (presets != null)
            {
                for (int i = 0; i < presets.Count; i++)
                {
                    string presetName = presets[i] != null ? presets[i].Name : null;
                    if (string.IsNullOrWhiteSpace(presetName)) continue;
                    options.Add(presetName);
                }
            }

            return options.ToArray();
        }

        /// <summary>
        /// Applies a Config preset by name. If name is 'Custom' or not found, no preset values are applied.<br></br>
        /// When a preset is applied, ConfigName is set to that preset name.
        /// </summary>
        /// <param name="configName">Preset name to apply, or 'Custom'.</param>
        public void ApplyConfig(string configName)
        {
            if (string.IsNullOrWhiteSpace(configName) || string.Equals(configName, CustomConfigName, StringComparison.OrdinalIgnoreCase))
            {
                SetConfigName(CustomConfigName);
                return;
            }

            GameSettingsPreset preset = FindPresetByName(configName);
            if (preset == null)
            {
                SetConfigName(CustomConfigName);
                return;
            }

            _isApplyingPreset = true;
            try
            {
                SetConfigName(preset.Name);

                SetRegenSpeed(preset.PresetRegenSpeed);
                SetRegenDelayEnabled(preset.PresetRegenDelayEnabled);
                SetFriendlyFireEnabled(preset.PresetFriendlyFireEnabled);
                SetFriendlyFireProgressionEnabled(preset.PresetFriendlyFireProgressionEnabled);
                SetSurvivorLives(preset.PresetSurvivorLives);
                SetWinnerCount(preset.PresetWinnerCount);
                SetEctoplasm(preset.PresetEctoplasm);
                SetRoundTimeSeconds(preset.PresetRoundTimeSeconds);

                SetIntroEnabled(preset.PresetIntroEnabled);
                SetIntroDurationSeconds(preset.PresetIntroDurationSeconds);
            }
            finally
            {
                _isApplyingPreset = false;
            }
        }

        /// <summary>
        /// Sets the Config selection name without applying any preset values.<br></br>
        /// Most callers should prefer ApplyConfig(...).
        /// </summary>
        /// <param name="name">Config name to set.</param>
        public void SetConfigName(string name)
        {
            ConfigName.Value = string.IsNullOrWhiteSpace(name) ? CustomConfigName : name;
        }

        /// <summary>
        /// Sets regen speed and marks config as Custom if the user edits after applying a preset.
        /// </summary>
        /// <param name="speed">New regen speed.</param>
        public void SetRegenSpeed(RegenSpeed speed)
        {
            RegenSpeedSetting.Value = speed;
            MarkCustomIfUserEdit();
        }

        /// <summary>
        /// Sets regen delay enabled toggle and marks config as Custom if the user edits after applying a preset.
        /// </summary>
        /// <param name="enabled">True to enable regen delay; false otherwise.</param>
        public void SetRegenDelayEnabled(bool enabled)
        {
            RegenDelayEnabled.Value = enabled;
            MarkCustomIfUserEdit();
        }

        /// <summary>
        /// Sets friendly fire enabled toggle and marks config as Custom if the user edits after applying a preset.
        /// </summary>
        /// <param name="enabled">True to enable friendly fire; false otherwise.</param>
        public void SetFriendlyFireEnabled(bool enabled)
        {
            FriendlyFireEnabled.Value = enabled;
            MarkCustomIfUserEdit();
        }

        /// <summary>
        /// Sets friendly fire progression enabled toggle and marks config as Custom if the user edits after applying a preset.
        /// </summary>
        /// <param name="enabled">True to enable friendly fire progression; false otherwise.</param>
        public void SetFriendlyFireProgressionEnabled(bool enabled)
        {
            FriendlyFireProgressionEnabled.Value = enabled;
            MarkCustomIfUserEdit();
        }

        /// <summary>
        /// Sets survivor lives (clamped to configured range) and marks config as Custom if the user edits after applying a preset.
        /// </summary>
        /// <param name="lives">Survivor lives value.</param>
        public void SetSurvivorLives(int lives)
        {
            SurvivorLives.Value = ClampToRange(lives, survivorLivesMin, survivorLivesMax, survivorLivesStep);
            MarkCustomIfUserEdit();
        }

        /// <summary>
        /// Sets winner count (clamped to configured range) and marks config as Custom if the user edits after applying a preset.
        /// </summary>
        /// <param name="count">Winner count value.</param>
        public void SetWinnerCount(int count)
        {
            WinnerCount.Value = ClampToRange(count, winnerCountMin, winnerCountMax, winnerCountStep);
            MarkCustomIfUserEdit();
        }

        /// <summary>
        /// Sets ectoplasm (clamped to configured range) and marks config as Custom if the user edits after applying a preset.
        /// </summary>
        /// <param name="amount">Ectoplasm amount value.</param>
        public void SetEctoplasm(int amount)
        {
            Ectoplasm.Value = ClampToRange(amount, ectoplasmMin, ectoplasmMax, ectoplasmStep);
            MarkCustomIfUserEdit();
        }

        /// <summary>
        /// Sets round time seconds. 0 means Off/Infinite. Values are clamped to >= 0 and not validated against the options list.<br></br>
        /// </summary>
        /// <param name="seconds">Round time in seconds (0 for Off/Infinite).</param>
        public void SetRoundTimeSeconds(int seconds)
        {
            RoundTimeSeconds.Value = Mathf.Max(0, seconds);
            MarkCustomIfUserEdit();
        }

        /// <summary>
        /// Enables/disables the match intro sequence and marks config as Custom if the user edits after applying a preset.
        /// </summary>
        /// <param name="enabled">True to enable the intro; false to skip to controllers.</param>
        public void SetIntroEnabled(bool enabled)
        {
            IntroEnabled.Value = enabled;
            MarkCustomIfUserEdit();
        }

        /// <summary>
        /// Sets the match intro duration in seconds (clamped to >= 0) and marks config as Custom if the user edits after applying a preset.
        /// </summary>
        /// <param name="seconds">Duration in seconds before pods spawn (0 for immediate).</param>
        public void SetIntroDurationSeconds(float seconds)
        {
            IntroDurationSeconds.Value = Mathf.Max(0f, seconds);
            MarkCustomIfUserEdit();
        }

        /// <summary>
        /// Builds options for survivor lives based on the configured min/max/step.
        /// </summary>
        /// <returns>Array of numeric strings.</returns>
        public string[] BuildSurvivorLivesOptions() => BuildRangeOptions(survivorLivesMin, survivorLivesMax, survivorLivesStep);

        /// <summary>
        /// Builds options for winner count based on the configured min/max/step.<br></br>
        /// Note: UI may render 1 as "On".
        /// </summary>
        /// <returns>Array of numeric strings starting at winnerCountMin.</returns>
        public string[] BuildWinnerCountOptions() => BuildRangeOptions(winnerCountMin, winnerCountMax, winnerCountStep);

        /// <summary>
        /// Builds options for ectoplasm based on the configured min/max/step.
        /// </summary>
        /// <returns>Array of numeric strings.</returns>
        public string[] BuildEctoplasmOptions() => BuildRangeOptions(ectoplasmMin, ectoplasmMax, ectoplasmStep);

        /// <summary>
        /// Builds options for round time: "Off/Infinite" + configured seconds values.
        /// </summary>
        /// <returns>Array of option strings.</returns>
        public string[] BuildRoundTimeOptions()
        {
            List<string> values = new() { "Off/Infinite" };

            if (roundTimeOptionsSeconds != null)
            {
                for (int i = 0; i < roundTimeOptionsSeconds.Length; i++)
                {
                    int seconds = roundTimeOptionsSeconds[i];
                    if (seconds <= 0) continue;
                    values.Add(seconds.ToString());
                }
            }

            return values.ToArray();
        }

        /// <summary>
        /// Builds options for match intro duration in seconds.<br></br>
        /// Typical usage: UI uses this list to populate an option widget controlling <see cref="IntroDurationSeconds"/>.
        /// </summary>
        /// <returns>Array of seconds values as strings.</returns>
        public string[] BuildIntroDurationOptions()
        {
            List<string> values = new();

            if (introDurationOptionsSeconds != null)
            {
                for (int i = 0; i < introDurationOptionsSeconds.Length; i++)
                {
                    int seconds = introDurationOptionsSeconds[i];
                    if (seconds < 0) continue;
                    values.Add(seconds.ToString());
                }
            }

            if (values.Count == 0)
            {
                values.Add("0");
                values.Add("10");
            }

            return values.ToArray();
        }

        private void MarkCustomIfUserEdit()
        {
            if (_isApplyingPreset) return;

            if (!string.IsNullOrWhiteSpace(ConfigName.Value) && !string.Equals(ConfigName.Value, CustomConfigName, StringComparison.OrdinalIgnoreCase))
            {
                ConfigName.Value = CustomConfigName;
            }
        }

        private GameSettingsPreset FindPresetByName(string configName)
        {
            if (presets == null) return null;

            for (int i = 0; i < presets.Count; i++)
            {
                GameSettingsPreset preset = presets[i];
                if (preset == null) continue;
                if (string.Equals(preset.Name, configName, StringComparison.OrdinalIgnoreCase)) return preset;
            }

            return null;
        }

        private static int ClampToRange(int value, int min, int max, int step)
        {
            int clamped = Mathf.Clamp(value, min, max);

            if (step <= 1) return clamped;

            int offset = clamped - min;
            int snapped = min + Mathf.RoundToInt(offset / (float)step) * step;
            return Mathf.Clamp(snapped, min, max);
        }

        private static string[] BuildRangeOptions(int min, int max, int step)
        {
            if (step <= 0) step = 1;
            if (max < min) (min, max) = (max, min);

            List<string> options = new();

            for (int v = min; v <= max; v += step)
            {
                options.Add(v.ToString());
            }

            if (options.Count == 0)
            {
                options.Add(min.ToString());
            }

            return options.ToArray();
        }
    }
}

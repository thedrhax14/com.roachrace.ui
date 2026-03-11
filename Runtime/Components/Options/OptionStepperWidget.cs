using System;
using System.Collections.Generic;
using TMPro;
using RoachRace.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace RoachRace.UI.Components.Options
{
    /// <summary>
    /// Standalone UI widget for selecting from a list of string options using Previous/Next buttons.<br></br>
    /// Typical usage:<br></br>
    /// - Assign labelText, selectedOptionText, previousButton, and nextButton in the Inspector.<br></br>
    /// - Provide initial options via the Inspector (options) or at runtime via SetOptions(...).<br></br>
    /// - Observe selection changes via SelectedIndex / SelectedOption (Observable&lt;T&gt;).<br></br>
    /// Notes:<br></br>
    /// - Selection is clamped (no wrap-around). Buttons are disabled at ends.<br></br>
    /// - If the options list is empty, selection becomes -1 and SelectedOption becomes empty.
    /// </summary>
    public sealed class OptionStepperWidget : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text selectedOptionText;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;

        [Header("Data")]
        [Tooltip("Optional initial options. Can be overridden at runtime via SetOptions(...).")]
        [SerializeField] private string[] options;
        [Tooltip("Optional initial label text.")]
        [SerializeField] private string label = "";
        [Tooltip("Optional initial selection index (clamped).")]
        [SerializeField] private int initialSelectedIndex;

        private readonly List<string> _options = new();

        /// <summary>
        /// Emits whenever the options list is replaced via SetOptions(...).<br></br>
        /// Value is a snapshot array (do not mutate).
        /// </summary>
        public Observable<string[]> OptionsSnapshot { get; } = new(null);

        /// <summary>
        /// Selected option index within OptionsSnapshot, or -1 when no options exist.
        /// </summary>
        public Observable<int> SelectedIndex { get; } = new(-1);

        /// <summary>
        /// Selected option string, or empty when no options exist.
        /// </summary>
        public Observable<string> SelectedOption { get; } = new(string.Empty);

        /// <summary>
        /// Returns a read-only view of the current option list.
        /// </summary>
        public IReadOnlyList<string> Options => _options;

        private void Awake()
        {
            if (selectedOptionText == null)
            {
                Debug.LogError($"[{nameof(OptionStepperWidget)}] Missing required reference on '{gameObject.name}': selectedOptionText", gameObject);
                throw new InvalidOperationException($"[{nameof(OptionStepperWidget)}] Missing required reference on '{gameObject.name}': selectedOptionText");
            }

            if (previousButton == null)
            {
                Debug.LogError($"[{nameof(OptionStepperWidget)}] Missing required reference on '{gameObject.name}': previousButton", gameObject);
                throw new InvalidOperationException($"[{nameof(OptionStepperWidget)}] Missing required reference on '{gameObject.name}': previousButton");
            }

            if (nextButton == null)
            {
                Debug.LogError($"[{nameof(OptionStepperWidget)}] Missing required reference on '{gameObject.name}': nextButton", gameObject);
                throw new InvalidOperationException($"[{nameof(OptionStepperWidget)}] Missing required reference on '{gameObject.name}': nextButton");
            }

            ApplyLabel(label);

            // Load initial options.
            SetOptions(options, initialSelectedIndex);
        }

        private void OnEnable()
        {
            previousButton.onClick.AddListener(OnPreviousClicked);
            nextButton.onClick.AddListener(OnNextClicked);

            RefreshUI();
        }

        private void OnDisable()
        {
            previousButton.onClick.RemoveListener(OnPreviousClicked);
            nextButton.onClick.RemoveListener(OnNextClicked);
        }

        /// <summary>
        /// Sets the label text for the widget (optional).<br></br>
        /// If labelText is not assigned, the label is stored but not rendered.
        /// </summary>
        /// <param name="newLabel">Label to render; null becomes empty.</param>
        public void SetLabel(string newLabel)
        {
            label = newLabel ?? string.Empty;
            ApplyLabel(label);
        }

        /// <summary>
        /// Replaces the available options and sets the selected index.<br></br>
        /// Selection is clamped and selection observables are updated immediately.
        /// </summary>
        /// <param name="newOptions">Options to display; null is treated as empty.</param>
        /// <param name="selectedIndex">Desired selected index (clamped). Use -1 to select the first option if available.</param>
        public void SetOptions(IReadOnlyList<string> newOptions, int selectedIndex = -1)
        {
            _options.Clear();

            if (newOptions != null)
            {
                for (int i = 0; i < newOptions.Count; i++)
                {
                    // Normalize nulls to empty strings so the UI is stable.
                    _options.Add(newOptions[i] ?? string.Empty);
                }
            }

            OptionsSnapshot.Value = _options.Count == 0 ? Array.Empty<string>() : _options.ToArray();

            if (_options.Count == 0)
            {
                SetSelectionInternal(-1);
                return;
            }

            int resolvedIndex = selectedIndex < 0 ? 0 : Mathf.Clamp(selectedIndex, 0, _options.Count - 1);
            SetSelectionInternal(resolvedIndex);
        }

        /// <summary>
        /// Replaces the available options and attempts to preserve the current selected option text.<br></br>
        /// If the current selection does not exist in the new list, selects the first option.
        /// </summary>
        /// <param name="newOptions">Options to display; null is treated as empty.</param>
        public void SetOptionsPreserveSelection(IReadOnlyList<string> newOptions)
        {
            string current = SelectedOption.Value;
            SetOptions(newOptions, -1);

            if (_options.Count == 0) return;

            int index = _options.IndexOf(current);
            if (index >= 0)
            {
                SetSelectionInternal(index);
            }
        }

        /// <summary>
        /// Sets the selected index (clamped). If options are empty, selection becomes -1.
        /// </summary>
        /// <param name="index">Index into the current options list.</param>
        public void SetSelectedIndex(int index)
        {
            if (_options.Count == 0)
            {
                SetSelectionInternal(-1);
                return;
            }

            int resolvedIndex = Mathf.Clamp(index, 0, _options.Count - 1);
            SetSelectionInternal(resolvedIndex);
        }

        /// <summary>
        /// Sets the selected option by value. If not found, selection is unchanged.<br></br>
        /// Matching is exact (case-sensitive).
        /// </summary>
        /// <param name="option">Option text to select.</param>
        /// <returns>True if the option was found and selected; otherwise false.</returns>
        public bool TrySetSelectedOption(string option)
        {
            if (_options.Count == 0) return false;
            if (option == null) return false;

            int index = _options.IndexOf(option);
            if (index < 0) return false;

            SetSelectionInternal(index);
            return true;
        }

        private void OnPreviousClicked()
        {
            if (_options.Count == 0) return;

            int nextIndex = SelectedIndex.Value - 1;
            if (nextIndex < 0) nextIndex = 0;

            SetSelectionInternal(nextIndex);
        }

        private void OnNextClicked()
        {
            if (_options.Count == 0) return;

            int nextIndex = SelectedIndex.Value + 1;
            if (nextIndex > _options.Count - 1) nextIndex = _options.Count - 1;

            SetSelectionInternal(nextIndex);
        }

        private void SetSelectionInternal(int index)
        {
            if (_options.Count == 0 || index < 0)
            {
                SelectedIndex.Value = -1;
                SelectedOption.Value = string.Empty;
                RefreshUI();
                return;
            }

            int clamped = Mathf.Clamp(index, 0, _options.Count - 1);
            SelectedIndex.Value = clamped;
            SelectedOption.Value = _options[clamped] ?? string.Empty;

            RefreshUI();
        }

        private void ApplyLabel(string labelValue)
        {
            if (labelText == null) return;

            labelText.text = labelValue ?? string.Empty;
            labelText.enabled = !string.IsNullOrWhiteSpace(labelText.text);
        }

        private void RefreshUI()
        {
            selectedOptionText.text = SelectedOption.Value ?? string.Empty;
            RefreshButtonState();
        }

        private void RefreshButtonState()
        {
            bool hasOptions = _options.Count > 0;
            int index = SelectedIndex.Value;

            previousButton.interactable = hasOptions && index > 0;
            nextButton.interactable = hasOptions && index >= 0 && index < _options.Count - 1;
        }
    }
}

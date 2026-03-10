using UnityEngine;
using TMPro;
using System.Collections.Generic;
using RoachRace.UI.Core;
using RoachRace.UI.Models;
using RoachRace.UI.Services;

namespace RoachRace.UI.Components
{
    public class OptionsWindow : UIWindow
    {
        [Header("References")]
        [SerializeField] private OptionsModel optionsModel;
        [SerializeField] private OptionsService optionsService;

        [Header("UI Elements")]
        [SerializeField] private TMP_Dropdown microphoneDropdown;

        private MicrophonesObserver _microphonesObserver;
        private SelectedMicrophoneObserver _selectedMicrophoneObserver;

        protected override void Start()
        {
            base.Start();

            if (optionsModel == null)
            {
                Debug.LogError("[OptionsWindow] OptionsModel is not assigned! Please assign it in the Inspector.", gameObject);
                throw new System.NullReferenceException($"[OptionsWindow] OptionsModel is null on GameObject '{gameObject.name}'. This component requires a OptionsModel to function.");
            }

            if (microphoneDropdown != null)
            {
                microphoneDropdown.onValueChanged.AddListener(OnMicrophoneDropdownChanged);
            }

            SetupObservers();
        }

        protected override void OnShow()
        {
            base.OnShow();
            optionsService.RefreshMicrophones();
        }

        protected override void OnDestroy()
        {
            CleanupObservers();
            if (microphoneDropdown != null)
            {
                microphoneDropdown.onValueChanged.RemoveListener(OnMicrophoneDropdownChanged);
            }
            base.OnDestroy();
        }

        void SetupObservers()
        {
            _microphonesObserver = new MicrophonesObserver(this);
            _selectedMicrophoneObserver = new SelectedMicrophoneObserver(this);

            optionsModel.SetSelectedMicrophone(PlayerPrefs.GetString("SelectedMicrophone", optionsModel.SelectedMicrophone.Value));
            optionsModel.AvailableMicrophones.Attach(_microphonesObserver);
            optionsModel.SelectedMicrophone.Attach(_selectedMicrophoneObserver);
        }

        void CleanupObservers()
        {
            if (optionsModel != null)
            {
                optionsModel.AvailableMicrophones.Detach(_microphonesObserver);
                optionsModel.SelectedMicrophone.Detach(_selectedMicrophoneObserver);
            }
        }

        void UpdateMicrophonesList(List<string> microphones)
        {
            if (microphoneDropdown == null) return;

            microphoneDropdown.ClearOptions();
            
            // Add "Default" option if not present, or handle it as needed.
            // Dissonance usually returns device names.
            // We can add a "Default" option at the top if we want to allow unsetting specific device.
            // But for now let's just list what we get.
            
            List<string> options = new()
            {
                "Default" // Always allow default
            };
            if (microphones != null)
            {
                options.AddRange(microphones);
            }
            
            microphoneDropdown.AddOptions(options);
        }

        void UpdateSelectedMicrophone(string selectedMic)
        {
            if (microphoneDropdown == null) return;
            if (string.IsNullOrEmpty(selectedMic))
            {
                Debug.LogWarning("[OptionsWindow] Selected microphone is null or empty. Defaulting to 'Default'.");
            }
            string target = string.IsNullOrEmpty(selectedMic) ? "Default" : selectedMic;
            
            // Find index
            int index = 0;
            for (int i = 0; i < microphoneDropdown.options.Count; i++)
            {
                if (microphoneDropdown.options[i].text == target)
                {
                    index = i;
                    break;
                }
            }
            
            microphoneDropdown.SetValueWithoutNotify(index);
            OnMicrophoneDropdownChanged(index); // Ensure service is updated with the current selection, especially on initial load.
        }

        private void OnMicrophoneDropdownChanged(int index)
        {
            string selectedOption = microphoneDropdown.options[index].text;
            optionsService.SetMicrophone(selectedOption);
            PlayerPrefs.SetString("SelectedMicrophone", selectedOption);
        }

        // Observer classes
        private class MicrophonesObserver : IObserver<List<string>>
        {
            private readonly OptionsWindow _window;
            public MicrophonesObserver(OptionsWindow window) => _window = window;
            public void OnNotify(List<string> data) => _window.UpdateMicrophonesList(data);
        }

        private class SelectedMicrophoneObserver : IObserver<string>
        {
            private readonly OptionsWindow _window;
            public SelectedMicrophoneObserver(OptionsWindow window) => _window = window;
            public void OnNotify(string data) => _window.UpdateSelectedMicrophone(data);
        }
    }
}

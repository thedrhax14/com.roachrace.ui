using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RoachRace.UI.Models;
using Dissonance;
using Dissonance.Integrations.FMOD_Recording;

namespace RoachRace.UI.Services
{
    public class OptionsService : MonoBehaviour
    {
        [SerializeField] private OptionsModel optionsModel;

        private void Awake()
        {
            if (optionsModel == null)
            {
                Debug.LogError("[OptionsService] OptionsModel is not assigned! Please assign it in the Inspector.", gameObject);
                throw new System.NullReferenceException($"[OptionsService] OptionsModel is null on GameObject '{gameObject.name}'. This component requires a OptionsModel to function.");
            }
            optionsModel.SetSelectedMicrophone(PlayerPrefs.GetString("SelectedMicrophone", optionsModel.SelectedMicrophone.Value));
            SetMicrophone(optionsModel.SelectedMicrophone.Value);
        }

        public void RefreshMicrophones()
        {
            var comms = DissonanceComms.GetSingleton();
            if (comms == null)
            {
                Debug.LogWarning("[OptionsService] DissonanceComms not found in scene. Cannot refresh microphones.");
                return;
            }

            List<string> devices = new List<string>();
            // Use FMOD integration to get devices
            FMODMicrophoneInput.GetDevices(devices);
            
            optionsModel.SetAvailableMicrophones(devices);
            
            // Also update the currently selected mic from Dissonance
            if (comms.MicrophoneName != null)
            {
                optionsModel.SetSelectedMicrophone(comms.MicrophoneName);
            }
            else
            {
                // If null, it might be using default.
                optionsModel.SetSelectedMicrophone("Default"); 
            }
        }

        public void SetMicrophone(string microphoneName)
        {
            var comms = DissonanceComms.GetSingleton();
            if (comms == null)
            {
                Debug.LogWarning("[OptionsService] DissonanceComms not found in scene. Cannot set microphone.");
                return;
            }

            if (microphoneName == "Default")
            {
                comms.MicrophoneName = null;
            }
            else
            {
                comms.MicrophoneName = microphoneName;
            }
            Debug.Log($"[OptionsService] Microphone set to: {comms.MicrophoneName ?? "Default"}");
        }
    }
}

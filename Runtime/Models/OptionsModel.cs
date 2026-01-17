using System.Collections.Generic;
using RoachRace.UI.Core;
using UnityEngine;

namespace RoachRace.UI.Models
{
    [CreateAssetMenu(fileName = "OptionsModel", menuName = "RoachRace/Models/OptionsModel")]
    public class OptionsModel : UIModel
    {
        public readonly Observable<List<string>> AvailableMicrophones = new Observable<List<string>>(new List<string>());
        public readonly Observable<string> SelectedMicrophone = new Observable<string>(string.Empty);

        public void SetAvailableMicrophones(List<string> microphones)
        {
            AvailableMicrophones.Value = microphones;
        }

        public void SetSelectedMicrophone(string microphone)
        {
            SelectedMicrophone.Value = microphone;
        }
    }
}

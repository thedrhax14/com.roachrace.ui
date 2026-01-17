using UnityEngine;
using RoachRace.UI.Core;
using RoachRace.Data;
using System.Collections.Generic;

namespace RoachRace.UI.Models
{
    /// <summary>
    /// Model that manages region/location data from Edgegap API
    /// </summary>
    [CreateAssetMenu(fileName = "RegionsModel", menuName = "RoachRace/UI/Regions Model")]
    public class RegionsModel : UIModel
    {
        [Header("API Configuration")]
        [SerializeField] private string apiUrl = "https://api.edgegap.com/v1/locations";
        [SerializeField] private EdgegapApiConfig apiConfig;

        [Header("Observable Properties")]
        public Observable<List<Location>> AvailableRegions = new Observable<List<Location>>(new List<Location>());
        public Observable<Location> SelectedRegion = new Observable<Location>(null);
        public Observable<string> ErrorMessage = new Observable<string>("");

        public string ApiUrl => apiUrl;
        public string ApiKey => apiConfig != null ? apiConfig.ApiKey : "";

        public void SetApiConfig(EdgegapApiConfig config)
        {
            apiConfig = config;
        }

        public void SetAvailableRegions(List<Location> regions)
        {
            AvailableRegions.Value = regions;
        }

        public void SelectRegion(Location region)
        {
            SelectedRegion.Value = region;
        }

        public void SelectRegionByCity(string city)
        {
            var region = AvailableRegions.Value.Find(r => r.city == city);
            if (region != null)
            {
                SelectRegion(region);
            }
        }

        public void SetError(string error)
        {
            ErrorMessage.Value = error;
        }

        public void ClearError()
        {
            ErrorMessage.Value = "";
        }

        protected override void Initialize()
        {
            base.Initialize();
            AvailableRegions.Value = new List<Location>();
            SelectedRegion.Value = null;
            ErrorMessage.Value = "";
        }
    }
}

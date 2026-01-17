using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using RoachRace.UI.Core;
using RoachRace.UI.Models;
using RoachRace.UI.Services;
using RoachRace.Data;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// Regions window that displays available regions and allows user to select one
    /// </summary>
    public class RegionsWindow : UIWindow
    {
        [Header("References")]
        [SerializeField] private RegionsModel regionsModel;
        [SerializeField] private RegionsService regionsService;
        
        [Header("UI Elements")]
        [SerializeField] private Transform regionsContainer;
        [SerializeField] private RegionItem regionItemPrefab;
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private TextMeshProUGUI selectedRegionText;

        private List<RegionItem> _regionItems = new List<RegionItem>();
        private RegionObserver _regionsObserver;
        private ErrorObserver _errorObserver;
        private SelectedRegionObserver _selectedRegionObserver;

        protected override void Start()
        {
            base.Start();
            
            if (regionsModel == null)
            {
                Debug.LogError("[RegionsWindow] RegionsModel is not assigned! Please assign it in the Inspector.", gameObject);
                throw new System.NullReferenceException($"[RegionsWindow] RegionsModel is null on GameObject '{gameObject.name}'. This component requires a RegionsModel to function.");
            }

            SetupObservers();
        }

        protected override void OnShow()
        {
            base.OnShow();
            regionsService.FetchRegions();
        }

        protected override void OnDestroy()
        {
            CleanupObservers();
            base.OnDestroy();
        }

        private void SetupObservers()
        {
            _regionsObserver = new RegionObserver(this);
            _errorObserver = new ErrorObserver(this);
            _selectedRegionObserver = new SelectedRegionObserver(this);

            regionsModel.AvailableRegions.Attach(_regionsObserver);
            regionsModel.ErrorMessage.Attach(_errorObserver);
            regionsModel.SelectedRegion.Attach(_selectedRegionObserver);
        }

        private void CleanupObservers()
        {
            if (regionsModel != null)
            {
                regionsModel.AvailableRegions.Detach(_regionsObserver);
                regionsModel.ErrorMessage.Detach(_errorObserver);
                regionsModel.SelectedRegion.Detach(_selectedRegionObserver);
            }
        }

        private void UpdateRegionsList(List<Location> regions)
        {
            ClearRegionItems();

            if (regionsContainer == null || regionItemPrefab == null)
            {
                Debug.LogWarning("RegionsContainer or RegionItemPrefab is not assigned!");
                return;
            }

            foreach (var location in regions)
            {
                RegionItem item = Instantiate(regionItemPrefab, regionsContainer);
                
                if (item != null)
                {
                    item.Setup(location, regionsModel);
                    _regionItems.Add(item);
                }
            }
        }

        private void ClearRegionItems()
        {
            foreach (var item in _regionItems)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
            _regionItems.Clear();
        }

        private void UpdateErrorDisplay(string error)
        {
            if (errorText != null)
            {
                errorText.gameObject.SetActive(!string.IsNullOrEmpty(error));
                errorText.text = error;
            }
        }

        private void UpdateSelectedRegionDisplay(Location region)
        {
            if (selectedRegionText != null)
            {
                if (region != null)
                {
                    selectedRegionText.text = $"Selected: {region.id}";
                }
                else
                {
                    selectedRegionText.text = "No region selected";
                }
            }

            // Update visual state of region items
            foreach (var item in _regionItems)
            {
                item.UpdateSelectionState(region);
            }

            // Open PlayWindow when a region is selected
            if (region != null)
            {
                UIWindowManager.Instance?.ShowWindow<PlayWindow>();
            }
        }

        // Observer classes
        private class RegionObserver : IObserver<List<Location>>
        {
            private readonly RegionsWindow _window;
            public RegionObserver(RegionsWindow window) => _window = window;
            public void OnNotify(List<Location> data) => _window.UpdateRegionsList(data);
        }

        private class ErrorObserver : IObserver<string>
        {
            private readonly RegionsWindow _window;
            public ErrorObserver(RegionsWindow window) => _window = window;
            public void OnNotify(string data) => _window.UpdateErrorDisplay(data);
        }

        private class SelectedRegionObserver : IObserver<Location>
        {
            private readonly RegionsWindow _window;
            public SelectedRegionObserver(RegionsWindow window) => _window = window;
            public void OnNotify(Location data) => _window.UpdateSelectedRegionDisplay(data);
        }
    }
}

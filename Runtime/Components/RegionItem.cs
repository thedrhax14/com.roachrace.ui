using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoachRace.Data;
using RoachRace.UI.Models;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// Individual region item in the regions list
    /// </summary>
    public class RegionItem : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI regionNameText;
        [SerializeField] private TextMeshProUGUI playerCountText;
        [SerializeField] private TextMeshProUGUI latencyText;
        [SerializeField] private Button selectButton;
        [SerializeField] private GameObject selectedIndicator;

        private Location _location;
        private RegionsModel _regionsModel;

        public void Setup(Location location, RegionsModel model)
        {
            _location = location;
            _regionsModel = model;

            UpdateDisplay();
            SetupButton();
        }

        private void OnDestroy()
        {
            if (selectButton != null)
            {
                selectButton.onClick.RemoveListener(OnSelectClicked);
            }
        }

        private void UpdateDisplay()
        {
            if (_location == null) return;

            if (regionNameText != null)
            {
                regionNameText.text = _location.id;
            }

            if (playerCountText != null)
            {
                playerCountText.text = $"{_location.numberOfPlayers}";
            }

            if (latencyText != null)
            {
                latencyText.text = $"{_location.latency:F0}ms";
            }
        }

        private void SetupButton()
        {
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(OnSelectClicked);
            }
        }

        private void OnSelectClicked()
        {
            if (_regionsModel != null && _location != null)
            {
                _regionsModel.SelectRegion(_location);
            }
        }

        public void UpdateSelectionState(Location selectedRegion)
        {
            bool isSelected = selectedRegion != null && 
                              selectedRegion.city == _location.city &&
                              selectedRegion.country == _location.country;

            if (selectedIndicator != null)
            {
                selectedIndicator.SetActive(isSelected);
            }
        }
    }
}

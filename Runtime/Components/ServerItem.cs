using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoachRace.Data;
using RoachRace.UI.Models;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// UI component for displaying individual server deployment information
    /// </summary>
    public class ServerItem : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI roomNameText;
        [SerializeField] private TextMeshProUGUI hostNameText;
        [SerializeField] private TextMeshProUGUI playerCountText;
        [SerializeField] private Button selectButton;

        private ServerDeployment _server;
        private ServersModel _serversModel;

        public void Setup(ServerDeployment server, ServersModel model)
        {
            _server = server;
            _serversModel = model;

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
            if (_server == null) return;

            if (roomNameText != null)
            {
                roomNameText.text = _server.server_name;
            }

            if (hostNameText != null)
            {
                hostNameText.text = $"Host: {_server.created_by}";
            }

            if (playerCountText != null)
            {
                playerCountText.text = $"Players: {_server.number_of_players}";
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
            if (_serversModel != null && _server != null)
            {
                _serversModel.SelectServer(_server);
            }
        }
    }
}

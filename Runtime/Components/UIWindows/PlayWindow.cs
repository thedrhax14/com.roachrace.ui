using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using RoachRace.UI.Services;
using RoachRace.UI.Models;
using RoachRace.Data;
using RoachRace.UI.Core;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// Play window that allows users to create a new server or join existing ones
    /// Merges functionality of previous ServersWindow
    /// </summary>
    public class PlayWindow : UIWindow
    {
        [Header("References")]
        [SerializeField] private DeploymentService deploymentService;
        [SerializeField] private ServersModel serversModel;
        [SerializeField] private ServersService serversService;

        [Header("Local Host Settings")]
        [SerializeField] private ushort localHostPort = 7777;

        [Header("UI Elements")]
        [SerializeField] private Button createRoomButton;
        [SerializeField] private Transform serversContainer;
        [SerializeField] private ServerItem serverItemPrefab;
        [SerializeField] private TextMeshProUGUI errorText;
#if UNITY_EDITOR
        [SerializeField] private Button hostLocalButton;
#endif

        private List<ServerItem> _serverItems = new List<ServerItem>();
        private ServersObserver _serversObserver;
        private ErrorObserver _errorObserver;
        private SelectedServerObserver _selectedServerObserver;

        protected override void Start()
        {
            base.Start();
            
            if (serversModel == null)
            {
                Debug.LogError("[PlayWindow] ServersModel is not assigned! Please assign it in the Inspector.", gameObject);
                throw new System.NullReferenceException($"[PlayWindow] ServersModel is null on GameObject '{gameObject.name}'. This component requires a ServersModel to function.");
            }

            SetupButtons();
            SetupObservers();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CleanupButtons();
            CleanupObservers();
        }

        private void SetupButtons()
        {
            if (createRoomButton != null)
            {
                createRoomButton.onClick.AddListener(OnCreateRoomClicked);
            }

#if UNITY_EDITOR
            if (hostLocalButton != null)
            {
                hostLocalButton.onClick.AddListener(OnHostLocalClicked);
            }
#endif
        }

        protected override void OnShow()
        {
            base.OnShow();
            OnRefreshClicked();
        }

        private void CleanupButtons()
        {
            if (createRoomButton != null)
            {
                createRoomButton.onClick.RemoveListener(OnCreateRoomClicked);
            }

#if UNITY_EDITOR
            if (hostLocalButton != null)
            {
                hostLocalButton.onClick.RemoveListener(OnHostLocalClicked);
            }
#endif
        }

        private void SetupObservers()
        {
            _serversObserver = new ServersObserver(this);
            _errorObserver = new ErrorObserver(this);
            _selectedServerObserver = new SelectedServerObserver(this);

            serversModel.AvailableServers.Attach(_serversObserver);
            serversModel.ErrorMessage.Attach(_errorObserver);
            serversModel.SelectedServer.Attach(_selectedServerObserver);
        }

        private void CleanupObservers()
        {
            if (serversModel != null)
            {
                serversModel.AvailableServers.Detach(_serversObserver);
                serversModel.ErrorMessage.Detach(_errorObserver);
                serversModel.SelectedServer.Detach(_selectedServerObserver);
            }
        }

        private void OnCreateRoomClicked()
        {
            if (deploymentService == null)
            {
                Debug.LogError("[PlayWindow] DeploymentService is not assigned! Please assign it in the Inspector.", gameObject);
                throw new System.NullReferenceException($"[PlayWindow] DeploymentService is null on GameObject '{gameObject.name}'. Cannot create deployment.");
            }
            
            // Generate server name with version and timestamp
            long timestampMs = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string serverName = $"RoachRaceServer-{Application.version}-{timestampMs}";

            // Start deployment process
            deploymentService.CreateDeployment(serverName);

            // Open Room window (deployment service will update connection info when ready)
            UIWindowManager.Instance?.ShowWindow<RoomWindow>();
        }

        private void OnRefreshClicked()
        {
            if (serversService != null)
            {
                serversService.FetchServers();
            }
        }

        private void OnHostLocalClicked()
        {
            if (serversModel == null)
            {
                Debug.LogError("[PlayWindow] ServersModel is not assigned! Please assign it in the Inspector.", gameObject);
                throw new System.NullReferenceException($"[PlayWindow] ServersModel is null on GameObject '{gameObject.name}'. Cannot host local server.");
            }
            
            // Create a local server deployment entry
            ServerDeployment localServer = new ServerDeployment
            {
                request_id = "localhost",
                server_name = "Local Host",
                created_by = "You",
                number_of_players = 0,
                public_ip = "localhost",
                udp_port = localHostPort
            };

            // Update servers model with local server
            serversModel.SelectServer(localServer);
            Debug.Log($"[PlayWindow] Hosting local server on localhost:{localHostPort}");

            // Open Room window (NetworkConnectionObserver will auto-start server and connect)
            UIWindowManager.Instance?.ShowWindow<RoomWindow>();
        }

        private void UpdateServersList(List<ServerDeployment> servers)
        {
            ClearServerItems();

            if (serversContainer == null || serverItemPrefab == null)
            {
                Debug.LogWarning("ServersContainer or ServerItemPrefab is not assigned!", gameObject);
                return;
            }

            foreach (var server in servers)
            {
                ServerItem item = Instantiate(serverItemPrefab, serversContainer);
                
                if (item != null)
                {
                    item.Setup(server, serversModel);
                    _serverItems.Add(item);
                }
            }
        }

        private void ClearServerItems()
        {
            foreach (var item in _serverItems)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
            _serverItems.Clear();
        }

        private void UpdateErrorDisplay(string error)
        {
            if (errorText != null)
            {
                errorText.gameObject.SetActive(!string.IsNullOrEmpty(error));
                errorText.text = error;
            }
        }

        private void OnServerSelected(ServerDeployment server)
        {
            if (server != null)
            {
                // Open the Room window when a server is selected
                UIWindowManager.Instance?.ShowWindow<RoomWindow>();
            }
        }

        // Observer classes
        private class ServersObserver : IObserver<List<ServerDeployment>>
        {
            private readonly PlayWindow _window;
            public ServersObserver(PlayWindow window) => _window = window;
            public void OnNotify(List<ServerDeployment> data) => _window.UpdateServersList(data);
        }

        private class ErrorObserver : IObserver<string>
        {
            private readonly PlayWindow _window;
            public ErrorObserver(PlayWindow window) => _window = window;
            public void OnNotify(string data) => _window.UpdateErrorDisplay(data);
        }

        private class SelectedServerObserver : IObserver<ServerDeployment>
        {
            private readonly PlayWindow _window;
            public SelectedServerObserver(PlayWindow window) => _window = window;
            public void OnNotify(ServerDeployment data) => _window.OnServerSelected(data);
        }
    }
}

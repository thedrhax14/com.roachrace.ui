using UnityEngine;
using RoachRace.UI.Core;
using RoachRace.Data;
using System.Collections.Generic;

namespace RoachRace.UI.Models
{
    /// <summary>
    /// Model that manages server deployments from Edgegap API
    /// </summary>
    [CreateAssetMenu(fileName = "ServersModel", menuName = "RoachRace/UI/Servers Model")]
    public class ServersModel : UIModel
    {
        [Header("API Configuration")]
        [SerializeField] private string apiUrl = "https://api.edgegap.com/v1/deployments";
        [SerializeField] private EdgegapApiConfig apiConfig;

        [Header("Observable Properties")]
        public Observable<List<ServerDeployment>> AvailableServers = new Observable<List<ServerDeployment>>(new List<ServerDeployment>());
        public Observable<ServerDeployment> SelectedServer = new Observable<ServerDeployment>(null);
        public Observable<ServerConnectionInfo> ConnectionInfo = new Observable<ServerConnectionInfo>(null);
        public Observable<string> ErrorMessage = new Observable<string>("");

        public string ApiUrl => apiUrl;
        public string ApiKey => apiConfig != null ? apiConfig.ApiKey : "";

        public void SetApiConfig(EdgegapApiConfig config)
        {
            apiConfig = config;
        }

        public void SetAvailableServers(List<ServerDeployment> servers)
        {
            AvailableServers.Value = servers;
        }

        public void SelectServer(ServerDeployment server)
        {
            SelectedServer.Value = server;
            
            // Update connection info when server is selected
            if (server != null)
            {
                ConnectionInfo.Value = server.GetConnectionInfo();
            }
            else
            {
                ConnectionInfo.Value = null;
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
            AvailableServers.Value = new List<ServerDeployment>();
            SelectedServer.Value = null;
            ConnectionInfo.Value = null;
            ErrorMessage.Value = "";
        }
    }
}

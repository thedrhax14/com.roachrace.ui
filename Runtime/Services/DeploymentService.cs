using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using RoachRace.UI.Models;
using RoachRace.Data;
using RoachRace.Data.DTOs;
using Newtonsoft.Json;

namespace RoachRace.UI.Services
{
    /// <summary>
    /// Service for creating and monitoring Edgegap server deployments
    /// </summary>
    public class DeploymentService : MonoBehaviour
    {
        [SerializeField] private ServersModel serversModel;
        [SerializeField] private EdgegapApiConfig apiConfig;
        [SerializeField] private LoadingManager loadingManager;

        private const string DEPLOY_URL = "https://api.edgegap.com/v2/deployments";
        private const string STATUS_URL = "https://api.edgegap.com/v1/status";
        private const float STATUS_CHECK_INTERVAL = 5f;

        public void CreateDeployment(string serverName)
        {
            if (apiConfig == null || !apiConfig.IsConfigured())
            {
                serversModel?.SetError("API configuration is not set");
                Debug.LogError("EdgegapApiConfig is not configured!");
                return;
            }

            StartCoroutine(CreateDeploymentCoroutine(serverName));
        }

        private IEnumerator CreateDeploymentCoroutine(string serverName)
        {
            if (loadingManager != null)
            {
                loadingManager.StartLoading("Creating server...");
            }

            serversModel?.ClearError();

            // Build the deployment request
            DeploymentRequestDTO request = BuildDeploymentRequest(serverName);
            string jsonRequest = JsonConvert.SerializeObject(request);
            Debug.Log($"Deployment Request JSON: {jsonRequest}");

            using (UnityWebRequest webRequest = new UnityWebRequest(DEPLOY_URL, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonRequest);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Authorization", apiConfig.ApiKey);
                webRequest.SetRequestHeader("Content-Type", "application/json");

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    DeploymentResponseDTO response = null;
                    
                    try
                    {
                        response = JsonConvert.DeserializeObject<DeploymentResponseDTO>(jsonResponse);
                    }
                    catch (System.Exception e)
                    {
                        serversModel?.SetError($"Parse error: {e.Message}");
                        Debug.LogError($"Error parsing deployment response: {e.Message}");
                        
                        if (loadingManager != null)
                        {
                            loadingManager.EndLoading();
                        }
                    }

                    if (response != null && !string.IsNullOrEmpty(response.request_id))
                    {
                        Debug.Log($"Deployment created with request_id: {response.request_id}");
                        
                        // Start monitoring deployment status
                        yield return MonitorDeploymentStatus(response.request_id);
                    }
                    else if (response != null)
                    {
                        serversModel?.SetError("Invalid deployment response");
                        Debug.LogError("Failed to get request_id from deployment response");
                        
                        if (loadingManager != null)
                        {
                            loadingManager.EndLoading();
                        }
                    }
                }
                else
                {
                    string errorMsg = $"Deployment failed: {webRequest.error}";
                    serversModel?.SetError(errorMsg);
                    Debug.LogError($"Failed to create deployment: {webRequest.error}\nResponse: {webRequest.downloadHandler.text}");
                    
                    if (loadingManager != null)
                    {
                        loadingManager.EndLoading();
                    }
                }
            }
        }

        private IEnumerator MonitorDeploymentStatus(string requestId)
        {
            string statusUrl = $"{STATUS_URL}/{requestId}";
            bool isReady = false;
            int maxAttempts = 60; // Maximum 5 minutes (60 * 5 seconds)
            int attempts = 0;

            while (!isReady && attempts < maxAttempts)
            {
                attempts++;
                
                if (loadingManager != null)
                {
                    loadingManager.LoadingMessage.Value = $"Deploying server... (attempt {attempts})";
                }

                yield return new WaitForSeconds(STATUS_CHECK_INTERVAL);

                using (UnityWebRequest webRequest = UnityWebRequest.Get(statusUrl))
                {
                    webRequest.SetRequestHeader("authorization", apiConfig.ApiKey);
                    webRequest.SetRequestHeader("Accept", "*/*");

                    yield return webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        try
                        {
                            string jsonResponse = webRequest.downloadHandler.text;
                            Debug.Log($"Deployment Status Response: {jsonResponse}");
                            DeploymentStatusDTO status = JsonConvert.DeserializeObject<DeploymentStatusDTO>(jsonResponse);

                            if (status != null)
                            {
                                Debug.Log($"Deployment status: {status.current_status}");

                                if (status.current_status.Contains("READY"))
                                {
                                    isReady = true;
                                    OnDeploymentReady(status);
                                }
                                else if (status.error)
                                {
                                    serversModel?.SetError($"Deployment error: {status.error_detail}");
                                    Debug.LogError($"Deployment error: {status.error_detail}");
                                    break;
                                }
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"Error parsing status response: {e.Message}");
                        }
                    }
                    else
                    {
                        Debug.LogError($"Failed to check deployment status: {webRequest.error}");
                    }
                }
            }

            if (loadingManager != null)
            {
                loadingManager.EndLoading();
            }

            if (!isReady && attempts >= maxAttempts)
            {
                serversModel?.SetError("Deployment timeout - server took too long to start");
                Debug.LogError("Deployment timeout");
            }
        }

        private void OnDeploymentReady(DeploymentStatusDTO status)
        {
            Debug.Log($"Server is ready! IP: {status.public_ip}");

            // Extract UDP port
            int udpPort = 0;
            if (status.ports != null)
            {
                foreach (var port in status.ports)
                {
                    if (port.Value.protocol.ToUpper() == "UDP")
                    {
                        udpPort = port.Value.external;
                        break;
                    }
                }
            }

            // Create ServerDeployment and update the model
            ServerDeployment deployment = new ServerDeployment
            {
                request_id = status.request_id,
                server_name = status.app_name,
                created_by = "You",
                number_of_players = 0,
                public_ip = status.public_ip,
                udp_port = udpPort
            };

            if (serversModel != null)
            {
                serversModel.SelectServer(deployment);
            }

            Debug.Log($"Server connection: {status.public_ip}:{udpPort}");
        }

        private DeploymentRequestDTO BuildDeploymentRequest(string serverName)
        {
            // Determine if running in editor or build
            bool isEditor = Application.isEditor;
            string tag = isEditor ? "RoachRace-editor" : "RoachRace-live";

            // Build users array
            List<UserDTO> users = new List<UserDTO>();

            if (!string.IsNullOrEmpty(apiConfig.UserIpAddress))
            {
                users.Add(new UserDTO
                {
                    user_type = "ip_address",
                    user_data = new UserDataDTO
                    {
                        ip_address = apiConfig.UserIpAddress
                    }
                });
            }

            if (apiConfig.UserLatitude != 0f || apiConfig.UserLongitude != 0f)
            {
                users.Add(new UserDTO
                {
                    user_type = "geo_coordinates",
                    user_data = new UserDataDTO
                    {
                        latitude = apiConfig.UserLatitude,
                        longitude = apiConfig.UserLongitude
                    }
                });
            }

            // Build environment variables
            List<EnvironmentVariableDTO> envVars = new List<EnvironmentVariableDTO>
            {
                new EnvironmentVariableDTO
                {
                    key = "SERVER_NAME",
                    value = serverName,
                    is_hidden = false
                }
            };

            // Create the base request
            DeploymentRequestDTO request = new DeploymentRequestDTO
            {
                application = apiConfig.ApplicationName,
                version = apiConfig.ApplicationVersion,
                require_cached_locations = false,
                users = users.ToArray(),
                environment_variables = envVars.ToArray(),
                tags = new[] { tag }
            };

            // Only add webhooks if URLs are provided
            if (!string.IsNullOrEmpty(apiConfig.WebhookOnReady))
            {
                request.webhook_on_ready = new WebhookDTO { url = apiConfig.WebhookOnReady };
            }
            
            if (!string.IsNullOrEmpty(apiConfig.WebhookOnError))
            {
                request.webhook_on_error = new WebhookDTO { url = apiConfig.WebhookOnError };
            }
            
            if (!string.IsNullOrEmpty(apiConfig.WebhookOnTerminated))
            {
                request.webhook_on_terminated = new WebhookDTO { url = apiConfig.WebhookOnTerminated };
            }

            return request;
        }
    }
}

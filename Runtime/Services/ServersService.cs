using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RoachRace.UI.Models;
using RoachRace.Data;
using RoachRace.Data.DTOs;
using Newtonsoft.Json;

namespace RoachRace.UI.Services
{
    /// <summary>
    /// Service for fetching server deployments from Edgegap API
    /// </summary>
    public class ServersService : MonoBehaviour
    {
        [SerializeField] private ServersModel serversModel;
        [SerializeField] private LoadingManager loadingManager;

        public void FetchServers()
        {
            if (serversModel == null)
            {
                Debug.LogError("[ServersService] ServersModel is not assigned! Please assign it in the Inspector.", gameObject);
                throw new System.NullReferenceException($"[ServersService] ServersModel is null on GameObject '{gameObject.name}'. This component requires a ServersModel to function.");
            }

            if (string.IsNullOrEmpty(serversModel.ApiKey))
            {
                serversModel.SetError("API Key is not configured");
                Debug.LogError("[ServersService] Edgegap API Key is not set in ServersModel!", gameObject);
                return;
            }

            StartCoroutine(FetchServersCoroutine());
        }
        private IEnumerator FetchServersCoroutine()
        {
            serversModel.ClearError();
            
            loadingManager.StartLoading("Fetching servers...");

            using (UnityWebRequest request = UnityWebRequest.Get(serversModel.ApiUrl))
            {
                request.SetRequestHeader("authorization", serversModel.ApiKey);
                request.SetRequestHeader("Accept", "*/*");

                yield return request.SendWebRequest();

                loadingManager.EndLoading();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string jsonResponse = request.downloadHandler.text;
                        DeploymentsResponseDTO responseDto = JsonConvert.DeserializeObject<DeploymentsResponseDTO>(jsonResponse);

                        if (responseDto != null && responseDto.data != null)
                        {
                            // Convert DTOs to domain models
                            List<ServerDeployment> servers = responseDto.data
                                .Select(dto => dto.ToModel())
                                .ToList();
                            
                            serversModel.SetAvailableServers(servers);
                            
                            Debug.Log($"Successfully fetched {servers.Count} servers");
                        }
                        else
                        {
                            serversModel.SetError("Invalid response format");
                            Debug.LogError("Failed to parse deployments response");
                        }
                    }
                    catch (System.Exception e)
                    {
                        serversModel.SetError($"Parse error: {e.Message}");
                        Debug.LogError($"Error parsing servers data: {e.Message}");
                    }
                }
                else
                {
                    string errorMsg = $"API Error: {request.error}";
                    serversModel.SetError(errorMsg);
                    Debug.LogError($"Failed to fetch servers: {request.error}");
                }
            }
        }

        public void SetServersModel(ServersModel model)
        {
            serversModel = model;
        }
    }
}

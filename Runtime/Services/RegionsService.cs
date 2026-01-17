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
    /// Service for fetching regions from Edgegap API
    /// </summary>
    public class RegionsService : MonoBehaviour
    {
        [SerializeField] private RegionsModel regionsModel;
        [SerializeField] private LoadingManager loadingManager;

        public void FetchRegions()
        {
            if (regionsModel == null)
            {
                Debug.LogError("[RegionsService] RegionsModel is not assigned! Please assign it in the Inspector.", gameObject);
                throw new System.NullReferenceException($"[RegionsService] RegionsModel is null on GameObject '{gameObject.name}'. This component requires a RegionsModel to function.");
            }

            if (string.IsNullOrEmpty(regionsModel.ApiKey))
            {
                regionsModel.SetError("API Key is not configured");
                Debug.LogError("[RegionsService] Edgegap API Key is not set in RegionsModel!", gameObject);
                return;
            }

            StartCoroutine(FetchRegionsCoroutine());
        }

        private IEnumerator FetchRegionsCoroutine()
        {
            regionsModel.ClearError();
            
            if (loadingManager != null)
            {
                loadingManager.StartLoading("Fetching regions...");
            }

            using (UnityWebRequest request = UnityWebRequest.Get(regionsModel.ApiUrl))
            {
                request.SetRequestHeader("authorization", regionsModel.ApiKey);
                request.SetRequestHeader("Accept", "*/*");

                yield return request.SendWebRequest();

                if (loadingManager != null)
                {
                    loadingManager.EndLoading();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string jsonResponse = request.downloadHandler.text;
                        LocationsResponseDTO responseDto = JsonConvert.DeserializeObject<LocationsResponseDTO>(jsonResponse);

                        if (responseDto != null && responseDto.locations != null)
                        {
                            // Convert DTOs to domain models
                            List<Location> locations = responseDto.locations
                                .Select(dto => dto.ToModel())
                                .ToList();
                            
                            regionsModel.SetAvailableRegions(locations);
                            
                            Debug.Log($"Successfully fetched {locations.Count} regions");
                        }
                        else
                        {
                            regionsModel.SetError("Invalid response format");
                            Debug.LogError("Failed to parse locations response");
                        }
                    }
                    catch (System.Exception e)
                    {
                        regionsModel.SetError($"Parse error: {e.Message}");
                        Debug.LogError($"Error parsing regions data: {e.Message}");
                    }
                }
                else
                {
                    string errorMsg = $"API Error: {request.error}";
                    regionsModel.SetError(errorMsg);
                    Debug.LogError($"Failed to fetch regions: {request.error}");
                }
            }
        }

        public void SetRegionsModel(RegionsModel model)
        {
            regionsModel = model;
        }
    }
}

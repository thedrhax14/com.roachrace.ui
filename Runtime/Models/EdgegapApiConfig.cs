using UnityEngine;

namespace RoachRace.UI.Models
{
    /// <summary>
    /// Configuration for Edgegap API and deployment settings
    /// </summary>
    [CreateAssetMenu(fileName = "EdgegapApiConfig", menuName = "RoachRace/UI/Edgegap API Config")]
    public class EdgegapApiConfig : ScriptableObject
    {
        [Header("API Configuration")]
        [SerializeField] private string apiKey = "";

        [Header("Deployment Settings")]
        [SerializeField] private string applicationName = "RoachRace";
        [SerializeField] private string applicationVersion = "1.0.0";
        
        [Header("User Location (for optimal server placement)")]
        [SerializeField] private string userIpAddress = "";
        [SerializeField] private float userLatitude = 0f;
        [SerializeField] private float userLongitude = 0f;

        [Header("Webhooks (optional)")]
        [SerializeField] private string webhookOnReady = "";
        [SerializeField] private string webhookOnError = "";
        [SerializeField] private string webhookOnTerminated = "";

        public string ApiKey => apiKey;
        public string ApplicationName => applicationName;
        public string ApplicationVersion => applicationVersion;
        public string UserIpAddress => userIpAddress;
        public float UserLatitude => userLatitude;
        public float UserLongitude => userLongitude;
        public string WebhookOnReady => webhookOnReady;
        public string WebhookOnError => webhookOnError;
        public string WebhookOnTerminated => webhookOnTerminated;

        public void SetApiKey(string key)
        {
            apiKey = key;
        }

        public bool IsConfigured()
        {
            return !string.IsNullOrEmpty(apiKey);
        }
    }
}

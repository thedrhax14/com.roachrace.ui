using UnityEngine;
using TMPro;
using RoachRace.UI.Core;
using RoachRace.UI.Models;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// Global loading UI that observes the LoadingManager
    /// </summary>
    public class LoadingUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LoadingManager loadingManager;

        [Header("UI Elements")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private TextMeshProUGUI loadingText;

        private LoadingStateObserver _loadingStateObserver;
        private LoadingMessageObserver _loadingMessageObserver;

        private void Start()
        {
            if (loadingManager == null)
            {
                Debug.LogError("LoadingManager is not assigned to LoadingUI!");
                return;
            }

            SetupObservers();
        }

        private void OnDestroy()
        {
            CleanupObservers();
        }

        private void SetupObservers()
        {
            _loadingStateObserver = new LoadingStateObserver(this);
            _loadingMessageObserver = new LoadingMessageObserver(this);

            loadingManager.IsLoading.Attach(_loadingStateObserver);
            loadingManager.LoadingMessage.Attach(_loadingMessageObserver);
        }

        private void CleanupObservers()
        {
            if (loadingManager != null)
            {
                loadingManager.IsLoading.Detach(_loadingStateObserver);
                loadingManager.LoadingMessage.Detach(_loadingMessageObserver);
            }
        }

        private void UpdateLoadingState(bool isLoading)
        {
            loadingPanel.SetActive(isLoading);
        }

        private void UpdateLoadingMessage(string message)
        {
            loadingText.text = message;
        }

        // Observer classes
        private class LoadingStateObserver : IObserver<bool>
        {
            private readonly LoadingUI _ui;
            public LoadingStateObserver(LoadingUI ui) => _ui = ui;
            public void OnNotify(bool data) => _ui.UpdateLoadingState(data);
        }

        private class LoadingMessageObserver : IObserver<string>
        {
            private readonly LoadingUI _ui;
            public LoadingMessageObserver(LoadingUI ui) => _ui = ui;
            public void OnNotify(string data) => _ui.UpdateLoadingMessage(data);
        }
    }
}

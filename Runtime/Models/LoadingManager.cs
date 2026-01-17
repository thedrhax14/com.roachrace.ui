using UnityEngine;
using RoachRace.UI.Core;

namespace RoachRace.UI.Models
{
    /// <summary>
    /// Global loading state manager that tracks all loading operations
    /// </summary>
    [CreateAssetMenu(fileName = "LoadingManager", menuName = "RoachRace/UI/Loading Manager")]
    public class LoadingManager : UIModel
    {
        [Header("Observable Properties")]
        public Observable<bool> IsLoading = new Observable<bool>(false);
        public Observable<string> LoadingMessage = new Observable<string>("");

        private int _loadingOperationsCount = 0;

        /// <summary>
        /// Start a loading operation
        /// </summary>
        public void StartLoading(string message = "Loading...")
        {
            _loadingOperationsCount++;
            LoadingMessage.Value = message;
            UpdateLoadingState();
        }

        /// <summary>
        /// End a loading operation
        /// </summary>
        public void EndLoading()
        {
            _loadingOperationsCount = Mathf.Max(0, _loadingOperationsCount - 1);
            UpdateLoadingState();
            
            if (_loadingOperationsCount == 0)
            {
                LoadingMessage.Value = "";
            }
        }

        /// <summary>
        /// Force clear all loading operations
        /// </summary>
        public void ClearAllLoading()
        {
            _loadingOperationsCount = 0;
            LoadingMessage.Value = "";
            UpdateLoadingState();
        }

        private void UpdateLoadingState()
        {
            IsLoading.Value = _loadingOperationsCount > 0;
        }

        protected override void Initialize()
        {
            base.Initialize();
            _loadingOperationsCount = 0;
            IsLoading.Value = false;
            LoadingMessage.Value = "";
        }
    }
}

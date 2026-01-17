using System;
using System.Collections.Generic;
using RoachRace.UI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace RoachRace.UI.Core
{
    /// <summary>
    /// Centralized UI window management system with type-safe window retrieval
    /// Windows auto-register on Awake
    /// </summary>
    public class UIWindowManager : MonoBehaviour
    {
        private static UIWindowManager _instance;
        public static UIWindowManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<UIWindowManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("UIWindowManager");
                        _instance = go.AddComponent<UIWindowManager>();
                        Debug.LogWarning("[UIWindowManager] No instance found - created new one");
                    }
                }
                return _instance;
            }
        }

        public static bool HasInstance => _instance != null;

        [Header("Global Controls")]
        [SerializeField] private Button _quitButton;

        private void Start()
        {
            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(QuitApplication);
            }
        }

        private Dictionary<Type, UIWindow> _registeredWindows = new Dictionary<Type, UIWindow>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("[UIWindowManager] Duplicate instance detected - destroying this one");
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        /// <summary>
        /// Register a window for type-safe retrieval
        /// Called automatically by UIWindow.Awake()
        /// </summary>
        public void RegisterWindow(UIWindow window)
        {
            Type windowType = window.GetType();
            
            if (_registeredWindows.ContainsKey(windowType))
            {
                Debug.LogWarning($"[UIWindowManager] Window of type {windowType.Name} already registered - replacing", window);
            }
            
            _registeredWindows[windowType] = window;
            // Debug.Log($"[UIWindowManager] Registered window: {windowType.Name}", window);

            if (window.OpenButton != null)
            {
                window.OpenAction = () => ShowWindow(windowType);
                window.OpenButton.onClick.AddListener(window.OpenAction);
            }
            else
            {
                Debug.LogWarning($"[UIWindowManager] Open Button is not assigned for window '{windowType.Name}' on '{window.gameObject.name}'.", window.gameObject);
            }
        }

        /// <summary>
        /// Unregister a window
        /// Called automatically by UIWindow.OnDestroy()
        /// </summary>
        public void UnregisterWindow(UIWindow window)
        {
            Type windowType = window.GetType();
            
            if (_registeredWindows.ContainsKey(windowType))
            {
                if (window.OpenButton != null && window.OpenAction != null)
                {
                    window.OpenButton.onClick.RemoveListener(window.OpenAction);
                    window.OpenAction = null;
                }

                _registeredWindows.Remove(windowType);
                Debug.Log($"[UIWindowManager] Unregistered window: {windowType.Name}");
            }
        }

        /// <summary>
        /// Get a window by type
        /// </summary>
        public T GetWindow<T>() where T : UIWindow
        {
            Type windowType = typeof(T);
            
            if (_registeredWindows.TryGetValue(windowType, out UIWindow window))
            {
                return window as T;
            }
            
            Debug.LogWarning($"[UIWindowManager] Window of type {windowType.Name} not found");
            return null;
        }

        /// <summary>
        /// Show a window by type
        /// </summary>
        public void ShowWindow<T>() where T : UIWindow
        {
            ShowWindow(typeof(T));
        }

        /// <summary>
        /// Show a window by type (non-generic)
        /// </summary>
        public void ShowWindow(Type windowType)
        {
            if (_registeredWindows.TryGetValue(windowType, out UIWindow window))
            {
                foreach (var win in _registeredWindows.Values)
                {
                    if (win != window)
                    {
                        win.Hide();
                    }
                }
                window.Show();
            }
            else
            {
                Debug.LogWarning($"[UIWindowManager] Window of type {windowType.Name} not found");
            }
        }

        /// <summary>
        /// Hide a window by type
        /// </summary>
        public void HideWindow<T>() where T : UIWindow
        {
            T window = GetWindow<T>();
            if (window != null)
            {
                window.Hide();
            }
        }

        /// <summary>
        /// Hide all registered windows
        /// </summary>
        public void HideAllWindows()
        {
            foreach (var window in _registeredWindows.Values)
            {
                window.Hide();
            }
            Debug.Log("[UIWindowManager] All windows hidden");
        }

        /// <summary>
        /// Check if a window is registered
        /// </summary>
        public bool HasWindow<T>() where T : UIWindow
        {
            return _registeredWindows.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Exits the application
        /// </summary>
        public void QuitApplication()
        {
            Debug.Log("[UIWindowManager] Quitting application...");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}

using UnityEngine;
using RoachRace.Data;
using RoachRace.UI.Core;
using RoachRace.UI.Models;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// Observes game state and controls which UI windows are visible
    /// Centralized UI flow control based on game state transitions
    /// </summary>
    public class UIStateController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private GameStateModel gameStateModel;
        [SerializeField] private UIWindowManager windowManager;
        [SerializeField] private GameObject mainMenuTabs;

        private GameStateObserver _gameStateObserver;

        private void Awake()
        {
            if (windowManager == null)
            {
                windowManager = UIWindowManager.Instance;
            }
            _gameStateObserver = new GameStateObserver(this);
        }

        private void Start()
        {
            if (gameStateModel == null)
            {
                Debug.LogError("[UIStateController] GameStateModel is not assigned! Please assign it in the Inspector.", gameObject);
                throw new System.NullReferenceException($"[UIStateController] GameStateModel is null on GameObject '{gameObject.name}'. This component requires a GameStateModel to function.");
            }

            // Set initial state
            windowManager.ShowWindow<HomeWindow>();
        }

        private void OnEnable()
        {
            if (gameStateModel == null) return;

            gameStateModel.CurrentState.Attach(_gameStateObserver);
        }

        private void OnDisable()
        {
            if (gameStateModel == null) return;

            gameStateModel.CurrentState.Detach(_gameStateObserver);
        }

        private void UpdateUIState(GameState state)
        {
            switch (state)
            {
                case GameState.Lobby:
                    mainMenuTabs.SetActive(true);
                    windowManager.GetWindow<RoomWindow>().SetInteractable(true);
                    break;
                case GameState.Starting:
                    windowManager.ShowWindow<RoomWindow>();
                    windowManager.GetWindow<RoomWindow>().SetInteractable(false);
                    break;
                case GameState.InProgress:
                    mainMenuTabs.SetActive(false);
                    windowManager.HideWindow<HomeWindow>();
                    windowManager.ShowWindow<GameWindow>();
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    break;
                case GameState.GameOver:
                    windowManager.ShowWindow<GameOverWindow>();
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;
            }

            Debug.Log($"[UIStateController] UI state updated - GameState: {state}");
        }

        private class GameStateObserver : IObserver<GameState>
        {
            private readonly UIStateController _controller;

            public GameStateObserver(UIStateController controller)
            {
                _controller = controller;
            }

            public void OnNotify(GameState state)
            {
                _controller.UpdateUIState(state);
            }
        }
    }
}

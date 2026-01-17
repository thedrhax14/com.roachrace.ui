using System;
using System.Collections.Generic;
using UnityEngine;
using RoachRace.Data;
using RoachRace.UI.Core;
using RoachRace.UI.Models;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// Observes game state and controls which music GameObjects are active.
    /// Activates the GameObject corresponding to the current state and disables others.
    /// </summary>
    public class MusicStateController : MonoBehaviour
    {
        [Serializable]
        public struct GameStateMusicBinding
        {
            public GameState state;
            public GameObject musicObject;
        }

        [Header("Dependencies")]
        [SerializeField] private GameStateModel gameStateModel;
        
        [Header("Configuration")]
        [SerializeField] private List<GameStateMusicBinding> musicBindings;

        private GameStateObserver _gameStateObserver;

        private void Awake()
        {
            _gameStateObserver = new GameStateObserver(this);
        }

        private void Start()
        {
            if (gameStateModel == null)
            {
                Debug.LogError("[MusicStateController] GameStateModel is not assigned! Please assign it in the Inspector.", gameObject);
                throw new System.NullReferenceException($"[MusicStateController] GameStateModel is null on GameObject '{gameObject.name}'. This component requires a GameStateModel to function.");
            }

            // Set initial state
            UpdateMusicState(gameStateModel.CurrentState.Value);
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

        private void UpdateMusicState(GameState state)
        {
            foreach (var binding in musicBindings)
            {
                if (binding.musicObject == null) continue;

                bool shouldBeActive = binding.state == state;
                
                // Only change active state if needed to avoid unnecessary overhead/restarts
                if (binding.musicObject.activeSelf != shouldBeActive)
                {
                    binding.musicObject.SetActive(shouldBeActive);
                }
            }

            Debug.Log($"[MusicStateController] Music state updated - GameState: {state}");
        }

        private class GameStateObserver : Core.IObserver<GameState>
        {
            private readonly MusicStateController _controller;

            public GameStateObserver(MusicStateController controller)
            {
                _controller = controller;
            }

            public void OnNotify(GameState state)
            {
                _controller.UpdateMusicState(state);
            }
        }
    }
}

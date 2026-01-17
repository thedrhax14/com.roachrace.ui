using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoachRace.Data;
using RoachRace.UI.Core;
using RoachRace.UI.Models;
using System.Collections.Generic;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// UI window that displays game over results
    /// Auto-registers with UIWindowManager and responds to GameState changes
    /// </summary>
    public class GameOverWindow : UIWindow, IObserver<GameResult>
    {
        [Header("Dependencies")]
        [SerializeField] private GameStateModel gameStateModel;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private TextMeshProUGUI gameTimeText;
        [SerializeField] private Transform playerStatsContainer;
        [SerializeField] private PlayerStatsItem playerStatsItemPrefab;
        [SerializeField] private Button returnToLobbyButton;

        [Header("Settings")]
        [SerializeField] private Color winnersColor = new Color(0.3f, 1f, 0.3f);
        [SerializeField] private Color losersColor = new Color(1f, 0.3f, 0.3f);

        private List<GameObject> statItemInstances = new List<GameObject>();
        private IGameManager gameManager;

        protected override void Awake()
        {
            base.Awake(); // Register with UIWindowManager
            
            // Ensure this window has its own CanvasGroup for independent visibility control
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            if (returnToLobbyButton != null)
            {
                returnToLobbyButton.onClick.AddListener(OnReturnToLobbyClicked);
            }
        }

        protected override void Start()
        {
            base.Start();
            // Initially hide
            Hide();
        }

        /// <summary>
        /// Set the game manager implementation (called by NetworkGameManager on registration)
        /// </summary>
        public void SetGameManager(IGameManager manager)
        {
            gameManager = manager;
            Debug.Log("[GameOverWindow] Game manager registered");
        }

        void OnEnable()
        {
            if (gameStateModel == null)
            {
                Debug.LogError("[GameOverWindow] GameStateModel is not assigned! Please assign it in the Inspector.", gameObject);
                throw new System.NullReferenceException($"[GameOverWindow] GameStateModel is null on GameObject '{gameObject.name}'. This component requires a GameStateModel to function.");
            }
            
            gameStateModel.CurrentResult.Attach(this);
        }

        void OnDisable()
        {            
            if (gameStateModel != null)
            {
                gameStateModel.CurrentResult.Detach(this);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy(); // Unregister from UIWindowManager
            
            if (returnToLobbyButton != null)
            {
                returnToLobbyButton.onClick.RemoveListener(OnReturnToLobbyClicked);
            }
        }

        public override void Show()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }
            
            gameObject.SetActive(true);
            // Debug.Log("[GameOverWindow] Shown");
        }

        public override void Hide()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
            // Debug.Log("[GameOverWindow] Hidden");
        }

        public void OnNotify(GameResult result)
        {
            if (result == null)
                return;

            DisplayResults(result);
        }

        private void DisplayResults(GameResult result)
        {
            // Clear previous stats
            foreach (var item in statItemInstances)
            {
                Destroy(item);
            }
            statItemInstances.Clear();

            // Update title and result
            if (titleText != null)
            {
                titleText.text = "GAME OVER";
            }

            if (resultText != null)
            {
                string resultMessage = result.survivorsWon ? "SURVIVORS WIN!" : "GHOSTS WIN!";
                resultText.text = resultMessage;
                resultText.color = result.survivorsWon ? winnersColor : losersColor;
            }

            if (gameTimeText != null)
            {
                int minutes = Mathf.FloorToInt(result.totalGameTime / 60f);
                int seconds = Mathf.FloorToInt(result.totalGameTime % 60f);
                gameTimeText.text = $"Game Time: {minutes:00}:{seconds:00}";
            }

            // Display player stats
            if (playerStatsContainer != null && playerStatsItemPrefab != null)
            {
                // Sort: survivors first, then by survival time/deaths
                var sortedStats = new List<PlayerStats>(result.playerStats);
                sortedStats.Sort((a, b) =>
                {
                    // Survivors first
                    if (a.team != b.team)
                        return a.team == Team.Survivor ? -1 : 1;

                    // For survivors: who reached end first, then by survival time
                    if (a.team == Team.Survivor)
                    {
                        if (a.reachedEnd != b.reachedEnd)
                            return a.reachedEnd ? -1 : 1;
                        if (a.reachedEnd && b.reachedEnd)
                            return a.survivalTime.CompareTo(b.survivalTime);
                        return b.respawnsLeft.CompareTo(a.respawnsLeft);
                    }

                    // For ghosts: by kills (deaths they caused)
                    return b.deaths.CompareTo(a.deaths);
                });

                foreach (var stats in sortedStats)
                {
                    PlayerStatsItem item = Instantiate(playerStatsItemPrefab, playerStatsContainer);
                    if (item != null)
                    {
                        item.SetPlayerStats(stats);
                    }
                    statItemInstances.Add(item.gameObject);
                }
            }

            Debug.Log($"[GameOverWindow] Displaying results - {result.playerStats.Length} players");
        }

        private void OnReturnToLobbyClicked()
        {
            if (gameManager != null)
            {
                gameManager.LeaveGame();
                gameStateModel.CurrentState.Value = GameState.Lobby;
                Debug.Log("[GameOverWindow] Returning to lobby");
            }
            else
            {
                Debug.LogWarning("[GameOverWindow] Game manager not registered!");
            }
            
            // Navigate back to PlayWindow
            UIWindowManager.Instance?.ShowWindow<PlayWindow>();
            
            Debug.Log("[GameOverWindow] Leaving game");
        }
    }
}

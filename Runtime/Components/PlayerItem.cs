using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoachRace.Data;
using RoachRace.UI.Core;
using RoachRace.UI.Models;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// UI component for displaying individual player information
    /// </summary>
    public class PlayerItem : MonoBehaviour, IObserver<PlayerStats>
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI teamText;
        [SerializeField] private RawImage playerImage;
        [SerializeField] private TextMeshProUGUI pingText;
        [SerializeField] private Button kickButton;
        [SerializeField] private RawImage speakingIndicator;
        [SerializeField] private GamePlayersModel playersModel;

        [Header("Settings")]
        [SerializeField] private Color ghostTeamColor = new(0.7f, 0.7f, 1f);
        [SerializeField] private Color survivorTeamColor = new(1f, 0.7f, 0.7f);

        public float Amplitude;

        private Observable<PlayerStats> playerStatsObservable;
        private Player _player;

        private void Awake()
        {
            if (kickButton != null)
            {
                kickButton.onClick.AddListener(OnKickClicked);
            }
            if(playersModel == null)
            {
                Debug.LogError($"[{nameof(PlayerItem)}] PlayersModel reference is not set!", gameObject);
            }
        }

        private void OnDestroy()
        {
            if (kickButton != null)
            {
                kickButton.onClick.RemoveListener(OnKickClicked);
            }
        }

        void ResubscribeToPlayerModel()
        {
            playerStatsObservable?.Detach(this);
            if(_player == null) return;
            playerStatsObservable = playersModel.GetPlayerStats(_player.playerName);
            playerStatsObservable.Attach(this);
        }

        /// <summary>
        /// Set the player data to display
        /// </summary>
        public void SetPlayer(Player player)
        {
            _player = player;
            if(player != null) ResubscribeToPlayerModel();
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_player == null)
                return;

            // Update player name
            playerNameText.text = _player.playerName;

            // Update team
            teamText.text = _player.team.ToString();
            teamText.color = _player.team == Team.Ghost ? ghostTeamColor : survivorTeamColor;

            // Update ping if network player is available
            if (pingText != null && _player.networkPlayer != null)
            {
                pingText.text = $"{_player.networkPlayer.GetPing()}ms";
            }
            else if (pingText != null)
            {
                pingText.text = "-";
            }

            // Show/hide kick button based on permissions
            if (kickButton != null && _player.networkPlayer != null)
            {
                // Only show kick button if we're the server and it's not the local player
                bool canKick = _player.networkPlayer.IsServer && !_player.networkPlayer.IsLocalPlayer;
                kickButton.gameObject.SetActive(canKick);
            }
            else if (kickButton != null)
            {
                kickButton.gameObject.SetActive(false);
            }

            // Load player image (placeholder for now)
            if (playerImage != null && !string.IsNullOrEmpty(_player.imageUrl))
            {
                // TODO: Load image from URL using UnityWebRequest or sprite atlas
                Debug.Log($"Loading player image from: {_player.imageUrl}");
            }
        }

        private void OnKickClicked()
        {
            if (_player?.networkPlayer != null)
            {
                _player.networkPlayer.Kick();
            }
        }

        public void OnNotify(PlayerStats data)
        {
            if(data == null)
            {
                Debug.LogWarning($"[{nameof(PlayerItem)}] Received null PlayerStats update.");
                return;
            }
            speakingIndicator.gameObject.SetActive(data.isSpeaking);
            Amplitude = data.Amplitude;
            pingText.text = $"{data.ping}ms";
        }

        void OnDisable()
        {
            playerStatsObservable?.Detach(this);
        }
    }
}

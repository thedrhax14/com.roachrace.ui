using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using RoachRace.Data;
using RoachRace.UI.Core;
using RoachRace.UI.Models;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// UI window that displays room/lobby information and player list
    /// </summary>
    public class RoomWindow : UIWindow, IObserver<RoomInfo>
    {
        [Header("Room Dependencies")]
        [SerializeField] private RoomModel roomModel;

        [Header("UI Elements")]
        [SerializeField] private TMP_InputField roomNameText;
        [SerializeField] private TextMeshProUGUI playerCountText;
        [SerializeField] private Transform playerListContainer;
        [SerializeField] private PlayerItem playerItemPrefab;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button toggleTeamButton;

        private const int ExpectedMaxPlayers = 45;
        private List<PlayerItem> _playerItems = new List<PlayerItem>(ExpectedMaxPlayers);
        private Queue<PlayerItem> _playerItemPool = new Queue<PlayerItem>(ExpectedMaxPlayers);
        
        // Network references (assigned at runtime)
        private INetworkRoom _networkRoom;
        private INetworkPlayer _ownedPlayer;

        protected override void Awake()
        {
            base.Awake();

            if (leaveButton != null)
            {
                leaveButton.onClick.AddListener(OnLeaveClicked);
            }

            if (toggleTeamButton != null)
            {
                toggleTeamButton.onClick.AddListener(OnToggleTeamClicked);
            }

            if (startGameButton != null)
            {
                startGameButton.onClick.AddListener(OnStartGameClicked);
            }
        }

        void OnEnable()
        {
            if (roomModel == null)
            {
                Debug.LogError("[RoomWindow] RoomModel is not assigned! Please assign it in the Inspector.", gameObject);
                throw new System.NullReferenceException($"[RoomWindow] RoomModel is null on GameObject '{gameObject.name}'. This component requires a RoomModel to function.");
            }
            
            roomModel.CurrentRoom.Attach(this);
        }

        void OnDisable()
        {
            if (roomModel != null)
            {
                roomModel.CurrentRoom.Detach(this);
            }
        }

        protected override void OnDestroy()
        {
            if (leaveButton != null)
            {
                leaveButton.onClick.RemoveListener(OnLeaveClicked);
            }

            if (toggleTeamButton != null)
            {
                toggleTeamButton.onClick.RemoveListener(OnToggleTeamClicked);
            }

            if (startGameButton != null)
            {
                startGameButton.onClick.RemoveListener(OnStartGameClicked);
            }
            base.OnDestroy();
        }

        public void OnNotify(RoomInfo roomInfo)
        {
            UpdateRoomDisplay(roomInfo);
        }

        private void UpdateRoomDisplay(RoomInfo roomInfo)
        {
            if (roomInfo == null)
            {
                // Room is null, clear display
                if (roomNameText != null)
                    roomNameText.text = "No Room";

                if (playerCountText != null)
                    playerCountText.text = "0 Players";

                ClearPlayerList();
                Debug.Log($"[{nameof(RoomWindow)}] cleared room display (no room info)");
                return;
            }

            // Update room name
            if (roomNameText != null)
            {
                roomNameText.text = roomInfo.roomName;
            }

            // Update player count
            if (playerCountText != null)
            {
                int playerCount = roomInfo.GetPlayerCount();
                playerCountText.text = $"{playerCount} {(playerCount == 1 ? "Player" : "Players")}";
            }

            
            // Update toggle team button (only if we have an owned player)
            if (toggleTeamButton != null)
            {
                toggleTeamButton.interactable = _ownedPlayer != null && _networkRoom != null;
            }
            // Update player list
            UpdatePlayerList(roomInfo.players);

            // Update start game button (only room owner can start)
            if (startGameButton != null)
            {
                bool isRoomOwner = roomInfo.isRoomOwner;
                startGameButton.interactable = isRoomOwner && roomInfo.GetPlayerCount() >= 1 && _networkRoom != null;
            }

            // Update leave button
            if (leaveButton != null)
            {
                leaveButton.interactable = _networkRoom != null;
            }
            Debug.Log($"[{nameof(RoomWindow)}] updated room display for room '{roomInfo.roomName}' with {roomInfo.GetPlayerCount()} players. IsOwner: {roomInfo.isRoomOwner}");
        }

        private void UpdatePlayerList(List<Player> players)
        {
            ClearPlayerList();

            if (players == null || playerItemPrefab == null || playerListContainer == null)
                return;

            foreach (var player in players)
            {
                PlayerItem playerItem;
                if (_playerItemPool.Count > 0)
                {
                    playerItem = _playerItemPool.Dequeue();
                    playerItem.gameObject.SetActive(true);
                    playerItem.transform.SetAsLastSibling();
                }
                else
                {
                    playerItem = Instantiate(playerItemPrefab, playerListContainer);
                }

                if (playerItem != null)
                {
                    playerItem.SetPlayer(player);
                    _playerItems.Add(playerItem);
                }
            }
        }

        private void ClearPlayerList()
        {
            foreach (var item in _playerItems)
            {
                if (item != null)
                {
                    item.gameObject.SetActive(false);
                    _playerItemPool.Enqueue(item);
                }
            }

            _playerItems.Clear();
        }

        private void OnLeaveClicked()
        {
            if (_networkRoom != null)
            {
                _networkRoom.LeaveRoom();
            }
            else if (roomModel != null)
            {
                roomModel.LeaveRoom();
            }

            // Navigate back to PlayWindow
            UIWindowManager.Instance?.ShowWindow<PlayWindow>();
        }

        private void OnStartGameClicked()
        {
            if (_networkRoom != null)
            {
                _networkRoom.StartGame();
            }
            else
            {
                Debug.LogWarning("[RoomWindow] Cannot start game - no network room assigned");
            }
        }

        private void OnToggleTeamClicked()
        {
            if (_ownedPlayer != null)
            {
                _ownedPlayer.ToggleTeam();
            }
            else
            {
                Debug.LogWarning("[RoomWindow] Cannot toggle team - no owned player assigned");
            }
        }

        #region Network Integration

        /// <summary>
        /// Set the network room manager (called by NetworkRoomManager when owned)
        /// </summary>
        public void SetNetworkRoom(INetworkRoom networkRoom)
        {
            _networkRoom = networkRoom;
            Debug.Log($"[RoomWindow] Network room assigned: {networkRoom?.RoomName ?? "null"}");
        }

        /// <summary>
        /// Set the owned player (called by NetworkPlayer when owned)
        /// </summary>
        public void SetOwnedPlayer(INetworkPlayer player)
        {
            _ownedPlayer = player;
            Debug.Log($"[RoomWindow] Owned player assigned (ID: {player?.NetworkId ?? -1})");
            
            // Update toggle team button state
            if (toggleTeamButton != null)
            {
                toggleTeamButton.interactable = _ownedPlayer != null;
            }
        }

        #endregion
    }
}

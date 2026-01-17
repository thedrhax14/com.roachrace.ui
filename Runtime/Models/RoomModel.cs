using UnityEngine;
using RoachRace.UI.Core;
using RoachRace.Data;

namespace RoachRace.UI.Models
{
    /// <summary>
    /// Model that manages the current room/lobby information
    /// </summary>
    [CreateAssetMenu(fileName = "RoomModel", menuName = "RoachRace/UI/Room Model")]
    public class RoomModel : UIModel
    {
        [Header("Observable Properties")]
        public Observable<RoomInfo> CurrentRoom = new Observable<RoomInfo>(null);
        public Observable<string> ErrorMessage = new Observable<string>("");

        /// <summary>
        /// Set the current room information
        /// </summary>
        public void SetRoom(RoomInfo room)
        {
            CurrentRoom.Value = room;
        }

        /// <summary>
        /// Set whether the local player owns this room
        /// </summary>
        public void SetRoomOwner(bool isOwner)
        {
            if (CurrentRoom.Value != null)
            {
                CurrentRoom.Value.isRoomOwner = isOwner;
                CurrentRoom.Notify(CurrentRoom.Value);
            }
        }

        /// <summary>
        /// Update the room name
        /// </summary>
        public void SetRoomName(string roomName)
        {
            if (CurrentRoom.Value != null)
            {
                CurrentRoom.Value.roomName = roomName;
                CurrentRoom.Notify(CurrentRoom.Value); // Manually notify since we modified the object
            }
        }

        /// <summary>
        /// Add a player to the current room
        /// </summary>
        public void AddPlayer(Player player)
        {
            if (CurrentRoom.Value == null)
            {
                CurrentRoom.Value = new RoomInfo();
            }

            CurrentRoom.Value.AddPlayer(player);
            CurrentRoom.Notify(CurrentRoom.Value);
        }

        /// <summary>
        /// Remove a player by network ID
        /// </summary>
        public void RemovePlayerByNetworkId(int networkId)
        {
            if (CurrentRoom.Value != null)
            {
                CurrentRoom.Value.RemovePlayerByNetworkId(networkId);
                CurrentRoom.Notify(CurrentRoom.Value);
            }
        }

        /// <summary>
        /// Clear all players from the room
        /// </summary>
        public void ClearPlayers()
        {
            if (CurrentRoom.Value != null)
            {
                CurrentRoom.Value.ClearPlayers();
                CurrentRoom.Notify(CurrentRoom.Value);
            }
        }

        /// <summary>
        /// Leave the current room
        /// </summary>
        public void LeaveRoom()
        {
            CurrentRoom.Value = null;
            ClearError();
        }

        /// <summary>
        /// Set error message
        /// </summary>
        public void SetError(string error)
        {
            ErrorMessage.Value = error;
        }

        /// <summary>
        /// Clear error message
        /// </summary>
        public void ClearError()
        {
            ErrorMessage.Value = "";
        }
    }
}

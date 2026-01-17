using System.Collections.Generic;
using UnityEngine;
using RoachRace.Data;
using RoachRace.UI.Core;

namespace RoachRace.UI.Models
{
    /// <summary>
    /// Model that tracks players currently in the game and their real-time stats/states
    /// </summary>
    [CreateAssetMenu(fileName = "GamePlayersModel", menuName = "RoachRace/Models/Game Players Model")]
    public class GamePlayersModel : UIModel
    {
        /// <summary>
        /// Dictionary of active player observables mapped by player name (ID)
        /// </summary>
        private readonly Dictionary<string, Observable<PlayerStats>> _playerStats = new();

        /// <summary>
        /// Get the observable stats for a specific player.
        /// Creates a new observable if one doesn't exist.
        /// </summary>
        public Observable<PlayerStats> GetPlayerStats(string playerName)
        {
            if (!_playerStats.ContainsKey(playerName))
            {
                _playerStats[playerName] = new Observable<PlayerStats>(new PlayerStats().SetPlayerName(playerName));
            }
            return _playerStats[playerName];
        }

        /// <summary>
        /// Add or update a player in the tracking dictionary
        /// </summary>
        public PlayerStats UpdatePlayer(string playerName, PlayerStats stats)
        {
            var observable = GetPlayerStats(playerName);
            observable.Value = stats.SetPlayerName(playerName);
            return observable.Value;
        }

        /// <summary>
        /// Remove a player from tracking
        /// </summary>
        public void RemovePlayer(string playerName)
        {
            var observable = GetPlayerStats(playerName);
            observable.Notify(observable.Value.SetStatus(Status.Offline));
        }

        public void SetPlayerPing(string playerName, long ping)
        {
            var observable = GetPlayerStats(playerName);
            observable.Notify(observable.Value.SetPing(ping));
        }

        /// <summary>
        /// Update the speaking status of a specific player
        /// </summary>
        public void SetPlayerSpeaking(string playerName, bool isSpeaking)
        {
            var observable = GetPlayerStats(playerName);
            observable.Notify(observable.Value.SetIsSpeaking(isSpeaking));
        }

        public void SetPlayerAmplitude(string playerName, float amplitude)
        {
            var observable = GetPlayerStats(playerName);
            observable.Notify(observable.Value.SetAmplitude(amplitude));
        }

        /// <summary>
        /// Clear all players (e.g. on game end)
        /// </summary>
        public void ClearPlayers()
        {
            foreach (var observable in _playerStats.Values)
            {
                observable.Value = null;
            }
            _playerStats.Clear();
        }
    }
}

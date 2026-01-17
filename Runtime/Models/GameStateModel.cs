using UnityEngine;
using RoachRace.UI.Core;
using RoachRace.Data;

namespace RoachRace.UI.Models
{
    /// <summary>
    /// Model that manages the current game state and results
    /// </summary>
    [CreateAssetMenu(fileName = "GameStateModel", menuName = "RoachRace/UI/Game State Model")]
    public class GameStateModel : UIModel
    {
        [Header("Observable Properties")]
        public Observable<GameState> CurrentState = new Observable<GameState>(GameState.Lobby);
        public Observable<GameResult> CurrentResult = new Observable<GameResult>(null);
        public Observable<int> MaxRespawns = new Observable<int>(3);
        public Observable<float> CountdownSeconds = new Observable<float>(0f);

        /// <summary>
        /// Set the current game state
        /// </summary>
        public void SetGameState(GameState state)
        {
            CurrentState.Value = state;
        }

        /// <summary>
        /// Set the game results
        /// </summary>
        public void SetGameResult(GameResult result)
        {
            CurrentResult.Value = result;
        }

        /// <summary>
        /// Set maximum respawns for survivors
        /// </summary>
        public void SetMaxRespawns(int maxRespawns)
        {
            MaxRespawns.Value = maxRespawns;
        }

        /// <summary>
        /// Set countdown seconds until game starts
        /// </summary>
        public void SetCountdown(float seconds)
        {
            CountdownSeconds.Value = seconds;
        }

        /// <summary>
        /// Reset the game state to lobby
        /// </summary>
        public void ResetToLobby()
        {
            CurrentState.Value = GameState.Lobby;
            CurrentResult.Value = null;
            CountdownSeconds.Value = 0f;
        }

        protected override void Initialize()
        {
            // Reset to default state on enable
            CurrentState.Value = GameState.Lobby;
            CurrentResult.Value = null;
            MaxRespawns.Value = 3;
            CountdownSeconds.Value = 0f;
        }
    }
}

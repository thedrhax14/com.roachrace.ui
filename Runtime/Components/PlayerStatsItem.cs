using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoachRace.Data;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// UI component for displaying individual player stats in game over screen
    /// </summary>
    public class PlayerStatsItem : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI teamText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private Image backgroundImage;

        [Header("Settings")]
        [SerializeField] private Color ghostTeamColor = new Color(0.7f, 0.7f, 1f, 0.3f);
        [SerializeField] private Color survivorTeamColor = new Color(1f, 0.7f, 0.7f, 0.3f);
        [SerializeField] private Color winnerColor = new Color(1f, 0.9f, 0.3f, 0.5f);

        private PlayerStats _stats;

        /// <summary>
        /// Set the player stats to display
        /// </summary>
        public void SetPlayerStats(PlayerStats stats)
        {
            _stats = stats;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_stats == null)
                return;

            // Player name
            if (playerNameText != null)
            {
                playerNameText.text = _stats.playerName;
            }

            // Team
            if (teamText != null)
            {
                teamText.text = _stats.team.ToString();
                teamText.color = _stats.team == Team.Ghost ? ghostTeamColor : survivorTeamColor;
            }

            // Stats based on team
            if (statsText != null)
            {
                if (_stats.team == Team.Survivor)
                {
                    if (_stats.reachedEnd)
                    {
                        int minutes = Mathf.FloorToInt(_stats.survivalTime / 60f);
                        int seconds = Mathf.FloorToInt(_stats.survivalTime % 60f);
                        statsText.text = $"REACHED END - {minutes:00}:{seconds:00}";
                    }
                    else
                    {
                        statsText.text = $"Deaths: {_stats.deaths} | Respawns left: {_stats.respawnsLeft}";
                    }
                }
                else // Ghost
                {
                    statsText.text = $"Kills: {_stats.deaths}";
                }
            }

            // Background color
            if (backgroundImage != null)
            {
                if (_stats.reachedEnd)
                {
                    backgroundImage.color = winnerColor;
                }
                else
                {
                    backgroundImage.color = _stats.team == Team.Ghost ? ghostTeamColor : survivorTeamColor;
                }
            }
        }
    }
}

using System;
using RoachRace.UI.Core;
using RoachRace.UI.Models;
using UnityEngine;
using UnityEngine.Playables;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// Plays a looping Timeline flyover during the match start intro cinematic phase.<br></br>
    /// Purpose: make the spawn flow more immersive by showing a camera flyover while MapGen finishes initial generation and
    /// before spawn pods/controllers appear.<br></br>
    /// Typical usage:<br></br>
    /// - Add this component to a scene GameObject that has (or references) a <see cref="PlayableDirector"/> configured with a Timeline asset.<br></br>
    /// - Assign a <see cref="SpawnSequenceModel"/> asset used by the session.<br></br>
    /// - When <see cref="SpawnSequenceModel.Phase"/> becomes <see cref="SpawnSequenceModel.SpawnSequencePhase.Cinematic"/>, this component sets the director to Loop and plays it.<br></br>
    /// - When leaving Cinematic, it stops the director and rewinds to time 0.
    /// </summary>
    [AddComponentMenu("RoachRace/UI/Spawn Intro Timeline Controller")]
    public sealed class SpawnIntroTimelineController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private SpawnSequenceModel spawnSequenceModel;
        [SerializeField] private PlayableDirector playableDirector;

        [Header("Behavior")]
        [Tooltip("If true, forces the PlayableDirector wrap mode to Loop when the cinematic starts.")]
        [SerializeField] private bool forceLoop = true;

        [Tooltip("If true, stops and rewinds the timeline when leaving Cinematic.")]
        [SerializeField] private bool stopAndRewindOnExit = true;

        private Core.IObserver<SpawnSequenceModel.SpawnSequencePhase> _phaseObserver;

        private void Awake()
        {
            if (spawnSequenceModel == null)
            {
                Debug.LogError($"[{nameof(SpawnIntroTimelineController)}] Missing required reference on '{gameObject.name}': spawnSequenceModel", gameObject);
                throw new InvalidOperationException($"[{nameof(SpawnIntroTimelineController)}] Missing required reference on '{gameObject.name}': spawnSequenceModel");
            }

            if (playableDirector == null)
            {
                Debug.LogError($"[{nameof(SpawnIntroTimelineController)}] Missing required reference on '{gameObject.name}': playableDirector", gameObject);
                throw new InvalidOperationException($"[{nameof(SpawnIntroTimelineController)}] Missing required reference on '{gameObject.name}': playableDirector");
            }

            _phaseObserver = new ActionObserver<SpawnSequenceModel.SpawnSequencePhase>(OnPhaseChanged);
        }

        private void OnEnable()
        {
            spawnSequenceModel.Phase.Attach(_phaseObserver);
        }

        private void OnDisable()
        {
            if (spawnSequenceModel != null && _phaseObserver != null)
                spawnSequenceModel.Phase.Detach(_phaseObserver);
        }

        private void OnPhaseChanged(SpawnSequenceModel.SpawnSequencePhase phase)
        {
            if (phase == SpawnSequenceModel.SpawnSequencePhase.Cinematic)
            {
                StartCinematic();
                return;
            }

            StopCinematicIfNeeded();
        }

        private void StartCinematic()
        {
            if (playableDirector.playableAsset == null)
            {
                Debug.LogError($"[{nameof(SpawnIntroTimelineController)}] Cannot start cinematic on '{gameObject.name}': playableDirector.playableAsset is null", gameObject);
                return;
            }

            if (forceLoop)
                playableDirector.extrapolationMode = DirectorWrapMode.Loop;

            // Always restart from time 0 so the flyover is consistent.
            playableDirector.time = 0d;
            playableDirector.Evaluate();
            playableDirector.Play();

            Debug.Log($"[{nameof(SpawnIntroTimelineController)}] Started intro cinematic Timeline on '{gameObject.name}'", gameObject);
        }

        private void StopCinematicIfNeeded()
        {
            if (!stopAndRewindOnExit)
                return;

            if (playableDirector == null)
                return;

            if (playableDirector.state != PlayState.Playing && playableDirector.time <= 0d)
                return;

            playableDirector.Stop();
            playableDirector.time = 0d;
            playableDirector.Evaluate();

            Debug.Log($"[{nameof(SpawnIntroTimelineController)}] Stopped intro cinematic Timeline on '{gameObject.name}'", gameObject);
        }
    }
}

using System.Collections.Generic;
using RoachRace.Controls;
using UnityEngine;

namespace RoachRace.UI.Components.Resources
{
    /// <summary>
    /// Spawns PlayerResource widgets under a container and keeps them in sync with the locally controlled character.
    /// 
    /// Behavior:
    /// - Listens to LocalPlayerControllerContext.ControllerChanged
    /// - When the active controller changes, clears and respawns widgets under resourcesContainer
    /// - Uses per-resource UiWidgetPrefab if provided; otherwise uses defaultWidgetPrefab
    /// </summary>
    public sealed class PlayerResourcesView : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private Transform resourcesContainer;

        [Tooltip("Fallback prefab to use if a resource has no UiWidgetPrefab assigned. Must contain PlayerResourceWidget.")]
        [SerializeField] private GameObject defaultWidgetPrefab;

        private readonly List<PlayerResourceWidget> _spawned = new();

        private void Awake()
        {
            if (resourcesContainer == null)
            {
                Debug.LogError($"[{nameof(PlayerResourcesView)}] resourcesContainer is not assigned on '{gameObject.name}'.", gameObject);
                throw new System.NullReferenceException($"[{nameof(PlayerResourcesView)}] resourcesContainer is null on '{gameObject.name}'.");
            }

            if (defaultWidgetPrefab == null)
            {
                Debug.LogError($"[{nameof(PlayerResourcesView)}] defaultWidgetPrefab is not assigned on '{gameObject.name}'.", gameObject);
                throw new System.NullReferenceException($"[{nameof(PlayerResourcesView)}] defaultWidgetPrefab is null on '{gameObject.name}'.");
            }
        }

        private void OnEnable()
        {
            LocalPlayerControllerContext.ControllerChanged += Apply;

            // Populate immediately if a controller is already active.
            Apply(LocalPlayerControllerContext.Current);
        }

        private void OnDisable()
        {
            LocalPlayerControllerContext.ControllerChanged -= Apply;
            Clear();
        }

        private void Apply(GameObject controller)
        {
            Clear();

            if (controller == null) return;

            PlayerResource[] resources = controller.GetComponentsInChildren<PlayerResource>(true);
            
            if (resources == null || resources.Length == 0) return;

            for (int i = 0; i < resources.Length; i++)
            {
                var res = resources[i];
                if (res == null) continue;

                var prefab = res.UiWidgetPrefab != null ? res.UiWidgetPrefab : defaultWidgetPrefab;
                var instance = Instantiate(prefab, resourcesContainer);

                if (!instance.TryGetComponent<PlayerResourceWidget>(out var widget))
                {
                    Debug.LogError($"[{nameof(PlayerResourcesView)}] Resource widget prefab '{prefab.name}' does not contain {nameof(PlayerResourceWidget)}.", instance);
                    Destroy(instance);
                    continue;
                }

                widget.Bind(res);
                _spawned.Add(widget);
            }
        }

        private void Clear()
        {
            for (int i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i] == null) continue;
                _spawned[i].Unbind();
                Destroy(_spawned[i].gameObject);
            }
            _spawned.Clear();
        }
    }
}

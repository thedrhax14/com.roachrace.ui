using System.Collections.Generic;
using RoachRace.UI.Core;
using RoachRace.UI.Models;
using UnityEngine;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// Displays dealt damage as pooled floating world-space text projected into HUD space.<br/>
    /// Typical usage: place this under the gameplay HUD canvas, assign a <see cref="DealtDamageModel"/>, a container, and a <see cref="DealtDamageFloatingTextItem"/> prefab, then let the view recycle up to <see cref="maxEntries"/> items.<br/>
    /// Configuration/context: each entry uses a camera-forward dot check so damage behind the camera is hidden until it comes back into view.
    /// </summary>
    public sealed class DealtDamageFloatingTextView : MonoBehaviour, IObserver<DealtDamageEntry>
    {
        [Header("Model")]
        [SerializeField] private DealtDamageModel dealtDamageModel;

        [Header("UI")]
        [SerializeField] private RectTransform container;
        [SerializeField] private DealtDamageFloatingTextItem itemPrefab;
        [SerializeField] private Canvas targetCanvas;

        [Header("Camera")]
        [SerializeField] private Camera targetCamera;

        [Header("Behavior")]
        [SerializeField, Min(1)] private int maxEntries = 10;
        [SerializeField, Min(0.1f)] private float entryLifetimeSeconds = 0.9f;
        [SerializeField, Min(0f)] private float riseSpeedWorldUnits = 0.75f;
        [SerializeField, Min(0f)] private float fadeOutDurationSeconds = 0.25f;

        private readonly List<DealtDamageFloatingTextItem> pool = new();
        private readonly Queue<DealtDamageFloatingTextItem> activeOldestFirst = new();
        private readonly Queue<DealtDamageFloatingTextItem> inactive = new();
        private readonly HashSet<DealtDamageFloatingTextItem> inactiveSet = new();

        /// <summary>
        /// Resolves optional scene references.<br/>
        /// Typical usage: Unity invokes this during initialization before the view is enabled.<br/>
        /// Configuration/context: if no explicit container is assigned, the component uses its own <see cref="RectTransform"/>.
        /// </summary>
        private void Awake()
        {
            if (container == null)
                container = transform as RectTransform;
        }

        /// <summary>
        /// Subscribes to dealt-damage events and validates required dependencies.<br/>
        /// Typical usage: Unity invokes this when the HUD becomes active.<br/>
        /// Configuration/context: attachment immediately pushes the observable's current value, so default payloads are filtered in <see cref="OnNotify"/>.
        /// </summary>
        private void OnEnable()
        {
            ValidateDependencies();

            if (dealtDamageModel != null)
                dealtDamageModel.LatestEntry.Attach(this);
        }

        /// <summary>
        /// Unsubscribes from the dealt-damage model and returns pooled entries to the inactive queue.<br/>
        /// Typical usage: Unity invokes this during HUD teardown or deactivation.<br/>
        /// Configuration/context: pooled items are hidden rather than destroyed so they can be reused after re-enabling the view.
        /// </summary>
        private void OnDisable()
        {
            if (dealtDamageModel != null)
                dealtDamageModel.LatestEntry.Detach(this);

            activeOldestFirst.Clear();
            inactive.Clear();
            inactiveSet.Clear();

            foreach (var item in pool)
                MarkInactive(item);
        }

        /// <summary>
        /// Updates active floating text items each frame.<br/>
        /// Typical usage: Unity invokes this while the HUD remains visible.<br/>
        /// Configuration/context: expired items are recycled into the inactive pool for later reuse.
        /// </summary>
        private void Update()
        {
            Camera cameraToUse = ResolveCamera();
            int activeCount = activeOldestFirst.Count;
            for (int i = 0; i < activeCount; i++)
            {
                DealtDamageFloatingTextItem item = activeOldestFirst.Dequeue();
                if (item == null)
                    continue;

                bool keepAlive = item.Tick(cameraToUse, fadeOutDurationSeconds);
                if (keepAlive)
                {
                    activeOldestFirst.Enqueue(item);
                    continue;
                }

                MarkInactive(item);
            }
        }

        /// <summary>
        /// Receives a dealt-damage event and shows or reuses a floating text item for it.<br/>
        /// Typical usage: invoked by <see cref="DealtDamageModel"/> whenever the local player deals damage.<br/>
        /// Configuration/context: default observable payloads are ignored to prevent bogus startup entries.
        /// </summary>
        /// <param name="entry">The dealt-damage event to visualize.</param>
        public void OnNotify(DealtDamageEntry entry)
        {
            if (entry.DamageAmount <= 0 && entry.TargetConnectionId == 0 && entry.TargetObjectId == 0 && entry.TargetWorldPosition == Vector3.zero && !entry.IsFatal)
                return;

            if (itemPrefab == null || container == null)
                return;

            DealtDamageFloatingTextItem item = GetNextItem();
            if (item == null)
                return;

            item.Show(entry, entry.TargetWorldPosition, entryLifetimeSeconds, riseSpeedWorldUnits);
            item.transform.SetAsLastSibling();
            activeOldestFirst.Enqueue(item);
        }

        /// <summary>
        /// Returns the next pooled item, creating or reusing entries as needed.<br/>
        /// Typical usage: called whenever a new dealt-damage event arrives.<br/>
        /// Configuration/context: when the pool is already full, the oldest active entry is reused.
        /// </summary>
        /// <returns>The pooled item ready to display a new hit.</returns>
        private DealtDamageFloatingTextItem GetNextItem()
        {
            if (pool.Count < maxEntries)
            {
                DealtDamageFloatingTextItem created = Instantiate(itemPrefab, container);
                created.SetVisible(false, 0f);
                pool.Add(created);
                MarkInactive(created);
            }

            while (inactive.Count > 0)
            {
                DealtDamageFloatingTextItem item = inactive.Dequeue();
                if (item == null)
                    continue;

                if (!inactiveSet.Remove(item))
                    continue;

                return item;
            }

            while (activeOldestFirst.Count > 0)
            {
                DealtDamageFloatingTextItem oldest = activeOldestFirst.Dequeue();
                if (oldest == null)
                    continue;

                return oldest;
            }

            return pool.Count > 0 ? pool[0] : null;
        }

        /// <summary>
        /// Marks a pooled item as inactive and available for reuse.<br/>
        /// Typical usage: called when entries expire or when the view is disabled.<br/>
        /// Configuration/context: duplicate inactive registrations are ignored defensively.
        /// </summary>
        /// <param name="item">The pooled item to recycle.</param>
        private void MarkInactive(DealtDamageFloatingTextItem item)
        {
            if (item == null)
                return;

            item.SetVisible(false, 0f);
            if (!inactiveSet.Add(item))
                return;

            inactive.Enqueue(item);
        }

        /// <summary>
        /// Resolves the camera used to project world positions into screen space.<br/>
        /// Typical usage: called during frame updates for floating text positioning.<br/>
        /// Configuration/context: falls back to <see cref="Camera.main"/> when no explicit camera is assigned.
        /// </summary>
        /// <returns>The camera used for projection, or null when no camera is available.</returns>
        private Camera ResolveCamera()
        {
            return targetCamera != null ? targetCamera : Camera.main;
        }

        /// <summary>
        /// Validates required serialized references before the view starts listening for events.<br/>
        /// Typical usage: called from <see cref="OnEnable"/>.<br/>
        /// Configuration/context: logs setup problems without throwing so incomplete HUD prefabs can be fixed in-scene.
        /// </summary>
        private void ValidateDependencies()
        {
            if (dealtDamageModel == null)
                Debug.LogError($"[{nameof(DealtDamageFloatingTextView)}] Missing required reference on '{gameObject.name}': {nameof(dealtDamageModel)}.", gameObject);

            if (container == null)
                Debug.LogError($"[{nameof(DealtDamageFloatingTextView)}] Missing required reference on '{gameObject.name}': {nameof(container)}.", gameObject);

            if (itemPrefab == null)
                Debug.LogError($"[{nameof(DealtDamageFloatingTextView)}] Missing required reference on '{gameObject.name}': {nameof(itemPrefab)}.", gameObject);
        }
    }
}
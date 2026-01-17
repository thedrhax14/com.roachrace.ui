using System.Collections.Generic;
using RoachRace.Data;
using RoachRace.UI.Core;
using RoachRace.UI.Models;
using UnityEngine;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// Displays a rolling death log (kill feed) with pooling:
    /// - Each entry lives for <see cref="entryLifetimeSeconds"/> seconds.
    /// - Shows at most <see cref="maxEntries"/> entries.
    /// - When full, reuses the oldest entry.
    /// </summary>
    public sealed class DeathLogView : MonoBehaviour, IObserver<DeathLogEntry>
    {
        [Header("Model")]
        [SerializeField] private DeathLogModel deathLogModel;

        [Header("UI")]
        [SerializeField] private Transform container;
        [SerializeField] private DeathLogItemView itemPrefab;

        [Header("Behavior")]
        [SerializeField, Min(1)] private int maxEntries = 10;
        [SerializeField, Min(0.1f)] private float entryLifetimeSeconds = 5f;

        private readonly List<DeathLogItemView> _pool = new();
        private readonly Queue<DeathLogItemView> _activeOldestFirst = new();

        private readonly Queue<DeathLogItemView> _inactive = new();
        private readonly HashSet<DeathLogItemView> _inactiveSet = new();

        private void Awake()
        {
            if (container == null) container = transform;
        }

        private void OnEnable()
        {
            if (deathLogModel != null)
                deathLogModel.LatestEntry.Attach(this);
        }

        private void OnDisable()
        {
            if (deathLogModel != null)
                deathLogModel.LatestEntry.Detach(this);
        }

        private void Update()
        {
            if (_activeOldestFirst.Count == 0) return;

            float now = Time.time;

            // Expire as many as needed (oldest first).
            while (_activeOldestFirst.Count > 0)
            {
                DeathLogItemView oldest = _activeOldestFirst.Peek();
                if (oldest == null)
                {
                    _activeOldestFirst.Dequeue();
                    continue;
                }

                if (now < oldest.ExpireAtTime)
                    break;

                _activeOldestFirst.Dequeue();
                oldest.SetVisible(false);

                MarkInactive(oldest);
            }
        }

        public void OnNotify(DeathLogEntry entry)
        {
            // Observable<T>.Attach pushes the current value immediately. Since this model is event-like,
            // ignore the default struct value to prevent a bogus row on startup.
            if (entry.Victim.Name == null && entry.Attacker.Name == null && entry.WeaponIconKey == null && entry.ServerTick == 0u)
                return;

            AddEntry(entry);
        }

        private void AddEntry(DeathLogEntry entry)
        {
            if (itemPrefab == null)
            {
                Debug.LogError($"[{nameof(DeathLogView)}] ItemPrefab is not assigned.", gameObject);
                return;
            }

            DeathLogItemView view = GetNextViewToUse();
            if (view == null)
                return;

            float expireAt = Time.time + Mathf.Max(0.1f, entryLifetimeSeconds);
            view.Show(entry, expireAt);

            // Ensure newest entry renders last in the hierarchy (helps with layout groups and overlap).
            view.transform.SetAsLastSibling();

            // Mark as newest: we want oldest at queue front.
            _activeOldestFirst.Enqueue(view);

            // Ensure an active view is never considered inactive.
            UnmarkInactive(view);
        }

        private DeathLogItemView GetNextViewToUse()
        {
            // 1) If we can still allocate new items up to maxEntries, do so (and treat it as inactive until used).
            if (_pool.Count < maxEntries)
            {
                DeathLogItemView created = Instantiate(itemPrefab, container);
                created.SetVisible(false);
                _pool.Add(created);
                MarkInactive(created);
            }

            // 2) Prefer reusing an inactive (expired) pooled item.
            while (_inactive.Count > 0)
            {
                DeathLogItemView view = _inactive.Dequeue();
                if (view == null) continue;
                if (!_inactiveSet.Remove(view))
                    continue;

                return view;
            }

            // 3) If everything is active and we're at capacity, reuse the oldest active.
            while (_activeOldestFirst.Count > 0)
            {
                DeathLogItemView oldest = _activeOldestFirst.Dequeue();
                if (oldest == null) continue;

                return oldest;
            }

            // 4) Shouldn't happen, but as a last resort reuse first in pool.
            return _pool.Count > 0 ? _pool[0] : null;
        }

        private void MarkInactive(DeathLogItemView view)
        {
            if (view == null) return;
            if (!_inactiveSet.Add(view)) return;
            _inactive.Enqueue(view);
        }

        private void UnmarkInactive(DeathLogItemView view)
        {
            if (view == null) return;
            _inactiveSet.Remove(view);
        }
    }
}

using RoachRace.UI.Core;
using RoachRace.UI.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoachRace.UI.Components.ActionPrompts
{
    /// <summary>
    /// Renders a single ActionPromptModel into UI.
    ///
    /// Scene/prefab setup:
    /// - Assign an ActionPromptModel asset (each widget slot should have its own model instance).
    /// - Assign UI references (icon/text/action/progress/uses left).
    /// - Optionally assign a CanvasGroup for show/hide without disabling the GameObject.
    /// </summary>
    public sealed class ActionPromptWidget : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private ActionPromptModel model;

        [Header("UI")]
        [SerializeField] private Image keyIconImage;
        [SerializeField] private TMP_Text keyText;
        [SerializeField] private TMP_Text actionNameText;
        [Tooltip("Optional. If assigned, uses fillAmount (0..1).")]
        [SerializeField] private Image holdProgressFill;
        [SerializeField] private TMP_Text usesLeftText;

        private IObserver<bool> _visibleObserver;
        private IObserver<Sprite> _iconObserver;
        private IObserver<string> _keyTextObserver;
        private IObserver<string> _actionObserver;
        private IObserver<float> _holdObserver;
        private IObserver<int> _usesObserver;

        private void Awake()
        {
            if (model == null)
            {
                Debug.LogError($"[{nameof(ActionPromptWidget)}] model is not assigned on '{gameObject.name}'.", gameObject);
                throw new System.NullReferenceException($"[{nameof(ActionPromptWidget)}] model is null on '{gameObject.name}'.");
            }

            _visibleObserver = new ActionObserver<bool>(SetVisible);
            _iconObserver = new ActionObserver<Sprite>(SetIcon);
            _keyTextObserver = new ActionObserver<string>(SetKeyText);
            _actionObserver = new ActionObserver<string>(SetActionName);
            _holdObserver = new ActionObserver<float>(SetHoldProgress);
            _usesObserver = new ActionObserver<int>(SetUsesLeft);
        }

        private void Start()
        {
            if (model == null) return;

            model.IsVisible.Attach(_visibleObserver);
            model.KeyIcon.Attach(_iconObserver);
            model.KeyText.Attach(_keyTextObserver);
            model.ActionName.Attach(_actionObserver);
            model.HoldProgress01.Attach(_holdObserver);
            model.UsesLeft.Attach(_usesObserver);

            // Ensure an initial render even if observers didn't fire (safety).
            SetVisible(model.IsVisible.Value);
            SetIcon(model.KeyIcon.Value);
            SetKeyText(model.KeyText.Value);
            SetActionName(model.ActionName.Value);
            SetHoldProgress(model.HoldProgress01.Value);
            SetUsesLeft(model.UsesLeft.Value);
        }

        private void OnDestroy()
        {
            if (model == null) return;

            model.IsVisible.Detach(_visibleObserver);
            model.KeyIcon.Detach(_iconObserver);
            model.KeyText.Detach(_keyTextObserver);
            model.ActionName.Detach(_actionObserver);
            model.HoldProgress01.Detach(_holdObserver);
            model.UsesLeft.Detach(_usesObserver);
        }

        private void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void SetIcon(Sprite icon)
        {
            if (keyIconImage == null) return;
            keyIconImage.sprite = icon;
            keyIconImage.enabled = icon != null;
        }

        private void SetKeyText(string text)
        {
            if (keyText == null) return;
            keyText.text = text ?? string.Empty;
            keyText.enabled = !string.IsNullOrWhiteSpace(keyText.text);
        }

        private void SetActionName(string name)
        {
            if (actionNameText == null) return;
            actionNameText.text = name ?? string.Empty;
        }

        private void SetHoldProgress(float progress01)
        {
            if (holdProgressFill == null) return;

            holdProgressFill.fillAmount = Mathf.Clamp01(progress01);
            // Hide the bar when not in use.
            holdProgressFill.enabled = holdProgressFill.fillAmount > 0f && holdProgressFill.fillAmount < 1f;
        }

        private void SetUsesLeft(int usesLeft)
        {
            if (usesLeftText == null) return;

            if (usesLeft < 0)
            {
                usesLeftText.text = string.Empty;
                usesLeftText.enabled = false;
                return;
            }

            usesLeftText.text = usesLeft.ToString();
            usesLeftText.enabled = true;
        }
    }
}

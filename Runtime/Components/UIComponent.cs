using UnityEngine;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// Base class for UI components that can observe and react to model changes
    /// </summary>
    public abstract class UIComponent : MonoBehaviour
    {
        protected virtual void Awake()
        {
            Initialize();
        }

        protected virtual void OnDestroy()
        {
            Cleanup();
        }

        protected virtual void Initialize() { }
        
        protected virtual void Cleanup() { }
    }
}

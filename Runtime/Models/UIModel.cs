using UnityEngine;

namespace RoachRace.UI.Models
{
    /// <summary>
    /// Base class for UI models that hold data and state
    /// </summary>
    public abstract class UIModel : ScriptableObject
    {
        protected virtual void OnEnable()
        {
            Initialize();
        }

        protected virtual void OnDisable()
        {
            Cleanup();
        }

        protected virtual void Initialize() { }
        
        protected virtual void Cleanup() { }
    }
}

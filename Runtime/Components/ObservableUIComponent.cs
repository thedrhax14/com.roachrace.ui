using RoachRace.UI.Core;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// UI component that implements the observer pattern
    /// </summary>
    /// <typeparam name="T">Type of data being observed</typeparam>
    public abstract class ObservableUIComponent<T> : UIComponent, IObserver<T>
    {
        protected override void Initialize()
        {
            base.Initialize();
            SubscribeToObservables();
        }

        protected override void Cleanup()
        {
            UnsubscribeFromObservables();
            base.Cleanup();
        }

        protected virtual void SubscribeToObservables() { }
        
        protected virtual void UnsubscribeFromObservables() { }

        public abstract void OnNotify(T data);
    }
}

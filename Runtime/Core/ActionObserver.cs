using System;

namespace RoachRace.UI.Core
{
    /// <summary>
    /// Generic observer that executes an action when notified.
    /// Useful for avoiding boilerplate IObserver implementations.
    /// </summary>
    /// <typeparam name="T">Type of data being observed</typeparam>
    public class ActionObserver<T> : IObserver<T>
    {
        private readonly Action<T> _onNotify;

        public ActionObserver(Action<T> onNotify)
        {
            _onNotify = onNotify;
        }

        public void OnNotify(T data)
        {
            _onNotify?.Invoke(data);
        }
    }
}

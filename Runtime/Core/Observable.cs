using System.Collections.Generic;
using RoachRace.Data;

namespace RoachRace.UI.Core
{
    /// <summary>
    /// Generic observable implementation for the observer pattern
    /// </summary>
    /// <typeparam name="T">Type of data being observed</typeparam>
    public class Observable<T> : ISubject<T>
    {
        private readonly List<IObserver<T>> _observers = new List<IObserver<T>>();
        private T _value;

        public T Value
        {
            get => _value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_value, value))
                {
                    _value = value;
                    Notify(_value);
                }
            }
        }

        public Observable(T initialValue = default)
        {
            _value = initialValue;
        }

        public void Attach(IObserver<T> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
                observer.OnNotify(_value);
            }
        }

        public void Detach(IObserver<T> observer)
        {
            _observers.Remove(observer);
        }

        public void Notify(T data)
        {
            for (int i = _observers.Count - 1; i >= 0; i--)
            {
                _observers[i].OnNotify(data);
            }
        }

        public void NotifyCurrentValue()
        {
            Notify(_value);
        }
    }
}

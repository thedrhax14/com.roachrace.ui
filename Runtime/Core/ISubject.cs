namespace RoachRace.UI.Core
{
    /// <summary>
    /// Interface for observable subjects that notify observers of changes
    /// </summary>
    /// <typeparam name="T">Type of data being observed</typeparam>
    public interface ISubject<T>
    {
        void Attach(IObserver<T> observer);
        void Detach(IObserver<T> observer);
        void Notify(T data);
    }
}

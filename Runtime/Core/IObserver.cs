namespace RoachRace.UI.Core
{
    /// <summary>
    /// Interface for observers that react to changes in observable subjects
    /// </summary>
    /// <typeparam name="T">Type of data being observed</typeparam>
    public interface IObserver<T>
    {
        void OnNotify(T data);
    }
}

namespace WebsupplyConnect.Application.Interfaces.ExternalServices
{
    public interface ISignalRConnection
    {
        void AddConnection(int userId, string connectionId);
        void RemoveConnection(int userId, string connectionId);
        bool IsUserOnline(int userId);
        void UpdateConnectionActivity(int userId, string connectionId);
    }
}

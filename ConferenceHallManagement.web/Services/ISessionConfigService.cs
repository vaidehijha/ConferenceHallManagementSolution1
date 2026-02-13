using ConferenceHallManagement.web.ViewModels;

namespace ConferenceHallManagement.web.Services
{
    public interface ISessionConfigService
    {
        Task<List<SessionConfigVM>> GetAllSessionsByHallIdAsync(int hallId);
        Task<SessionConfigVM?> GetSessionByIdAsync(int sessionId);
        Task<bool> CreateSessionAsync(SessionConfigVM session);
        Task<bool> UpdateSessionAsync(SessionConfigVM session);
        Task<bool> DeleteSessionAsync(int sessionId);
    }
}

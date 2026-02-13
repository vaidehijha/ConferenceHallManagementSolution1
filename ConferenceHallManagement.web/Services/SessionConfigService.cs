using ConferenceHallManagement.web.ViewModels;
using Models_ConferenceHallManagement.AppDbModels;
using Repository_ConferenceHallManagement.AppDataRepositoy;
using UoW_ConferenceHallManagement;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallManagement.web.Services
{
    public class SessionConfigService : ISessionConfigService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SessionConfigService> _logger;

        public SessionConfigService(IUnitOfWork unitOfWork, ILogger<SessionConfigService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<SessionConfigVM>> GetAllSessionsByHallIdAsync(int hallId)
        {
            try
            {
                var sessions = await _unitOfWork.CHSessionDataRepository.GetConferenceHallSessionByHallId(hallId);
                
                return sessions.Select(s => new SessionConfigVM
                {
                    SessionId = s.SessionId,
                    HallId = s.HallId,
                    SessionEn = s.SessionEn ?? string.Empty,
                    SessionHi = s.SessionHi ?? string.Empty,
                    IsActive = s.Status
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting sessions for hall {hallId}");
                return new List<SessionConfigVM>();
            }
        }

        public async Task<SessionConfigVM?> GetSessionByIdAsync(int sessionId)
        {
            try
            {
                var session = await _unitOfWork.CHSessionDataRepository.GetAsync(sessionId);
                
                if (session == null) return null;

                return new SessionConfigVM
                {
                    SessionId = session.SessionId,
                    HallId = session.HallId,
                    SessionEn = session.SessionEn ?? string.Empty,
                    SessionHi = session.SessionHi ?? string.Empty,
                    IsActive = session.Status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting session {sessionId}");
                return null;
            }
        }

        public async Task<bool> CreateSessionAsync(SessionConfigVM session)
        {
            try
            {
                var newSession = new ConferenceHallSession
                {
                    HallId = session.HallId,
                    SessionEn = session.SessionEn,
                    SessionHi = session.SessionHi ?? string.Empty,
                    Status = true,
                    CreatedBy = "System",
                    CreatedOn = DateTime.Now,
                    CreatedFrom = "Blazor",
                    UpdatedBy = "System",
                    UpdatedOn = DateTime.Now,
                    UpdatedFrom = "Blazor"
                };

                _unitOfWork.CHSessionDataRepository.Add(newSession);
                var result = await _unitOfWork.SaveChangesAsync();

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating session");
                return false;
            }
        }

        public async Task<bool> UpdateSessionAsync(SessionConfigVM session)
        {
            try
            {
                var existingSession = await _unitOfWork.CHSessionDataRepository.GetAsync(session.SessionId);
                
                if (existingSession == null)
                {
                    _logger.LogWarning($"Session {session.SessionId} not found");
                    return false;
                }

                existingSession.SessionEn = session.SessionEn;
                existingSession.SessionHi = session.SessionHi ?? string.Empty;
                existingSession.UpdatedBy = "System";
                existingSession.UpdatedOn = DateTime.Now;
                existingSession.UpdatedFrom = "Blazor";

                _unitOfWork.CHSessionDataRepository.Update(existingSession);
                var result = await _unitOfWork.SaveChangesAsync();

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating session {session.SessionId}");
                return false;
            }
        }

        public async Task<bool> DeleteSessionAsync(int sessionId)
        {
            try
            {
                var session = await _unitOfWork.CHSessionDataRepository.GetAsync(sessionId);
                
                if (session == null)
                {
                    _logger.LogWarning($"Session {sessionId} not found");
                    return false;
                }

                // Soft delete by setting Status to false
                session.Status = false;
                session.UpdatedBy = "System";
                session.UpdatedOn = DateTime.Now;
                session.UpdatedFrom = "Blazor";

                _unitOfWork.CHSessionDataRepository.Update(session);
                var result = await _unitOfWork.SaveChangesAsync();

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting session {sessionId}");
                return false;
            }
        }
    }
}

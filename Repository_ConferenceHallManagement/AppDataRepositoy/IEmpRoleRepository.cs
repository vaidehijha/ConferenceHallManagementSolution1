using Models_ConferenceHallManagement.AppDbModels;

namespace Repository_ConferenceHallManagement.UtilityRepository
{
    public interface IEmpRoleRepository : IRepository<EmpRole> // Assuming generic repo exists
    {
        // Login ke baad ye call hoga
        Task<EmpRole?> GetUserRoleDetailsAsync(string empNo);
    }
}
using DAL_ConferenceHallManagement.DbContexts;
using Microsoft.EntityFrameworkCore;
using Models_ConferenceHallManagement.AppDbModels;
// Add appropriate namespaces

namespace Repository_ConferenceHallManagement.UtilityRepository
{
    public class EmpRoleRepository : Repository<EmpRole>, IEmpRoleRepository
    {
        private readonly ConferenceHallManagementContext _db;

        public EmpRoleRepository(ConferenceHallManagementContext db) : base(db)
        {
            _db = db;
        }

        public async Task<EmpRole?> GetUserRoleDetailsAsync(string empNo)
        {
            // Ye query user ka role, region aur location sab ek saath layegi
            return await _db.EmpRoles
                .Include(u => u.Region)
                .Include(u => u.Location)
                .Where(u => u.EmpNo == empNo && u.Status == true)
                .FirstOrDefaultAsync();
        }
    }
}
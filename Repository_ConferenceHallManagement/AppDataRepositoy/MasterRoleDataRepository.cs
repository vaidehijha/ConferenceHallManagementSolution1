using DAL_ConferenceHallManagement.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models_ConferenceHallManagement.AppDbModels; // Ye namespace add karna zaroori hai

namespace Repository_ConferenceHallManagement.AppDataRepositoy
{
    public class MasterRoleDataRepository : IMasterRoleDataRepository
    {
        private readonly ConferenceHallManagementContext _context;

        public MasterRoleDataRepository(ConferenceHallManagementContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<MasterRole>> GetAllAsync()
        {
            // Database se asli MasterRoles table ka data la rahe hain
            return await _context.MasterRoles.ToListAsync();
        }
    }
}
using DAL_ConferenceHallManagement.DbContexts;
using Microsoft.EntityFrameworkCore;
using Models_ConferenceHallManagement.AppDbModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository_ConferenceHallManagement.AppDataRepositoy
{
    public class MasterRegionDataRepository : IMasterRegionDataRepository
    {
        private readonly ConferenceHallManagementContext _context;

        public MasterRegionDataRepository(ConferenceHallManagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MasterRegion>> GetAllAsync()
        {
            return await _context.MasterRegions.ToListAsync();
        }
    }
}
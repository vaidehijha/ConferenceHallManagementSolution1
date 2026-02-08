using DAL_ConferenceHallManagement.DbContexts;
using Microsoft.EntityFrameworkCore;
using Models_ConferenceHallManagement.AppDbModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository_ConferenceHallManagement.AppDataRepositoy
{
    public class MasterLocationDataRepository : IMasterLocationDataRepository
    {
        private readonly ConferenceHallManagementContext _context;

        public MasterLocationDataRepository(ConferenceHallManagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MasterLocation>> GetAllAsync()
        {
            return await _context.MasterLocations.Include(l => l.Region).ToListAsync();
        }
    }
}
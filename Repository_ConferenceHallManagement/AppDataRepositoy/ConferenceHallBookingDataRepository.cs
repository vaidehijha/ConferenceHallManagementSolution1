using DAL_ConferenceHallManagement.DbContexts;
using Models_ConferenceHallManagement.AppDbModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository_ConferenceHallManagement.AppDataRepositoy
{
    public class ConferenceHallBookingDataRepository : Repository<ConferenceHallBooking>, IConferenceHallBookingDataRepository
    {
        private readonly ConferenceHallManagementContext _appContext;

        public ConferenceHallBookingDataRepository(ConferenceHallManagementContext context) : base(context)
        {
            _appContext = context;
        }

        // --- FIX 1: Ye Method Missing tha, isliye error aa raha tha ---
        public async Task<List<ConferenceHall>> GetAllHallsAsync()
        {
            return await _appContext.ConferenceHalls
                .Where(h => h.Status == true)
                .OrderBy(h => h.HallNameEn)
                .ToListAsync();
        }
        // -------------------------------------------------------------

        public async Task AddAsync(ConferenceHallBooking entity)
        {
            await _appContext.ConferenceHallBookings.AddAsync(entity);
        }

        public async Task<ConferenceHallBooking> GetAsync(int id)
        {
            return await _appContext.ConferenceHallBookings
                .Include(b => b.Hall)
                .Include(b => b.StatusNavigation)
                .Include(b => b.RoomType)
                .Include(b => b.ConferenceHallBookingSessions).ThenInclude(s => s.Session)
                .Include(b => b.ConferenceHallBookingSessions).ThenInclude(s => s.StatusNavigation)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        public void Update(ConferenceHallBooking entity)
        {
            _appContext.ConferenceHallBookings.Update(entity);
        }

        public async Task<IEnumerable<ConferenceHallBooking>> GetFilteredBookings(int roleId, int? regionId, int? locationId)
        {
            var query = _appContext.ConferenceHallBookings
                .Include(b => b.Hall)
                .Include(b => b.StatusNavigation)
                .Include(b => b.RoomType)
                .Include(b => b.ConferenceHallBookingSessions).ThenInclude(s => s.Session)
                .Include(b => b.ConferenceHallBookingSessions).ThenInclude(s => s.StatusNavigation)
                .AsQueryable();

            if (roleId == 3 && regionId.HasValue)
                query = query.Where(b => b.Hall.RegionId == regionId.Value);
            else if (roleId == 4 && locationId.HasValue)
                query = query.Where(b => b.Hall.LocationId == locationId.Value);

            return await query.OrderByDescending(b => b.BookingId).ToListAsync();
        }

        public async Task<IEnumerable<ConferenceHallBooking>> GetBookingsByUserIdAsync(string userId)
        {
            return await _appContext.ConferenceHallBookings
                .Include(b => b.Hall)
                .Include(b => b.StatusNavigation)
                .Include(b => b.RoomType)
                .Include(b => b.ConferenceHallBookingSessions).ThenInclude(s => s.Session)
                .Include(b => b.ConferenceHallBookingSessions).ThenInclude(s => s.StatusNavigation)
                .Where(b => b.CreatedBy == userId)
                .OrderByDescending(b => b.BookingId)
                .ToListAsync();
        }
    }
}
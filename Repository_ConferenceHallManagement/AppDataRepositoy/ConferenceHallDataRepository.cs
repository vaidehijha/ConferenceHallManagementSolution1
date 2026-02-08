using DAL_ConferenceHallManagement.DbContexts;
using Models_ConferenceHallManagement.AppDbModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models_ConferenceHallManagement;

namespace Repository_ConferenceHallManagement.AppDataRepositoy
{
    public class ConferenceHallDataRepository : Repository<ConferenceHall>, IConferenceHallDataRepository
    {
        private readonly ConferenceHallManagementContext _appContext;

        public ConferenceHallDataRepository(ConferenceHallManagementContext context) : base(context)
        {
            _appContext = context;
        }

        // ---------------------------------------------------------
        // 1. GET ALL (For List View - Returns Entities)
        // ---------------------------------------------------------
        public async Task<List<ConferenceHall>> GetAllHallsAsync()
        {
            return await _appContext.ConferenceHalls
                .Where(h => h.Status == true)
                .OrderBy(h => h.HallNameEn)
                .ToListAsync();
        }

        public async Task<IEnumerable<ConferenceHall>> GetAllAsync()
        {
            return await _appContext.ConferenceHalls
                .Where(h => h.Status == true)
                .OrderBy(h => h.HallNameEn)
                .ToListAsync();
        }

        // ---------------------------------------------------------
        // 2. GET BY ID (For Edit View - Includes Sessions)
        // ---------------------------------------------------------
        public async Task<ConferenceHall> GetHallByIdAsync(int id)
        {
            return await _appContext.ConferenceHalls
                .Include(h => h.ConferenceHallSessions)
                .FirstOrDefaultAsync(h => h.HallId == id);
        }

        // ---------------------------------------------------------
        // 3. UPDATE (Handles Hall + Sessions)
        // ---------------------------------------------------------
        public async Task<bool> UpdateHallWithSessionsAsync(ConferenceHall updatedHall)
        {
            try
            {
                var strategy = _appContext.Database.CreateExecutionStrategy();

                return await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _appContext.Database.BeginTransactionAsync();
                    try
                    {
                        var existingHall = await _appContext.ConferenceHalls
                            .Include(h => h.ConferenceHallSessions)
                            .FirstOrDefaultAsync(h => h.HallId == updatedHall.HallId);

                        if (existingHall == null) return false;

                        // Update Hall Details ONLY
                        existingHall.HallName = updatedHall.HallName;
                        existingHall.HallNameEn = updatedHall.HallNameEn;
                        existingHall.HallNameHi = updatedHall.HallNameHi;
                        existingHall.Location = updatedHall.Location;
                        existingHall.Capacity = updatedHall.Capacity;
                        existingHall.Floor = updatedHall.Floor;
                        existingHall.IsApprovalRequired = updatedHall.IsApprovalRequired;
                        existingHall.LocationId = updatedHall.LocationId;
                        existingHall.RegionId = updatedHall.RegionId;
                        existingHall.UpdatedOn = DateTime.Now;
                        existingHall.UpdatedBy = updatedHall.UpdatedBy;
                        existingHall.UpdatedFrom = updatedHall.UpdatedFrom;

                        // DO NOT touch sessions at all - they are managed separately
                        // This prevents FK constraint violations when bookings exist

                        await _appContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return true;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ---------------------------------------------------------
        // Delete Hall
        // ---------------------------------------------------------
        public async Task<bool> DeleteHallAsync(int id)
        {
            var hall = await _appContext.ConferenceHalls.FindAsync(id);
            if (hall == null) return false;

            // Soft Delete: Just mark status as false (0)
            hall.Status = false;
            hall.UpdatedOn = DateTime.Now;
            hall.UpdatedBy = "System"; // Replace with user if available

            await _appContext.SaveChangesAsync();
            return true;
        }

        // ---------------------------------------------------------
        // Create Hall with Sessions
        // ---------------------------------------------------------
        public async Task<bool> CreateHallWithSessionsAsync(ConferenceHall hall)
        {
            try
            {
                var existingTransaction = _appContext.Database.CurrentTransaction;
                if (existingTransaction != null)
                {
                    await _appContext.ConferenceHalls.AddAsync(hall);
                    await _appContext.SaveChangesAsync();
                    return true;
                }

                using var transaction = await _appContext.Database.BeginTransactionAsync();
                try
                {
                    await _appContext.ConferenceHalls.AddAsync(hall);
                    await _appContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ---------------------------------------------------------
        // Get Filtered Bookings
        // ---------------------------------------------------------
        public async Task<IEnumerable<ConferenceHallBooking>> GetFilteredBookings(int roleId, int? regionId, int? locationId)
        {
            var query = _appContext.ConferenceHallBookings
                .Include(b => b.Hall)
                .Include(b => b.StatusNavigation)
                .Include(b => b.ConferenceHallBookingSessions)
                    .ThenInclude(s => s.Session)
                .Include(b => b.ConferenceHallBookingSessions)
                    .ThenInclude(s => s.StatusNavigation)
                .AsQueryable();

            if (roleId == 2 && regionId.HasValue)
            {
                query = query.Where(b => b.Hall.RegionId == regionId.Value);
            }
            else if (roleId == 3 && locationId.HasValue)
            {
                query = query.Where(b => b.Hall.LocationId == locationId.Value);
            }

            return await query.ToListAsync();
        }

        // ---------------------------------------------------------
        // Add Hall
        // ---------------------------------------------------------
        public async Task AddAsync(ConferenceHall entity)
        {
            await _context.ConferenceHalls.AddAsync(entity);
        }

        // ---------------------------------------------------------
        // Get All Hall Configurations (Legacy Method)
        // ---------------------------------------------------------
        public List<HallConfigurationModel> GetAllHallConfigurations()
        {
            return _appContext.ConferenceHalls
                .Where(h => h.Status == true)
                .Select(h => new HallConfigurationModel
                {
                    HallId = h.HallId,
                    HallName = h.HallNameEn,
                    Location = string.Empty,
                    Capacity = h.Capacity,
                    Status = h.Status,
                    IsApprovalRequired = h.IsApprovalRequired,
                    CreatedOn = h.CreatedOn,
                    CreatedBy = h.CreatedBy
                })
                .OrderBy(h => h.HallName)
                .ToList();
        }
    }
}
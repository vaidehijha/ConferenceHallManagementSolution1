using ConferenceHallManagement.web.ViewModels;
using Models_ConferenceHallManagement.AppDbModels;
using UoW_ConferenceHallManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConferenceHallManagement.web.Services
{
    public class HallConfigurationService : IHallConfigurationService
    {
        private readonly IUnitOfWork _uow;

        public HallConfigurationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // --- 1. DROPDOWN DATA METHODS ---
        public async Task<List<MasterRegion>> GetRegionsAsync()
        {
            var data = await _uow.MasterRegionDataRepository.GetAllAsync();
            return data.ToList();
        }

        public async Task<List<MasterLocation>> GetLocationsAsync()
        {
            var data = await _uow.MasterLocationDataRepository.GetAllAsync();
            return data.ToList();
        }

        public async Task<List<dynamic>> GetRolesAsync()
        {
            return new List<dynamic>();
        }

        // --- 2. GET ALL HALLS (With Region and Location Filter) ---
        public async Task<List<HallConfigurationVM>> GetAllHallsAsync(int? regionId = null, int? locationId = null)
        {
            var entities = await _uow.ConferenceHallDataRepository.GetAllAsync();
            var regions = await _uow.MasterRegionDataRepository.GetAllAsync();
            var locations = await _uow.MasterLocationDataRepository.GetAllAsync();

            var query = entities.Where(e => e.Status == true);
            
            // Apply region filter if provided (for Regional Admin)
            if (regionId.HasValue && regionId.Value > 0)
            {
                query = query.Where(e => e.RegionId == regionId.Value);
            }

            // Apply location filter if provided (for Station Admin)
            if (locationId.HasValue && locationId.Value > 0)
            {
                query = query.Where(e => e.LocationId == locationId.Value);
            }

            return query
                .Select(e => new HallConfigurationVM
                {
                    Id = e.HallId,
                    HallName = e.HallNameEn ?? e.HallName,
                    HallNameHindi = e.HallNameHi ?? string.Empty,
                    Floor = e.Floor ?? "Ground",
                    IsAdminApprovalRequired = e.IsApprovalRequired,
                    Capacity = e.Capacity,

                    RegionId = e.RegionId,
                    LocationId = e.LocationId,

                    // Names mapping for display
                    RegionName = regions.FirstOrDefault(r => r.Id == e.RegionId)?.RegionName ?? "-",
                    LocationName = locations.FirstOrDefault(l => l.Id == e.LocationId)?.LocationName ?? "-"
                }).ToList();
        }

        // --- 3. GET BY ID ---
        public async Task<HallConfigurationVM> GetHallByIdAsync(int id)
        {
            var entity = (await _uow.ConferenceHallDataRepository.GetAllAsync())
                         .FirstOrDefault(x => x.HallId == id);

            if (entity == null) return new HallConfigurationVM();

            return new HallConfigurationVM
            {
                Id = entity.HallId,
                HallName = entity.HallNameEn ?? entity.HallName,
                HallNameHindi = entity.HallNameHi ?? string.Empty,
                Floor = entity.Floor ?? "Ground",
                Capacity = entity.Capacity,
                IsAdminApprovalRequired = entity.IsApprovalRequired,
                RegionId = entity.RegionId,
                LocationId = entity.LocationId,

                Sessions = entity.ConferenceHallSessions?.Select(s => new SessionConfigVM
                {
                    Id = s.SessionId,
                    SessionName = s.SessionName,
                    StartTime = DateTime.Today.Add(s.StartTime),
                    EndTime = DateTime.Today.Add(s.EndTime),
                    Price = s.Price
                }).ToList() ?? new List<SessionConfigVM>()
            };
        }

        // --- 4. CREATE ---
        public async Task<bool> CreateHallAsync(HallConfigurationVM vm)
        {
            try
            {
                var currentTime = DateTime.Now;
                var currentUser = "System";

                var hallEntity = new ConferenceHall
                {
                    HallName = vm.HallName,
                    HallNameEn = vm.HallName,
                    HallNameHi = vm.HallNameHindi,
                    Floor = vm.Floor,
                    Capacity = vm.Capacity,
                    IsApprovalRequired = vm.IsAdminApprovalRequired,
                    RegionId = vm.RegionId,
                    LocationId = vm.LocationId,

                    Status = true, // Default Active
                    CreatedBy = currentUser,
                    CreatedOn = currentTime,
                    CreatedFrom = "Web",
                    UpdatedBy = currentUser,
                    UpdatedOn = currentTime,
                    UpdatedFrom = "Web"
                };

                if (vm.Sessions != null && vm.Sessions.Any())
                {
                    hallEntity.ConferenceHallSessions = vm.Sessions.Select(s => new ConferenceHallSession
                    {
                        SessionName = s.SessionName,
                        StartTime = s.StartTime.TimeOfDay,
                        EndTime = s.EndTime.TimeOfDay,
                        Price = s.Price,
                        SessionEn = s.SessionName,
                        SessionHi = s.SessionName,
                        Status = true,
                        CreatedBy = currentUser,
                        CreatedOn = currentTime,
                        CreatedFrom = "Web",
                        UpdatedBy = currentUser,
                        UpdatedOn = currentTime,
                        UpdatedFrom = "Web"
                    }).ToList();
                }

                await _uow.ConferenceHallDataRepository.AddAsync(hallEntity);
                await _uow.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // --- 5. UPDATE ---
        public async Task<bool> UpdateHallAsync(HallConfigurationVM vm)
        {
            try
            {
                var halls = await _uow.ConferenceHallDataRepository.GetAllAsync();
                var existing = halls.FirstOrDefault(x => x.HallId == vm.Id);

                if (existing == null) return false;

                existing.HallName = vm.HallName;
                existing.HallNameEn = vm.HallName;
                existing.HallNameHi = vm.HallNameHindi;
                existing.Floor = vm.Floor;
                existing.Capacity = vm.Capacity;
                existing.IsApprovalRequired = vm.IsAdminApprovalRequired;
                existing.RegionId = vm.RegionId;
                existing.LocationId = vm.LocationId;

                existing.UpdatedBy = "System";
                existing.UpdatedOn = DateTime.Now;
                existing.UpdatedFrom = "Web";

                await _uow.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // --- 6. DELETE (Soft Delete) ---
        public async Task<bool> DeleteHallAsync(int id)
        {
            try
            {
                var halls = await _uow.ConferenceHallDataRepository.GetAllAsync();
                var entity = halls.FirstOrDefault(x => x.HallId == id);

                if (entity != null)
                {
                    entity.Status = false; // Soft delete (Hides from list)
                    entity.UpdatedBy = "System";
                    entity.UpdatedOn = DateTime.Now;

                    await _uow.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
using UoW_ConferenceHallManagement;
using Models_ConferenceHallManagement.AppDbModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ConferenceHallManagement.web.Services
{
    public class MasterDataService : IMasterDataService
    {
        private readonly IUnitOfWork _uow;

        public MasterDataService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<MasterRegion>> GetAllRegionsAsync()
        {
            // UnitOfWork ke naye repo se data la raha hai
            var data = await _uow.MasterRegionDataRepository.GetAllAsync();
            return data.ToList();
        }

        public async Task<List<MasterLocation>> GetAllLocationsAsync()
        {
            var data = await _uow.MasterLocationDataRepository.GetAllAsync();
            return data.ToList();
        }

        public async Task<List<MasterLocation>> GetLocationsByRegionAsync(int regionId)
        {
            // Pehle sab lao phir filter karo (Ya Repo me method bana lo to behtar hai)
            var allData = await _uow.MasterLocationDataRepository.GetAllAsync();
            return allData.Where(l => l.RegionId == regionId).ToList();
        }

        public async Task<List<dynamic>> GetAllRolesAsync()
        {
            // Agar MasterRoleDataRepository banayi hai to usse use karo
            // Nahi to TempEmployeeRole wali use kar sakte ho
            var data = await _uow.MasterRoleDataRepository.GetAllAsync();
            return data.ToList<dynamic>();
        }
    }
}
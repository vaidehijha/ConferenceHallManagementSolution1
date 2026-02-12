using ConferenceHallManagement.web.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConferenceHallManagement.web.Services
{
    public interface IHallConfigurationService
    {
        // CRUD Operations
        Task<List<HallConfigurationVM>> GetAllHallsAsync(int? regionId = null, int? locationId = null);
        Task<HallConfigurationVM> GetHallByIdAsync(int id);
        Task<bool> CreateHallAsync(HallConfigurationVM model);
        Task<bool> UpdateHallAsync(HallConfigurationVM model);
        Task<bool> DeleteHallAsync(int id);
    }
}
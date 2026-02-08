using System.Collections.Generic;
using System.Threading.Tasks;
using Models_ConferenceHallManagement.AppDbModels;
using Models_ConferenceHallManagement;

namespace Repository_ConferenceHallManagement.AppDataRepositoy
{
    public interface IConferenceHallDataRepository : IRepository<ConferenceHall>
    {
        // 1. Create New Hall
        Task<bool> CreateHallWithSessionsAsync(ConferenceHall hall);

        // 2. Get All Halls (For the List Page)
        Task<List<ConferenceHall>> GetAllHallsAsync();

        // 3. Get Single Hall by ID (For the Edit Page)
        Task<ConferenceHall> GetHallByIdAsync(int id);

        // 4. Update Existing Hall
        Task<bool> UpdateHallWithSessionsAsync(ConferenceHall hall);

        // 5. DELETE HALL (This was the missing line causing your error)
        Task<bool> DeleteHallAsync(int id);
        Task<IEnumerable<ConferenceHallBooking>> GetFilteredBookings(int roleId, int? regionId, int? locationId);

        // Interface ke andar
        Task AddAsync(ConferenceHall entity);

        // 6. Existing List Method (Legacy)
        List<HallConfigurationModel> GetAllHallConfigurations();
    }
}
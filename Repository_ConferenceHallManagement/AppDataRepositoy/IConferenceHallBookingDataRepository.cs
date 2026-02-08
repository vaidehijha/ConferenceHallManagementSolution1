using Models_ConferenceHallManagement.AppDbModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository_ConferenceHallManagement.AppDataRepositoy
{
    public interface IConferenceHallBookingDataRepository : IRepository<ConferenceHallBooking>
    {
        Task<List<ConferenceHall>> GetAllHallsAsync();
        Task AddAsync(ConferenceHallBooking entity);
        Task<ConferenceHallBooking> GetAsync(int id);
        void Update(ConferenceHallBooking entity);
        Task<IEnumerable<ConferenceHallBooking>> GetFilteredBookings(int roleId, int? regionId, int? locationId);
        Task<IEnumerable<ConferenceHallBooking>> GetBookingsByUserIdAsync(string userId);
    }
}

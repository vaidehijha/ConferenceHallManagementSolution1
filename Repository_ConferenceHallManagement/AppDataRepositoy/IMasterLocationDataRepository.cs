using Models_ConferenceHallManagement.AppDbModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository_ConferenceHallManagement.AppDataRepositoy
{
    public interface IMasterLocationDataRepository
    {
        Task<IEnumerable<MasterLocation>> GetAllAsync();
    }
}
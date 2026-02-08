
using System.Collections.Generic;
using System.Threading.Tasks;
using Models_ConferenceHallManagement.AppDbModels;

namespace Repository_ConferenceHallManagement.AppDataRepositoy
{
    public interface IMasterRoleDataRepository
    {
        // Yahan aap apna Role model use karein
        Task<IEnumerable<MasterRole>> GetAllAsync();
    }
}
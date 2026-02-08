using Models_ConferenceHallManagement.AppDbModels; // Apne models ka namespace check kar lena
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConferenceHallManagement.web.Services
{
    public interface IMasterDataService
    {
        // Regions Dropdown ke liye
        Task<List<MasterRegion>> GetAllRegionsAsync();

        // Locations Dropdown ke liye
        Task<List<MasterLocation>> GetAllLocationsAsync();

        // Cascading ke liye (Region select karne par Locations filter honge)
        Task<List<MasterLocation>> GetLocationsByRegionAsync(int regionId);

        // Roles Dropdown ke liye (Agar Role model dynamic hai to object/dynamic use kar sakte ho)
        // Agar MasterRole model hai to 'MasterRole' likhna
        Task<List<dynamic>> GetAllRolesAsync();
    }
}
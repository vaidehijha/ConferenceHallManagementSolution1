using DAL_ConferenceHallManagement.DbContexts;
using Microsoft.EntityFrameworkCore;
using Models_ConferenceHallManagement.AppDbModels;
using Models_ConferenceHallManagement.EmpDetDbModels;
using Microsoft.Extensions.DependencyInjection; // NOTE: Ye namespace zaroor add karein
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository_ConferenceHallManagement.UtilityRepository
{
    public interface IEmployeeRepository
    {
        Task<VActiveUserDetailsWith8DigitEmpNo?> GetEmployeeById(string empNum);
        Task<bool> AuthenticateUser(string userName, string password);
        Task<IEnumerable<EmpRole>?> GetEmployeeRoles(string empNo);
    }

    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly EmpdetContext _db;
        private readonly IServiceScopeFactory _scopeFactory; // FIX: Factory use karenge

        // FIX: Constructor update kiya hai (ConferenceHallContext hata kar Factory laye hain)
        public EmployeeRepository(EmpdetContext db, IServiceScopeFactory scopeFactory)
        {
            _db = db;
            _scopeFactory = scopeFactory;
        }

        // 1. Employee Details (Mock Data - Same as before)
        public async Task<VActiveUserDetailsWith8DigitEmpNo?> GetEmployeeById(string empNum)
        {
            string mockName = "";
            string mockEmail = "";
            string mockPhone = "";

            switch (empNum)
            {
                case "60020656": mockName = "Vaidehi"; mockEmail = "60020656@powergrid.in"; mockPhone = "9958096048"; break;
                case "60050045": mockName = "Saurav"; mockEmail = "60050045@powergrid.in"; mockPhone = "9266998577"; break;
                case "10000001": mockName = "Super Admin"; mockEmail = "superadmin@powergrid.in"; mockPhone = "1111111111"; break;
                case "10000002": mockName = "Central Admin"; mockEmail = "centraladmin@powergrid.in"; mockPhone = "2222222222"; break;
                case "10000003": mockName = "Regional Admin"; mockEmail = "regionaladmin@powergrid.in"; mockPhone = "3333333333"; break;
                case "10000004": mockName = "Station Admin"; mockEmail = "stationadmin@powergrid.in"; mockPhone = "4444444444"; break;
                default:
                    mockName = $"Employee {empNum}";
                    mockEmail = $"{empNum}@powergrid.in";
                    mockPhone = "9999999999";
                    break;
            }

            var mockEmp = new VActiveUserDetailsWith8DigitEmpNo
            {
                EightDigitEmpNo = empNum,
                Empname = mockName,
                Pgemail = mockEmail,
                Cellno = mockPhone,
                Empimgguid = System.Guid.NewGuid()
            };

            await Task.Delay(50);
            return mockEmp;
        }

        // 2. Login Check (FIXED with Scope)
        public async Task<bool> AuthenticateUser(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName)) return false;

            // FIX: Naya Scope create kiya taaki Concurrency error na aaye
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbApp = scope.ServiceProvider.GetRequiredService<ConferenceHallManagementContext>();

                var hasRole = await dbApp.EmpRoles.AnyAsync(x => x.EmpNo == userName);
                return hasRole;
            }
        }

        // 3. Roles Fetching (FIXED with Scope)
        public async Task<IEnumerable<EmpRole>?> GetEmployeeRoles(string empNo)
        {
            // FIX: Yahan bhi naya scope use kiya
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbApp = scope.ServiceProvider.GetRequiredService<ConferenceHallManagementContext>();

                return await dbApp.EmpRoles
                                  .Include(x => x.MasterRole)
                                  .Where(x => x.EmpNo == empNo)
                                  .ToListAsync();
            }
        }
    }
}
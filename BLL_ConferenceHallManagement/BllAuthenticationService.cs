using Models_ConferenceHallManagement.AppDbModels;
using Models_ConferenceHallManagement.DTOs;
using Models_ConferenceHallManagement.EmpDetDbModels;
using Repository_ConferenceHallManagement.UtilityRepository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL_ConferenceHallManagement
{
    public interface IBllAuthenticationService
    {
        /// <summary>
        /// Authenticates user and returns login result with status message
        /// </summary>
        Task<LoginResult> LoginAsync(string empNo, string password);
        
        /// <summary>
        /// Builds complete user session details with all roles, regions, and locations
        /// </summary>
        Task<UserSessionDetails?> BuildUserSessionAsync(string empNo);
    }

    public class BllAuthenticationService : IBllAuthenticationService
    {
        private readonly IEmployeeRepository _employeeRepo;
        private const string HARDCODED_PASSWORD = "Pass@123";

        public BllAuthenticationService(IEmployeeRepository employeeRepo)
        {
            _employeeRepo = employeeRepo;
        }

        public async Task<LoginResult> LoginAsync(string empNo, string password)
        {
            // 1. Check if employee ID exists in database
            var empDetails = await _employeeRepo.GetEmployeeById(empNo);
            if (empDetails == null)
            {
                return new LoginResult 
                { 
                    Success = false, 
                    Message = "Employee ID does not exist", 
                    MessageType = "error" 
                };
            }

            // 2. Validate password (hardcoded check since no password column in DB)
            if (password != HARDCODED_PASSWORD)
            {
                return new LoginResult 
                { 
                    Success = false, 
                    Message = "Invalid Password", 
                    MessageType = "error" 
                };
            }

            // 3. Build user session (works for both users with roles and without roles)
            var userSession = await BuildUserSessionAsync(empNo);
            if (userSession == null)
            {
                return new LoginResult 
                { 
                    Success = false, 
                    Message = "Error loading user details", 
                    MessageType = "error" 
                };
            }

            return new LoginResult 
            { 
                Success = true, 
                Message = "Login Successful", 
                MessageType = "success",
                UserSession = userSession
            };
        }

        public async Task<UserSessionDetails?> BuildUserSessionAsync(string empNo)
        {
            // 1. Get employee basic details
            var empDetails = await _employeeRepo.GetEmployeeById(empNo);
            if (empDetails == null)
                return null;

            // 2. Get all roles with navigation properties loaded (single query with includes)
            var empRoles = await _employeeRepo.GetEmployeeRoles(empNo);
            
            // 3. Build UserSessionDetails
            var userSession = new UserSessionDetails
            {
                EmpNo = empDetails.EightDigitEmpNo,
                UserName = empDetails.Empname ?? "Unknown User",
                Email = empDetails.Pgemail ?? string.Empty,
                Mobile = empDetails.Cellno ?? string.Empty,
                EmpImageGuid = empDetails.Empimgguid?.ToString() ?? string.Empty,
                Roles = new List<UserRoleInfo>()
            };

            // 4. Map roles if user has any, otherwise treat as normal employee
            if (empRoles != null && empRoles.Any())
            {
                userSession.Roles = empRoles.Select(role => new UserRoleInfo
                {
                    RoleId = role.RoleId,
                    RoleName = role.MasterRole?.RoleName ?? GetRoleNameById(role.RoleId),
                    RegionId = role.RegionId,
                    RegionName = role.Region?.RegionName ?? string.Empty,
                    LocationId = role.LocationId,
                    LocationName = role.Location?.LocationName ?? string.Empty
                }).ToList();
            }
            else
            {
                // User has NO roles - treat as normal employee (RoleId = 0)
                userSession.Roles = new List<UserRoleInfo>
                {
                    new UserRoleInfo
                    {
                        RoleId = 0,
                        RoleName = "Employee",
                        RegionId = null,
                        RegionName = string.Empty,
                        LocationId = null,
                        LocationName = string.Empty
                    }
                };
            }

            return userSession;
        }

        /// <summary>
        /// Fallback method to get role name by ID if navigation property is null
        /// </summary>
        private string GetRoleNameById(int roleId)
        {
            return roleId switch
            {
                1 => "Super Admin",
                2 => "Central Admin",
                3 => "Regional Admin",
                4 => "Station Admin",
                0 => "Employee",
                _ => "Unknown"
            };
        }
    }

    /// <summary>
    /// Login result DTO
    /// </summary>
    public class LoginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string MessageType { get; set; } = string.Empty; // success, error, warning
        public UserSessionDetails? UserSession { get; set; }
    }
}

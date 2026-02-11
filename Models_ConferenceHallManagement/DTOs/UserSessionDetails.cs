using System.Collections.Generic;

namespace Models_ConferenceHallManagement.DTOs
{
    /// <summary>
    /// User Session DTO that holds employee details and their role information
    /// Used for Claims-based authentication without session storage
    /// </summary>
    public class UserSessionDetails
    {
        public string EmpNo { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string EmpImageGuid { get; set; } = string.Empty;
        
        /// <summary>
        /// List of roles assigned to the user with region and location details
        /// A user can have multiple roles
        /// SINGLE SOURCE OF TRUTH for role information
        /// Use extension methods (GetPrimaryRole, GetPrimaryRoleId, GetPrimaryRoleName, HasRole) to access role data
        /// </summary>
        public List<UserRoleInfo> Roles { get; set; } = new List<UserRoleInfo>();
    }
}

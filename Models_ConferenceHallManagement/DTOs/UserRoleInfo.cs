namespace Models_ConferenceHallManagement.DTOs
{
    /// <summary>
    /// Contains role information with associated region and location details
    /// </summary>
    public class UserRoleInfo
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        
        public int? RegionId { get; set; }
        public string RegionName { get; set; } = string.Empty;
        
        public int? LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
    }
}

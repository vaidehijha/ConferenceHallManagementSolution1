using System.ComponentModel.DataAnnotations;

namespace Models_ConferenceHallManagement.AppDbModels
{
    public class MasterRole
    {
        [Key]
        public int Id { get; set; } // Ye database mein RoleId ke barabar hoga

        public string RoleName { get; set; } // e.g., "Super Admin", "Regional Admin"

        public int Status { get; set; }

        // Agar audit columns chahiye toh wo bhi add kar sakte ho
        // public string CreatedBy { get; set; }
        // public DateTime CreatedOn { get; set; }
    }
}
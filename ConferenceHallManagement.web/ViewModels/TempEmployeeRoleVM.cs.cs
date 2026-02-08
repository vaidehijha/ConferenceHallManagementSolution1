using System.ComponentModel.DataAnnotations;

namespace ConferenceHallManagement.Web.ViewModels
{
    public class TempEmployeeRoleVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee Number is required")]
        [StringLength(50)]
        public string EmployeeNo { get; set; } = "";

        public int ApplicationId { get; set; } = 1;

        [Range(1, int.MaxValue, ErrorMessage = "Region is required")]
        public int RegionId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Location is required")]
        public int LocationId { get; set; }

        // DepartmentId hata diya gaya hai

        [Range(1, int.MaxValue, ErrorMessage = "Role is required")]
        public int RoleId { get; set; }

        public bool IsAllowWrite { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
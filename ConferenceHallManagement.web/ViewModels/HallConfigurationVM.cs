using System.ComponentModel.DataAnnotations;

namespace ConferenceHallManagement.web.ViewModels
{
    // PARENT MODEL: The Hall
    public class HallConfigurationVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter the Hall Name (English)")]
        [StringLength(100, ErrorMessage = "Name is too long")]
        public string HallName { get; set; } = string.Empty;

        public string HallNameHindi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Floor is required")]
        public string Floor { get; set; } = string.Empty;

        [Range(1, 10000, ErrorMessage = "Capacity must be greater than 0")]
        public int Capacity { get; set; }

        public bool IsAdminApprovalRequired { get; set; }

        // --- NEW CHANGES START ---

        // Dropdown selection ke liye ID zaroori hai
        [Required(ErrorMessage = "Region is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Region")]
        public int RegionId { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Location")]
        public int LocationId { get; set; }

        // Display ke liye names (Optional, List page ke liye helpful)
        public string RegionName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;

        // --- NEW CHANGES END ---

        public List<SessionConfigVM> Sessions { get; set; } = new List<SessionConfigVM>();
    }

    public class SessionConfigVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Session Name is required")]
        public string SessionName { get; set; } = string.Empty;

        [Required]
        public DateTime StartTime { get; set; } = DateTime.Today;

        [Required]
        public DateTime EndTime { get; set; } = DateTime.Today;

        [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative")]
        public decimal Price { get; set; }
    }
}
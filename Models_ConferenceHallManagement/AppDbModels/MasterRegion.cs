using System.ComponentModel.DataAnnotations;

namespace Models_ConferenceHallManagement.AppDbModels
{
    public class MasterRegion
    {
        [Key]
        public int Id { get; set; } // Matches RegionId in DB
        public string RegionName { get; set; }
        public int Status { get; set; }
        // Add other audit fields (CreatedBy, CreatedOn etc.) if needed
    }
}
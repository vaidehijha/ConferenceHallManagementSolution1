using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models_ConferenceHallManagement.AppDbModels
{
    public class MasterLocation
    {
        [Key]
        public int Id { get; set; } // Matches LocationId
        public string LocationName { get; set; }
        public int RegionId { get; set; } // Foreign Key
        public int Status { get; set; }

        [ForeignKey("RegionId")]
        public MasterRegion Region { get; set; }
    }
}
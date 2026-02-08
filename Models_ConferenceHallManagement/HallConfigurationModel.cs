using System;

namespace Models_ConferenceHallManagement
{
    public class HallConfigurationModel
    {
        public int HallId { get; set; }
        public string HallName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public bool Status { get; set; }
        public bool IsApprovalRequired { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}

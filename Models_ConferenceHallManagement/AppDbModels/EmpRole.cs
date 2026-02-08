using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models_ConferenceHallManagement.AppDbModels
{
    [Table("EmpRoles")] // Database Table Name
    public class EmpRole
    {
        [Key]
        public int Id { get; set; }

        public string EmpNo { get; set; } // Employee Number

        public int RoleId { get; set; }

        public int? RegionId { get; set; } // Nullable

        public int? LocationId { get; set; } // Nullable

        public bool Status { get; set; } // Active/Inactive

        // --- Audit Fields (Jo pehle missing the) ---
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedFrom { get; set; }

        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedFrom { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("RegionId")]
        public virtual MasterRegion Region { get; set; }

        [ForeignKey("LocationId")]
        public virtual MasterLocation Location { get; set; }

        [ForeignKey("RoleId")]
        public virtual MasterRole MasterRole { get; set; }
    }
}
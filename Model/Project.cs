using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace ProjectWBSAPI.Model
{
    [Table("Project")]
    public class Project
    {
        [Key]
        public Int64 ProjectID { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public int BU { get; set; }
        public string? ProjectDescription { get; set; }
        public DateTime? ProjectStartDate { get; set; }
        public DateTime? ProjectEndDate { get; set; }
        public decimal ProjectDuration { get; set; }
        public string? ProjectStatus { get; set; }
        public DateTime CreatedAt { get; set; }=DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}

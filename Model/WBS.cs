using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace ProjectWBSAPI.Model
{
    [Table("WBS")]
    public class WBS
    {
        [Key]
        public Int64 WBSID { get; set; }
        public string? WBSCode { get; set; }
        public Int64 ProjectID { get; set; }
        public string? WBSName { get; set; }
        public string? WBSDescription { get; set; }
        public Int64 ParentWBSID { get; set; }
        public DateTime? WBSStartDate { get; set; }
        public DateTime? WBSEndDate { get; set; }
        public decimal WBSDuration { get; set; }
        public string? WBSStatus { get; set; }
        public DateTime? CreatedAt { get; set; }=DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectWBSAPI.Model
{
    [Table("BusinessDivision")]
    public class BusinessDivisions
    {
        [Key]
        public int BusinessID { get; set; }
        public string? BusinessDivision { get; set; }
        public string? CreatedByID { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedByID { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? Status { get; set; }
    }
}

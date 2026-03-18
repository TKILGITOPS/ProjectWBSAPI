using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace ProjectWBSAPI.Model
{
    public class ProjectDto
    {
        public string? ProjectCode { get; set; }
        public string? BU { get; set; }
        public string? ProjectDescription { get; set; }
    }
}

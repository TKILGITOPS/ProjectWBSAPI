using Microsoft.EntityFrameworkCore;

namespace ProjectWBSAPI.Model
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }

        public DbSet<Project> Project { get;set; }

        public DbSet<WBS> WBS { get; set; } 

        public DbSet<BusinessDivisions> BusinessDivisions { get; set; }
    }
}

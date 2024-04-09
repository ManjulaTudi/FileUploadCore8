using FileUploadCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileUploadCore.Data
{
    public class DbContextClass : DbContext
    {
        protected readonly IConfiguration Configuration;

        public DbContextClass(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
        }

        public DbSet<FileDetails> FileDetails { get; set; }
    }
}

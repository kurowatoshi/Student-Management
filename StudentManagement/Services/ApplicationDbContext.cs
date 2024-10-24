using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;

namespace StudentManagement.Services
{
    public class ApplicationDbContext: DbContext

    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Student> Students { get; set; }
    }
}

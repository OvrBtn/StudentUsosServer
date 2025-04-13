using Microsoft.EntityFrameworkCore;
using StudentUsosServer.Models;

namespace StudentUsosServer.Database
{
    public class MainDBContext : DbContext
    {
        public MainDBContext(DbContextOptions<MainDBContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<AppLog> AppLogs { get; set; }
    }
}

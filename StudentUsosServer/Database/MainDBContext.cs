using Microsoft.EntityFrameworkCore;
using StudentUsosServer.Features.CampusMap.Models;
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

        public DbSet<UserRoomInfoSuggestion> UserRoomInfoSuggestions { get; set; }
        public DbSet<RoomInfo> RoomInfos { get; set; }
        public DbSet<UserSuggestionVote> UserSuggestionVotes { get; set; }
    }
}

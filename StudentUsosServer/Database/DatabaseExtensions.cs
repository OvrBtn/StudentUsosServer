using Microsoft.EntityFrameworkCore;

namespace StudentUsosServer.Database
{
    public static class DatabaseExtensions
    {
        public static async Task MigrateDatabase(this WebApplication app)
        {
            var scope = app.Services.CreateScope();
            MainDBContext? mainDBContext = scope.ServiceProvider.GetService<MainDBContext>();
            if (mainDBContext != null)
            {
                await mainDBContext.Database.MigrateAsync();
            }
        }
    }
}

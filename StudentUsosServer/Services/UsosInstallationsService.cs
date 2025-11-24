using StudentUsosServer.Features.CampusMap.Models;
using StudentUsosServer.Models.Usos;
using System.Text.Json;

namespace StudentUsosServer.Services
{
    public class UsosInstallationsService
    {
        readonly IWebHostEnvironment _environment;
        public UsosInstallationsService(IWebHostEnvironment environment)
        {
            _environment = environment;
            Initialize().Wait();
        }

        async Task Initialize()
        {
            Installations = await LoadInstallations();
        }

        public List<UsosInstallation> Installations { get; private set; }

        async Task<List<UsosInstallation>> LoadInstallations()
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "Resources", "UsosInstallations.json");
            var jsonData = await File.ReadAllTextAsync(filePath);

            var deserialized = JsonSerializer.Deserialize<List<UsosInstallation>>(jsonData)!;

            return deserialized;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>null if no installation with given id was found</returns>
        public string? GetUrl(string id)
        {
            return Installations.Where(x => x.InstallationId == id).FirstOrDefault()?.InstallationUrl;
        }

        public InstallationConsumerKeys? GetUsosConsumerKeys(string installationUrl)
        {
            if (Secrets.Default.UsosConsumerKeys.TryGetValue(installationUrl, out var keys))
            {
                return keys;
            }
            return null;
        }
    }
}

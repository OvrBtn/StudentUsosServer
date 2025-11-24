namespace StudentUsosServer.Models.Usos
{
    public class UsosInstallation
    {
        public string InstallationId { get; set; }
        public string InstallationUrl { get; set; }
        public Dictionary<string, string> LocalizedName { get; set; }
    }
}

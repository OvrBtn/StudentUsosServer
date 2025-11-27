using System.ComponentModel.DataAnnotations;

namespace StudentUsosServer.Models
{
#pragma warning disable 8618
    public class AppLog
    {
        [Key]
        public int Id { get; set; }
        public string LogLevel { get; set; }
        public string Message { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionSerialized { get; set; }
        public string? DeviceInfo { get; set; }
        public string? OperatingSystem { get; set; }
        public string? OperatingSystemVersion { get; set; }
        public string? AppVersion { get; set; }
        public string CallerName { get; set; }
        public string CallerLineNumber { get; set; }
        public string CreationDate { get; set; }
        public long CreationDateUnix { get; set; }

        //Filled on server side
        public string? UserInstallation { get; set; }
        public string? UserUsosId { get; set; }
        public string? ApiVersion { get; set; }

    }
}

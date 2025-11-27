using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using StudentUsosServer.Database;
using StudentUsosServer.Filters;
using StudentUsosServer.Models;

namespace StudentUsosServer.Controllers.V1
{
    [ApiController, Route("v{version:apiVersion}/logs"), ApiVersion(1)]
    public class AppLogsController : ControllerBase
    {
        MainDBContext _dbContext;

        public AppLogsController(MainDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public class LogRequestPayload
        {
            public List<AppLog> Logs { get; set; }
            //public string Installation { get; set; }
            public string UserUsosId { get; set; }
        }

        /// <summary>
        /// Adds a log about error in app to local database
        /// </summary>
        /// <returns></returns>
        [HttpPost("log"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full)]
        public async Task<ActionResult> Log([FromBody] LogRequestPayload payload,
            [FromHeader] string installation,
            [FromHeader] string? apiVersion,
            [FromHeader] string? applicationVersion)
        {
            foreach (var item in payload.Logs)
            {
                item.UserUsosId = payload.UserUsosId;
                item.UserInstallation = installation;
                item.ApiVersion = apiVersion;
                if (item.AppVersion is null)
                {
                    //compatibility with older versions which do not send logs with app versions but 
                    //they do send app version in headers
                    //TODO: remove in the future
                    item.AppVersion = applicationVersion + "-server";
                }
            }

            await _dbContext.AppLogs.AddRangeAsync(payload.Logs);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
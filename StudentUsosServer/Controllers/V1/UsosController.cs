using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using StudentUsosServer.Database;
using StudentUsosServer.Filters;
using StudentUsosServer.Models;
using StudentUsosServer.Models.Usos;
using StudentUsosServer.Services;
using StudentUsosServer.Services.Interfaces;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentUsosServer.Controllers.V1
{
    [ApiController, Route("v{version:apiVersion}/usos"), ApiVersion(1)]
    public class UsosController : ControllerBase
    {
        MainDBContext _DbContext;
        IUsosApiService _usosApiService;
        UsosInstallationsService _usosInstallationsService;
        public UsosController(MainDBContext dbContext, IUsosApiService usosApiService, UsosInstallationsService usosInstallationsService)
        {
            _DbContext = dbContext;
            _usosApiService = usosApiService;
            _usosInstallationsService = usosInstallationsService;
        }


        /// <summary>
        /// Builds signed query and sends it to USOS API
        /// </summary>
        /// <param name="installation">USOS API installation</param>
        /// <param name="internalAccessToken">Internal access token</param>
        /// <param name="usosAccessToken">Access token</param>
        /// <param name="queryMethod">USOS API method to query e.g. services/users/user</param>
        /// <param name="queryArguments">Arguments for queried method in a JSON format
        /// <code>
        /// {
        ///     "fields": "email|first_name"
        /// }
        /// </code>
        /// </param>
        /// <returns></returns>
        [HttpGet("query"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full)]
        public async Task<ActionResult<string>> ForwardQueryToUSOS([FromHeader] string installation,
            [FromHeader] string internalAccessToken,
            [FromHeader] string usosAccessToken,
            string queryMethod,
            string queryArguments)
        {
            var deserializedArgs = JsonSerializer.Deserialize<Dictionary<string, string>>(queryArguments);
            if (deserializedArgs == null)
            {
                return BadRequest("Can't deserialize queryArguments");
            }
            var found = _DbContext.Users.Where(x => x.Installation == installation && x.InternalAccessToken == internalAccessToken)?.ToList();
            if (found == null || found.Count == 0)
            {
                return Problem("User not found", statusCode: StatusCodes.Status404NotFound);
            }
            User user = found[0];
            var result = await _usosApiService.SendRequestAsync(queryMethod, deserializedArgs, installation, usosAccessToken, user.AccessTokenSecret);
            if (result == null)
            {
                return Problem("Error requesting USOS API", statusCode: StatusCodes.Status424FailedDependency);
            }
            return result;
        }

        [HttpGet("UsosInstallations"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.InternalOnly)]
        public ActionResult<List<UsosInstallation>> GetUsosInstallations()
        {
            return _usosInstallationsService.Installations.Where(x => x.IsSupported).ToList();
        }

#if DEBUG

        class InstallationOriginal
        {
            [JsonPropertyName("base_url")]
            public string BaseUrl { get; set; }
            [JsonPropertyName("institution_name")]
            public Dictionary<string, string> InstitutionName { get; set; }
        }

        public class InstallationTarget
        {
            //enforcing PascalCase
            [JsonPropertyName("InstallationId")]
            public string InstallationId { get; set; }
            [JsonPropertyName("InstallationUrl")]
            public string InstallationUrl { get; set; }
            [JsonPropertyName("IsSupported")]
            public bool IsSupported { get; set; } = true;
            [JsonPropertyName("LocalizedName")]
            public Dictionary<string, string> LocalizedName { get; set; }
        }


        [HttpGet("TransformUsosInstallations")]
        public async Task<ActionResult<List<InstallationTarget>>> GetTransformedInstallationsListFromUsos()
        {
            var result = await _usosApiService.SendRequestAsync("services/apisrv/installations", new(),
                _usosInstallationsService.Installations[0].InstallationUrl, "", "");

            if (result is null)
            {
                return StatusCode(StatusCodes.Status424FailedDependency);
            }

            var deserialized = JsonSerializer.Deserialize<List<InstallationOriginal>>(result) ?? new();

            foreach (var item in deserialized)
            {
                item.BaseUrl = item.BaseUrl.Replace("http://", "https://");
            }

            List<InstallationTarget> transformed = new();
            int id = 1;
            foreach (var item in deserialized)
            {
                if (transformed.Any(x => x.InstallationUrl == item.BaseUrl))
                {
                    continue;
                }

                if (item.BaseUrl.EndsWith("/") == false)
                {
                    Debugger.Break();
                }

                InstallationTarget target = new()
                {
                    InstallationId = id.ToString(),
                    InstallationUrl = item.BaseUrl,
                    LocalizedName = item.InstitutionName
                };

                if (target.InstallationUrl.Contains("usosapps.put.poznan.pl"))
                {
                    target.InstallationId = "0";
                    transformed.Insert(0, target);
                }
                else
                {
                    transformed.Add(target);
                }

                id++;
            }

            return transformed;
        }
#endif
    }
}

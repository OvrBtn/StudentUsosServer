using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using StudentUsosServer.Database;
using StudentUsosServer.Filters;
using StudentUsosServer.Models;
using StudentUsosServer.Services.Interfaces;
using System.Text.Json;

namespace StudentUsosServer.Controllers.V1
{
    [ApiController, Route("v{version:apiVersion}/usos"), ApiVersion(1)]
    public class UsosController : ControllerBase
    {
        MainDBContext _DbContext;
        IUsosApiService _usosApiService;
        public UsosController(MainDBContext dbContext, IUsosApiService usosApiService)
        {
            _DbContext = dbContext;
            _usosApiService = usosApiService;
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
    }
}

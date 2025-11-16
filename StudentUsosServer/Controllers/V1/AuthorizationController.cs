using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentUsosServer.Database;
using StudentUsosServer.DTOs;
using StudentUsosServer.Filters;
using StudentUsosServer.Models;
using StudentUsosServer.Models.Usos;
using StudentUsosServer.Services.Interfaces;
using System.Text.Json;

namespace StudentUsosServer.Controllers.V1
{
    [ApiController, Route("v{version:apiVersion}/authorization"), ApiVersion(1)]
    public class AuthorizationController : ControllerBase
    {
        MainDBContext _dbContext;
        IUsosAuthorizationService _usosAuthorizationService;
        IUsosApiService _usosApiService;
        public AuthorizationController(MainDBContext dbContext,
            IUsosAuthorizationService usosAuthorizationService,
            IUsosApiService usosApiService)
        {
            _dbContext = dbContext;
            _usosAuthorizationService = usosAuthorizationService;
            _usosApiService = usosApiService;
        }

        static Dictionary<string, UsosRequestTokens> _requestTokenCache = new();
        /// <summary>
        /// Get the authorize URL to forward user to USOS sign in page 
        /// </summary>
        /// <param name="installation">Installation of USOS</param>
        /// <param name="scopes">Scopes which are to be authorized by user when signing in separated by |</param>
        /// <param name="callback">Callback method, either pass this string: <code>"oob"</code> for getting the verifier as a PIN code which user will have to manually copy 
        /// or an URL which user will be redirected after successfully signing in See USOS API docs for more details.</param>
        /// <returns></returns>
        [HttpGet("authorizeUrl"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.InternalOnly)]
        public async Task<ActionResult<RequestTokenDTO>> GetAuthorizeUrl([FromHeader] string installation, string scopes, string callback)
        {
            var requestToken = await _usosAuthorizationService.GetRequestToken(installation, scopes, callback);
            if (requestToken == null)
            {
                return Problem("Error requesting USOS API", statusCode: StatusCodes.Status424FailedDependency);
            }
            RemoveUnusedTokensFromCache();
            _requestTokenCache[requestToken.RequestToken] = requestToken;
            var authorizeArgs = new Dictionary<string, string>
            {
                { "oauth_token", requestToken.RequestToken }
            };
            var authorizeUrl = _usosApiService.GetSignedUsosUrl("services/oauth/authorize", installation, authorizeArgs);
            return Ok(new AuthorizeUrlDTO(requestToken.RequestToken, authorizeUrl));
        }

        static int removeUnusedTokensFromCacheCounter = 0;
        void RemoveUnusedTokensFromCache()
        {
            removeUnusedTokensFromCacheCounter++;
            if (removeUnusedTokensFromCacheCounter < 5)
            {
                return;
            }
            removeUnusedTokensFromCacheCounter = 0;
            foreach (var item in _requestTokenCache)
            {
                if (Math.Abs((DateTime.Now - item.Value.CreationDate).TotalHours) > 1)
                {
                    _requestTokenCache.Remove(item.Key);
                }
            }
        }

        /// <summary>
        /// Exchange the verifier from <see cref="GetAuthorizeUrl(string, string, string)"/> for user's access token and access token secret
        /// </summary>
        /// <param name="installation">USOS installation</param>
        /// <param name="requestToken">Request token from call to <see cref="GetAuthorizeUrl(string, string, string)"/></param>
        /// <param name="verifier">Verifier received after user was signed in, see <see cref="GetAccessToken(string, string, string)"/> callback param 
        /// or USOS API docs for more information</param>
        /// <returns></returns>
        [HttpGet("accessToken"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.InternalOnly)]
        public async Task<ActionResult<AccessTokenDTO>> GetAccessToken([FromHeader] string installation, string requestToken, string verifier)
        {
            if (_requestTokenCache.ContainsKey(requestToken) == false)
            {
                return BadRequest("Wrong request token");
            }
            var requestTokenFromCache = _requestTokenCache[requestToken];
            _requestTokenCache.Remove(requestToken);
            var accessTokens = await _usosAuthorizationService.GetAccessToken(installation, requestTokenFromCache.RequestToken, requestTokenFromCache.RequestTokenSecret, verifier);
            if (accessTokens == null)
            {
                return Problem("Error requesting USOS API", statusCode: StatusCodes.Status424FailedDependency);
            }
            User user = new(installation, new InternalAccessTokens(_dbContext), accessTokens.AccessTokenSecret);

            var userInfoResult = await GetUserInfo(user, installation, accessTokens);
            if (userInfoResult is null)
            {
                return Problem("Error requesting USOS API", statusCode: StatusCodes.Status424FailedDependency);
            }

            await RemoveDuplicateUserRecords(_dbContext, user);

            User? foundUser = _dbContext.Users.Where(x => x.USOSId == user.USOSId && x.StudentNumber == user.StudentNumber && x.Installation == user.Installation && x.AccessTokenSecret == user.AccessTokenSecret).FirstOrDefault();
            if (foundUser != null)
            {
                return Ok(accessTokens.ToDTO(foundUser.InternalAccessToken, foundUser.InternalAccessTokenSecret));
            }
            else
            {
                _dbContext.Add(user);
                await _dbContext.SaveChangesAsync();
                return Ok(accessTokens.ToDTO(user.InternalAccessToken, user.InternalAccessTokenSecret));
            }
        }

        async Task RemoveDuplicateUserRecords(MainDBContext dBContext, User user)
        {
            await dBContext.Users
                .Where(x => x.USOSId == user.USOSId && x.StudentNumber == user.StudentNumber && x.Installation == user.Installation && x.AccessTokenSecret != user.AccessTokenSecret)
                .ForEachAsync(foundUser => dBContext.Users.Remove(foundUser));
        }

        async Task<User?> GetUserInfo(User? user, string installation, UsosAccessTokens accessToken)
        {
            return await GetUserInfo(user, installation, accessToken.AccessToken, accessToken.AccessTokenSecret);
        }

        async Task<User?> GetUserInfo(User? user, string installation, string accessToken, string accessTokenSecret)
        {
            if (user == null)
            {
                var internalTokens = new InternalAccessTokens(_dbContext);
                user = new(installation, internalTokens.InternalAccessToken, internalTokens.InternalAccessTokenSecret, accessTokenSecret);
            }
            Dictionary<string, string> arguments = new()
            {
                {"fields", "id|first_name|last_name|student_number" }
            };
            var userInfo = await _usosApiService.SendRequestAsync("services/users/user", arguments, installation, accessToken, accessTokenSecret);
            if (userInfo == null)
            {
                return null;
            }
            User? deserialized = JsonSerializer.Deserialize<User>(userInfo);
            if (deserialized != null)
            {
                user.USOSId = deserialized.USOSId;
                user.StudentNumber = deserialized.StudentNumber;
            }
            return user;
        }

        public class CompatibilityRegisterBody
        {
            public required string UsosAccessTokenSecret { get; set; }
            public required string UsosAccessToken { get; set; }
            //public required string Installation { get; set; }
        }

        /// <summary>
        /// This endpoint exists only for compatibility reasons, it's not supposed to be used in any other way. 
        /// The use for it is to migrate users from old version of app which did not use this API.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="installation"></param>
        /// <returns></returns>
        [HttpPost("compatibilityRegister"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.InternalOnly)]
        public async Task<ActionResult<string>> CompatibilityRegister([FromBody] CompatibilityRegisterBody body, [FromHeader] string installation)
        {
            string accessToken = body.UsosAccessToken;
            string accessTokenSecret = body.UsosAccessTokenSecret;

            var foundUser = _dbContext.Users.FirstOrDefault(x => x.Installation == installation && x.AccessTokenSecret == accessTokenSecret);
            if (foundUser is not null)
            {
                foundUser.AccessTokenSecret = accessTokenSecret;
                await _dbContext.SaveChangesAsync();
                return Ok(new InternalAccessTokensDTO(foundUser.InternalAccessToken, foundUser.InternalAccessTokenSecret));
            }

            var internalAcessTokens = new InternalAccessTokens(_dbContext);
            User? user = new(installation, internalAcessTokens, accessTokenSecret);

            user = await GetUserInfo(user, installation, accessToken, accessTokenSecret);
            if (user == null)
            {
                return Problem("Error requesting USOS API, can't authenticate user", statusCode: StatusCodes.Status424FailedDependency);
            }

            _dbContext.Add(user);
            await _dbContext.SaveChangesAsync();
            return Ok(internalAcessTokens.ToDTO());
        }


        [HttpGet("isSessionActive"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full)]
        public async Task<ActionResult<bool>> IsUsersSessionActive([FromHeader] string internalAccessToken,
            [FromHeader] string usosAccessToken,
            [FromHeader] string installation,
            [FromHeader] string? applicationVersion)
        {
            User? user = _dbContext.Users.Where(x => x.InternalAccessToken == internalAccessToken).FirstOrDefault();
            if (user == null)
            {
                return BadRequest();
            }

            user.AppVersion = applicationVersion;

            var response = await _usosApiService.SendRequestFullResponseAsync("services/users/user", new(), installation, usosAccessToken, user.AccessTokenSecret);
            if (response.Response == null || response.ResponseContent == null)
            {
                Problem("Could not check, there might be an issue requesting USOS API", statusCode: StatusCodes.Status424FailedDependency);
            }
            if (response.Response!.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.ResponseContent!.Contains("Invalid access token"))
            {
                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();
                return Ok(false);
            }
            if (response.IsSuccess && response.ResponseContent!.Contains("first_name"))
            {
                await UpdateUsersLastActiveField(user);
                await _dbContext.SaveChangesAsync();
                return Ok(true);
            }
            return Problem("Could not check, there might be an issue requesting USOS API", statusCode: StatusCodes.Status424FailedDependency);
        }

        async Task UpdateUsersLastActiveField(User user)
        {
            user.LastActiveDate = DateTime.UtcNow;
            user.LastActiveDateUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }


        [HttpGet("logout"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full)]
        public async Task<ActionResult> LogOut([FromHeader] string installation, [FromHeader] string usosAccessToken, [FromHeader] string internalAccessToken)
        {
            User? user = _dbContext.Users.Where(x => x.InternalAccessToken == internalAccessToken && x.Installation == installation).FirstOrDefault();
            if (user == null)
            {
                return NotFound();
            }
            var result = await _usosApiService.SendRequestFullResponseAsync("services/oauth/revoke_token", new(), installation, usosAccessToken, user.AccessTokenSecret);
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
            return Ok();

        }

        //[HttpGet("testing"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full)]
        //public ActionResult<string> Testing([FromQuery] AuthorizeAccessFilter.AuthorizationArguments args)
        //{
        //    InternalAccessTokens a = new(_DbContext);
        //    return Ok();
        //}
    }
}

using StudentUsosServer.Services.Interfaces;
using UsosApiBrowser;

namespace StudentUsosServer.Services
{
    public class UsosApiService : IUsosApiService
    {
        UsosInstallationsService _usosInstallationsService;
        public UsosApiService(UsosInstallationsService usosInstallationsService)
        {
            _usosInstallationsService = usosInstallationsService;
        }

        static ApiConnector _apiConnector = new ApiConnector(new ApiInstallation());
        static Dictionary<string, ApiInstallation> _installations = new();

        ApiConnector GetApiConnector(string installation)
        {
            if (_installations.ContainsKey(installation))
            {
                _apiConnector.currentInstallation = _installations[installation];
            }
            else
            {
                var newInstallation = new ApiInstallation() { base_url = installation };
                _apiConnector.currentInstallation = newInstallation;
                _installations[installation] = newInstallation;
            }
            return _apiConnector;
        }

        public string? GetSignedUsosUrl(string methodName, string installation, Dictionary<string, string> arguments, string accessToken = "", string accessTokenSecret = "")
        {
            ApiConnector apiConnector = GetApiConnector(installation);
            string url;
            if (accessToken != string.Empty && accessTokenSecret != string.Empty)
            {
                var keys = _usosInstallationsService.GetUsosConsumerKeys(installation);
                if (keys is null)
                {
                    return null;
                }

                url = apiConnector.GetURL(new ApiMethod { name = methodName }, arguments,
                 keys.UsosConsumerKey, keys.UsosConsumerKeySecret, accessToken, accessTokenSecret, true);
            }
            else
            {
                //when trying to request services/oauth/authorize endpoint which doesn't need standard oauth arguments
                //it seems like using GetUrl with full parameters generates wrong url
                //generating url as below seems stable but it'd be good to investigate it further
                url = apiConnector.GetURL(new ApiMethod { name = methodName }, arguments);
            }
            return url;
        }

        public async Task<string?> SendRequestAsync(string methodName, Dictionary<string, string> arguments, string installation, string accessToken, string accessTokenSecret)
        {
            var keys = _usosInstallationsService.GetUsosConsumerKeys(installation);
            if (keys is null)
            {
                return null;
            }

            ApiConnector apiConnector = GetApiConnector(installation);
            var accessTokenUrl = apiConnector.GetURL(new ApiMethod { name = methodName }, arguments,
                keys.UsosConsumerKey, keys.UsosConsumerKeySecret, accessToken, accessTokenSecret, true);

            var response = await apiConnector.GetResponseAsync(accessTokenUrl);
            if (response == null) return null;
            return response;
        }

        public async Task<(bool IsSuccess, HttpResponseMessage? Response, string? ResponseContent)> SendRequestFullResponseAsync(string methodName, Dictionary<string, string> arguments,
            string installation, string accessToken, string accessTokenSecret)
        {
            var keys = _usosInstallationsService.GetUsosConsumerKeys(installation);
            if (keys is null)
            {
                return new(false, null, null);
            }

            ApiConnector apiConnector = GetApiConnector(installation);
            var accessTokenUrl = apiConnector.GetURL(new ApiMethod { name = methodName }, arguments,
                keys.UsosConsumerKey, keys.UsosConsumerKeySecret, accessToken, accessTokenSecret, true);

            var response = await apiConnector.GetResponseFullResponseAsync(accessTokenUrl);
            return response;
        }
    }
}

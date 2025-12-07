/*
 * 
 * 
 * Source: https://github.com/MUCI/usosapi-browser 
 * 
 * 
 * 
 */




#pragma warning disable

using System.Net;
using System.Web;

namespace UsosApiBrowser
{
    /// <summary>
    /// Description of an USOS API Installation.
    /// </summary>
    public class ApiInstallation
    {
        /// <summary>
        /// Base URL of the Installation. Should end with a slash.
        /// </summary>
        public string base_url;

        /// <summary>
        /// USOS API version string (or null if unknown).
        /// </summary>
        public string version;
    }

    /// <summary>
    /// Description of a single argument of an USOS API method.
    /// </summary>
    public class ApiMethodArgument
    {
        public string name;
        public bool is_required;
        public string default_value;
        public string description;
    }

    /// <summary>
    /// Brief description of a single USOS API method.
    /// </summary>
    public class ApiMethod
    {
        /// <summary>
        /// Fully-qualified (begins with "services/") name of the method.
        /// </summary>
        public string name;

        /// <summary>
        /// Brief (single line), plain-text description of what the method does.
        /// </summary>
        public string brief_description;

        /// <summary>
        /// Short name of the method (last element of the fully-qualified name).
        /// </summary>
        public string short_name
        {
            get
            {
                var arr = this.name.Split('/');
                return arr[arr.Length - 1];
            }
        }
    }

    /// <summary>
    /// Description of a single USOS API scope type.
    /// </summary>
    public class ApiScope
    {
        public string key;
        public string developers_description;
    }

    /// <summary>
    /// Full description of an USOS API method.
    /// </summary>
    public class ApiMethodFull : ApiMethod
    {
        /// <summary>
        /// A list of all possible arguments the method can be called with.
        /// This does not include standard OAuth signing arguments.
        /// </summary>
        public List<ApiMethodArgument> arguments = new List<ApiMethodArgument>();

        public string description;
        public string returns;
        public string ref_url;

        /// <summary>
        /// "required", "optional", "ignored"
        /// </summary>
        public string auth_options_consumer;

        /// <summary>
        /// "required", "optional", "ignored"
        /// </summary>
        public string auth_options_token;

        /// <summary>
        /// Is secure connection required to execute this method?
        /// </summary>
        public bool auth_options_ssl_required;
    }

    /// <summary>
    /// Implenentation of a simple USOS API connector. It cas generate properly signed
    /// OAuth requests and provides some USOS-API-specific helper functions.
    /// </summary>
    public class ApiConnector
    {
        /// <summary>
        /// Occurs when ApiConnector instance begins a web request.
        /// </summary>
        public event EventHandler BeginRequest;

        /// <summary>
        /// Occurs when ApiConnector instance ends previously started web request.
        /// </summary>
        public event EventHandler EndRequest;

        /// <summary>
        /// USOS API installation which the ApiConnector uses for method calls.
        /// </summary>
        public ApiInstallation currentInstallation;

        /// <summary>
        /// Create new USOS API connector.
        /// </summary>
        /// <param name="installation">
        ///     USOS API Installation which to initally use. This can be
        ///     switched later.
        /// </param>
        public ApiConnector(ApiInstallation installation)
        {
            this.currentInstallation = installation;
        }

        /// <summary>
        /// Switch connector to a different USOS API installation.
        /// </summary>
        public void SwitchInstallation(ApiInstallation apiInstallation)
        {
            this.currentInstallation = apiInstallation;
        }

        /// <summary>
        /// Read a WebResponse and return it's content as a string.
        /// </summary>
        public static string ReadResponse(WebResponse response)
        {
            if (response == null)
                return "";
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            var s = reader.ReadToEnd();
            return s;
        }

        /// <summary>
        /// Make a request for the specified URL, read the response and return
        /// it's content as a string. Will throw a WebException if response is
        /// not status 200.
        /// </summary>
        //public string GetResponse(string url)
        //{
        //    //commented because it's causing error and it's not being used anywhere
        //    //this.BeginRequest(this, null);
        //    try
        //    {
        //        WebRequest request = WebRequest.Create(url);
        //        request.Timeout = 15000;
        //        request.Proxy = null;
        //        using (var response = request.GetResponse())
        //        {
        //            return ReadResponse(response);
        //        }
        //    }
        //    catch (UriFormatException)
        //    {
        //        throw new WebException("Check your installation URL. Should start with http://");
        //    }
        //    catch (Exception e)
        //    {
        //        //Utilities.ShowError(e.Message);
        //        throw new Exception("Web request error " + e.Message);
        //    }
        //    finally
        //    {
        //        //commented because it's causing error and it's not being used anywhere
        //        //this.EndRequest(this, null);
        //    }
        //}

        static HttpClientHandler handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        static HttpClient httpClient = new(handler);

        public async Task<string> GetResponseAsync(string url)
        {
            try
            {
                using (var response = await httpClient.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();
                    var responseString = await response.Content.ReadAsStringAsync();
                    return responseString;
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<(bool IsSuccess, HttpResponseMessage? Response, string? ResponseContent)> GetResponseFullResponseAsync(string url)
        {
            try
            {
                using (var response = await httpClient.GetAsync(url))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        return new(true, response, responseString);
                    }
                    else
                    {
                        return new(false, response, string.Empty);
                    }
                }

            }
            catch (Exception ex)
            {
                return new(false, null, string.Empty);
            }
        }

        /// <summary>
        /// Construct a signed USOS API URL which points to a given method with
        /// given arguments.
        /// </summary>
        /// <param name="method">USOS API method to call.</param>
        /// <param name="args">A dictionary of method argument values for this call.</param>
        /// <param name="consumer_key">Your Consumer Key (if you want to sign this request).</param>
        /// <param name="consumer_secret">Your Consumer Secret (if you want to sign this request).</param>
        /// <param name="token">Your Token (if you want to sign this request).</param>
        /// <param name="token_secret">Your Token Secret (if you want to sign this request).</param>
        /// <returns></returns>
        public string GetURL(ApiMethod method, Dictionary<string, string>? args = null,
            string consumer_key = "", string consumer_secret = "", string token = "",
            string token_secret = "", bool use_ssl = false)
        {
            var oauth = new OAuth.OAuthBase();
            if (args == null)
                args = new Dictionary<string, string>();

            var argPairsEncoded = new List<string>();
            foreach (var pair in args)
            {
                argPairsEncoded.Add(oauth.UrlEncode(pair.Key) + "=" + oauth.UrlEncode(pair.Value));
            }
            string url = this.currentInstallation.base_url + method.name;
            if (use_ssl)
                url = url.Replace("http://", "https://");
            if (argPairsEncoded.Count > 0)
                url += "?" + string.Join("&", argPairsEncoded);

            // We have our base version of the URL, with no OAuth arguments. Now we will
            // add standard OAuth stuff and sign it given Consumer Secret (and optionally
            // also with Token Secret).

            if (consumer_key == "")
                return url;

            string timestamp = oauth.GenerateTimeStamp();
            string nonce = oauth.GenerateNonce();
            string normalized_url;
            string normalized_params;
            string signature = oauth.GenerateSignature(new System.Uri(url), consumer_key,
                consumer_secret, token, token_secret, "GET", timestamp, nonce, out normalized_url,
                out normalized_params);
            url = this.currentInstallation.base_url;
            if (use_ssl)
                url = url.Replace("http://", "https://");
            url += method.name + "?" + normalized_params + "&oauth_signature=" + HttpUtility.UrlEncode(signature);

            return url;
        }

    }
}
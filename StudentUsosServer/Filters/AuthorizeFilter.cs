using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StudentUsosServer.Database;
using StudentUsosServer.Models;
using System.Security.Cryptography;
using System.Text;
using static StudentUsosServer.Filters.AuthorizeFilterHelper;
using static StudentUsosServer.Filters.Constants;

#pragma warning disable 0162

namespace StudentUsosServer.Filters
{
    /// <summary>
    /// Purpose of this filter is to create additional layer of verification above what is already provided by USOS API.
    /// In theory requests which go straight to USOS API don't need this filter but there are requests
    /// which do not depend on USOS API, in such case this layer should provide enough security.
    /// </summary>
    public class AuthorizeAccessFilter : Attribute, IAuthorizationFilter
    {
        /// <summary>
        /// Mode of authorization, changed which arguments are required
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// Every possible argument is required
            /// </summary>
            Full,
            /// <summary>
            /// Only the internal arguments are required
            /// </summary>
            InternalOnly
        }
        Mode _mode;

        public AuthorizeAccessFilter(Mode mode)
        {
            _mode = mode;
        }

        public async void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
#if DEBUG
                //return;
#endif
                var result = await Authorize(context);
                if (result.isAuthorized == false)
                {
#if DEBUG
                    context.Result = new UnauthorizedObjectResult(new { message = result.reasonForFailure });
#else
                    context.Result = new UnauthorizedResult();
#endif
                }
            }
            catch (Exception e)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Authorization filter unexpected error \n" + e.ToString() });
            }
        }

        async Task<(bool isAuthorized, string reasonForFailure)> Authorize(AuthorizationFilterContext context)
        {
            var dbContext = context.HttpContext.RequestServices.GetService<MainDBContext>();
            if (dbContext is null)
            {
                return new(false, "Can't get database");
            }

            //depending on GET or POST one will be always null and other will contain request's body
            Dictionary<string, string>? argumentsFromUrl = new();
            string? requestBodyJson = null;

            if (context.HttpContext.Request.HasJsonContentType())
            {
                requestBodyJson = await ReadPostRequestBodyAsync(context.HttpContext.Request);
            }
            else
            {
                argumentsFromUrl = ReadGetRequestBody(context.HttpContext.Request);
            }

            Dictionary<string, string> headers = context.HttpContext.Request.Headers.ToDictionary(key => key.Key, value => value.Value.ToString());

            AuthorizationHeaders authorizationHeaders;
            try
            {
                authorizationHeaders = GetAuthorizationDataFromHeaders(headers, _mode);
            }
            catch (Exception ex)
            {
                return new(false, ex.Message);
            }

            const int timestampValiditySeconds = 30;
            if (authorizationHeaders.InternalConsumerKey != Secrets.Default.InternalConsumerKey)
            {
                return new(false, "Incorrect internal consumer key");
            }
            if (int.TryParse(authorizationHeaders.Timestamp, out int timestamp) == false)
            {
                return new(false, "Couldn't parse timestamp");
            }
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp > TimeSpan.FromSeconds(timestampValiditySeconds).TotalSeconds)
            {
                return new(false, "Incorrect timestamp, request might be too old");
            }

            var fullPath = "https://" + context.HttpContext.Request.Host + context.HttpContext.Request.Path;

            Dictionary<string, string> authorizationHeadersToHash = authorizationHeaders.ToDictionary();
            authorizationHeadersToHash.Add(InternalConsumerKeySecretKey, Secrets.Default.InternalConsumerKeySecret);

            if (_mode == Mode.Full)
            {
                User? user = dbContext.Users.FirstOrDefault(x => x.InternalAccessToken == authorizationHeaders.InternalAccessToken);
                if (user is null)
                {
                    return new(false, "User not found");
                }
                if (user.Installation != authorizationHeaders.Installation)
                {
                    return new(false, "Incorrect installation");
                }

                authorizationHeadersToHash[UsosAccessTokenKey] = authorizationHeaders.UsosAccessToken!;
                authorizationHeadersToHash[InternalAccessTokenKey] = authorizationHeaders.InternalAccessToken!;
                authorizationHeadersToHash[InternalAccessTokenSecretKey] = user.InternalAccessTokenSecret;
            }

            if (argumentsFromUrl is not null)
            {
                foreach (var item in argumentsFromUrl)
                {
                    if (authorizationHeadersToHash.ContainsKey(item.Key))
                    {
                        continue;
                    }
                    authorizationHeadersToHash.Add(item.Key, item.Value);
                }
            }

            authorizationHeadersToHash.Remove(HashKey);

            string createdHash = CreateHash(fullPath, authorizationHeadersToHash, requestBodyJson ?? string.Empty);
            if (createdHash != authorizationHeaders.Hash)
            {
                return new(false, "Incorrect hash");
            }

            return new(true, string.Empty);
        }

        string CreateHash(string fullPath, Dictionary<string, string> arguments, string body = "")
        {
            var argumentsSorted = arguments.OrderBy(x => x.Key).ToDictionary();
            string concateneted = fullPath;
            foreach (var item in argumentsSorted)
            {
                if (item.Key.ToLower() == HashKey)
                {
                    continue;
                }
                concateneted += item.Value;
            }
            concateneted += body;
            return HashValue(concateneted);
        }

        string HashValue(string value)
        {
            using HMACSHA256 sha256 = new(Encoding.UTF8.GetBytes(Secrets.Default.InternalConsumerKeySecret));
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower(System.Globalization.CultureInfo.CurrentCulture);
        }
    }
}

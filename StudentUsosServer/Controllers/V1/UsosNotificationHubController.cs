using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using StudentUsosServer.Database;
using StudentUsosServer.Filters;
using StudentUsosServer.Models.Usos;
using StudentUsosServer.Services;
using StudentUsosServer.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentUsosServer.Controllers.V1
{
    [ApiController, Route("v{version:apiVersion}/usosnotificationhub"), ApiVersion(1)]
    public class UsosNotificationHubController : ControllerBase
    {
        MainDBContext _dbContext;
        UsosInstallationsService _usosInstallationsService;
        IUsosPushNotificationsService _usosPushNotificationsService;

        public UsosNotificationHubController(MainDBContext dbContext,
            UsosInstallationsService usosInstallationsService,
            IUsosPushNotificationsService usosPushNotificationsService)
        {
            _dbContext = dbContext;
            _usosInstallationsService = usosInstallationsService;
            _usosPushNotificationsService = usosPushNotificationsService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="installationId"></param>
        /// <returns></returns>
        [HttpGet("receive/{installationId}")]
        public ActionResult<string> SubscribeToHub(string installationId)
        {
            Request.Query.TryGetValue("hub.mode", out StringValues hubMode);
            Request.Query.TryGetValue("hub.challenge", out StringValues hubChallenge);
            Request.Query.TryGetValue("hub.verify_token", out StringValues hubVerifyToken);

            if (hubMode == "subscribe" && hubVerifyToken == Secrets.Default.UsosPushNotificationsHubVerifyToken)
            {
                return Ok(hubChallenge.ToString());
            }

            return BadRequest("Invalid subscription verification request.");
        }

        public class RegisterFcmTokenBody
        {
            //public required string Installation { get; set; }
            //public required string InternalAccessToken { get; set; }
            public required string FcmToken { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("registerFcmToken"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full)]
        public ActionResult RegisterFcmToken([FromBody] RegisterFcmTokenBody body, [FromHeader] string installation, [FromHeader] string internalAccessToken)
        {
            var user = _dbContext.Users.FirstOrDefault(x => x.Installation == installation && x.InternalAccessToken == internalAccessToken);
            if (user == null)
            {
                return NotFound();
            }

            user.AddFcmToken(body.FcmToken);
            _dbContext.SaveChanges();

            return Ok();
        }

        public class ReceiveNotificationFromUsosPayload
        {
            [JsonPropertyName("event_type")]
            public required string EventType { get; set; }
            [JsonPropertyName("entry")]
            public required List<UsosGradeCreatedEntry> Entry { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("receive/{installationId}")]
        public async Task<ActionResult<string>> ReceiveNotificationFromUsos()//[FromRoute] string installationId)//, [FromBody] ReceiveNotificationFromUsosPayload payload)
        {
#pragma warning disable 0219
            bool shouldCheckSignature = true;
#pragma warning restore 0219
            //only for testing purposes in debug environment
#if DEBUG
            shouldCheckSignature = false;
#endif
            string? installationId = RouteData.Values["installationId"]?.ToString();
            if (installationId == null)
            {
                return BadRequest();
            }

            const string hubSignatureKey = "X-Hub-Signature";
            if (shouldCheckSignature && Request.Headers.ContainsKey(hubSignatureKey) == false)
            {
                return BadRequest();
            }
            string hubSignature = Request.Headers[hubSignatureKey]!;

            using var reader = new StreamReader(Request.Body);
            string rawJson = await reader.ReadToEndAsync();

            string calculatedSha1 = GenerateHmacSha1Signature(Secrets.Default.UsosConsumerKeySecret, rawJson);
            if (shouldCheckSignature && hubSignature != $"[{calculatedSha1}]" && hubSignature != calculatedSha1)
            {
                return BadRequest();
            }

            //temp logging
            string serializedHeaders = JsonSerializer.Serialize(Request.Headers.ToDictionary());
            await System.IO.File.AppendAllTextAsync("post_body_logs2.txt", $"{rawJson} ||| {serializedHeaders}\n");

            ReceiveNotificationFromUsosPayload? payload = JsonSerializer.Deserialize<ReceiveNotificationFromUsosPayload>(rawJson);
            if (payload == null)
            {
                return BadRequest();
            }

            string? installationUrl = _usosInstallationsService.GetUrl(installationId);
            if (installationUrl == null)
            {
                return BadRequest();
            }
            await _usosPushNotificationsService.SendNewGradeNotification(payload.Entry, installationUrl);

            return Ok();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="secretKey"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        static string GenerateHmacSha1Signature(string secretKey, string requestBody)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var bodyBytes = Encoding.UTF8.GetBytes(requestBody);

            using (var hmac = new HMACSHA1(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(bodyBytes);
                var hashHex = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                return $"sha1={hashHex}";
            }
        }
    }
}

using StudentUsosServer.Database;

namespace StudentUsosServer.Models
{
    public class InternalAccessTokens
    {
        MainDBContext _userDbContext;
        public InternalAccessTokens(MainDBContext userDBContext)
        {
            _userDbContext = userDBContext;
            PopulateUsedKeysCache();
            GenerateInternalAccessTokens();
        }

        public InternalAccessTokens(MainDBContext userDBContext, string internalAccessToken, string internalAccessTokenSecret)
        {
            _userDbContext = userDBContext;
            InternalAccessToken = internalAccessToken;
            InternalAccessTokenSecret = internalAccessTokenSecret;
            PopulateUsedKeysCache();
        }

        void PopulateUsedKeysCache()
        {
            if (_usedSecretKeysCache.Count != 0)
            {
                return;
            }
            var users = _userDbContext.Users;
            foreach (var user in users)
            {
                _usedPublicKeysCache.Add(user.InternalAccessToken, "");
                _usedSecretKeysCache.Add(user.InternalAccessTokenSecret, "");
            }
        }

        Dictionary<string, string> _usedPublicKeysCache = new();
        Dictionary<string, string> _usedSecretKeysCache = new();
        public string InternalAccessToken { get; set; }
        public string InternalAccessTokenSecret { get; set; }

        static Random _random = new();
        string _allowedCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string GenerateRandomToken(int length)
        {
            string result = "";
            int index = 0;
            for (int i = 0; i < length; i++)
            {
                index = _random.Next(0, _allowedCharacters.Length - 1);
                result += _allowedCharacters[index];
            }
            return result;
        }


        void GenerateInternalAccessTokens()
        {
            string generatedAccessToken, generatedAccessTokenSecret;
            do
            {
                generatedAccessToken = GenerateRandomToken(25);
            } while (_usedPublicKeysCache.ContainsKey(generatedAccessToken));
            InternalAccessToken = generatedAccessToken;
            _usedPublicKeysCache.Add(generatedAccessToken, "");

            do
            {
                generatedAccessTokenSecret = GenerateRandomToken(55);
            } while (_usedSecretKeysCache.ContainsKey(generatedAccessTokenSecret));
            InternalAccessTokenSecret = generatedAccessTokenSecret;
            _usedSecretKeysCache.Add(generatedAccessTokenSecret, "");
        }
    }
}

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PfSenseFauxApi.Net
{
    /// <summary>
    /// A key:secret pair for FauxAPI authorization.
    /// </summary>
    public class AuthorizationKey
    {
        private static readonly string ValidKeyPattern    = @"^PFFA(?!example0[12])[A-Za-z0-9]{8,36}$";
        private static readonly string ValidSecretPattern = @"^[A-Za-z0-9]{40,128}$";

        /// <summary>
        /// Alphanumeric, 12-40 chars, start with PFFA, not be PFFAexample01 or PFFAexample02.
        /// </summary>
        public static readonly Regex ValidKey = new Regex(ValidKeyPattern);

        /// <summary>
        /// Alphanumeric, 40-128 chars.
        /// </summary>
        public static readonly Regex ValidSecret = new Regex(ValidSecretPattern);

        private static readonly char[] AlphaNum = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

        private static readonly RandomNumberGenerator RNG = RandomNumberGenerator.Create();
        private readonly        SHA256                SHA = SHA256.Create();


        /// <summary>
        /// The key value.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// The secret value.
        /// </summary>
        public string Secret { get; }

        /// <summary>
        /// Constructs a new authorization key.
        /// </summary>
        public AuthorizationKey()
            : this(24, 60)
        {
        }

        /// <summary>
        /// Constructs an authorization key using the supplied key:secret pair.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="secret"></param>
        /// <exception cref="ArgumentException"></exception>
        public AuthorizationKey(string key, string secret)
        {
            Key    = key ?? throw new ArgumentNullException(nameof(key));
            Secret = secret ?? throw new ArgumentNullException(nameof(secret));

            if (string.IsNullOrEmpty(Key)) throw new ArgumentException("cannot be blank", nameof(key));
            if (string.IsNullOrEmpty(Secret)) throw new ArgumentException("cannot be blank", nameof(secret));
            if (!Regex.IsMatch(Key, ValidKeyPattern)) throw new ArgumentException("is not valid", nameof(key));
            if (!Regex.IsMatch(Secret, ValidSecretPattern)) throw new ArgumentException("is not valid", nameof(secret));
        }

        /// <summary>
        /// Constructs a new authorization key using the specified key:secret lengths.
        /// </summary>
        /// <param name="keyLen"></param>
        /// <param name="secretLen"></param>
        /// <exception cref="ArgumentException"></exception>
        public AuthorizationKey(int keyLen, int secretLen)
        {
            if (keyLen < 12 ||
                keyLen > 40) throw new ArgumentException("Key length must be between 12 and 40 characters.");
            if (secretLen < 40 ||
                secretLen > 128) throw new ArgumentException("Secret length must be between 40 and 128 characters.");

            keyLen -= 4;
            var data = new byte[keyLen + secretLen];

            while (true)
            {
                lock (RNG)
                {
                    RNG.GetBytes(data);
                }

                var sb = new StringBuilder("PFFA");
                for (var i = 0; i < keyLen; i++)
                {
                    sb.Append(AlphaNum[data[i] % AlphaNum.Length]);
                }

                var key = sb.ToString();

                sb.Clear();
                for (var i = 0; i < secretLen; i++)
                {
                    var j = i + keyLen;
                    sb.Append(AlphaNum[data[i] % AlphaNum.Length]);
                }

                var secret = sb.ToString();

                // theoretically possible, but highly unlikely.
                if (!Regex.IsMatch(key, ValidKeyPattern) ||
                    !Regex.IsMatch(secret, ValidSecretPattern)) continue;

                Key    = key;
                Secret = secret;
                return;
            }
        }

        private uint RandomUInt32()
        {
            var b = new byte[4];

            lock (RNG)
            {
                RNG.GetBytes(b);
            }

            return BitConverter.ToUInt32(b);
        }

        /// <summary>
        /// Generates an auth token for FauxAPI.
        /// </summary>
        /// <returns></returns>
        public string GenerateToken()
        {
            var dt         = DateTime.Now.ToUniversalTime().ToString("yyyyMMdd'Z'HHmmss");
            var nonce      = RandomUInt32().ToString("x8");
            var data       = Encoding.ASCII.GetBytes($"{Secret}{dt}{nonce}");
            var hashBytes  = SHA.ComputeHash(data);
            var hashString = string.Join("", hashBytes.Select(x => x.ToString("x2")));
            return $"{Key}:{dt}:{nonce}:{hashString}";
        }

        /// <summary>
        /// Determines if a generated token is valid for this authorization key, but does not verify if the timestamp would be valid.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;
            var bits = token.Split(':');
            if (bits.Length != 4) return false;
            if (bits[0] != Key) return false;

            var data       = Encoding.ASCII.GetBytes($"{Secret}{bits[1]}{bits[2]}");
            var hashBytes  = SHA.ComputeHash(data);
            var hashString = string.Join("", hashBytes.Select(x => x.ToString("x2")));
            return bits[3] == hashString;
        }
    }
}

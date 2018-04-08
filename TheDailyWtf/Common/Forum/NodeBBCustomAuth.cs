using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;

namespace TheDailyWtf.Forum
{
    public static class NodeBBCustomAuth
    {
        // Encrypted messages are [sha256 hmac of cleartext][16-byte IV][aes256-cbc-encrypted ciphertext], encoded to base64. Cleartext is always UTF-8.

        private static readonly Lazy<byte[]> KeyS = new Lazy<byte[]>(() => Convert.FromBase64String(Config.NodeBB.KeyS)); // sign
        private static readonly Lazy<byte[]> KeyE = new Lazy<byte[]>(() => Convert.FromBase64String(Config.NodeBB.KeyE)); // encrypt
        private static readonly Lazy<byte[]> KeyV = new Lazy<byte[]>(() => Convert.FromBase64String(Config.NodeBB.KeyV)); // verify
        private static readonly Lazy<byte[]> KeyD = new Lazy<byte[]>(() => Convert.FromBase64String(Config.NodeBB.KeyD)); // decrypt

        public static string GenerateAuthUrl(HttpContextBase context)
        {
            AntiForgery.GetTokens(context.Request.Cookies[AntiForgeryConfig.CookieName]?.Value, out var newCookieToken, out var formToken);
            if (newCookieToken != null)
            {
                var cookie = new HttpCookie(AntiForgeryConfig.CookieName, newCookieToken)
                {
                    HttpOnly = true
                };

                // Only override to true, never to false.
                if (AntiForgeryConfig.RequireSsl)
                {
                    cookie.Secure = true;
                }

                context.Response.Cookies.Set(cookie);
            }

            var uri = "https://" + Config.NodeBB.Host + "/api/tdwtf-front-page-auth?state=" + Uri.EscapeDataString(Encrypt(formToken));
            if (!string.Equals(Config.Wtf.Host, "thedailywtf.com", StringComparison.OrdinalIgnoreCase))
            {
                uri += "&target=" + Uri.EscapeDataString(Encrypt("https://" + Config.Wtf.Host + "/login/nodebb"));
            }
            return uri;
        }

        public struct AuthResult
        {
            [JsonIgnore]
            public string Token => "nodebb:" + this.Slug;
            [JsonProperty("s", Required = Required.Always)]
            public string Slug { get; set; }
            [JsonProperty("n", Required = Required.Always)]
            public string Name { get; set; }
            [JsonProperty("m", Required = Required.Always)]
            public bool IsAdminOrGlobalMod { get; set; }
            [JsonProperty("t", Required = Required.Always)]
            public string AntiForgeryToken { get; set; }
        }

        public static AuthResult VerifyAuth(HttpContextBase context)
        {
            var result = JsonConvert.DeserializeObject<AuthResult>(Decrypt(context.Request.QueryString["token"]));

            AntiForgery.Validate(context.Request.Cookies[AntiForgeryConfig.CookieName]?.Value, result.AntiForgeryToken);

            return result;
        }

        private static string Decrypt(string encrypted)
        {
            var data = Convert.FromBase64String(encrypted);
            if (data.Length <= 48)
            {
                throw new InvalidDataException();
            }

            var iv = new byte[16];
            Array.Copy(data, 32, iv, 0, 16);

            using (var memory = new MemoryStream())
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = KeyD.Value;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    using (var decrypt = new CryptoStream(new UndisposableStream(memory), aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        decrypt.Write(data, 48, data.Length - 48);
                    }
                }

                memory.Position = 0;
                using (var hash = new HMACSHA256(KeyV.Value))
                {
                    var toCheck = hash.ComputeHash(memory);

                    // .NET does not have a constant-time byte compare function, so we must commit a cardinal sin by writing our own crypto code.

                    int check = 0;
                    for (int i = 0; i < 32; i++)
                    {
                        check |= (toCheck[i] ^ data[i]);
                    }

                    if (check != 0)
                    {
                        throw new InvalidDataException();
                    }
                }

                return Encoding.UTF8.GetString(memory.ToArray());
            }
        }

        private static string Encrypt(string data)
        {
            var payload = Encoding.UTF8.GetBytes(data);

            using (var memory = new MemoryStream())
            {
                using (var hash = new HMACSHA256(KeyS.Value))
                {
                    memory.Write(hash.ComputeHash(payload), 0, 32);
                }

                using (var aes = Aes.Create())
                {
                    aes.Key = KeyE.Value;
                    aes.GenerateIV();
                    aes.Mode = CipherMode.CBC;
                    memory.Write(aes.IV, 0, 16);

                    using (var encryptor = new CryptoStream(memory, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        encryptor.Write(payload, 0, payload.Length);
                    }
                }

                return Convert.ToBase64String(memory.ToArray());
            }
        }

        private sealed class UndisposableStream : Stream
        {
            private Stream stream;

            public UndisposableStream(Stream stream)
            {
                this.stream = stream;
            }

            public override bool CanRead => stream.CanRead;
            public override bool CanSeek => stream.CanSeek;
            public override bool CanWrite => stream.CanWrite;
            public override long Length => stream.Length;
            public override long Position { get => stream.Position; set => stream.Position = value; }
            public override void Flush() => stream.Flush();
            public override int Read(byte[] buffer, int offset, int count) => stream.Read(buffer, offset, count);
            public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);
            public override void SetLength(long value) => stream.SetLength(value);
            public override void Write(byte[] buffer, int offset, int count) => stream.Write(buffer, offset, count);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web.Configuration;
using Inedo;
using Inedo.Diagnostics;

namespace TheDailyWtf
{
    /// <summary>
    /// Helper class for strongly-typed configuration values stored in web.config
    /// </summary>
    public static class Config
    {
        public static string RecaptchaPublicKey { get { return WebConfigurationManager.AppSettings["recaptchaPublicKey"]; } }
        public static string RecaptchaPrivateKey { get { return WebConfigurationManager.AppSettings["recaptchaPrivateKey"]; } }

        public static string GoogleClientId { get { return WebConfigurationManager.AppSettings["GoogleClientId"]; } }
        public static string GoogleSecret { get { return WebConfigurationManager.AppSettings["GoogleSecret"]; } }

        public static class Wtf
        {
            public static string Host { get { return ReadFromFile(); } }
            public static string AdsBaseDirectory { get { return ReadFromFile(); } }

            public static class Logs
            {
                public static string BaseDirectory { get { return ReadFromFile(); } }
                public static bool Enabled { get { bool b; return bool.TryParse(ReadFromFile(), out b) ? b : false; } }
                public static MessageLevel MinimumLevel { get { return (MessageLevel)InedoLib.Util.Int.ParseZ(ReadFromFile()); } }

                private static string ReadFromFile([CallerMemberName] string key = null)
                {
                    return WebConfigurationManager.AppSettings["Wtf.Logs." + key];
                }
            }

            public static class Mail
            {
                public static string Host { get { return ReadFromFile(); } }
                public static string ToAddress { get { return ReadFromFile(); } }
                public static string FromAddress { get { return ReadFromFile(); } }
                public static Dictionary<string, string> CustomEmailAddresses 
                { 
                    get 
                    {
                        return ReadFromFile()
                            .Split(';')
                            .Select(s => s.Split(new[] { "=" }, 2, StringSplitOptions.RemoveEmptyEntries))
                            .Select(parts => new { FullName = parts[0], ToAddress = parts[1] })
                            .ToDictionary(a => a.FullName, a => a.ToAddress, StringComparer.OrdinalIgnoreCase);
                    } 
                }
                
                private static string ReadFromFile([CallerMemberName] string key = null)
                {
                    return WebConfigurationManager.AppSettings["Wtf.Mail." + key];
                }
            }

            private static string ReadFromFile([CallerMemberName] string key = null)
            {
                return WebConfigurationManager.AppSettings["Wtf." + key];
            }
        }

        public static class Discourse
        {
            public static string Host { get { return ReadFromFile(); } }
            public static string Username { get { return ReadFromFile(); } }
            public static string ApiKey { get { return ReadFromFile(); } }
            public static int ApiRequestTimeout { get { return int.Parse(ReadFromFile()); } }
            public static string CommentCategory { get { return ReadFromFile(); } }
            public static string SideBarWtfCategory { get { return ReadFromFile(); } }
            
            private static string ReadFromFile([CallerMemberName] string key = null)
            {
                return WebConfigurationManager.AppSettings["Discourse." + key];
            }
        }

        public static class NodeBB
        {
            public static string Host { get { return ReadFromFile(); } }

            private static string ReadFromFile([CallerMemberName] string key = null)
            {
                return WebConfigurationManager.AppSettings["NodeBB." + key];
            }
        }
    }
}
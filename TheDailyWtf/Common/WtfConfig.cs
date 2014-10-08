using System.Runtime.CompilerServices;
using System.Web.Configuration;

namespace TheDailyWtf
{
    /// <summary>
    /// Helper class for strongly-typed configuration values stored in web.config
    /// </summary>
    public static class Config
    {
        public static class Wtf
        {
            public static string Host { get { return ReadFromFile(); } }
            public static string AdsBaseDirectory { get { return ReadFromFile(); } }

            public static class Mail
            {
                public static string Host { get { return ReadFromFile(); } }
                public static string ToAddress { get { return ReadFromFile(); } }
                public static string FromAddress { get { return ReadFromFile(); } }
                
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
    }
}
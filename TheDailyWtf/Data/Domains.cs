using Inedo.Data;

#pragma warning disable 1591

namespace TheDailyWtf.Data
{
    /// <summary>
    /// Allowed values for various discrete domains.
    /// </summary>
    public static class Domains
    {
        public abstract class PublishedStatus : DataDomain<PublishedStatus>
        {
            public static readonly string Draft = "Draft";
            public static readonly string Pending = "Pending";
            public static readonly string Published = "Published";

            private PublishedStatus() { }
        }
    }
}
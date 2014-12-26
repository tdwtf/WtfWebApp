using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Inedo;
using Inedo.Diagnostics;
using TheDailyWtf.Data;

namespace TheDailyWtf
{
    public static class AdRotator
    {
        private static ReadOnlyCollection<DimensionRoot> dimensionRoots = new List<DimensionRoot>().AsReadOnly();
        private static readonly Random rng = new Random();
        private static Dictionary<string, Ad> adsById;
        private static AdUrlRedirects adRedirects;

        public static IEnumerable<DimensionRoot> DimensionRoots { get { return dimensionRoots; } }
        public static Exception LoadException { get; private set; }

        public static void Initialize(string rootPath)
        {
            Logger.Information("Initializing the AdRotator at path \"{0}\"...", rootPath);
            try
            {
                var dirInfo = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rootPath));
                var urlInfo = new AdUrlDocument(Path.Combine(dirInfo.FullName, "urls.xml"));

                dimensionRoots = dirInfo.EnumerateDirectories()
                    .Select(d => new DimensionRoot(d, urlInfo))
                    .Where(r => r.Companies.Count > 0)
                    .ToList()
                    .AsReadOnly();

                adsById = dimensionRoots
                    .SelectMany(r => r.Companies)
                    .SelectMany(c => c.Ads)
                    .ToDictionary(a => a.UniqueId, StringComparer.OrdinalIgnoreCase);

                adRedirects = new AdUrlRedirects(adsById.Values.Select(a => a.OriginalUrl));

                Logger.Information("AdRotator initialized successfully.");
            }
            catch (Exception ex)
            {
                Logger.Error("There was an error loading the AdRotator: " + ex);
                LoadException = ex;
            }
        }

        public static Ad GetNextAd(Dimensions dimensions)
        {
            if (LoadException != null)
                return Ad.Error;

            var root = dimensionRoots.FirstOrDefault(r => r.Dimensions.Equals(dimensions));
            if (root == null)
                return Ad.Error;
            
            var company = root.GetNextCompany();
            if (company.Ads.Count == 0)
                return Ad.Error;

            return company.Ads[GetRandomIndex(company.Ads.Count)];
        }

        public static Ad GetAdById(string id)
        {
            if (LoadException != null)
                return Ad.Error;

            return adsById.GetValueOrDefault(id);
        }

        public static string GetOriginalUrlByRedirectGuid(string guid)
        {
            return adRedirects.OriginalUrlsByGuid.GetValueOrDefault(guid);
        }

        public static string GetRedirectUrlFromOriginalUrl(string originalUrl)
        {
            return "/fbuster/" + adRedirects.GuidsByOriginalUrl.GetValueOrDefault(originalUrl);
        }

        private static int GetRandomIndex(int length) 
        {
            lock(rng) 
            {
                return rng.Next(0, length);
            }
        }
    }

    public struct Dimensions : IEquatable<Dimensions>
    {
        public static readonly Dimensions Leaderboard = new Dimensions(728, 90);
        public static readonly Dimensions SideBar = new Dimensions(300, 250);

        public static Dimensions? TryParse(string s)
        {
            var parts = s.Split(new[] { "x" }, 2, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => InedoLib.Util.Int.ParseN(p))
                    .ToArray();
            if (parts[0] == null || parts[1] == null)
                 return null;

            return new Dimensions((int)parts[0], (int)parts[1]);
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Dimensions(int width, int height)
            : this()
        {
            this.Width = width;
            this.Height = height;
        }

        public bool Equals(Dimensions other)
        {
            return this.Width == other.Width && this.Height == other.Height;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Dimensions))
                return false;

            return this.Equals((Dimensions)obj);
        }

        public override int GetHashCode()
        {
            return this.Width + 19 * this.Height;
        }

        public override string ToString()
        {
            return this.Width + "x" + this.Height;
        }
    }

    public sealed class DimensionRoot : IEquatable<DimensionRoot>
    {
        private int currentCompanyIndex = -1;

        public DimensionRoot(DirectoryInfo dir, AdUrlDocument urls)
        {
            var dim = Dimensions.TryParse(dir.Name);
            if (dim == null)
                return;

            this.Dimensions = (Dimensions)dim;

            this.Companies = dir.EnumerateDirectories()
                .Select(d => new Company(d, this.Dimensions, urls))
                .Where(c => c.Ads.Count > 0)
                .ToList()
                .AsReadOnly();
        }

        public Company GetNextCompany()
        {
            if (this.Companies.Count == 0)
                return null;

            currentCompanyIndex = (currentCompanyIndex + 1) % this.Companies.Count;
            return this.Companies[currentCompanyIndex];            
        }

        public Dimensions Dimensions { get; private set; }
        public ReadOnlyCollection<Company> Companies { get; private set; }

        public bool Equals(DimensionRoot other)
        {
            if (other == null)
                return false;

            return this.Dimensions.Equals(other.Dimensions);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DimensionRoot))
                return false;

            return this.Equals((DimensionRoot)obj);
        }

        public override int GetHashCode()
        {
            return this.Dimensions.GetHashCode();
        }
    }

    public sealed class Company : IEquatable<Company>
    {
        public static readonly Company Unknown = new Company { Name = "Unknown" };

        private Company() { }

        public Company(DirectoryInfo companyDir, Dimensions dimensions, AdUrlDocument urls)
        {
            this.Directory = companyDir.FullName;
            this.Name = companyDir.Name;

            this.Ads = companyDir.EnumerateFiles().Select(f => new Ad(f, dimensions, urls.GetDefaultUrl(this.Name), urls.GetCustomAdUrls(this.Name))).ToList().AsReadOnly();
        }

        public string Directory { get; private set; }
        public string Name { get; private set; }
        public ReadOnlyCollection<Ad> Ads { get; private set; }

        public bool Equals(Company other)
        {
            if (other == null)
                return false;

            return string.Equals(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Company))
                return false;

            return this.Equals((Company)obj);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(this.Name);
        }
    }

    public sealed class Ad
    {
        private FileInfo file;

        public static readonly Ad Error = new Ad { ImageUrl = "/content/images/ad-load-error.png" };

        private Ad() { }

        public Ad(FileInfo file, Dimensions dimensions, string defaultUrl, Dictionary<string, string> customAdUrls)
        {
            this.file = file;
            this.Dimensions = dimensions;
            this.UniqueId = Guid.NewGuid().ToString("N");
            this.ImageUrl = "/fblast/" + this.UniqueId;
            
            string adUrl;
            if (customAdUrls != null && customAdUrls.TryGetValue(file.Name, out adUrl))
                this.OriginalUrl = adUrl;
            else
                this.OriginalUrl = defaultUrl ?? "#";
        }

        public string UniqueId { get; private set; }
        public string DiskPath { get { return this.file.FullName; } }
        public string FileName { get { return this.file.Name; } }
        public Dimensions Dimensions { get; private set; }
        public string ImageUrl { get; private set; }
        public string ImageUrlWithoutImpression { get { return this.ImageUrl + "?noimpression=true"; } }
        public string OriginalUrl { get; private set; }
    }

    public sealed class AdUrlDocument
    {
        private XDocument document;

        public AdUrlDocument(string urlFilePath)
        {
            using (var fs = File.OpenRead(urlFilePath))
            {
                this.document = XDocument.Load(fs);
            }
        }

        public string GetDefaultUrl(string companyName)
        {
            var companyElement = this.document.Root.Elements("company")
                .FirstOrDefault(a => (string)a.Attribute("name") == companyName);

            if (companyElement == null)
                return null;

            return (string)companyElement.Attribute("defaultUrl");
        }

        public Dictionary<string, string> GetCustomAdUrls(string companyName)
        {
            var companyElement = this.document.Root.Elements("company")
                .FirstOrDefault(a => (string)a.Attribute("name") == companyName);

            if (companyElement == null)
                return null;

            return companyElement.Elements("ad")
                .Select(el => new { FileName = (string)el.Attribute("fileName"), Url = (string)el.Attribute("url") })
                .Where(a => a.FileName != null && a.Url != null)
                .ToDictionary(a => a.FileName, a => a.Url, StringComparer.OrdinalIgnoreCase);
        }
    }

    public sealed class AdUrlRedirects
    {
        public AdUrlRedirects(IEnumerable<string> originalAdUrls)
        {
            foreach (string url in originalAdUrls)
                StoredProcs.AdRedirectUrls_AddRedirectUrl(url).Execute();

            var urls = StoredProcs.AdRedirectUrls_GetRedirectUrls().Execute().ToList();

            this.OriginalUrlsByGuid = urls
                .ToDictionary(r => r.Ad_Guid.ToString("N"), r => r.Redirect_Url, StringComparer.OrdinalIgnoreCase);

            this.GuidsByOriginalUrl = urls
                .ToDictionary(r => r.Redirect_Url, r => r.Ad_Guid.ToString("N"), StringComparer.OrdinalIgnoreCase);
        }

        public Dictionary<string, string> OriginalUrlsByGuid { get; private set; }
        public Dictionary<string, string> GuidsByOriginalUrl { get; private set; }
    }
}
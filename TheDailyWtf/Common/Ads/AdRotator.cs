using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Inedo;

namespace TheDailyWtf
{
    public static class AdRotator
    {
        private static ReadOnlyCollection<DimensionRoot> dimensionRoots = new List<DimensionRoot>().AsReadOnly();
        private static readonly Random rng = new Random();
        private static Dictionary<string, Ad> adsById;        

        public static IEnumerable<DimensionRoot> DimensionRoots { get { return dimensionRoots; } }
        public static Exception LoadException { get; private set; }

        public static void Initialize(string rootPath)
        {
            try
            {
                var dirInfo = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rootPath));
                string urlFilePath = Path.Combine(dirInfo.FullName, "urls.xml");
                using (var fs = File.OpenRead(urlFilePath))
                {
                    var urls = XDocument.Load(fs);

                    dimensionRoots = dirInfo.EnumerateDirectories()
                        .Select(d => new DimensionRoot(d, urls))
                        .Where(r => r.Companies.Count > 0)
                        .ToList()
                        .AsReadOnly();

                    adsById = dimensionRoots
                        .SelectMany(r => r.Companies)
                        .SelectMany(c => c.Ads)
                        .ToDictionary(a => a.UniqueId, StringComparer.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex)
            {
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

            return adsById[id];
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

        public DimensionRoot(DirectoryInfo dir, XDocument urls)
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
        public static readonly Company Unknown = new Company { Name = "Unknown", Url = "/ads" };

        private Company() { }

        public Company(DirectoryInfo companyDir, Dimensions dimensions, XDocument urls)
        {
            this.Directory = companyDir.FullName;
            this.Name = companyDir.Name;
            this.Ads = companyDir.EnumerateFiles().Select(f => new Ad(f, dimensions, this)).ToList().AsReadOnly();
            var companyElement = urls.Root.Elements("company")
                .FirstOrDefault(a => (string)a.Attribute("name") == companyDir.Name);
            if (companyElement != null)
                this.Url = companyElement.Attribute("url").Value;
        }

        public string Directory { get; private set; }
        public string Name { get; private set; }
        public ReadOnlyCollection<Ad> Ads { get; private set; }
        public string Url { get; private set; }

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
        public static readonly Ad Error = new Ad { ImageUrl = "/content/images/ad-load-error.png", Company = Company.Unknown };

        private Ad() { }

        public Ad(FileInfo file, Dimensions dimensions, Company company)
        {
            this.DiskPath = file.FullName;
            this.Company = company;
            this.Dimensions = dimensions;
            this.UniqueId = Guid.NewGuid().ToString("N");
            this.ImageUrl = "/ads/" + this.UniqueId;
        }

        public string UniqueId { get; private set; }
        public string DiskPath { get; private set; }
        public Company Company { get; private set; }
        public Dimensions Dimensions { get; private set; }
        public string ImageUrl { get; private set; }
    }
}
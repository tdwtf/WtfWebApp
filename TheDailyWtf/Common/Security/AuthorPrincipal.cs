using System;
using System.Security.Principal;
using TheDailyWtf.Models;

namespace TheDailyWtf.Security
{
    public sealed class AuthorPrincipal : IPrincipal
    {
        public AuthorPrincipal(AuthorPrincipalSerializeModel serialized)
        {
            this.Identity = new GenericIdentity(serialized.Name);
            this.IsAdmin = serialized.IsAdmin;
            this.DisplayName = serialized.DisplayName;
        }

        public AuthorPrincipal(AuthorModel author)
        {
            this.Identity = new GenericIdentity(author.Slug);
            this.IsAdmin = author.IsAdmin;
            this.DisplayName = author.Name;
        }

        public IIdentity Identity { get; private set; }
        public bool IsAdmin { get; private set; }
        public string DisplayName { get; private set; }

        public override string ToString()
        {
            return this.DisplayName;
        }

        public bool IsInRole(string role)
        {
            return this.IsAdmin;
        }

        public AuthorPrincipalSerializeModel ToSerializableModel()
        {
            return new AuthorPrincipalSerializeModel() { Name = this.Identity.Name, IsAdmin = this.IsAdmin, DisplayName = this.DisplayName };
        }
    }

    public sealed class AuthorPrincipalSerializeModel
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool IsAdmin { get; set; }
    }
}
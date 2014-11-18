using System;
using System.Collections.Generic;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class ContactFormViewModel : HomeIndexViewModel
    {
        public ContactFormViewModel()
        {
            this.ShowLeaderboardAd = false;
            this.ContactForm = new ContactFormModel();
        }

        public ContactFormModel ContactForm { get; set; }
        public string SelectedContact { get; set; }

        public IEnumerable<Contact> Editors
        {
            get
            {
                yield return CreateContact("Mark Bowytz", "Lead Editor");
                yield return CreateContact("Remy Porter", "Editor");
            }
        }

        public IEnumerable<Contact> Writers
        {
            get
            {
                yield return CreateContact("snoofle", "Writer");
                yield return CreateContact("Charles Robinson", "Writer");
                yield return CreateContact("Erik Gern", "Writer");
                yield return CreateContact("Bruce Johnson", "Writer");
                yield return CreateContact("Lorne Kates", "Writer");
            }
        }

        public IEnumerable<Contact> Admins
        {
            get
            {
                yield return CreateContact("Alex Papadimoulis", "Founder");
                yield return CreateContact("Tim Sylvia", "Sponsorship");
            }
        }

        private Contact CreateContact(string name, string title)
        {
            return new Contact
            {
                Name = name,
                Title = title,
                Selected = StringComparer.OrdinalIgnoreCase.Equals(name, this.SelectedContact)
            };
        }

        public sealed class Contact
        {
            public string Name { get; set; }
            public string Title { get; set; }
            public bool Selected { get; set; }
        }
    }
}
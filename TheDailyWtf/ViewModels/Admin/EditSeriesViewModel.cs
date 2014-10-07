using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class EditSeriesViewModel : WtfViewModelBase
    {
        public EditSeriesViewModel()
        {
            this.Series = new SeriesModel();
        }

        public EditSeriesViewModel(string slug)
        {
            if (slug != null)
                this.Series = SeriesModel.GetSeriesBySlug(slug);
            else
                this.Series = new SeriesModel();
        }

        public string Slug { get; set; }
        public SeriesModel Series { get; set; }
        public string Heading { get { return this.Slug != null ? string.Format("Edit Series {0}", this.Series.Title) : "Create New Series"; } }
    }
}
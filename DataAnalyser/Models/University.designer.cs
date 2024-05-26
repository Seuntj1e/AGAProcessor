using DevExpress.Xpo;
using System;

namespace DataAnalyser.Models
{
    public partial class University : XPObject
    {

        Region universityRegion;
        string name;

        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string Name
        {
            get => name;
            set => SetPropertyValue(nameof(Name), ref name, value);
        }
        [Association("Region-Universities")]
        public Region UniversityRegion
        {
            get => universityRegion;
            set => SetPropertyValue(nameof(UniversityRegion), ref universityRegion, value);
        }

        [Association("University-Entrants")]
        public XPCollection<Entrant> Entrants
        {
            get
            {
                return GetCollection<Entrant>(nameof(Entrants));
            }
        }
    }
}

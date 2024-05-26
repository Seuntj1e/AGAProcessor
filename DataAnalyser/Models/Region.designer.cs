using DevExpress.Xpo;
using System;

namespace DataAnalyser.Models
{
    public partial class Region : XPObject
    {

        string name;

        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string Name
        {
            get => name;
            set => SetPropertyValue(nameof(Name), ref name, value);
        }

        [Association("Region-Universities")]
        public XPCollection<University> Universities
        {
            get
            {
                return GetCollection<University>(nameof(Universities));
            }
        }
    }
}

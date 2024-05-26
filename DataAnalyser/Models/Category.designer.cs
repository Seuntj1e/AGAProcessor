using DevExpress.Xpo;
using System;

namespace DataAnalyser.Models
{
    public partial class Category : XPObject
    {

        string name;

        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string Name
        {
            get => name;
            set => SetPropertyValue(nameof(Name), ref name, value);
        }

    }
}

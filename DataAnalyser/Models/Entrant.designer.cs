using DevExpress.Xpo;
using System;

namespace DataAnalyser.Models
{
    public partial class Entrant : XPObject
    {

        University entrantUniversity;
        string hash;
        DateTime entryDate;

        public DateTime EntryDate
        {
            get => entryDate;
            set => SetPropertyValue(nameof(EntryDate), ref entryDate, value);
        }


        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string Hash
        {
            get => hash;
            set => SetPropertyValue(nameof(Hash), ref hash, value);
        }

        [Association("University-Entrants")]
        public University EntrantUniversity
        {
            get => entrantUniversity;
            set => SetPropertyValue(nameof(EntrantUniversity), ref entrantUniversity, value);
        }

        [Association("Entrant-Values")]
        public XPCollection<Value> Values
        {
            get
            {
                return GetCollection<Value>(nameof(Values));
            }
        }
    }
}

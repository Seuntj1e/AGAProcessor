using DevExpress.Xpo;
using System;

namespace DataAnalyser.Models
{
    public partial class Value : XPObject
    {

        int score;
        string stringValue;
        Category category;
        Entrant entrant;

        [Association("Entrant-Values")]
        public Entrant Entrant
        {
            get => entrant;
            set => SetPropertyValue(nameof(Entrant), ref entrant, value);
        }


        public Category Category
        {
            get => category;
            set => SetPropertyValue(nameof(Category), ref category, value);
        }


        public string StringValue
        {
            get => stringValue;
            set => SetPropertyValue(nameof(StringValue), ref stringValue, value);
        }
        
        public int Score
        {
            get => score;
            set => SetPropertyValue(nameof(Score), ref score, value);
        }
    }
}

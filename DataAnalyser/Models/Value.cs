using DevExpress.Xpo;

namespace DataAnalyser.Models
{
    public partial class Value
    {
        /// <summary>
        /// Insert some valuable class information here...
        /// </summary>
        /// <param name="session">Session/UnitOfWork required by XPO to get object from DB</param>
        public Value(Session session) : base(session)
        {

        }

        // Business logic to go here...
    }
}

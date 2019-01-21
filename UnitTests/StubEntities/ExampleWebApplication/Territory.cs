using System.Collections.Generic;

namespace UnitTests.StubEntities.ExampleWebApplication
{
    public class Territory
    {
        public Territory()
        {
            EmployeeTerritories = new List<EmployeeTerritory>();
        }

        public virtual string TerritoryID { get; set; }

        public virtual Region Region { get; set; }

        public virtual IList<EmployeeTerritory> EmployeeTerritories { get; set; }

        public virtual string TerritoryDescription { get; set; }
    }
}
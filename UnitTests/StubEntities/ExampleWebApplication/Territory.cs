using System.Collections.Generic;

namespace UnitTests.StubEntities.ExampleWebApplication
{
    public sealed class Territory
    {
        public Territory()
        {
            EmployeeTerritories = new List<EmployeeTerritory>();
        }

        public string TerritoryId { get; set; }

        public Region Region { get; set; }

        public IList<EmployeeTerritory> EmployeeTerritories { get; set; }

        public string TerritoryDescription { get; set; }
    }
}
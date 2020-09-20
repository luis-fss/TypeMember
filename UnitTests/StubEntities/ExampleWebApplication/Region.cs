using System.Collections.Generic;

namespace UnitTests.StubEntities.ExampleWebApplication
{
    public sealed class Region
    {
        public Region()
        {
            Territories = new List<Territory>();
        }

        public int RegionId { get; set; }

        public IList<Territory> Territories { get; set; }

        public string RegionDescription { get; set; }
    }
}
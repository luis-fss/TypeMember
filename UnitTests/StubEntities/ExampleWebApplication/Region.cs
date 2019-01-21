using System.Collections.Generic;

namespace UnitTests.StubEntities.ExampleWebApplication
{
    public class Region
    {
        public Region()
        {
            Territories = new List<Territory>();
        }

        public virtual int RegionID { get; set; }

        public virtual IList<Territory> Territories { get; set; }

        public virtual string RegionDescription { get; set; }
    }
}
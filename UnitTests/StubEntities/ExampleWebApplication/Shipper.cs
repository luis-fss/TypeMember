using System.Collections.Generic;

namespace UnitTests.StubEntities.ExampleWebApplication
{
    public class Shipper
    {
        public Shipper()
        {
            Orders = new List<Order>();
        }

        public virtual int ShipperID { get; set; }

        public virtual string CompanyName { get; set; }

        public virtual string Phone { get; set; }

        public virtual IList<Order> Orders { get; set; }
    }
}
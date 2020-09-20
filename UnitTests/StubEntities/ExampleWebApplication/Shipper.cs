using System.Collections.Generic;

namespace UnitTests.StubEntities.ExampleWebApplication
{
    public sealed class Shipper
    {
        public Shipper()
        {
            Orders = new List<Order>();
        }

        public int ShipperId { get; set; }

        public string CompanyName { get; set; }

        public string Phone { get; set; }

        public IList<Order> Orders { get; set; }
    }
}
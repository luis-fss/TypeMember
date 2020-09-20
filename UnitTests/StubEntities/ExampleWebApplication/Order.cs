using System;
using System.Collections.Generic;

namespace UnitTests.StubEntities.ExampleWebApplication
{
    public sealed class Order
    {
        public Order()
        {
            OrderDetails = new List<OrderDetail>();
        }

        public int OrderId { get; set; }

        public DateTime? OrderDate { get; set; }

        public DateTime? RequiredDate { get; set; }

        public DateTime? ShippedDate { get; set; }

        public decimal? Freight { get; set; }

        public string ShipName { get; set; }

        public string ShipAddress { get; set; }

        public string ShipCity { get; set; }

        public string ShipRegion { get; set; }

        public string ShipPostalCode { get; set; }

        public string ShipCountry { get; set; }

        public Customer Customer { get; set; }

        public Employee Employee { get; set; }

        public Shipper Shipper { get; set; }

        public IList<OrderDetail> OrderDetails { get; set; }
    }
}
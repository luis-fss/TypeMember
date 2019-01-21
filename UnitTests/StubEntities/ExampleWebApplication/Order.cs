using System;
using System.Collections.Generic;

namespace UnitTests.StubEntities.ExampleWebApplication
{
    public class Order
    {
        public Order()
        {
            OrderDetails = new List<OrderDetail>();
        }

        public virtual int OrderID { get; set; }

        public virtual DateTime? OrderDate { get; set; }

        public virtual DateTime? RequiredDate { get; set; }

        public virtual DateTime? ShippedDate { get; set; }

        public virtual decimal? Freight { get; set; }

        public virtual string ShipName { get; set; }

        public virtual string ShipAddress { get; set; }

        public virtual string ShipCity { get; set; }

        public virtual string ShipRegion { get; set; }

        public virtual string ShipPostalCode { get; set; }

        public virtual string ShipCountry { get; set; }

        public virtual Customer Customer { get; set; }

        public virtual Employee Employee { get; set; }

        public virtual Shipper Shipper { get; set; }

        public virtual IList<OrderDetail> OrderDetails { get; set; }
    }
}
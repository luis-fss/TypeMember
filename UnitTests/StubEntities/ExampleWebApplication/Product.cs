using System.Collections.Generic;

namespace UnitTests.StubEntities.ExampleWebApplication
{
    public class Product
    {
        public virtual string EntityProp { get; set; }

        public Product()
        {
            OrderDetails = new List<OrderDetail>();
        }

        public virtual int ProductID { get; set; }

        public virtual string ProductName { get; set; }

        public virtual string QuantityPerUnit { get; set; }

        public virtual decimal? UnitPrice { get; set; }

        public virtual short? UnitsInStock { get; set; }

        public virtual short? Units_On_Order { get; set; }

        public virtual short? ReorderLevel { get; set; }

        public virtual bool Discontinued { get; set; }

        public virtual Supplier Supplier { get; set; }

        public virtual Category Category { get; set; }

        public virtual IList<OrderDetail> OrderDetails { get; set; }
    }
}
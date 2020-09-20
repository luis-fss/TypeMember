using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace UnitTests.StubEntities.ExampleWebApplication
{
    public sealed class Product
    {
        public string EntityProp { get; set; }

        public Product()
        {
            OrderDetails = new List<OrderDetail>();
        }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string QuantityPerUnit { get; set; }

        public decimal? UnitPrice { get; set; }

        public short? UnitsInStock { get; set; }

        public short? Units_On_Order { get; set; }

        public short? ReorderLevel { get; set; }

        public bool Discontinued { get; set; }

        public Supplier Supplier { get; set; }

        public Category Category { get; set; }

        public IList<OrderDetail> OrderDetails { get; set; }
    }
}
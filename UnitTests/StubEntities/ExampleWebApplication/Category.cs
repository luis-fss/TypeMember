using System.Collections.Generic;

namespace UnitTests.StubEntities.ExampleWebApplication
{
    public class Category
    {
        public Category()
        {
            Products = new List<Product>();
        }

        public virtual int CategoryID { get; set; }

        public virtual string CategoryName { get; set; }

        public virtual string Description { get; set; }

        public virtual byte[] Picture { get; set; }

        public virtual IList<Product> Products { get; set; }
    }
}
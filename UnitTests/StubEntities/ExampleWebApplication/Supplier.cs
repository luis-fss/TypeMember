namespace UnitTests.StubEntities.ExampleWebApplication
{
    public class Supplier
    {
        //public Supplier()
        //{
        //    Products = new List<Product>();
        //}

        public virtual int SupplierId { get; set; }

        public virtual string CompanyName { get; set; }

        public virtual string ContactName { get; set; }

        public virtual string ContactTitle { get; set; }

        public virtual string Address { get; set; }

        public virtual string City { get; set; }

        public virtual string Region { get; set; }

        public virtual string PostalCode { get; set; }

        public virtual string Country { get; set; }

        public virtual string Phone { get; set; }

        public virtual string Fax { get; set; }

        public virtual string HomePage { get; set; }

        // Disable to make work the Example #08: Direct usage of a TEntity (without TEntity to TViewModel mappings)
        //public virtual IList<Product> Products { get; set; }
    }
}
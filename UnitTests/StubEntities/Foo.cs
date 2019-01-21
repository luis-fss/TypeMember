using System.Collections.Generic;

namespace UnitTests.StubEntities
{
    class Foo
    {
        public Foo()
        {
            Bar = new Bar();
            Bars = new List<Bar>();
            ImInAnEntity = string.Empty;
        }

        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Name { get; set; }
        public decimal? NullableDecimal { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitsInStock { get; set; }
        public string ImInAnEntity { get; set; }
        public Bar Bar { get; set; }
        public IList<Bar> Bars { set; get; }
    }
}

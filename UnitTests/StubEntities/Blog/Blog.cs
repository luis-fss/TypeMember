using System.Collections.Generic;

namespace UnitTests.StubEntities.Blog
{
    public class Blog
    {
        public string Name { get; set; }
        public Autor Admin { get; set; }
        public IList<Post> Posts { get; set; }
    }
}

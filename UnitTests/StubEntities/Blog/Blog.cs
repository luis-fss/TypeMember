using System;
using System.Collections.Generic;

namespace UnitTests.StubEntities.Blog
{
    public class Blog
    {
        public string Name { get; set; }
        public Author Admin { get; set; }
        public IList<Post> Posts { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}

using System.Collections.Generic;

namespace UnitTests.StubEntities.Blog
{
    public class Post
    {
        public Autor Autor { get; set; }
        public IList<Comment> Comments { get; set; }
    }
}
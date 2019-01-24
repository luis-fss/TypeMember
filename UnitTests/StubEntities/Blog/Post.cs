using System.Collections.Generic;

namespace UnitTests.StubEntities.Blog
{
    public class Post
    {
        public Author Author { get; set; }
        public IList<Comment> Comments { get; set; }
    }
}
namespace UnitTests.StubEntities
{
    public class Bar
    {
        public Bar()
        {
            Bee = new Bee();
            Name = string.Empty;
            ImInAnEntity = string.Empty;
        }

        public string ImInAnEntity { get; set; }
        public string Name { get; set; }
        public Bee Bee { get; set; }
    }
}
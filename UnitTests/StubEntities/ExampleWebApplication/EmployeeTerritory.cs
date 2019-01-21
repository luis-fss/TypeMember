namespace UnitTests.StubEntities.ExampleWebApplication
{
    public class EmployeeTerritory
    {
        public virtual int EmployeeID { get; set; }

        public virtual string TerritoryID { get; set; }

        public virtual Employee Employee { get; set; }

        public virtual Territory Territory { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != typeof(EmployeeTerritory))
            {
                return false;
            }

            return Equals((EmployeeTerritory)obj);
        }

        public virtual bool Equals(EmployeeTerritory other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other.EmployeeID == this.EmployeeID && Equals(other.TerritoryID, this.TerritoryID)
                                                       && Equals(other.Employee, this.Employee) && Equals(other.Territory, this.Territory);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = this.EmployeeID;
                result = (result * 397) ^ (this.TerritoryID != null ? this.TerritoryID.GetHashCode() : 0);
                result = (result * 397) ^ (this.Employee != null ? this.Employee.GetHashCode() : 0);
                result = (result * 397) ^ (this.Territory != null ? this.Territory.GetHashCode() : 0);
                return result;
            }
        }
    }
}
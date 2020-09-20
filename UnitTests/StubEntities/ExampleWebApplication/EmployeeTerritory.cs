// ReSharper disable NonReadonlyMemberInGetHashCode
namespace UnitTests.StubEntities.ExampleWebApplication
{
    public class EmployeeTerritory
    {
        public virtual int EmployeeId { get; set; }

        public virtual string TerritoryId { get; set; }

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

            return other.EmployeeId == EmployeeId && Equals(other.TerritoryId, TerritoryId)
                                                  && Equals(other.Employee, Employee) && Equals(other.Territory, Territory);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = EmployeeId;
                result = (result * 397) ^ (TerritoryId != null ? TerritoryId.GetHashCode() : 0);
                result = (result * 397) ^ (Employee != null ? Employee.GetHashCode() : 0);
                result = (result * 397) ^ (Territory != null ? Territory.GetHashCode() : 0);
                return result;
            }
        }
    }
}
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace UnitTests.StubEntities.ExampleWebApplication
{
    public class OrderDetail
    {
        public virtual int OrderId { get; set; }

        public virtual int ProductId { get; set; }

        public virtual Order Order { get; set; }

        public virtual Product Product { get; set; }

        public virtual decimal UnitPrice { get; set; }

        public virtual short Quantity { get; set; }

        public virtual float Discount { get; set; }

        public virtual bool Equals(OrderDetail other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return other.OrderId == OrderId && other.ProductId == ProductId && Equals(other.Order, Order) && Equals(other.Product, Product) && other.UnitPrice == UnitPrice && other.Quantity == Quantity && other.Discount.Equals(Discount);
        }

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
            if (obj.GetType() != typeof(OrderDetail))
            {
                return false;
            }
            return Equals((OrderDetail)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = OrderId;
                result = (result * 397) ^ ProductId;
                result = (result * 397) ^ (Order != null ? Order.GetHashCode() : 0);
                result = (result * 397) ^ (Product != null ? Product.GetHashCode() : 0);
                result = (result * 397) ^ UnitPrice.GetHashCode();
                result = (result * 397) ^ Quantity.GetHashCode();
                result = (result * 397) ^ Discount.GetHashCode();
                return result;
            }
        }
    }
}
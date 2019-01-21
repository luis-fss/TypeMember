namespace UnitTests.StubEntities.ExampleWebApplication
{
    public class OrderDetail
    {
        public virtual int OrderID { get; set; }

        public virtual int ProductID { get; set; }

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
            return other.OrderID == this.OrderID && other.ProductID == this.ProductID && Equals(other.Order, this.Order) && Equals(other.Product, this.Product) && other.UnitPrice == this.UnitPrice && other.Quantity == this.Quantity && other.Discount.Equals(this.Discount);
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
                int result = this.OrderID;
                result = (result * 397) ^ this.ProductID;
                result = (result * 397) ^ (this.Order != null ? this.Order.GetHashCode() : 0);
                result = (result * 397) ^ (this.Product != null ? this.Product.GetHashCode() : 0);
                result = (result * 397) ^ this.UnitPrice.GetHashCode();
                result = (result * 397) ^ this.Quantity.GetHashCode();
                result = (result * 397) ^ this.Discount.GetHashCode();
                return result;
            }
        }
    }
}
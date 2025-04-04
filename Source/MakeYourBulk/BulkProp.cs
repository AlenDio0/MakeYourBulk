using System;

namespace MakeYourBulk
{
    public class BulkProp
    {
        public int products;
        public string productsBuffer;

        public float workAmount;
        public float cost;

        public BulkProp()
        {
            products = 5;
            workAmount = 1f;
            cost = 1f;
        }

        public BulkProp(int products, float workAmount, float cost)
        {
            this.products = products;
            this.workAmount = workAmount;
            this.cost = cost;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BulkProp other)) return false;
            return products == other.products && workAmount == other.workAmount && cost == other.cost;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(products, workAmount, cost);
        }
    }
}

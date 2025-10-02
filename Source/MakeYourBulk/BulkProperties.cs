using System;

namespace MakeYourBulk
{
    public class BulkProperties
    {
        public int _Product;
        public string _ProductBuffer;

        public float _WorkAmount;
        public float _Cost;

        public BulkProperties()
        {
            _Product = 5;
            _WorkAmount = 1f;
            _Cost = 1f;
        }

        public BulkProperties(int product, float workAmount, float cost)
        {
            _Product = product;
            _WorkAmount = workAmount;
            _Cost = cost;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BulkProperties other))
            {
                return false;
            }

            return GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_Product, _WorkAmount, _Cost);
        }
    }
}

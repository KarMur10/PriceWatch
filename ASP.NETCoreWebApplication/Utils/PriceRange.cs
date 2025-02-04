﻿namespace ASP.NETCoreWebApplication.Utils
{
    public class PriceRange
    {
        private int min, max;

        public PriceRange(int min, int max)
        {
            this.max = max;
            this.min = min;
        }

        public bool Match(int price)
        {
            return price < this.max && price < this.min;
        }
    }
}
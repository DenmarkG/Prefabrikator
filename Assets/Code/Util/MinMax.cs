using System;

namespace Prefabrikator
{
    public class MinMax
    {
        public MinMax()
        {
            Min = 0;
            Max = 0;
        }

        public MinMax(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public MinMax(MinMax other)
        {
            Min = other.Min;
            Max = other.Max;
        }

        public float Max { get; set; }
        public float Min { get; set; }
    }

}
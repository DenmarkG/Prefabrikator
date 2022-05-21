
namespace Prefabrikator
{
    public static class MathUtils
    {
        private static int Factorial(int n)
        {
            if (n <= 1)
            {
                return 1;
            }
            else
            {
                return n * Factorial(n - 1);
            }
        }

        private static int[] FactorialTable =
        {
            Factorial(0),
            Factorial(1),
            Factorial(2),
            Factorial(3),
            Factorial(4),
            Factorial(5),
            Factorial(6),
            Factorial(7),
            Factorial(8),
            Factorial(9),
            Factorial(10),
        };

        public static int FastFactorial(int n)
        {
            if (n < FactorialTable.Length)
            {
                return FactorialTable[n];
            }
            else
            {
                return Factorial(n);
            }
        }

        public static int Bang(this int n)
        {
            return FastFactorial(n);
        }
    }
}

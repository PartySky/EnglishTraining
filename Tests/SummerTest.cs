using System;

namespace EnglishTraining
{   
    public class SummerTest
    {
        public static void run(ISummer testedClass)
        {
            string TestName = "SummerTest";

            if (testedClass.GetSum(2, 2) != 4)
            {
                Console.WriteLine("Test {0} is faled", TestName);
            }
            if (testedClass.GetSum(1, 1) != 2)
            {
                Console.WriteLine("Test {0} is faled", TestName);
            }
        }
    }
}

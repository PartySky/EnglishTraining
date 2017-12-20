using System;

namespace EnglishTraining
{   
    public class GreeterTest
    {
        public static void run(IGreeter testedClass)
        {
            string TestName = "GreeterTest";

            if (testedClass.Hello("Hello world.") != "Hello world.")
            {
				Console.WriteLine("Test {0} is failed", TestName);
            }
        }
    }
}

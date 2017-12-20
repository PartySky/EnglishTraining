using System;

namespace EnglishTraining
{
    public class TestRunner
    {
        public static void RunTests()
        {
            IGreeter greeter = new Greeter01();
			GreeterTest.run(greeter);

            IGreeter greeter02 = new Greeter02();
            GreeterTest.run(greeter02);

            ISummer summer = new Summer();
            SummerTest.run(summer);
        }
    }
}

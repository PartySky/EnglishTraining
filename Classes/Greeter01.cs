using System;

namespace EnglishTraining
{
    public class Greeter01 : IGreeter
    {
        public string Hello(string statement)
        {
            return "Hello world.";
        }
    }
}

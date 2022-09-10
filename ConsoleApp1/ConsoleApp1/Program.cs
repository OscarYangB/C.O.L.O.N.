using System;

namespace ConsoleApp1 {

    public class Program 
    {
        public static Bot bot;
        static void Main(string[] args)
        {
            APIHelper.StartClient();
            Console.WriteLine("Hello World!");
            bot = new Bot();
            bot.RunAsync().GetAwaiter();

            while (true)
            {
                string input = Console.ReadLine();
                if (input == "stop")
                {
                    bot.Save();
                    Environment.Exit(0);
                } 
                else if (input == "save")
                {
                    bot.Save();
                }
            }
        }
    }
}

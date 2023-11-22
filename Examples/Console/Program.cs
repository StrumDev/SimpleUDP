using System;

namespace Network
{
    public class Program
    {
        public static void Main()
        {
            do
            {
                Console.Clear();
                Console.WriteLine("Server [1]: Client [2]:");
            
                string key = Console.ReadLine();
                int.TryParse(key, out int value);

                if (value == 1)
                {
                    new NetServer().Start(12700);
                    return;
                }

                if (value == 2)
                {
                    Console.Write("Your name: ");
                    string name = Console.ReadLine();

                    new NetClient().Start("127.0.0.1", 12700, name);
                    return;
                }
            }
            while (true);       
        }
    }
}
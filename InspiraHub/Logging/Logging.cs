namespace InspiraHub.Logging
{
    public class Logging : ILogging
    {
        public void Log(string message, string type)
        {
            if(type == "error")
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR - " + message);
            }
            else
            {
                if(type == "warning")
                {
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("ERROR - " + message);
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                else
                {
                    Console.WriteLine(message);
                }
            }
        }
    }
}

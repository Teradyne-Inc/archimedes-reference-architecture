namespace Teradyne.Archimedes.DAS.Listener.Example
{
    internal class Program
    {
        /// <summary>
        /// Entry point of the application. Initializes and starts the DAS message listener.
        /// </summary>
        static async Task Main()
        {
            Console.WriteLine("Hello, Welcome to my DAS!");

            DASMessageListener listener = new("http://localhost:3000/tems/");

            listener.Connected += () => Console.WriteLine("Connected to DAS.");
            listener.Disconnected += () => Console.WriteLine("Disconnected.");
            listener.Error += (ex) => Console.WriteLine($"Error: {ex.Message}");
            listener.NewMessage += (cellid, message) =>
            {
                Console.WriteLine($"New message from {cellid}: {message.MessageName}");
            };

            await  listener.Start();
      
        }
    }
}

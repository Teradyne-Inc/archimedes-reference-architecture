using Teradyne.FA.DIA.DAS.Adaptive;

namespace Example
{
    /// <summary>
    /// Example program to demonstrate AdaptiveCommunication for adaptive testing.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The DAS identifier
        /// </summary>
        public string dasId = "http://127.0.0.1:3000/tems";

        /// <summary>
        /// RabbitMQ communication handler
        /// </summary>
        public AdaptiveCommunication adaptivecomm = new AdaptiveCommunication("127.0.0.1");  // The system is running on the local host. Update with the right IP Address

        /// <summary>
        /// Clears all adaptive commands except enable words
        /// </summary>
        public void ClearAdaptiveCommand()
        {
            AdaptiveCommand adaptiveCommand = new AdaptiveCommand(dasId);
            adaptiveCommand.addAction(adaptiveCommand.createClearAction());
            string rsp = adaptivecomm.SendAction(adaptiveCommand);
            Console.WriteLine("Clear command Response =" + rsp);
        }

        /// <summary>
        /// Enables a specific test step
        /// </summary>
        /// <param name="name">Name of the test to enable</param>
        public void EnableTest(string name)
        {
            AdaptiveCommand adaptiveCommand = new AdaptiveCommand(dasId);
            adaptiveCommand.addAction(adaptiveCommand.createTestStepAction(TestStepActionType.ENABLE, new List<string>() { name }));
            string rsp = adaptivecomm.SendAction(adaptiveCommand);
            Console.WriteLine($"Enable Test {name}  command Response =" + rsp);
        }

        /// <summary>
        /// Disables a specific test step
        /// </summary>
        /// <param name="name">Name of the test to disable</param>
        public void DisableTest(string name)
        {
            AdaptiveCommand adaptiveCommand = new AdaptiveCommand(dasId);
            adaptiveCommand.addAction(adaptiveCommand.createTestStepAction(TestStepActionType.DISABLE, new List<string>() { name }));
            string rsp = adaptivecomm.SendAction(adaptiveCommand);
            Console.WriteLine($"Disable Test {name}  command Response =" + rsp);
        }

        /// <summary>
        /// Enables or disables a specific enable word
        /// </summary>
        /// <param name="name">Name of the enable word</param>
        /// <param name="status">True to enable, False to disable</param>
        public void EnableWord(string name, bool status)
        {
            AdaptiveCommand adaptiveCommand = new AdaptiveCommand(dasId);
            adaptiveCommand.addAction(adaptiveCommand.createEnableWordAction(status ? EnableWordActionType.ENABLE : EnableWordActionType.DISABLE,
                new List<string>() { name }));
            string rsp = adaptivecomm.SendAction(adaptiveCommand);
            Console.WriteLine($"Enable Word {name}={status}  command Response =" + rsp);
        }

        /// <summary>
        /// Updates the limits of a specific test
        /// </summary>
        /// <param name="name">Test name</param>
        /// <param name="lowlimit">Lower limit</param>
        /// <param name="highlimit">Upper limit</param>
        public void SetNewLimit(string name, double lowlimit, double highlimit)
        {
            AdaptiveCommand adaptiveCommand = new AdaptiveCommand(dasId);
            adaptiveCommand.addAction(adaptiveCommand.createLimitsAction(TestStepActionType.UPDATE_LIMITS, new List<string>() { name },
                new Limits() { HI_LIMIT = highlimit, LO_LIMIT = lowlimit, LIMIT_NAMES = new List<string>() { name } }));
            string rsp = adaptivecomm.SendAction(adaptiveCommand);
            Console.WriteLine($"Set New Limit for Test {name} with LowLimit {lowlimit} and highlimit {highlimit} command Response =" + rsp);
        }

        /// <summary>
        /// Closes the RabbitMQ connection
        /// </summary>
        public void Close()
        {
            adaptivecomm.Close();
            Console.WriteLine("Communication Closed");
        }

        /// <summary>
        /// Displays an interactive menu and executes commands
        /// </summary>
        static void Main(string[] args)
        {
            var prog = new Program();
            bool exit = false;

            while (!exit)
            {
                Console.WriteLine("\n--- Adaptive Test Command Menu ---");
                Console.WriteLine("1. Clear Adaptive Command");
                Console.WriteLine("2. Enable Test");
                Console.WriteLine("3. Disable Test");
                Console.WriteLine("4. Enable/Disable Word");
                Console.WriteLine("5. Set New Limit");
                Console.WriteLine("6. Close and Exit");
                Console.Write("Choose an option (1-6): ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        prog.ClearAdaptiveCommand();
                        break;
                    case "2":
                        Console.Write("Enter Test Name to Enable: ");
                        string? enableTest = Console.ReadLine();
                        prog.EnableTest(enableTest!);
                        break;
                    case "3":
                        Console.Write("Enter Test Name to Disable: ");
                        string? disableTest = Console.ReadLine();
                        prog.DisableTest(disableTest!);
                        break;
                    case "4":
                        Console.Write("Enter Enable Word Name: ");
                        string? ewName = Console.ReadLine();
                        Console.Write("Enable (true/false): ");
                        bool status = bool.Parse(Console.ReadLine()!);
                        prog.EnableWord(ewName!, status);
                        break;
                    case "5":
                        Console.Write("Enter Test Name: ");
                        string? limitName = Console.ReadLine();
                        Console.Write("Enter Low Limit: ");
                        double low = double.Parse(Console.ReadLine()!);
                        Console.Write("Enter High Limit: ");
                        double high = double.Parse(Console.ReadLine()!);
                        prog.SetNewLimit(limitName!, low, high);
                        break;
                    case "6":
                        prog.Close();
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Try again.");
                        break;
                }
            }
        }
    }
}

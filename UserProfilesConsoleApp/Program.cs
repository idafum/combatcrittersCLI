using System.Net;
using System.Text;
using System.Text.Json;
using UserProfilesConsoleApp.Models;

namespace UserProfilesConsoleApp
{
    class Program
    {
        private static readonly HttpClient client;

        //Static Constructor
        static Program()
        {
            var cookieContainer = new CookieContainer();

            //This handler will establish a new connection to the server after 30 min
            //This will respect DNS changes and manage resources
            var handler = new SocketsHttpHandler
            {
                UseCookies = true,
                CookieContainer = cookieContainer,
                PooledConnectionLifetime = TimeSpan.FromMinutes(30)
            };

            client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://api.combatcritters.ca:4000")
            };
        }

        private static readonly Dictionary<string, Func<string[], Task>> commands = new Dictionary<string, Func<string[], Task>>
        {
            { "critter register", RegisterUserAsync },
        };

        static async Task Main(string[] args)
        {
            IntroductoryMessage();
            while (true)
            {
                Console.WriteLine("Enter command: ");
                string? input = Console.ReadLine()?.Trim().ToLower();

                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Invalid command. Please try again.");
                    continue;
                }
                if (input == "exit")
                {
                    Console.WriteLine("Exiting application...");
                    break;
                }

                //Split the input,
                string[] inputParts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                //Create a Command Key
                string commandKey = $"{inputParts[0]} {(inputParts.Length > 1 ? inputParts[1] : string.Empty)}".Trim();

                //Check if the command exists in the dictionary
                if (commands.TryGetValue(commandKey, out var action))
                {
                    await action(inputParts);
                }
                else
                {
                    Console.WriteLine($"Unknown command '{input}'");
                }

            }
        }

        public static void IntroductoryMessage()
        {
            Console.Clear();
            Console.WriteLine("Welcome to the CombatCritter 'User Profiles CLI'");
            Console.WriteLine("------This is developed as a .NET console Application-----");
            Console.WriteLine();
            Console.WriteLine("Type the commands below to perform User Profiles Operations");
            Console.WriteLine();
            Console.WriteLine("Register: Create a new account");
            Console.WriteLine("Command: 'critter register <username> <password>' ");
            Console.WriteLine();
            Console.WriteLine("Enter 'Exit' to close");
        }

        private static async Task RegisterUserAsync(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: critter register <username> <password>");
                return;
            }

            string username = args[2];
            string password = args[3];

            var userRegistration = new UserRegistration
            {
                username = username,
                password = password
            };

            try
            {
                Console.WriteLine($"Registering user '{username}'...");
                using StringContent jsonContent = new(
                    JsonSerializer.Serialize(userRegistration),
                    Encoding.UTF8,
                    "application/json"
                );

                using HttpResponseMessage response = await client.PostAsync("/users/auth/register", jsonContent);
                WriteRequestToConsole(response);
                var responseMessage = response.EnsureSuccessStatusCode();

                Console.WriteLine(responseMessage);
                Console.WriteLine($"User {username} was registered");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Failed to register user. Error: {e.Message}");
            }
        }

        /// <summary>
        /// this method prints the request details to the console.
        /// </summary>
        /// <param name="response"></param>
        static void WriteRequestToConsole(HttpResponseMessage response)
        {
            if (response is null)
            {
                return;
            }

            var request = response.RequestMessage;
            Console.Write($"{request?.Method} ");
            Console.Write($"{request?.RequestUri} ");
            Console.WriteLine($"HTTP/{request?.Version}");
        }

    }
}
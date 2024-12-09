using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using UserProfilesConsoleApp.Models;

namespace UserProfilesConsoleApp
{
    class Program
    {
        private static readonly HttpClient client;
        private static readonly CookieContainer cookieContainer;
        private static int? currentUserId;

        //Static Constructor
        static Program()
        {
            cookieContainer = new CookieContainer();

            //This handler will establish a new connection to the server after 30 min
            //This will respect DNS changes and manage resources
            var handler = new SocketsHttpHandler
            {
                UseCookies = true,
                CookieContainer = cookieContainer,
                PooledConnectionLifetime = TimeSpan.FromMinutes(30),
            };

            client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://api.combatcritters.ca:4000"),
            };
        }

        private static readonly Dictionary<string, Func<string[], Task>> commands = new Dictionary<
            string,
            Func<string[], Task>
        >
        {
            { "critter register", RegisterUserAsync },
            { "critter login", LoginUserAsync },
            { "critter admin", AdminFunctionsAsync }, //Admin Command
            { "critter friends", FriendFunctionsAsync },
        };

        static async Task Main(string[] args)
        {
            IntroductoryMessage();
            while (true)
            {
                Console.WriteLine();
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
                string commandKey =
                    $"{inputParts[0]} {(inputParts.Length > 1 ? inputParts[1] : string.Empty)}".Trim();

                if (inputParts.Length == 2 && commandKey == "critter help")
                {
                    DisplayCommands();
                    continue;
                }
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

        /// <summary>
        /// Documentation of how to use commands
        /// </summary>
        public static void IntroductoryMessage()
        {
            Console.Clear();
            Console.WriteLine("Welcome to the CombatCritter 'User Profiles CLI'");
            Console.WriteLine("------This is developed as a .NET console Application-----");
            Console.WriteLine();
            Console.WriteLine("Type the commands below to perform User Profiles Operations");
            Console.WriteLine();
            DisplayCommands();


        }

        private static void DisplayCommands()
        {
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine("Register: Create a new account");
            Console.WriteLine("Command: 'critter register <username> <password>' ");
            Console.WriteLine();
            Console.WriteLine("Login: Login in");
            Console.WriteLine("Command: 'critter login <username> <password>' ");
            Console.WriteLine();
            Console.WriteLine("'Admin function': Get all Users");
            Console.WriteLine("Command: 'critter admin users'");
            Console.WriteLine();
            Console.WriteLine("'Admin function: Delete a User");
            Console.WriteLine("Command: 'critter admin remove <userid>'");
            Console.WriteLine();
            Console.WriteLine("Friends: Get all Users friends");
            Console.WriteLine("Command: 'critter friends all'");
            Console.WriteLine();
            Console.WriteLine("Pending Friend Request: View Pending Friend Request");
            Console.WriteLine("Command: 'critter friends pending");
            Console.WriteLine();
            Console.WriteLine("Send Friend Request: Sends a Friend Request");
            Console.WriteLine("Command: 'critter friends add <username>'");
            Console.WriteLine();
            Console.WriteLine("Help: See all cli commandds");
            Console.WriteLine("Command: 'critter help'");
            Console.WriteLine();

            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine("Enter 'Exit' to close");
        }

        /// <summary>
        /// Send Request to Register a new user
        /// </summary>
        /// <param name="args">Register command parameters</param>
        /// <returns></returns>
        private static async Task RegisterUserAsync(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: critter register <username> <password>");
                return;
            }

            string username = args[2];
            string password = args[3];

            var userRegistration = new UserRegistration
            {
                username = username,
                password = password,
            };

            try
            {
                Console.WriteLine($"Register user '{username}' ...");
                using StringContent jsonContent =
                    new(
                        JsonSerializer.Serialize(userRegistration),
                        Encoding.UTF8,
                        "application/json"
                    );

                using HttpResponseMessage response = await client.PostAsync(
                    "/users/auth/register",
                    jsonContent
                );
                WriteRequestToConsole(response);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"User '{username}' was registered successfully.");
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(
                        $"Failed to register user. Status Code: {(int)response.StatusCode} - {response.ReasonPhrase}"
                    );
                    Console.WriteLine($"Server Response: {errorContent}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }
        }

        /// <summary>
        /// Send Request to login user
        /// </summary>
        /// <param name="args">Login Command Parameters</param>
        /// <returns></returns>
        private static async Task LoginUserAsync(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: critter login <username> <password>");
                return;
            }

            string username = args[2];
            string password = args[3];

            var loginDetails = new LoginDetails { username = username, password = password };

            try
            {
                Console.WriteLine($"Loggin in user '{username}'...");
                using StringContent jsonContent =
                    new(JsonSerializer.Serialize(loginDetails), Encoding.UTF8, "application/json");
                using HttpResponseMessage response = await client.PostAsync(
                    "/users/auth/login",
                    jsonContent
                );
                WriteRequestToConsole(response);
                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the response to extract the user ID
                    var responseData = await response.Content.ReadFromJsonAsync<User>();
                    if (responseData != null)
                    {
                        currentUserId = responseData.id;
                        Console.WriteLine(
                            $"User '{username}' logged in successfully with ID: {currentUserId}."
                        );
                    }
                    else
                    {
                        Console.WriteLine("Failed to parse login response.");
                    }
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(
                        $"Failed to log in user. Status Code: {(int)response.StatusCode} - {response.ReasonPhrase}"
                    );
                    Console.WriteLine($"Server Response: {errorContent}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }
        }

        /// <summary>
        /// Handle Admin Commands
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static async Task AdminFunctionsAsync(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine(
                    "Invalid command. Please use 'critter admin users' or 'critter admin remove <userid>'."
                );
                return;
            }

            string adminCommand = args[2];

            switch (adminCommand)
            {
                case "users":
                    await GetAllUsersAsync();
                    break;
                case "remove":
                    if (args.Length < 4)
                    {
                        Console.WriteLine(
                            "Please provide a user ID to remove. Usage: 'critter admin remove <userid>'."
                        );
                    }
                    else
                    {
                        string userIdString = args[3];
                        if (int.TryParse(userIdString, out int userId))
                        {
                            await DeleteUserAsync(userId);
                        }
                        else
                        {
                            Console.WriteLine(
                                $"Invalid user ID: '{userIdString}. Please provide a valid integer."
                            );
                        }
                    }
                    break;
                default:
                    Console.WriteLine($"Unknown admin command '{adminCommand}'.");
                    break;
            }
        }

        /// <summary>
        /// Send Request to Get all Users
        /// </summary>
        /// <returns></returns>
        private static async Task GetAllUsersAsync()
        {
            try
            {
                Console.WriteLine("Getting All Users...");

                var users = await client.GetFromJsonAsync<List<User>>("/admin/users");
                users?.ForEach(Console.WriteLine);
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }
        }

        /// <summary>
        /// Send request to remove a user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private static async Task DeleteUserAsync(int userId)
        {
            try
            {
                Console.WriteLine($"Attempting to delete user {userId}");

                using HttpResponseMessage response = await client.DeleteAsync(
                    $"/admin/users/{userId}"
                );
                WriteRequestToConsole(response);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"User {userId} removed successfully!");
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine(
                        $"Failed to log in user. Status Code: {(int)response.StatusCode} - {response.ReasonPhrase}"
                    );
                    Console.WriteLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }
        }

        /// <summary>
        /// Handle Friends Command
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static async Task FriendFunctionsAsync(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine(
                    "Invalid command. Please refer to the help section for the correct usage."
                );
                return;
            }

            string friendCommand = args[2];

            switch (friendCommand)
            {
                case "all":
                    await GetAllFriendsAsync();
                    break;
                case "pending":
                    await GetPendingFriendRequestAsync();
                    break;
                case "add":
                    if (args.Length < 4)
                    {
                        Console.WriteLine("Usage: critter friends add <username>");
                    }
                    else
                    {
                        await SendFriendRequestAsync(args[3]);
                    }
                    break;
                default:
                    Console.WriteLine($"Unknown action '{friendCommand}' for friends command.");
                    break;
            }
        }

        /// <summary>
        /// Send Request to get all Users friends
        /// </summary>
        /// <returns></returns>
        private static async Task GetAllFriendsAsync()
        {
            try
            {
                Console.WriteLine("Getting all Friends..");
                var friends = await client.GetFromJsonAsync<List<User>>(
                    $"/users/{currentUserId}/friends"
                );
                if (friends != null && friends.Any())
                {
                    foreach (var friend in friends)
                    {
                        Console.WriteLine($"ID: {friend.id}, Username: {friend.username}");
                    }
                }
                else
                {
                    Console.WriteLine("No friends found.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }
        }

        /// <summary>
        /// Send Request to get Pending friend request
        /// </summary>
        /// <returns></returns>
        private static async Task GetPendingFriendRequestAsync()
        {
            if (currentUserId == null)
            {
                Console.WriteLine("User is not logged in.");
                return;
            }
            try
            {
                Console.WriteLine("Retrieving pending friend requests...");
                var pendingRequests = await client.GetFromJsonAsync<List<User>>(
                    $"/users/{currentUserId}/friends/pending"
                );
                if (pendingRequests != null && pendingRequests.Any())
                {
                    foreach (var request in pendingRequests)
                    {
                        Console.WriteLine($"User ID: {request.id}, UserName: {request.username}");
                    }
                }
                else
                {
                    Console.WriteLine("No pending friend requests.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }
        }

        /// <summary>
        /// Send Request to accept a friend Request
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private static async Task SendFriendRequestAsync(string username)
        {
            if (currentUserId == null)
            {
                Console.WriteLine("User is not logged in.");
                return;
            }

            var payload = new { username };
            try
            {
                Console.WriteLine($"Sending friend request to username: {username}...");

                using StringContent jsonContent =
                    new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                using HttpResponseMessage response = await client.PostAsync(
                    $"/users/{currentUserId}/friends",
                    jsonContent
                );
                WriteRequestToConsole(response);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Request sent successfully.");
                }
                else
                {
                    Console.WriteLine(
                        $"Failed to send friend request. Status Code: {(int)response.StatusCode} - {response.ReasonPhrase}"
                    );
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
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

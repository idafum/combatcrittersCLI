using System.Net;

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

        static async Task Main(string[] args)
        {

        }
    }
}
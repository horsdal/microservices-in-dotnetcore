using static System.Console;

namespace ApiGatewayMock
{
  using System;
  using System.Linq;
  using System.Net;
  using System.Net.Http;
  using System.Net.Http.Headers;
  using System.Threading.Tasks;
  using IdentityModel.Client;
  using Newtonsoft.Json;

  public interface IHttpClientFactory
  {
    Task<HttpClient> Create(Uri uri, string scope);
  }

  public class HttpClientFactory : IHttpClientFactory
  {
    private readonly TokenClient tokenClient;

    public HttpClientFactory()
    {
      this.tokenClient = new TokenClient(
        "http://localhost:5001/connect/token",
        "api_gateway",
        "secret");
    }

    public async Task<HttpClient> Create(Uri uri, string scope)
    {
      var response = await this.tokenClient.RequestClientCredentialsAsync(scope).ConfigureAwait(false);
      var client = new HttpClient() {BaseAddress = uri};
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", response.AccessToken);
      return client;
    }
  }


  public class Program
  {
    private LoyaltyProgramClient client;

    public static void Main(string[] arg) => new Program().Main();

    public void Main()
    {
      this.client = new LoyaltyProgramClient("localhost:5000", new HttpClientFactory());
      WriteLine("Welcome to the API Gateway Mock.");

      var cont = true;
      while (cont)
      {
        WriteLine();
        WriteLine();
        WriteLine("********************");
        WriteLine("Choose one of:");
        WriteLine("q <userid> - to query the Loyalty Program Microservice for a user with id <userid>.");
        WriteLine("r <userid> - to register a user with id <userid> with the Loyalty Program Microservice.");
        WriteLine("u <userid> <interests> - to update a user with new comman separated interests");
        WriteLine("exit - to exit");
        WriteLine("********************");
        var cmd = ReadLine();
        cont = ProcessCommand(cmd);
      }
    }

    private bool ProcessCommand(string cmd)
    {
      if ("exit".Equals(cmd))
        return false;
      if (cmd.StartsWith("q"))
        ProcessUserQuery(cmd);
      else if (cmd.StartsWith("r"))
        ProcessUserRegistration(cmd);
      else if (cmd.StartsWith("u"))
        ProcessUpdateUser(cmd);
      else
        WriteLine("Did not understand command :(");
      return true;
    }

    private void ProcessUserQuery(string cmd)
    {
      int userId;
      if (!int.TryParse(cmd.Substring(1), out userId))
        WriteLine("Please specify user id as an int");
      else
      {
        var response = this.client.QueryUser(userId).Result;
        PrettyPrintResponse(response);
      }
    }

    private void ProcessUserRegistration(string cmd)
    {
      var newUser = new LoyaltyProgramUser {Name = cmd.Substring(1).Trim()};
      var response = this.client.RegisterUser(newUser).Result;
      PrettyPrintResponse(response);
    }

    private static async void PrettyPrintResponse(HttpResponseMessage response)
    {
      WriteLine("Status code: " + (response?.StatusCode.ToString() ?? "command failed"));
      WriteLine("Headers: " + (response?.Headers.Aggregate("", (acc, h) => acc + "\n\t" + h.Key + ": " + h.Value) ?? ""));
      WriteLine("Body: " + await (response?.Content.ReadAsStringAsync() ?? Task.FromResult("")).ConfigureAwait(false));
    }

    private async void ProcessUpdateUser(string cmd)
    {
      int userId;
      if (!int.TryParse(cmd.Split(' ').Skip(1).First(), out userId))
        WriteLine("Plaese speciffy user id as an int");
      else
      {
        var response = this.client.QueryUser(userId).Result;
        if (response.StatusCode == HttpStatusCode.OK)
        {
          var user = JsonConvert.DeserializeObject<LoyaltyProgramUser>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
          var newInterests = cmd.Substring(cmd.IndexOf(' ', 2)).Split(',').Select(i => i.Trim());
          user.Settings =
            new LoyaltyProgramSettings
            {
              Interests =
                user.Settings?.Interests.Union(newInterests).ToArray()
                ?? newInterests.ToArray()
            };
          PrettyPrintResponse(this.client.UpdateUser(user).Result);
        }
      }
    }
  }
}

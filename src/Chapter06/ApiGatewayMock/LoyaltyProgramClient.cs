using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;

namespace ApiGatewayMock
{
  public class LoyaltyProgramClient
  {
    private static readonly Policy exponentialRetryPolicy =
      Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
                           3,
                           attempt => TimeSpan.FromMilliseconds(100*Math.Pow(2, attempt)),
                           (_, __) => Console.WriteLine("retrying..." + _)
        );

    private static readonly Policy circuitBreaker =
      Policy
        .Handle<Exception>()
        .CircuitBreakerAsync(5, TimeSpan.FromMinutes(5));

    private readonly string hostName;

    public LoyaltyProgramClient(string loyalProgramMicroserviceHostName)
    {
      this.hostName = loyalProgramMicroserviceHostName;
    }

    public async Task<HttpResponseMessage> QueryUser(int userId)
    {
      return await circuitBreaker.ExecuteAsync(() => DoUserQuery(userId));
    }

    private async Task<HttpResponseMessage> DoUserQuery(int userId)
    {
      var userResource = $"/users/{userId}";
      using (var httpClient = new HttpClient())
      {
        httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        httpClient.BaseAddress = new Uri($"http://{this.hostName}");
        var response = await httpClient.GetAsync(userResource);
        ThrowOnTransientFailure(response);
        return response;
      }
    }

    private static void ThrowOnTransientFailure(HttpResponseMessage response)
    {
      if (((int) response.StatusCode) < 200 || ((int) response.StatusCode) > 499)
        throw new Exception(response.StatusCode.ToString());
    }

    public async Task<HttpResponseMessage> RegisterUser(LoyaltyProgramUser newUser)
    {
      return await exponentialRetryPolicy.ExecuteAsync(() => DoRegisterUser(newUser));
    }

    private async Task<HttpResponseMessage> DoRegisterUser(LoyaltyProgramUser newUser)
    {
      using (var httpClient = new HttpClient())
      {
        httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        httpClient.BaseAddress = new Uri($"http://{this.hostName}");
        var response =
          await
          httpClient.PostAsync("/users/",
                               new StringContent(JsonConvert.SerializeObject(newUser), Encoding.UTF8, "application/json"));
        ThrowOnTransientFailure(response);
        return response;
      }
    }

    public async Task<HttpResponseMessage> UpdateUser(LoyaltyProgramUser user)
    {
      return await exponentialRetryPolicy.ExecuteAsync(() => DoUpdateUser(user));
    }

    private async Task<HttpResponseMessage> DoUpdateUser(LoyaltyProgramUser user)
    {
      using (var httpClient = new HttpClient())
      {
        httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        httpClient.BaseAddress = new Uri($"http://{this.hostName}");
        var response =
          await
          httpClient.PutAsync($"/users/{user.Id}",
                              new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json"));
        ThrowOnTransientFailure(response);
        return response;
      }
    }
  }

  public class LoyaltyProgramUser
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public int LoyaltyPoints { get; set; }
    public LoyaltyProgramSettings Settings { get; set; }
  }

  public class LoyaltyProgramSettings
  {
    public string[] Interests { get; set; }
  }
}

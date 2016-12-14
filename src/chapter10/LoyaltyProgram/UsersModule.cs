namespace LoyaltyProgram
{
  using System.Linq;
  using System.Net;
  using System.Collections.Generic;
  using Nancy;
  using Nancy.ModelBinding;

  using HttpStatusCode = Nancy.HttpStatusCode;

  public class UsersModule : NancyModule
  {
    private static IDictionary<int, LoyaltyProgramUser> registeredUsers =
      new Dictionary<int, LoyaltyProgramUser>();

    public UsersModule() : base("/users")
    {
      Get("/", _ => registeredUsers.Values);

      Get("/{userId:int}", parameters =>
      {
        int userId = parameters.userId;
        if (registeredUsers.ContainsKey(userId))
          return registeredUsers[userId];
        else
          return HttpStatusCode.NotFound;
      });

      Post("/", _ =>
      {
        var newUser = this.Bind<LoyaltyProgramUser>();
        this.AddRegisteredUser(newUser);
        return this.CreatedResponse(newUser);
      });

      Put("/{userId:int}", parameters =>
      {
        int loggedInUserId;
        int.TryParse(
          this.Context
            .CurrentUser
            .Claims.FirstOrDefault(c => c.Type.StartsWith("id"))
            ?.Value.Split(':').Last() ?? "",
          out loggedInUserId);
        int userId = parameters.userId;
        if (loggedInUserId != userId)
          return HttpStatusCode.Forbidden;
        var updatedUser = this.Bind<LoyaltyProgramUser>();
        registeredUsers[userId] = updatedUser;
        return updatedUser;
      });
    }

    private dynamic CreatedResponse(LoyaltyProgramUser newUser)
    {
      return
          this.Negotiate
              .WithStatusCode(HttpStatusCode.Created)
              .WithHeader("Location", this.Request.Url.SiteBase + "/users/" + newUser.Id)
              .WithModel(newUser);
    }

    private void AddRegisteredUser(LoyaltyProgramUser newUser)
    {
      var userId = registeredUsers.Count;
      newUser.Id = userId;
      registeredUsers[userId] = newUser;
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

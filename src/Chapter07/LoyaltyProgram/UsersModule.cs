using System.Collections.Generic;
using Nancy;
using Nancy.ModelBinding;

namespace LoyaltyProgram
{
  public class UsersModule : NancyModule
  {
    private static IDictionary<int, LoyaltyProgramUser> registerUsers =
      new Dictionary<int, LoyaltyProgramUser>();

    public UsersModule() : base("/users")
    {
      Get("/", _ => registerUsers.Values);

      Get("/{userId:int}", parameters =>
      {
        int userId = parameters.userId;
        if (registerUsers.ContainsKey(userId))
          return registerUsers[userId];
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
        int userId = parameters.userId;
        var updatedUser = this.Bind<LoyaltyProgramUser>();
        registerUsers[userId] = updatedUser;
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
      var userId = registerUsers.Count;
      newUser.Id = userId;
      registerUsers[userId] = newUser;
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

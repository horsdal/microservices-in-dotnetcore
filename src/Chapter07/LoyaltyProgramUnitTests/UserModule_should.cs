namespace LoyaltyProgramUnitTests
{
  using System;
  using System.Threading.Tasks;
  using LoyaltyProgram;
  using Nancy;
  using Nancy.Testing;
  using Xunit;

  public class UserModule_should
  {
    private Browser sut;

    public UserModule_should()
    {
      this.sut = new Browser(
        new Bootstrapper(),
        defaultsTo => defaultsTo.Accept("application/json"));
    }

    [Fact]
    public async Task respond_not_when_queried_for_unregistered_user()
    {
      var actual = await sut.Get("/users/1000");
      Assert.Equal(HttpStatusCode.NotFound, actual.StatusCode);
    }

    [Fact]
    public async Task allow_to_register_new_user()
    {
      var expected = new LoyaltyProgramUser() {Name = "Chr"};
      var registrationResponse = await sut.Post("/users", with => with.JsonBody(expected));
      var newUser = registrationResponse.Body.DeserializeJson<LoyaltyProgramUser>();

      var actual = await sut.Get($"/users/{newUser.Id}");

      Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
      Assert.Equal(expected.Name, actual.Body.DeserializeJson<LoyaltyProgramUser>().Name);
    }

    [Fact]
    public async Task allow_modififying_users()
    {
      var expected = "jane";
      var user = new LoyaltyProgramUser() {Name = "Chr"};
      var registrationResponse = await sut.Post("/users", with => with.JsonBody(user));
      var newUser = registrationResponse.Body.DeserializeJson<LoyaltyProgramUser>();

      newUser.Name = expected;
      var actual = await sut.Put($"/users/{newUser.Id}", with => with.JsonBody(newUser));

      Assert.Equal(expected, actual.Body.DeserializeJson<LoyaltyProgramUser>().Name);
    }
  }
}

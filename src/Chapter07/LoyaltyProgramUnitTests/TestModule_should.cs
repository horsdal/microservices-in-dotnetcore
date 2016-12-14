namespace LoyaltyProgramUnitTests
{
  using System.Threading.Tasks;
  using Nancy;
  using Nancy.Testing;
  using Xunit;

  public class TestModule_should
  {
    public class TestModule : NancyModule
    {
      public TestModule()
      {
        Get("/", _ => 200);
      }
    }

    [Fact]
    public async Task respond_ok_to_request_to_root()
    {
      var sut = new Browser(with => with.Module<TestModule>());
      var actual = await sut.Get("/");

      Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
    }
  }
}

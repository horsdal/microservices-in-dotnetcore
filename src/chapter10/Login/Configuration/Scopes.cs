namespace Login.Configuration
{
  using System.Collections.Generic;
  using IdentityServer4.Models;

  public class Scopes
  {
    public static IEnumerable<Scope> Get() =>
      new[]
      {
        // standard OpenID Connect scopes
        StandardScopes.OpenId,
        StandardScopes.ProfileAlwaysInclude,
        StandardScopes.EmailAlwaysInclude,
        new Scope
        {
          Name = "loyalty_program_write",
          DisplayName = "Loyalty Program write access",
          Type = ScopeType.Resource,
        }
      };
  }
}
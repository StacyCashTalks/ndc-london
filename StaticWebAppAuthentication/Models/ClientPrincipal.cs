namespace StaticWebAppAuthentication.Models;

public class ClientPrincipal
{
    public string IdentityProvider { get; set; }
    public string UserId { get; set; }
    public string UserDetails { get; set; }
    public IEnumerable<string> UserRoles { get; set; }
    public IEnumerable<SwaClaims> Claims { get; set; }
    public string AccessToken { get; set; }
}

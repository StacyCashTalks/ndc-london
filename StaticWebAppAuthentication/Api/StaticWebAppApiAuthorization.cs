using System.Text;
using System.Text.Json;

namespace StaticWebAppAuthentication.Api;

public static class StaticWebAppApiAuthorization
{
    public static ClientPrincipal ParseHttpHeaderForClientPrinciple(List<KeyValuePair<string, IEnumerable<string>>> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        if (headers.Any(x => x.Key == "x-ms-client-principal"))
        {
            var data = headers.FirstOrDefault(x => x.Key == "x-ms-client-principal").Value;
            var decoded = Convert.FromBase64String(data.FirstOrDefault());
            var json = Encoding.UTF8.GetString(decoded);
            var principal = JsonSerializer.Deserialize<ClientPrincipal>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return principal ?? new ClientPrincipal();
        }
        else
        { 
            return new ClientPrincipal();
        }
    }
}

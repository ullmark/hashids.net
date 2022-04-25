using HashidsNet;
using Microsoft.Extensions.DependencyInjection;

namespace Hashids.net.Microsoft.Extensions.DependencyInjection;

public static class ExtensionMethods
{
    public const string DEFAULT_ALPHABET = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
    public const string DEFAULT_SEPS = "cfhistuCFHISTU";
    
    public static void AddHashIdsNet(this IServiceCollection services,
        string salt = "",
        int minHashLength = 0,
        string alphabet = DEFAULT_ALPHABET,
        string seps = DEFAULT_SEPS)
    {
        services.AddScoped<IHashids, HashidsNet.Hashids>(_ => new HashidsNet.Hashids(salt, minHashLength, alphabet, seps));
    }
}
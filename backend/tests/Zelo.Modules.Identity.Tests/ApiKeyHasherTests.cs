using Xunit;
using Zelo.Modules.Identity.Infrastructure;

namespace Zelo.Modules.Identity.Tests;

public class ApiKeyHasherTests
{
    [Fact]
    public void GenerateRawKey_ComecaComPrefixoZelo_EGeraValoresDiferentes()
    {
        var a = ApiKeyHasher.GenerateRawKey();
        var b = ApiKeyHasher.GenerateRawKey();

        Assert.StartsWith("zelo_", a);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Hash_EDeterministico_EDiferenteParaChavesDiferentes()
    {
        var key = ApiKeyHasher.GenerateRawKey();

        Assert.Equal(ApiKeyHasher.Hash(key), ApiKeyHasher.Hash(key));
        Assert.NotEqual(ApiKeyHasher.Hash(key), ApiKeyHasher.Hash(ApiKeyHasher.GenerateRawKey()));
    }

    [Fact]
    public void DisplayPrefix_NuncaExpoeAChaveCompleta()
    {
        var key = ApiKeyHasher.GenerateRawKey();

        var prefix = ApiKeyHasher.DisplayPrefix(key);

        Assert.True(prefix.Length < key.Length);
        Assert.StartsWith(prefix, key);
    }
}

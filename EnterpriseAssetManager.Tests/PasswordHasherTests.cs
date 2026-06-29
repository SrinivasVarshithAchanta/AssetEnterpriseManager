using EnterpriseAssetManager.Services;
using Xunit;

namespace EnterpriseAssetManager.Tests;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Verify_ReturnsTrue_ForCorrectPassword()
    {
        string hash = _hasher.Hash("Secret@123");
        Assert.True(_hasher.Verify("Secret@123", hash));
    }

    [Fact]
    public void Verify_ReturnsFalse_ForWrongPassword()
    {
        string hash = _hasher.Hash("Secret@123");
        Assert.False(_hasher.Verify("WrongPassword", hash));
    }

    [Fact]
    public void Hash_IsNotStoredAsPlainText()
    {
        string hash = _hasher.Hash("Secret@123");
        Assert.DoesNotContain("Secret@123", hash);
    }

    [Fact]
    public void Hash_UsesRandomSalt_SoHashesDiffer()
    {
        string a = _hasher.Hash("Secret@123");
        string b = _hasher.Hash("Secret@123");
        Assert.NotEqual(a, b);
    }
}

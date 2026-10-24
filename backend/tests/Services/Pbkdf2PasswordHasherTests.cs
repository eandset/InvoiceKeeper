using InvoiceKeeper.Infrastructure.Security;

namespace InvoiceKeeper.Tests.Services;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Verify_AcceptsCorrectPassword()
    {
        var hash = _hasher.Hash("S3cret-password!");

        Assert.True(_hasher.Verify("S3cret-password!", hash));
    }

    [Fact]
    public void Verify_RejectsWrongPassword()
    {
        var hash = _hasher.Hash("S3cret-password!");

        Assert.False(_hasher.Verify("s3cret-password!", hash));
    }

    [Fact]
    public void Hash_UsesRandomSalt()
    {
        var first = _hasher.Hash("same-password");
        var second = _hasher.Hash("same-password");

        Assert.NotEqual(first, second);
    }

    [Theory]
    [InlineData("")]
    [InlineData("plain-text-password")]
    [InlineData("pbkdf2-sha256$abc$def$ghi")]
    public void Verify_RejectsMalformedHash(string hash)
    {
        Assert.False(_hasher.Verify("password", hash));
    }
}


using StockMind.Domain.Common;

namespace StockMind.Domain.Entities;

public sealed class RefreshToken : BaseEntity
{
    public string Token { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public Guid UserId { get; private set; }

    // EF Core constructor
    private RefreshToken()
    {
        Token = string.Empty;
    }

    private RefreshToken(string token, DateTime expiryDate, Guid userId)
    {
        Token = token;
        ExpiryDate = expiryDate;
        UserId = userId;
        IsRevoked = false;
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public static RefreshToken Create(string token, DateTime expiryDate, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));

        if (expiryDate <= DateTime.UtcNow)
            throw new ArgumentException("Expiry date must be in the future", nameof(expiryDate));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        return new RefreshToken(token, expiryDate, userId);
    }

    public void Revoke(string? replacedByToken = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        ReplacedByToken = replacedByToken;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
    public bool IsActive => !IsRevoked && !IsExpired;
}

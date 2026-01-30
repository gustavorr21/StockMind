using StockMind.Domain.Common;

namespace StockMind.Domain.Entities;

public sealed class User : BaseEntity
{
    public string Email { get; private set; }
    public string FullName { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    // Navigation property
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    // EF Core constructor
    private User()
    {
        Email = string.Empty;
        FullName = string.Empty;
    }

    private User(string email, string fullName)
    {
        Email = email;
        FullName = fullName;
        IsActive = true;
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public static User Create(string email, string fullName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name cannot be empty", nameof(fullName));

        return new User(email, fullName);
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

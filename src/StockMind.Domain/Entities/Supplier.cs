using StockMind.Domain.Common;
using StockMind.Domain.Enums;
using StockMind.Domain.ValueObjects;

namespace StockMind.Domain.Entities;

public sealed class Supplier : BaseEntity
{
    public string CompanyName { get; private set; }
    public string TradeName { get; private set; }
    public string Document { get; private set; }
    public Email Email { get; private set; }
    public Phone Phone { get; private set; }
    public Address Address { get; private set; }
    public SupplierStatus Status { get; private set; }
    public string? Notes { get; private set; }

    private Supplier(string companyName, string tradeName, string document, Email email, Phone phone, Address address)
    {
        CompanyName = companyName;
        TradeName = tradeName;
        Document = document;
        Email = email;
        Phone = phone;
        Address = address;
        Status = SupplierStatus.Active;
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public static Supplier Create(string companyName, string tradeName, string document, Email email, Phone phone, Address address)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name cannot be empty", nameof(companyName));

        if (string.IsNullOrWhiteSpace(tradeName))
            throw new ArgumentException("Trade name cannot be empty", nameof(tradeName));

        if (string.IsNullOrWhiteSpace(document))
            throw new ArgumentException("Document cannot be empty", nameof(document));

        if (email == null)
            throw new ArgumentNullException(nameof(email));

        if (phone == null)
            throw new ArgumentNullException(nameof(phone));

        if (address == null)
            throw new ArgumentNullException(nameof(address));

        return new Supplier(companyName.Trim(), tradeName.Trim(), document.Trim(), email, phone, address);
    }

    public void UpdateContactInfo(Email email, Phone phone)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Phone = phone ?? throw new ArgumentNullException(nameof(phone));
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAddress(Address address)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(SupplierStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddNotes(string notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = SupplierStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = SupplierStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Block()
    {
        Status = SupplierStatus.Blocked;
        UpdatedAt = DateTime.UtcNow;
    }
}

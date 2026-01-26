# Role Management with UserRole Enum

## Overview
The authentication system now uses a type-safe `UserRole` enum instead of strings for role management, providing compile-time safety and better IntelliSense support.

## UserRole Enum Definition

```csharp
namespace StockMind.Domain.Enums;

public enum UserRole
{
    Admin = 1,      // Full system access
    Manager = 2,    // Manage products, stock, categories
    Operator = 3,   // Add/remove stock operations
    Viewer = 4      // Read-only access (default)
}
```

## Benefits of Using Enum

### ? **Type Safety**
```csharp
// Before (strings - error-prone)
var role = "Manger"; // ? Typo! No compile-time error

// After (enum - compile-time safety)
var role = UserRole.Manager; // ? Typo impossible
```

### ? **IntelliSense Support**
- Auto-completion when typing `UserRole.`
- No need to remember exact role names
- Prevents typos and invalid values

### ? **Refactoring**
- Rename a role in one place (enum)
- All usages update automatically
- Find all references easily

### ? **API Integration**
JSON serialization automatically converts enum to string:

**Request:**
```json
{
  "newRole": "Manager"
}
```

**Response:**
```json
{
  "roles": ["Manager"]
}
```

## Usage Examples

### 1. Change User Role (API)
```http
PUT /api/auth/users/{userId}/role
Content-Type: application/json

{
  "newRole": "Manager"
}
```

Valid values: `"Admin"`, `"Manager"`, `"Operator"`, `"Viewer"` (case-insensitive)

### 2. Check User Role (Code)
```csharp
var user = await _userManager.FindByEmailAsync(email);
var roles = await _userManager.GetRolesAsync(user);

// Check if user is admin
if (roles.Contains(UserRole.Admin.ToString()))
{
    // Admin-only logic
}
```

### 3. Authorization Attributes
```csharp
[Authorize(Roles = "Admin")] // Still uses string for ASP.NET Identity compatibility
public async Task<IActionResult> AdminOnlyEndpoint()
{
    // ...
}
```

### 4. Assign Role Programmatically
```csharp
// Using enum
var roleName = UserRole.Manager.ToString();
await _userManager.AddToRoleAsync(user, roleName);

// Direct string (still supported by Identity)
await _userManager.AddToRoleAsync(user, "Manager");
```

## JSON Serialization

The API is configured to serialize enums as strings:

**Configuration in Program.cs:**
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });
```

**Result:**
- API accepts: `"Admin"`, `"Manager"`, `"Operator"`, `"Viewer"`
- API returns: String values instead of numbers
- Case-insensitive parsing

## Migration Summary

### Files Updated

| File | Change |
|------|--------|
| `DataSeeder.cs` | Uses `Enum.GetNames<UserRole>()` |
| `AuthService.cs` | Uses `UserRole.Viewer.ToString()` |
| `ChangeUserRoleCommand.cs` | Parameter type: `UserRole` |
| `ChangeUserRoleCommandHandler.cs` | Uses enum, removed string validation |
| `ChangeUserRoleCommandValidator.cs` | Uses `.IsInEnum()` validation |
| `AuthController.cs` | Added `using StockMind.Domain.Enums` |
| `Program.cs` | Added `JsonStringEnumConverter` |

### Backwards Compatibility

? **API remains compatible:**
- Clients still send/receive strings
- No breaking changes for API consumers
- Internal code uses type-safe enum

## Testing

### Valid Request:
```bash
curl -X PUT /api/auth/users/{userId}/role \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"newRole": "Manager"}'

# Response: 200 OK
```

### Invalid Request:
```bash
curl -X PUT /api/auth/users/{userId}/role \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"newRole": "InvalidRole"}'

# Response: 400 Bad Request
# Error: "Invalid role. Must be Admin, Manager, Operator, or Viewer"
```

### Case-Insensitive:
```bash
# All these work:
{"newRole": "Manager"}
{"newRole": "manager"}
{"newRole": "MANAGER"}
```

## Best Practices

### ? DO:
```csharp
// Use enum in code
var defaultRole = UserRole.Viewer;

// Convert to string only when needed
await _userManager.AddToRoleAsync(user, defaultRole.ToString());

// Use enum in Commands/DTOs
public record ChangeUserRoleCommand(Guid UserId, UserRole NewRole);
```

### ? DON'T:
```csharp
// Don't use magic strings
await _userManager.AddToRoleAsync(user, "Viewer"); // ?

// Don't parse strings manually
var role = (UserRole)Enum.Parse(typeof(UserRole), "Manager"); // ? Use enum directly
```

## Validation

FluentValidation automatically validates enum values:

```csharp
RuleFor(x => x.NewRole)
    .IsInEnum()
    .WithMessage("Invalid role. Must be Admin, Manager, Operator, or Viewer");
```

This ensures only valid enum values are accepted.

## Future Enhancements

Possible improvements:
- Add role descriptions to enum (using attributes)
- Add role hierarchy methods (`IsHigherThan()`, `CanPromoteTo()`)
- Add role permissions matrix
- Add custom JsonConverter with aliases

## Conclusion

The migration to `UserRole` enum provides:
- ? Type safety
- ? Better developer experience
- ? Easier maintenance
- ? No breaking changes for API consumers
- ? Compile-time error detection

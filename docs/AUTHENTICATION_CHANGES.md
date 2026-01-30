# Authentication System - User Registration Changes

## Overview
Updated the registration system to automatically assign the default "Viewer" role to new users, eliminating the need to specify a role during registration.

## Changes Made

### 1. Simplified Registration Flow
**Before:**
```json
POST /api/auth/register
{
  "email": "user@example.com",
  "password": "password",
  "fullName": "John Doe",
  "role": "Viewer"  // ? User had to specify role
}
```

**After:**
```json
POST /api/auth/register
{
  "email": "user@example.com",
  "password": "password",
  "fullName": "John Doe"
  // ? Role "Viewer" is automatically assigned
}
```

### 2. Role Hierarchy
```
Admin (1)    ? Full access (promote/demote users)
Manager (2)  ? Manage products, stock, categories
Operator (3) ? Add/remove stock operations
Viewer (4)   ? Read-only access (DEFAULT for new users)
```

### 3. New Admin Endpoint
Admins can now promote/demote users:

```http
PUT /api/auth/users/{userId}/role
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "newRole": "Manager"
}
```

**Valid roles (case-insensitive):** Admin, Manager, Operator, Viewer

**Note:** Roles are now type-safe using the `UserRole` enum instead of strings.

### 4. Files Modified

**Application Layer:**
- ? `RegisterRequestDto.cs` - Removed `Role` parameter
- ? `RegisterCommand.cs` - Removed `Role` parameter
- ? `RegisterCommandHandler.cs` - Removed role from service call
- ? `RegisterCommandValidator.cs` - Removed role validation
- ? `IAuthService.cs` - Updated interface signature
- ? `ChangeUserRoleCommand.cs` - NEW: Command for changing roles
- ? `ChangeUserRoleCommandValidator.cs` - NEW: Validator

**Infrastructure Layer:**
- ? `AuthService.cs` - Auto-assigns "Viewer" role
- ? `ChangeUserRoleCommandHandler.cs` - NEW: Handler for role changes

**API Layer:**
- ? `AuthController.cs` - Updated register endpoint + new change role endpoint

## Usage Examples

### Register a New User (Automatic Viewer Role)
```bash
curl -X POST https://localhost:7xxx/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@example.com",
    "password": "SecurePass123",
    "fullName": "New User"
  }'
```

### Promote User to Manager (Admin Only)
```bash
curl -X PUT https://localhost:7xxx/api/auth/users/{userId}/role \
  -H "Authorization: Bearer {admin-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "newRole": "Manager"
  }'
```

## Security Notes
- Only users with "Admin" role can change other users' roles
- Users automatically get "Viewer" role on registration
- Role changes replace all existing roles (user has only one role at a time)
- Invalid roles are rejected with validation error

## Benefits
? Simplified user registration (3 fields instead of 4)  
? Secure by default (new users get minimal permissions)  
? Clear role promotion workflow for admins  
? Prevents role escalation during self-registration  

# ASP.NET Core Identity Setup

ASP.NET Core Identity is a membership system that handles user authentication, authorization, and role management. This project uses it to manage user registration, login, and access control.

## Overview

Identity is configured in `Program.cs` and used in `UsersController.cs` and it provides three main pieces of functionality:

1. **Authentication** — registering and logging in users through the use of cookies
2. **User Management** — creating, updating, and deleting users
3. **Role-based Authorization** — assigning roles (currently Admin and User) to control access

## Authentication Flow

### Registration

When a user is registered, the system:

1. Checks if the email already exists in the database
2. Creates a new `User` object with the provided details
3. Hashes the password and stores it securely via `UserManager.CreateAsync()`
4. Assigns the role `User` or `Admin`
   If the user is registering themself:
5. Immediately sign the user in

Example registration request:

```json
{
    "email": "Hans@example.com",
    "password": "Abcd123!",
    "displayName": "Hans_Hansen",
    "firstName": "Hans",
    "lastName": "Hansen"
}
```

The endpoint validates that the email doesn't already exist. If registration fails due to any incorrect value, the transaction is aborted and the database operations are rolled back.

### Login

Login works as follows:

1. Look up the user by email
2. Retrieve their username from the user object
3. Use `SignInManager.PasswordSignInAsync()` to verify the password
4. If incorrect, the account locks after 5 failed attempts (5-minute lockout)
5. On success, eiher a persistent cookie is used or a normal cookie, depending on the "Remember Me" checkbox

Example login request:

```json
{
    "email": "hans@example.com",
    "password": "Abcd123!"
}
```

If login fails, the API returns a 500 with either "Invalid credentials" or "Account locked" depending on the reason.

### Cookie Configuration

Cookies are configured in `Program.cs` and have these configurations:

- **Expiration**: 10 minutes
- **Sliding expiration**: enabled (resets on each request)
- **HttpOnly**: prevents JavaScript access
- **SameSite**: Lax (protects against CSRF)
- **Secure**: only sent over HTTPS

## Password Policy

The password requirements are:

- Minimum length: **8 characters**
- At least 1 digit requird
- At least 1 lowercase character required
- At least 1 uppercase character required
- At least 1 special character required

## User Management

### Creating Users (Admin Only)

Admins can create users via the API:

```json
POST /api/users/add
{
  "email": "newuser@example.com",
  "password": "Pw123!",
  "displayName": "newuser",
  "firstName": "New",
  "lastName": "User"
}
```

The system wraps creation in a database transaction. If any step fails, the entire operation rolls back.

### Updating Users

Users can be updated with new information:

```json
PUT /api/users/update/42
{
  "email": "updated@example.com",
  "password": "Pw12345!",
  "firstName": "Updated",
  "lastName": "Name"
}
```

Password changes use `GeneratePasswordResetTokenAsync()` and `ResetPasswordAsync()` to ensure security.

### Deleting Users

Admins can delete users:

```
DELETE /api/users/delete/42
```

Deletion is soft (marks as deleted) or hard depending on configuration. The system checks if the user exists before attempting deletion.

### Retrieving Users

```
GET /api/users           → all users
GET /api/users/42        → specific user
```

## Roles and Authorization

Two roles are seeded automatically at startup:

- **Admin** — full access to user management endpoints
- **User** — standard authenticated user

Roles are assigned during registration (all manual signup is given User role, admin-sided registration can assign any role) and can be changed by admins.

To require authentication on an endpoint, add the `Authorize` attribute:

```csharp
[HttpPost("logout")]
[Authorize]
public async Task<IActionResult> Logout()
{
    await signInManager.SignOutAsync();
    return Ok();
}
```

To require a specific role, add a role to the attribute:

```csharp
[Authorize(Roles = "Admin")]
public async Task<IActionResult> AdminOnly()
{
    // Admin only functionality
    return Ok();
}
```

## Identity Configuration

Some additional key settings configured in `Program.cs` (excluding previously mentioned password constraints):

```csharp
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
});
```

Notable choices:

- **Email** must be unique, not **username**/**displayname**
- Email confirmation is **not** required
- Phone number confirmation is **not** required
- Account confirmation is **not** required

## Database Schema

Identity uses the following key tables:

- **AspNetUsers** — user accounts (email, username, password hash, etc. and our own added fields)
- **AspNetRoles** — role definitions (Admin, User)
- **AspNetUserRoles** — mapping of users to roles

The `User` model extends `IdentityUser<int>` and adds custom fields:

- `FirstName`
- `Infix` (optional middle name)
- `LastName`

## Common Tasks

### Check if user is authenticated

```csharp
[Authorize]
public async Task MyProtectedEndpoint()
{
    var user = User; // ClaimsPrincipal
    var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
}
```

### Get current user details

```csharp
var user = await userManager.GetUserAsync(User);
```

### Change a user's role

```csharp
await userManager.RemoveFromRoleAsync(user, "User");
await userManager.AddToRoleAsync(user, "Admin");
```

### Lock/unlock an account

```csharp
await userManager.SetLockoutEnabledAsync(user, true);
await userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddMinutes(5));
```

## Security Considerations

- Passwords are hashed with PBKDF2 (configurable algorithm)
- Failed login attempts trigger account lockout
- Sessions are cookie-based and tied to the user's device
- Sensitive operations should require re-authentication
- Consider adding email confirmation for production

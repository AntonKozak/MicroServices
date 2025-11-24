# Identity Service - Complete Feature Set

## 🎯 New Features Added

### 1. **Email Confirmation**
- Users must confirm their email before logging in
- Automatic email token generation
- Resend confirmation email capability
- Email change requires re-confirmation

**Endpoints:**
- `GET /api/account/confirm-email?userId={id}&token={token}` - Confirm email
- `POST /api/account/resend-confirmation-email` - Resend confirmation

### 2. **Forgot Password / Password Reset**
- Secure password reset via email token
- Token expiration for security
- Email notification on password change
- Protection against email enumeration

**Endpoints:**
- `POST /api/account/forgot-password` - Request password reset
- `POST /api/account/reset-password` - Reset password with token

### 3. **Change Password**
- Authenticated users can change their password
- Requires current password verification
- Email notification on successful change

**Endpoint:**
- `POST /api/account/change-password` - Change password (authenticated)

### 4. **Refresh Tokens**
- Long-lived refresh tokens (7 days default)
- Short-lived access tokens (60 minutes default)
- Token rotation on refresh
- Secure token storage in database

**Endpoint:**
- `POST /api/account/refresh-token` - Get new access token

### 5. **Profile Management**
- Update user profile information
- Change email (requires confirmation)
- Add phone number
- Update first/last name

**Endpoint:**
- `PUT /api/account/update-profile` - Update user profile (authenticated)

### 6. **Account Deletion**
- Users can delete their own accounts
- Permanent removal from database
- Requires authentication

**Endpoint:**
- `DELETE /api/account/delete-account` - Delete account (authenticated)

### 7. **Enhanced Security Features**
- Account lockout after failed login attempts
- Email confirmation required for login
- Last login tracking
- Refresh token invalidation on logout

### 8. **Email Service**
- Email confirmation links
- Password reset links
- Password change notifications
- Console logging for development (replace with real email service in production)

---

## 📋 Complete API Reference

### Public Endpoints

#### Register User
```http
POST /api/account/register
Content-Type: application/json

{
  "email": "user@example.com",
  "userName": "username",
  "password": "Password123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response:**
```json
{
  "message": "Registration successful. Please check your email to confirm your account.",
  "userId": "user-id"
}
```

#### Confirm Email
```http
GET /api/account/confirm-email?userId={userId}&token={token}
```

**Response:**
```json
{
  "message": "Email confirmed successfully. You can now login."
}
```

#### Resend Confirmation Email
```http
POST /api/account/resend-confirmation-email
Content-Type: application/json

"user@example.com"
```

#### Login
```http
POST /api/account/login
Content-Type: application/json

{
  "userName": "username",
  "password": "Password123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGci...",
  "userName": "username",
  "email": "user@example.com",
  "expiresAt": "2025-11-24T13:00:00Z"
}
```

#### Refresh Token
```http
POST /api/account/refresh-token
Content-Type: application/json

{
  "token": "current-access-token",
  "refreshToken": "refresh-token"
}
```

#### Forgot Password
```http
POST /api/account/forgot-password
Content-Type: application/json

{
  "email": "user@example.com"
}
```

**Response:**
```json
{
  "message": "If the email exists, a password reset link has been sent."
}
```

#### Reset Password
```http
POST /api/account/reset-password
Content-Type: application/json

{
  "email": "user@example.com",
  "token": "reset-token",
  "newPassword": "NewPassword123!",
  "confirmPassword": "NewPassword123!"
}
```

### Authenticated Endpoints

All endpoints below require `Authorization: Bearer {token}` header.

#### Get Current User
```http
GET /api/account/me
```

**Response:**
```json
{
  "id": "user-id",
  "userName": "username",
  "email": "user@example.com",
  "phoneNumber": "+1234567890",
  "firstName": "John",
  "lastName": "Doe",
  "emailConfirmed": true,
  "createdAt": "2025-11-01T10:00:00Z",
  "lastLoginAt": "2025-11-24T12:00:00Z",
  "roles": ["User"]
}
```

#### Update Profile
```http
PUT /api/account/update-profile
Content-Type: application/json

{
  "email": "newemail@example.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "phoneNumber": "+1234567890"
}
```

#### Change Password
```http
POST /api/account/change-password
Content-Type: application/json

{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword123!",
  "confirmPassword": "NewPassword123!"
}
```

#### Logout
```http
POST /api/account/logout
```

#### Delete Account
```http
DELETE /api/account/delete-account
```

### Admin Endpoints

All endpoints require `Authorization: Bearer {admin-token}` header and **Admin** role.

#### List All Users
```http
GET /api/admin/users
```

#### Get User by ID
```http
GET /api/admin/users/{userId}
```

#### Add User to Role
```http
POST /api/admin/users/{userId}/roles/{roleName}
```

#### Remove User from Role
```http
DELETE /api/admin/users/{userId}/roles/{roleName}
```

#### List All Roles
```http
GET /api/admin/roles
```

#### Create Role
```http
POST /api/admin/roles
Content-Type: application/json

"NewRoleName"
```

---

## 🔧 Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=identity;User Id=postgres;Password=postgrespw;"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForSecurity",
    "Issuer": "IdentityService",
    "Audience": "MicroServicesApp",
    "AccessTokenExpiryInMinutes": "60",
    "RefreshTokenExpiryInDays": "7"
  }
}
```

### JWT Configuration Explained

- **Secret**: Signing key (CHANGE IN PRODUCTION!)
- **Issuer**: Token issuer identifier
- **Audience**: Token audience identifier
- **AccessTokenExpiryInMinutes**: Access token lifetime (default: 60 minutes)
- **RefreshTokenExpiryInDays**: Refresh token lifetime (default: 7 days)

---

## 🔐 Security Features

### Password Requirements
- Minimum 6 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 digit
- Special characters optional

### Account Lockout
- Enabled by default
- 5 failed attempts
- 5-minute lockout duration
- Applies to new users

### Email Confirmation
- Required before login
- Token-based verification
- Required after email change

### Token Security
- Short-lived access tokens (1 hour)
- Long-lived refresh tokens (7 days)
- Refresh token rotation
- Tokens invalidated on logout

---

## 📧 Email Service

The `IEmailService` interface is implemented with console logging for development. In production, implement with:

- **SendGrid**
- **AWS SES**
- **SMTP Server**
- **Azure Communication Services**

### Example SendGrid Implementation

```csharp
public class SendGridEmailService : IEmailService
{
    private readonly ISendGridClient _sendGridClient;

    public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
    {
        var msg = new SendGridMessage()
        {
            From = new EmailAddress("noreply@yourapp.com", "Your App"),
            Subject = "Confirm your email",
            PlainTextContent = $"Please confirm your email: {confirmationLink}",
            HtmlContent = $"<a href='{confirmationLink}'>Confirm Email</a>"
        };
        msg.AddTo(new EmailAddress(email));

        await _sendGridClient.SendEmailAsync(msg);
    }

    // Implement other methods...
}
```

---

## 🚀 Usage Flow

### Complete User Registration Flow

1. **User registers** → `POST /api/account/register`
2. **System sends confirmation email** (check console for link)
3. **User clicks link** → `GET /api/account/confirm-email?userId=X&token=Y`
4. **User logs in** → `POST /api/account/login`
5. **System returns JWT + refresh token**

### Token Refresh Flow

1. **Access token expires** (after 60 minutes)
2. **Client requests new token** → `POST /api/account/refresh-token`
3. **System validates refresh token**
4. **System returns new access token + new refresh token**

### Password Reset Flow

1. **User requests reset** → `POST /api/account/forgot-password`
2. **System sends reset email** (check console for link)
3. **User clicks link and enters new password** → `POST /api/account/reset-password`
4. **System resets password and sends notification**

---

## 🧪 Testing

Use the included `IdentityService.http` file to test all endpoints.

### Quick Test Sequence

1. Start PostgreSQL: `docker-compose up -d postgres`
2. Run migrations: `dotnet ef database update`
3. Start service: `dotnet run`
4. Open `IdentityService.http` in VS Code
5. Run requests in order

### Development Testing

Email links are logged to the console:
```
========================================
Email Confirmation Link for user@example.com:
http://localhost:5001/api/account/confirm-email?userId=...&token=...
========================================
```

Copy these links to test the flow.

---

## 📊 Database Schema

### ApplicationUser (extends IdentityUser)

| Column | Type | Description |
|--------|------|-------------|
| FirstName | string? | User's first name |
| LastName | string? | User's last name |
| CreatedAt | DateTime | Account creation date |
| LastLoginAt | DateTime? | Last login timestamp |
| RefreshToken | string? | Current refresh token |
| RefreshTokenExpiryTime | DateTime? | Refresh token expiration |

Plus all Identity tables: Users, Roles, UserRoles, UserClaims, etc.

---

## 🔄 Migration History

1. **InitialCreate** - ASP.NET Core Identity tables
2. **AddRefreshTokenAndUserTracking** - Refresh tokens and user tracking

---

## 🎁 What's Included

✅ Email confirmation
✅ Forgot password / Reset password
✅ Change password
✅ Refresh tokens
✅ Profile management
✅ Account deletion
✅ Account lockout
✅ Last login tracking
✅ Role-based authorization
✅ Admin user management
✅ JWT authentication
✅ PostgreSQL integration
✅ Comprehensive API documentation
✅ HTTP test file

---

## 🚨 Production Checklist

- [ ] Change JWT Secret in production
- [ ] Implement real email service (SendGrid, etc.)
- [ ] Enable HTTPS
- [ ] Set up proper CORS policy
- [ ] Configure rate limiting
- [ ] Set up logging and monitoring
- [ ] Review password policies
- [ ] Configure token expiration times
- [ ] Set up database backups
- [ ] Implement 2FA (optional)
- [ ] Add reCAPTCHA for registration (optional)

---

## 📝 Notes

- All emails are logged to console in development
- Admin account is auto-created on first run (username: `admin`, password: `Admin123!`)
- Default roles: Admin, User, Seller
- Email confirmation is required for login
- Account lockout after 5 failed attempts

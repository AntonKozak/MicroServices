# Identity Service

A custom ASP.NET Core Identity Service with JWT authentication for the MicroServices application.

## Features

- **ASP.NET Core Identity**: Full user management with UserManager, RoleManager, and SignInManager
- **JWT Authentication**: Stateless token-based authentication
- **PostgreSQL Database**: Entity Framework Core with Postgres
- **Role-Based Authorization**: Admin and User roles with extensible role system
- **RESTful API**: Clean API endpoints for authentication and user management

## Technology Stack

- ASP.NET Core 8.0
- ASP.NET Core Identity
- Entity Framework Core 8.0
- PostgreSQL (via Npgsql)
- JWT Bearer Authentication
- Swagger/OpenAPI

## Prerequisites

- .NET 8.0 SDK
- PostgreSQL database server

## Configuration

Update `appsettings.json` with your database connection and JWT settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=identity;User Id=postgres;Password=postgres;"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForSecurity",
    "Issuer": "IdentityService",
    "Audience": "MicroServicesApp",
    "ExpiryInDays": "7"
  }
}
```

## Database Setup

Run Entity Framework migrations to create the database:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## API Endpoints

### Account Controller

- `POST /api/account/register` - Register a new user
- `POST /api/account/login` - Login and receive JWT token
- `GET /api/account/me` - Get current user information (requires authentication)
- `POST /api/account/logout` - Logout (requires authentication)

### Admin Controller (Admin role required)

- `GET /api/admin/users` - Get all users
- `GET /api/admin/users/{userId}` - Get specific user
- `POST /api/admin/users/{userId}/roles/{roleName}` - Add user to role
- `DELETE /api/admin/users/{userId}/roles/{roleName}` - Remove user from role
- `GET /api/admin/roles` - Get all roles
- `POST /api/admin/roles` - Create new role

## Usage Examples

### Register a new user

```bash
POST /api/account/register
Content-Type: application/json

{
  "email": "user@example.com",
  "userName": "johndoe",
  "password": "Password123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

### Login

```bash
POST /api/account/login
Content-Type: application/json

{
  "userName": "johndoe",
  "password": "Password123!"
}
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9...",
  "userName": "johndoe",
  "email": "user@example.com",
  "expiresAt": "2025-12-01T12:00:00Z"
}
```

### Access protected endpoints

```bash
GET /api/account/me
Authorization: Bearer {your-jwt-token}
```

## Default Users

On first run, the application creates:

- **Admin User**
  - Username: `admin`
  - Password: `Admin123!`
  - Role: Admin

## Default Roles

- Admin
- User
- Seller

## Running the Service

```bash
cd src/IdentityService
dotnet restore
dotnet run
```

The service will be available at `http://localhost:5001`

Swagger UI: `http://localhost:5001/swagger`

## Security Considerations

- Change the JWT secret in production
- Use environment variables for sensitive configuration
- Enable HTTPS in production
- Implement rate limiting for authentication endpoints
- Add email verification for user registration
- Implement refresh tokens for long-lived sessions

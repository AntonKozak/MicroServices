# JWT Configuration - Security Best Practices

## Current Setup (Development)

All services share the same JWT secret for token validation. This is **acceptable for development** but needs improvement for production.

### Files with JWT Settings:
- `IdentityService/appsettings.json` - **Signs tokens** (needs secret)
- `GatewayService/appsettings.json` - **Validates tokens** (needs secret)
- `AuctionService/appsettings.json` - **Validates tokens** (needs secret)
- `BiddingService/appsettings.json` - **Validates tokens** (needs secret)

---

## Why Current Setup is OK for Now:

1. ✅ **Trusted Network** - All services are internal, behind the Gateway
2. ✅ **Gateway First Validation** - Gateway validates before routing to services
3. ✅ **Simple for Development** - Easy to manage during learning/development
4. ✅ **Symmetric JWT (HS512)** - Industry standard for microservices

---

## Production Improvements (Choose One):

### **Option 1: Environment Variables (Recommended Next Step)**

Move secrets out of appsettings.json:

**Docker Compose:**
```yaml
services:
  identity-svc:
    environment:
      - JwtSettings__Secret=${JWT_SECRET}
  gateway-svc:
    environment:
      - JwtSettings__Secret=${JWT_SECRET}
```

**.env file (NOT committed to git):**
```
JWT_SECRET=YourSuperSecretKeyThatMustBeAtLeast64CharactersLongForHS512Algorithm!!
```

**Benefits:**
- ✅ Secret not in source control
- ✅ Different secrets per environment
- ✅ Easy rotation
- ❌ Still shared across services

---

### **Option 2: Asymmetric JWT (RSA) - Best Security**

IdentityService signs with **private key**, services validate with **public key**.

**IdentityService (only):**
```json
{
  "JwtSettings": {
    "PrivateKey": "path/to/private.key",  // Keep secret!
    "Issuer": "IdentityService",
    "Audience": "MicroServicesApp"
  }
}
```

**Other Services:**
```json
{
  "JwtSettings": {
    "PublicKey": "path/to/public.key",  // Safe to share
    "Issuer": "IdentityService",
    "Audience": "MicroServicesApp"
  }
}
```

**Benefits:**
- ✅ Only IdentityService has private key
- ✅ Public key can be exposed without risk
- ✅ True security separation
- ❌ More complex setup

**Code Changes Required:**
```csharp
// IdentityService - Sign with RSA private key
var rsa = RSA.Create();
rsa.ImportFromPem(privateKeyPem);
var signingCredentials = new SigningCredentials(
    new RsaSecurityKey(rsa),
    SecurityAlgorithms.RsaSha256
);

// Other Services - Validate with RSA public key
var rsa = RSA.Create();
rsa.ImportFromPem(publicKeyPem);
options.TokenValidationParameters.IssuerSigningKey = new RsaSecurityKey(rsa);
```

---

### **Option 3: Centralized Secret Management**

Use Azure Key Vault, AWS Secrets Manager, or HashiCorp Vault:

```csharp
builder.Configuration.AddAzureKeyVault(/* ... */);
var secret = builder.Configuration["JwtSecret"]; // Retrieved from vault
```

**Benefits:**
- ✅ Centralized secret management
- ✅ Audit logging
- ✅ Automatic rotation
- ✅ Access control per service
- ❌ Requires cloud infrastructure

---

## Current Architecture Justification

Your microservices are **NOT directly exposed** to the internet:

```
Internet → Gateway (validates JWT) → Internal Services
```

**Security Layers:**
1. **Gateway** validates JWT first
2. **Services** double-check (defense in depth)
3. **Network isolation** - services only accessible within Docker network

**This means:**
- Even if one service is compromised, it's behind the Gateway
- Gateway is the security boundary
- Services share secret within trusted network

---

## Action Items for Production:

### **Phase 1: Environment Variables (Easy)**
- [ ] Create `.env` file (add to `.gitignore`)
- [ ] Move `JwtSettings:Secret` to environment variables
- [ ] Update docker-compose.yml to use `${JWT_SECRET}`
- [ ] Update documentation

### **Phase 2: Asymmetric JWT (Better Security)**
- [ ] Generate RSA key pair
- [ ] Update IdentityService to use private key
- [ ] Update other services to use public key
- [ ] Test token validation

### **Phase 3: Secret Management (Production)**
- [ ] Set up Azure Key Vault / AWS Secrets Manager
- [ ] Configure services to retrieve secrets
- [ ] Enable secret rotation
- [ ] Set up monitoring/alerts

---

## Quick Fix for Now

**Add to `.gitignore`:**
```
*.secret.json
appsettings.Production.json
.env
```

**Create `appsettings.Production.json` (not committed):**
```json
{
  "JwtSettings": {
    "Secret": "PRODUCTION_SECRET_FROM_ENVIRONMENT"
  }
}
```

**In production, use:**
```bash
export JwtSettings__Secret="your-production-secret"
```

---

## Summary

| Approach | Security | Complexity | Current? |
|----------|----------|------------|----------|
| Hardcoded in files | ⚠️ Low | ✅ Simple | ✅ **YES** |
| Environment Variables | ✅ Medium | ✅ Simple | Next step |
| Asymmetric (RSA) | ✅✅ High | ⚠️ Medium | Future |
| Secret Management | ✅✅✅ Highest | ⚠️⚠️ Complex | Production |

**Your current setup is fine for development.** Before deploying to production, move to at least **Option 1 (Environment Variables)**.

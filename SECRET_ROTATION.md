# 🔐 SECRET ROTATION GUIDE

## ✅ SOLVED: Change JWT Secret in ONE Place!

### **Before (BAD):**
Had to edit 4 files:
- ❌ `IdentityService/appsettings.json`
- ❌ `GatewayService/appsettings.json`
- ❌ `AuctionService/appsettings.json`
- ❌ `BiddingService/appsettings.json`

### **After (GOOD):**
Edit only `.env` file:
- ✅ `.env` (ONE place!)

---

## 📝 How to Rotate JWT Secret

### **Step 1: Edit `.env` file**
```bash
# Change this line:
JWT_SECRET=YourNewSuperSecretKeyMustBeAtLeast64CharactersLongForHS512!!
```

### **Step 2: Restart services**
```bash
docker compose down
docker compose up -d
```

### **That's it!** All 4 services now use the new secret. ✅

---

## 🔍 How It Works

### **Running in Docker (via docker-compose):**
**Docker Compose** reads `.env` automatically and injects variables:

```yaml
services:
  identity-svc:
    environment:
      - JwtSettings__Secret=${JWT_SECRET}  # From .env
      - JwtSettings__Issuer=${JWT_ISSUER}  # From .env
      - JwtSettings__Audience=${JWT_AUDIENCE}  # From .env
```

### **Running Locally in VS Code:**
Uses `appsettings.Development.json` which has the secret:

```json
{
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatMustBeAtLeast64CharactersLongForHS512Algorithm!!"
  }
}
```

**.NET Configuration** hierarchy (priority order):
1. **Environment Variables** ← Docker uses this from `.env`
2. **appsettings.Development.json** ← VS Code uses this
3. appsettings.json (fallback, now empty)
4. Command line args

### **To Rotate Secrets:**

| Where | Files to Update | Command |
|-------|----------------|---------|
| **Docker** | `.env` only | `docker compose restart` |
| **Local (VS Code)** | 4× `appsettings.Development.json` | Restart debugger |
| **Both** | `.env` + 4× Development files | - |

---

## 🛡️ Security Best Practices

### ✅ **DO:**
- Keep `.env` in `.gitignore` (already done)
- Use `.env.example` as template (already created)
- Use different secrets per environment (dev/staging/prod)
- Rotate secrets at least yearly

### ❌ **DON'T:**
- Commit `.env` to Git
- Share `.env` in Slack/email
- Use the example secret in production
- Use secrets shorter than 64 characters

---

## 📂 Files Changed

**Created:**
- `.env` - Your actual secrets (NOT in Git)
- `.env.example` - Template for team members

**Updated:**
- `docker-compose.yml` - All services now use `${JWT_SECRET}`
- `IdentityService/appsettings.json` - Secret removed
- `GatewayService/appsettings.json` - Secret removed
- `AuctionService/appsettings.json` - Secret removed
- `BiddingService/appsettings.json` - Secret removed

---

## 🚀 For New Team Members

1. Copy the template:
   ```bash
   cp .env.example .env
   ```

2. Ask team lead for the actual `JWT_SECRET`

3. Update `.env` with real values

4. Start services:
   ```bash
   docker compose up
   ```

---

## 🔄 Secret Rotation Schedule

| Environment | Frequency | Who |
|-------------|-----------|-----|
| Development | When compromised | Any dev |
| Staging | Every 6 months | DevOps |
| Production | Every 3 months | Security team |

---

## 🆘 Troubleshooting

**Issue:** Services show "Unauthorized" after rotation

**Solution:**
1. Verify `.env` has new secret (64+ chars)
2. Restart ALL services: `docker compose restart`
3. Check logs: `docker compose logs identity-svc`

**Issue:** `.env` not being read

**Solution:**
1. Must be in same directory as `docker-compose.yml`
2. No spaces around `=`: `JWT_SECRET=value` (not `JWT_SECRET = value`)
3. No quotes needed: `JWT_SECRET=abc123` (not `JWT_SECRET="abc123"`)

---

## 🎯 Summary

**One command to rotate all JWT secrets:**
```bash
# Edit .env, then:
docker compose restart
```

That's it! No more hunting through 4 different files. 🎉

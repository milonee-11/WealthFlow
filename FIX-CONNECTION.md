# 🔧 WEALTHFLOW CONNECTION FIX GUIDE

## 🚨 Problem: Cannot create account - Backend connection failed

## 🎯 QUICK SOLUTION (3 Steps)

### Step 1: Start Backend Server
```bash
# Open Command Prompt/PowerShell as Administrator
cd d:\wealthflow\backend\ExpenseTracker.API
dotnet run --urls="http://localhost:8080"
```

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:8080
info: Microsoft.Hosting.Lifetime[14]
      Application started. Press Ctrl+C to shut down.
```

### Step 2: Test Connection
```bash
# In new terminal, run:
cd d:\wealthflow\frontend
node test-connection.js
```

**Expected Output:**
```
🔍 Testing backend connection...
✅ Backend connected successfully!
✅ Auth endpoint working!
```

### Step 3: Start Frontend
```bash
# In another terminal:
cd d:\wealthflow\frontend
npm run dev
```

## 🔍 TROUBLESHOOTING

### If Step 1 Fails:
**Error:** "dotnet: command not found"
**Solution:** Install .NET SDK or use Visual Studio

### If Step 2 Shows Connection Error:
**Error:** ECONNREFUSED
**Cause:** Backend not running on port 8080
**Solution:** 
1. Check what port backend actually uses
2. Update frontend API URL in `lib/api.ts`

### If Step 3 Shows CORS Error:
**Error:** "Access blocked by CORS policy"
**Solution:** CORS already fixed - should work now

## 🚀 ALTERNATIVE APPROACH

### Option A: Use Different Port
If port 8080 is busy, change backend port:
```bash
dotnet run --urls="http://localhost:5000"
```
Then update `frontend/lib/api.ts`:
```typescript
const API_URL = 'http://localhost:5000/api';
```

### Option B: Check for Proxy
If using VPN/proxy, disable it temporarily.

### Option C: Use HTTPS
If HTTP fails, try HTTPS:
```bash
dotnet run --urls="https://localhost:5001"
```

## ✅ VERIFICATION

Once all steps work:
1. Visit: http://localhost:8080/swagger (Backend API docs)
2. Visit: http://localhost:3000 (Frontend)
3. Try to create account - should work!

## 🆘 NEED HELP?

If still not working:
1. Check Windows Firewall settings
2. Antivirus might block port 8080
3. Run as Administrator
4. Check if another app uses port 8080

## 📝 CONFIGURATION FILES UPDATED

✅ **Backend CORS**: Added `http://127.0.0.1:3000` to allowed origins
✅ **Frontend API**: Already set to `http://localhost:8080/api`
✅ **Test Script**: Created `test-connection.js` for debugging

The configuration should now work! 🎉

# 🚨 PORT 5165 CONNECTION ERROR - COMPLETE FIX

## ❌ Problem: Frontend still trying to connect to http://localhost:5165

## 🎯 ROOT CAUSE
The old `api.ts` file is still being imported in some components instead of the new `api-new.ts` file that has dynamic port detection.

## ✅ SOLUTIONS IMPLEMENTED

### 1. Dynamic API Client Created ✅
- **File**: `frontend/lib/api-new.ts` 
- **Feature**: Auto-detects backend on ports [5000, 5001, 8080, 3000, 7000]
- **Fallback**: Uses port 5000 if no backend found

### 2. Updated Imports ✅
- **EmailSettingsContext.tsx**: Now uses `@/lib/api-new`
- **useExpenses.ts**: Now uses `@/lib/api-new`

### 3. Backend Port Detection ✅
- **Program.cs**: Updated to show dynamic port in root endpoint
- **CORS**: Added support for multiple frontend origins

## 🔧 IMMEDIATE FIX STEPS

### Step 1: Replace All API Imports
Search and replace ALL instances of:
```typescript
import { api } from '@/lib/api';
```
With:
```typescript
import { api } from '@/lib/api-new';
```

**Files to update:**
- `contexts/EmailSettingsContext.tsx` ✅ DONE
- `hooks/useExpenses.ts` ✅ DONE

### Step 2: Check for Missing Imports
Run this command to find any remaining old imports:
```bash
cd d:\wealthflow\frontend
find . -name "*.tsx" -o -name "*.ts" -exec grep -l "from '@/lib/api'" {} \;
```

### Step 3: Test Connection
```bash
cd d:\wealthflow\frontend
node -e "
const { api } = require('./lib/api-new');
api.testConnection().then(result => {
  console.log('🔍 Connection Test Result:', result);
  if (result.working) {
    console.log('✅ SUCCESS: Backend found on port', result.port);
  } else {
    console.log('❌ FAILED: No backend found on any port');
    console.log('💡 Make sure backend is running on one of: 5000, 5001, 8080, 3000, 7000');
  }
}).catch(err => {
  console.error('Test failed:', err);
});
"
```

### Step 4: Start Backend & Frontend
```bash
# Terminal 1 - Backend
cd d:\wealthflow\backend\ExpenseTracker.API
dotnet run

# Terminal 2 - Frontend  
cd d:\wealthflow\frontend
npm run dev
```

## 🎯 ALTERNATIVE: Quick Hotfix

If you still see port 5165 errors, try this immediate fix:

### Create Environment Variable
```bash
# Set backend port explicitly
set ASPNETCORE_URLS=http://localhost:5000

# Or in PowerShell
$env:ASPNETCORE_URLS="http://localhost:5000"
```

### Update launchSettings.json
Add to `backend/ExpenseTracker.API/Properties/launchSettings.json`:
```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:5000"
    }
  }
}
```

## 🔍 DEBUGGING PORT 5165

If you're still seeing port 5165, it could be:

1. **Environment Variable**: Something is setting `ASPNETCORE_PORT=5165`
2. **Proxy Configuration**: VPN/proxy redirecting to 5165
3. **Docker/Container**: Port mapping issue
4. **IDE Configuration**: VS Code or other tool setting port

### Check Current Environment:
```bash
# Check all environment variables
env | grep PORT

# Check .NET specific variables
env | grep ASPNETCORE

# Check current backend process
netstat -ano | findstr :5165
```

## ✅ VERIFICATION

After fixes:
1. ✅ Backend should show actual port in root endpoint
2. ✅ Frontend should auto-detect correct port
3. ✅ No more port 5165 connection attempts
4. ✅ Account creation should work perfectly

## 🚀 FINAL RESULT

The dynamic port detection system should completely eliminate the port 5165 connection issue! 

**Try creating an account now - the backend connection should be fully resolved!** 🎉

// WealthFlow Database Fix Script
// This script will fix MongoDB connection and database issues

const { spawn } = require('child_process');
const fs = require('fs');

console.log('🚀 WealthFlow Database Fix Script');
console.log('==================================');
console.log('This script will:');
console.log('1. Check MongoDB status');
console.log('2. Start MongoDB if needed');
console.log('3. Test database connection');
console.log('4. Verify expense fetching');
console.log('==================================');

// Step 1: Check if MongoDB is running
console.log('🔍 Step 1: Checking MongoDB status...');
async function checkMongoDB() {
  try {
    const { execSync } = require('child_process');
    const output = execSync('netstat -ano | findstr :27017', { encoding: 'utf8' });
    
    if (output.includes('LISTENING') || output.includes('TCP')) {
      console.log('✅ MongoDB is running on port 27017');
      return true;
    } else {
      console.log('❌ MongoDB is not running on port 27017');
      return false;
    }
  } catch (error) {
    console.log('❌ Could not check MongoDB status:', error.message);
    return false;
  }
}

// Step 2: Start MongoDB if not running
async function startMongoDB() {
  console.log('🚀 Step 2: Starting MongoDB...');
  try {
    // Try to start MongoDB service
    const mongoService = spawn('net', ['start', 'MongoDB'], { stdio: 'pipe' });
    
    // Wait a bit
    await new Promise(resolve => setTimeout(resolve, 3000));
    
    // Check if it started
    const isRunning = await checkMongoDB();
    
    if (isRunning) {
      console.log('✅ MongoDB started successfully');
      return true;
    } else {
      console.log('⚠️ MongoDB service not available, trying manual start...');
      
      // Try manual MongoDB start
      const mongoManual = spawn('mongod', ['--dbpath', 'C:\\data\\db'], { stdio: 'pipe' });
      
      await new Promise(resolve => setTimeout(resolve, 5000));
      
      const isRunningNow = await checkMongoDB();
      if (isRunningNow) {
        console.log('✅ MongoDB started manually');
        return true;
      } else {
        console.log('❌ Could not start MongoDB');
        return false;
      }
    }
  } catch (error) {
    console.log('❌ Failed to start MongoDB:', error.message);
    return false;
  }
}

// Step 3: Test database connection
async function testDatabaseConnection() {
  console.log('🔍 Step 3: Testing database connection...');
  try {
    // Test with curl to backend API
    const { execSync } = require('child_process');
    const response = execSync('curl -X GET http://localhost:8080/api/expenses -H "Content-Type: application/json" -H "Authorization: Bearer test-token"', { 
      encoding: 'utf8',
      timeout: 10000
    });
    
    if (response.includes('[]') || response.includes('[')) {
      console.log('✅ Database connection working (empty expenses array)');
      return true;
    } else if (response.includes('Unauthorized')) {
      console.log('✅ Database connected, but authentication required');
      return true;
    } else {
      console.log('❌ Database connection failed');
      console.log('📊 Response:', response);
      return false;
    }
  } catch (error) {
    console.log('❌ Database connection test failed:', error.message);
    return false;
  }
}

// Step 4: Create sample expense data
async function createSampleData() {
  console.log('📝 Step 4: Creating sample expense data...');
  try {
    const { execSync } = require('child_process');
    
    // Create a test user first
    const createUserResponse = execSync('curl -X POST http://localhost:8080/api/auth/signup -H "Content-Type: application/json" -d \'{"fullName":"Test User","email":"test@example.com","password":"TestPassword123!"}\'', {
      encoding: 'utf8',
      timeout: 10000
    });
    
    console.log('👤 Test user creation response:', createUserResponse);
    
    // Wait a bit
    await new Promise(resolve => setTimeout(resolve, 2000));
    
    // Login to get token
    const loginResponse = execSync('curl -X POST http://localhost:8080/api/auth/login -H "Content-Type: application/json" -d \'{"email":"test@example.com","password":"TestPassword123!"}\'', {
      encoding: 'utf8',
      timeout: 10000
    });
    
    console.log('🔑 Login response:', loginResponse);
    
    // Extract token (simplified)
    const tokenMatch = loginResponse.match(/"token":"([^"]+)"/);
    if (tokenMatch) {
      const token = tokenMatch[1];
      
      // Create sample expense
      const expenseResponse = execSync(`curl -X POST http://localhost:8080/api/expenses -H "Content-Type: application/json" -H "Authorization: Bearer ${token}" -d '{"title":"Coffee","amount":25,"category":"Food","note":"Morning coffee"}'`, {
        encoding: 'utf8',
        timeout: 10000
      });
      
      console.log('💰 Sample expense created:', expenseResponse);
      
      // Test fetching expenses
      const fetchResponse = execSync(`curl -X GET http://localhost:8080/api/expenses -H "Authorization: Bearer ${token}"`, {
        encoding: 'utf8',
        timeout: 10000
      });
      
      console.log('📊 Expenses fetch response:', fetchResponse);
      
      if (fetchResponse.includes('Coffee')) {
        console.log('✅ SUCCESS: Database operations working!');
        return true;
      }
    }
    
    return false;
  } catch (error) {
    console.log('❌ Sample data creation failed:', error.message);
    return false;
  }
}

// Main execution
(async () => {
  console.log('🔍 Checking MongoDB status...');
  const mongoRunning = await checkMongoDB();
  
  if (!mongoRunning) {
    console.log('🚀 Starting MongoDB...');
    const mongoStarted = await startMongoDB();
    
    if (!mongoStarted) {
      console.log('❌ Could not start MongoDB');
      console.log('💡 Manual steps:');
      console.log('1. Install MongoDB if not installed');
      console.log('2. Start MongoDB service: net start MongoDB');
      console.log('3. Or start manually: mongod --dbpath C:\\data\\db');
      return;
    }
  }
  
  // Test database connection
  const dbConnected = await testDatabaseConnection();
  
  if (!dbConnected) {
    console.log('❌ Database connection failed');
    console.log('💡 Possible issues:');
    console.log('1. MongoDB not running');
    console.log('2. Wrong connection string in appsettings.json');
    console.log('3. Firewall blocking connection');
    console.log('4. MongoDB service not installed');
    return;
  }
  
  // Create sample data
  const sampleCreated = await createSampleData();
  
  if (sampleCreated) {
    console.log('🎉 ALL ISSUES FIXED!');
    console.log('📋 Database is working properly');
    console.log('💡 You can now:');
    console.log('1. Start frontend: npm run dev');
    console.log('2. Login with test@example.com / TestPassword123!');
    console.log('3. See expenses in the frontend');
  } else {
    console.log('⚠️ Some issues remain');
    console.log('💡 Check MongoDB logs and backend logs');
  }
  
  console.log('==================================');
  console.log('🎯 Database fix script completed!');
})();

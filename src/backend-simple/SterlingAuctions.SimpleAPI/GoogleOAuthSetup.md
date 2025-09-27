# Google OAuth Integration Setup

## Overview
The Sterling Auctions API now includes Google OAuth integration for external authentication. Users can sign in using their Google accounts, and the system will automatically create user accounts or link existing ones.

## Configuration

### 1. Google Cloud Console Setup
1. Go to the [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the Google+ API
4. Go to "Credentials" and create OAuth 2.0 Client IDs
5. Set the authorized redirect URI to: `https://localhost:5000/api/auth/google-callback`
6. Copy the Client ID and Client Secret

### 2. Application Configuration
Update the `appsettings.json` file with your Google OAuth credentials:

```json
{
  "GoogleOAuth": {
    "ClientId": "your-actual-google-client-id.apps.googleusercontent.com",
    "ClientSecret": "your-actual-google-client-secret",
    "RedirectUri": "https://localhost:5000/api/auth/google-callback"
  }
}
```

## API Endpoints

### Google OAuth Flow

#### 1. Initiate Google Login
```
GET /api/googleauth/google-login
```
This endpoint redirects users to Google's OAuth consent screen.

#### 2. Google OAuth Callback
```
GET /api/googleauth/google-callback
```
This endpoint handles the callback from Google after user authentication.

**Response on Success:**
```json
{
  "message": "Google authentication successful",
  "token": "jwt-token-here",
  "user": {
    "id": "user-id",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "roles": ["Member"]
  }
}
```

### Account Linking

#### Link Google Account to Existing User
```
POST /api/auth/link-google
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "googleToken": "google-access-token"
}
```

## Features

### Automatic User Creation
- When a user signs in with Google for the first time, a new account is automatically created
- User information (name, email) is populated from Google profile
- Default role "Member" is assigned
- Email is automatically verified

### User Linking
- Existing users can link their Google accounts
- This allows for multiple authentication methods

### JWT Token Generation
- After successful Google authentication, a JWT token is generated
- The token includes user information and roles
- Token expires in 60 minutes (configurable)

## Security Considerations

1. **HTTPS Required**: Google OAuth requires HTTPS in production
2. **Client Secret**: Keep the Google Client Secret secure
3. **Redirect URI**: Ensure redirect URIs match exactly
4. **Token Validation**: Always validate JWT tokens on protected endpoints

## Testing

### Test Google OAuth Flow
1. Start the application: `dotnet run --urls "http://localhost:5000"`
2. Navigate to: `http://localhost:5000/api/googleauth/google-login`
3. Complete Google authentication
4. You'll be redirected back with a JWT token

### Swagger Documentation
Visit `http://localhost:5000/swagger` to see all available endpoints including the new Google OAuth endpoints.

## Error Handling

The API handles various error scenarios:
- Invalid Google credentials
- Missing user information from Google
- User creation failures
- Account linking failures

All errors return appropriate HTTP status codes and error messages.

## Next Steps

1. Configure actual Google OAuth credentials
2. Set up HTTPS for production
3. Implement additional OAuth providers (Facebook, Microsoft, etc.)
4. Add user profile management features
5. Implement account unlinking functionality

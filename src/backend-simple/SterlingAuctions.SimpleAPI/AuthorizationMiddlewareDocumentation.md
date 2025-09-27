# Role-Based Authorization Middleware Documentation

## Overview
The Sterling Auctions API now includes a comprehensive role-based authorization middleware system that provides fine-grained access control for different user roles and auction-specific permissions.

## Components

### 1. RoleBasedAuthorizationMiddleware
A custom middleware that processes authorization requirements for endpoints marked with `[Authorize]` attributes.

**Features:**
- Checks user authentication status
- Validates user roles against endpoint requirements
- Provides detailed logging for authorization attempts
- Returns appropriate HTTP status codes (401, 403)

### 2. RoleBasedPolicyProvider
A custom authorization policy provider that handles dynamic role-based policies.

**Supported Policies:**
- `Role:Admin` - Requires Admin role
- `Role:Member` - Requires Member role
- `Roles:Admin,Member` - Requires Admin OR Member role
- `AdminOnly` - Admin-only access
- `MemberOrAdmin` - Member or Admin access
- `AuctionView` - Auction viewing permission
- `AuctionCreate` - Auction creation permission
- `AuctionBid` - Auction bidding permission
- `AuctionManage` - Auction management permission

### 3. AuctionAuthorizationHandler
A custom authorization handler for auction-specific permissions.

**Auction Permissions:**
- `View` - All authenticated users can view auctions
- `Create` - Members and Admins can create auctions
- `Bid` - Members and Admins can place bids
- `Manage` - Only Admins can manage auctions

### 4. Custom Authorization Attributes
Easy-to-use attributes for controller methods:

- `[RequireRole("Admin", "Member")]` - Requires any of the specified roles
- `[RequireRole(true, "Admin", "Member")]` - Requires ALL specified roles
- `[RequireAuctionPermission(AuctionPermission.View)]` - Requires specific auction permission
- `[AdminOnly]` - Admin-only access
- `[MemberOrAdmin]` - Member or Admin access

## User Roles

### Admin
- Full system access
- Can manage all auctions
- Can view system statistics
- Can perform administrative tasks

### Member
- Can view auctions
- Can create auctions
- Can place bids
- Can view their own auction history

## API Endpoints

### Test Endpoints
- `GET /api/test/public` - Public access (no authentication required)
- `GET /api/test/protected` - Requires authentication only
- `GET /api/test/admin-only` - Admin role required
- `GET /api/test/member-or-admin` - Member or Admin role required
- `GET /api/test/custom-roles` - Custom role requirements
- `GET /api/test/multiple-roles` - Multiple role requirements
- `GET /api/test/policy-test` - Policy-based authorization
- `GET /api/test/auction-permissions` - Auction permission testing

### Auction Endpoints
- `GET /api/auction` - View all auctions (authentication required)
- `GET /api/auction/{id}` - View specific auction (authentication required)
- `POST /api/auction` - Create auction (Member or Admin required)
- `POST /api/auction/{id}/bid` - Place bid (Member or Admin required)
- `PUT /api/auction/{id}` - Update auction (Admin only)
- `DELETE /api/auction/{id}` - Delete auction (Admin only)
- `GET /api/auction/statistics` - View statistics (Admin only)
- `GET /api/auction/my-auctions` - View user's auctions (Member or Admin)

## Configuration

### Program.cs Setup
```csharp
// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin"));

    options.AddPolicy("MemberOrAdmin", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Member", "Admin"));

    // Auction-specific policies
    options.AddPolicy("AuctionView", policy =>
        policy.RequireAuthenticatedUser());

    options.AddPolicy("AuctionCreate", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Member", "Admin"));

    options.AddPolicy("AuctionBid", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Member", "Admin"));

    options.AddPolicy("AuctionManage", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin"));
});

// Register custom providers and handlers
builder.Services.AddSingleton<IAuthorizationPolicyProvider, RoleBasedPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, AuctionAuthorizationHandler>();
```

### Middleware Pipeline
```csharp
app.UseAuthentication();
app.UseMiddleware<RoleBasedAuthorizationMiddleware>();
app.UseAuthorization();
```

## Usage Examples

### Controller Method Authorization
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class AuctionController : ControllerBase
{
    [HttpGet]
    [RequireAuctionPermission(AuctionPermission.View)]
    public IActionResult GetAuctions() { }

    [HttpPost]
    [RequireAuctionPermission(AuctionPermission.Create)]
    public IActionResult CreateAuction() { }

    [HttpPut("{id}")]
    [RequireAuctionPermission(AuctionPermission.Manage)]
    public IActionResult UpdateAuction(int id) { }

    [HttpGet("statistics")]
    [AdminOnly]
    public IActionResult GetStatistics() { }
}
```

### Policy-Based Authorization
```csharp
[HttpGet("admin-data")]
[Authorize(Policy = "AdminOnly")]
public IActionResult GetAdminData() { }

[HttpGet("member-data")]
[Authorize(Policy = "MemberOrAdmin")]
public IActionResult GetMemberData() { }
```

## Security Features

### Authentication Validation
- All protected endpoints require valid JWT tokens
- Token expiration is enforced
- Invalid tokens result in 401 Unauthorized

### Role Validation
- User roles are validated against endpoint requirements
- Missing required roles result in 403 Forbidden
- Role inheritance (Admin has all Member permissions)

### Logging
- All authorization attempts are logged
- Failed authorization attempts include user ID and required roles
- Successful authorization includes user context

### Error Handling
- Consistent error responses across all endpoints
- Detailed error messages for debugging
- Proper HTTP status codes

## Testing

### Public Endpoints
```bash
curl -X GET "http://localhost:5000/api/test/public"
# Returns: {"message":"This is a public endpoint - no authentication required"}
```

### Protected Endpoints (No Token)
```bash
curl -X GET "http://localhost:5000/api/test/protected"
# Returns: 401 Unauthorized
```

### Admin Endpoints (No Token)
```bash
curl -X GET "http://localhost:5000/api/test/admin-only"
# Returns: 401 Unauthorized
```

### With Valid JWT Token
```bash
# First, get a token by logging in
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@sterling-auctions.com","password":"Admin123!"}'

# Then use the token
curl -X GET "http://localhost:5000/api/test/protected" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Best Practices

### 1. Principle of Least Privilege
- Grant minimum required permissions
- Use specific roles rather than broad access

### 2. Consistent Authorization
- Apply authorization at controller level
- Use consistent attribute patterns
- Document authorization requirements

### 3. Security Logging
- Log all authorization attempts
- Monitor failed authorization attempts
- Track privilege escalation attempts

### 4. Error Handling
- Don't expose sensitive information in error messages
- Use consistent error response formats
- Log security-related errors

## Future Enhancements

### 1. Resource-Based Authorization
- Implement auction ownership checks
- Add user-specific data access controls
- Implement auction participant validation

### 2. Advanced Permissions
- Time-based permissions
- Location-based restrictions
- Feature flag integration

### 3. Audit Trail
- Comprehensive authorization audit log
- User activity tracking
- Security event monitoring

### 4. Performance Optimization
- Role caching
- Permission pre-computation
- Database query optimization

## Troubleshooting

### Common Issues

1. **401 Unauthorized**
   - Check JWT token validity
   - Verify token expiration
   - Ensure proper Authorization header format

2. **403 Forbidden**
   - Verify user has required roles
   - Check role assignment in database
   - Validate permission requirements

3. **Middleware Not Working**
   - Ensure middleware is registered in correct order
   - Check Program.cs configuration
   - Verify attribute usage

### Debug Information
- Check application logs for authorization details
- Use Swagger UI to test endpoints
- Verify user roles in database
- Test with different user accounts

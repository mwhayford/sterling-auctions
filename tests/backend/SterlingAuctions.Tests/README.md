# NUnit Testing Framework Setup

This document describes the NUnit testing framework setup for the Sterling Auctions backend.

## Project Structure

```
tests/backend/SterlingAuctions.Tests/
├── Models/
│   ├── PaymentModelTests.cs
│   ├── AuctionModelTests.cs
│   ├── ApplicationUserTests.cs
│   ├── BidModelTests.cs
│   ├── CategoryModelTests.cs
│   ├── AuctionImageModelTests.cs
│   └── WatchlistItemModelTests.cs
├── Services/
│   └── SimplePaymentServiceTests.cs
└── SterlingAuctions.Tests.csproj
```

## Test Categories

### Model Tests
- **PaymentModelTests**: Tests for Payment entity, PaymentStatus, and PaymentMethod enums
- **AuctionModelTests**: Tests for Auction entity and AuctionStatus enum
- **ApplicationUserTests**: Tests for ApplicationUser entity
- **BidModelTests**: Tests for Bid entity
- **CategoryModelTests**: Tests for Category entity
- **AuctionImageModelTests**: Tests for AuctionImage entity
- **WatchlistItemModelTests**: Tests for WatchlistItem entity

### Service Tests
- **SimplePaymentServiceTests**: Tests for payment service interface and models

## Test Framework

- **NUnit**: Primary testing framework
- **FluentAssertions**: Fluent assertion library for readable test assertions
- **Moq**: Mocking framework for dependencies
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database for testing
- **Microsoft.AspNetCore.Mvc.Testing**: Integration testing support

## Running Tests

```bash
# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "ClassName=PaymentModelTests"

# Run tests and generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

## Test Coverage

Current test coverage includes:
- ✅ Model validation and property testing
- ✅ Enum value verification
- ✅ Service interface contract testing
- ✅ Basic service method existence verification

## Future Enhancements

- Integration tests with in-memory database
- Controller action testing
- Service method behavior testing with mocked dependencies
- Performance testing
- End-to-end testing with Playwright

## Test Data

Tests use mock data and simple assertions to verify:
- Model instantiation
- Property assignment
- Enum values
- Interface contracts
- Method signatures

## Dependencies

The test project references:
- `SterlingAuctions.SimpleAPI` - Main API project
- `NUnit` - Testing framework
- `FluentAssertions` - Assertion library
- `Moq` - Mocking framework
- `Microsoft.EntityFrameworkCore.InMemory` - Database testing
- `Microsoft.AspNetCore.Mvc.Testing` - Integration testing
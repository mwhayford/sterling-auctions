# Sterling Auctions 🏆

A premium online auction platform built with modern web technologies, featuring real-time bidding, comprehensive testing, and scalable architecture.

## 🚀 Live Demo

- **Frontend**: [https://mwhayford.github.io/sterling-auctions](https://mwhayford.github.io/sterling-auctions)
- **API Documentation**: [Swagger UI](https://api.sterling-auctions.com/swagger)
- **Test Reports**: [E2E Test Results](https://github.com/mwhayford/sterling-auctions/actions)

## ✨ Features

### 🎯 Core Functionality
- **Real-time Bidding**: Live auction updates with SignalR WebSocket connections
- **User Authentication**: JWT-based auth with Google OAuth integration
- **Auction Management**: Create, manage, and participate in auctions
- **Payment Processing**: Stripe integration for secure transactions
- **Redis Caching**: High-performance caching for improved response times
- **Responsive Design**: Mobile-first design with Tailwind CSS

### 🔧 Technical Stack
- **Frontend**: React 18 + TypeScript + Tailwind CSS
- **Backend**: ASP.NET Core 9.0 + Entity Framework Core
- **Database**: PostgreSQL with Redis caching
- **Real-time**: SignalR for WebSocket communication
- **Testing**: NUnit (Backend) + Playwright (E2E)
- **CI/CD**: GitHub Actions with automated testing
- **Deployment**: Docker + Docker Compose

### 🧪 Testing Coverage
- **Backend Tests**: 15 NUnit tests covering models and services
- **E2E Tests**: 196 Playwright tests across 7 browser configurations
- **Cross-browser**: Chrome, Firefox, Safari, Edge support
- **Mobile Testing**: Mobile Chrome and Safari emulation
- **Real-time Testing**: SignalR connection and notification testing

## 🏗️ Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   React Frontend │    │  ASP.NET Core   │    │   PostgreSQL    │
│   (Port 3000)   │◄──►│   (Port 5000)   │◄──►│   Database      │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   SignalR Hub   │    │   Redis Cache   │    │   Docker        │
│   Real-time     │    │   Sessions      │    │   Containers   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 🚀 Quick Start

### Prerequisites
- Node.js 18+
- .NET 9.0 SDK
- Docker & Docker Compose
- Git

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/mwhayford/sterling-auctions.git
   cd sterling-auctions
   ```

2. **Start with Docker (Recommended)**
   ```bash
   cd docker
   docker-compose up -d
   ```
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:5000
   - Swagger UI: http://localhost:5000/swagger
   - Redis: localhost:6379

3. **Manual Setup**
   ```bash
   # Backend
   cd src/backend-simple/SterlingAuctions.SimpleAPI
   dotnet run
   
   # Frontend
   cd src/frontend
   npm install
   npm start
   ```

### Running Tests

```bash
# Backend Tests
cd tests/backend/SterlingAuctions.Tests
dotnet test

# E2E Tests
cd tests/e2e
npm install
npx playwright install
npm test
```

## 📁 Project Structure

```
sterling-auctions/
├── src/
│   ├── frontend/                 # React TypeScript frontend
│   │   ├── src/
│   │   │   ├── components/      # React components
│   │   │   ├── hooks/           # Custom React hooks
│   │   │   ├── services/        # API and SignalR services
│   │   │   └── utils/          # Utility functions
│   │   └── public/             # Static assets
│   └── backend-simple/         # ASP.NET Core API
│       └── SterlingAuctions.SimpleAPI/
│           ├── Controllers/     # API controllers
│           ├── Models/          # Domain models and DTOs
│           ├── Services/        # Business logic services
│           ├── Hubs/            # SignalR hubs
│           └── Data/            # Entity Framework context
├── tests/
│   ├── backend/                # NUnit backend tests
│   └── e2e/                    # Playwright E2E tests
├── docker/                     # Docker configuration
└── .github/workflows/          # CI/CD pipelines
```

## 🔧 Configuration

### Environment Variables

```bash
# Backend (.env)
JWT_SECRET=your-jwt-secret
REDIS_CONNECTION=redis:6379
STRIPE_SECRET_KEY=sk_test_...
GOOGLE_CLIENT_ID=your-google-client-id

# Frontend (.env)
REACT_APP_API_URL=http://localhost:5000
REACT_APP_SIGNALR_URL=http://localhost:5000
```

### Docker Configuration

```yaml
# docker-compose.yml
services:
  api:
    build: .
    ports:
      - "5000:80"
    environment:
      - REDIS_CONNECTION=redis:6379
  
  redis:
    image: redis:alpine
    ports:
      - "6379:6379"
```

## 🧪 Testing

### Test Coverage
- **Backend**: 15 NUnit tests (Models, Services, Interfaces)
- **E2E**: 196 Playwright tests (UI, Auth, Auctions, Real-time)
- **Browsers**: Chrome, Firefox, Safari, Edge
- **Mobile**: Mobile Chrome, Mobile Safari
- **CI/CD**: Automated testing on every push

### Running Tests

```bash
# All tests
npm run test:all

# Backend only
dotnet test tests/backend/

# E2E only
cd tests/e2e && npm test

# Specific browser
npx playwright test --project=chromium

# With UI
npx playwright test --ui
```

## 🚀 Deployment

### Docker Deployment
```bash
# Production deployment
cd docker
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### Manual Deployment
```bash
# Build frontend
cd src/frontend
npm run build

# Build backend
cd src/backend-simple/SterlingAuctions.SimpleAPI
dotnet publish -c Release
```

## 📊 CI/CD Pipeline

The project includes comprehensive GitHub Actions workflows:

- **E2E Tests**: Automated testing across multiple browsers
- **Backend Tests**: NUnit test execution
- **Docker Build**: Container image building
- **Security Scanning**: Dependency vulnerability checks
- **Performance Testing**: Load testing with Playwright

## 🔒 Security Features

- **JWT Authentication**: Secure token-based authentication
- **Google OAuth**: Social login integration
- **CORS Configuration**: Cross-origin request security
- **Input Validation**: Comprehensive form validation
- **SQL Injection Protection**: Entity Framework parameterized queries
- **XSS Protection**: React's built-in XSS protection

## 📈 Performance Features

- **Redis Caching**: High-performance caching layer
- **SignalR Optimization**: Efficient real-time communication
- **Database Optimization**: Indexed queries and connection pooling
- **Frontend Optimization**: Code splitting and lazy loading
- **CDN Ready**: Static asset optimization

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines
- Follow TypeScript best practices
- Write comprehensive tests
- Update documentation
- Follow conventional commit messages
- Ensure all tests pass

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **React Team** - For the amazing frontend framework
- **Microsoft** - For ASP.NET Core and SignalR
- **Playwright Team** - For comprehensive E2E testing
- **Tailwind CSS** - For the utility-first CSS framework
- **Redis** - For high-performance caching

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/mwhayford/sterling-auctions/issues)
- **Discussions**: [GitHub Discussions](https://github.com/mwhayford/sterling-auctions/discussions)
- **Documentation**: [Wiki](https://github.com/mwhayford/sterling-auctions/wiki)

---

**Built with ❤️ by [mwhayford](https://github.com/mwhayford)**

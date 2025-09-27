# Sterling Auctions - Frontend SignalR Client

This document describes the React-based SignalR client implementation for the Sterling Auctions application, providing real-time communication capabilities for live bidding, notifications, and auction management.

## 🚀 Features

### Real-Time Communication
- **Live Bidding**: Real-time bid updates and notifications
- **Auction Chat**: Real-time messaging between bidders
- **Notifications**: Instant alerts for auction events
- **Connection Management**: Automatic reconnection and error handling
- **Status Monitoring**: Live connection status indicators

### SignalR Hubs Integration
- **AuctionHub**: Real-time auction operations
- **NotificationHub**: System-wide notifications
- **WebSocket Connections**: Persistent real-time connections
- **Authentication**: JWT token-based authentication

## 📁 Project Structure

```
src/frontend/src/
├── services/
│   └── signalRService.ts          # Core SignalR service
├── hooks/
│   └── useSignalR.ts              # React hooks for SignalR
├── components/
│   ├── AuctionLiveView.tsx        # Live auction component
│   ├── NotificationCenter.tsx      # Notification management
│   └── ConnectionStatus.tsx       # Connection status display
└── App.tsx                        # Main app with SignalR integration
```

## 🔧 Installation

### Prerequisites
- Node.js 16+ 
- npm or yarn
- React 18+
- TypeScript 4.9+

### Dependencies
```bash
npm install @microsoft/signalr --legacy-peer-deps
```

## 📖 Usage

### 1. SignalR Service

The `SignalRService` class manages connections to both SignalR hubs:

```typescript
import { signalRService } from './services/signalRService';

// Initialize connections
await signalRService.initialize();

// Join an auction room
await signalRService.joinAuction(auctionId);

// Place a bid
await signalRService.placeBid(auctionId, amount);

// Listen for events
signalRService.on('bidPlaced', (data) => {
  console.log('New bid:', data);
});
```

### 2. React Hooks

#### Basic SignalR Hook
```typescript
import { useSignalR } from './hooks/useSignalR';

function MyComponent() {
  const { connectionStatus, initialize, disconnect } = useSignalR({
    autoConnect: true,
    accessToken: 'your-jwt-token',
    onConnected: () => console.log('Connected!'),
    onError: (error) => console.error('Error:', error)
  });

  return (
    <div>
      Status: {connectionStatus.isConnected ? 'Connected' : 'Disconnected'}
    </div>
  );
}
```

#### Auction-Specific Hook
```typescript
import { useAuctionSignalR } from './hooks/useSignalR';

function AuctionComponent({ auctionId }) {
  const {
    isJoined,
    currentBid,
    bidHistory,
    placeBid,
    joinAuction
  } = useAuctionSignalR(auctionId);

  return (
    <div>
      <p>Current Bid: ${currentBid}</p>
      <button onClick={() => placeBid(100)}>Place Bid</button>
    </div>
  );
}
```

#### Notification Hook
```typescript
import { useNotificationSignalR } from './hooks/useSignalR';

function NotificationComponent() {
  const {
    notifications,
    systemAnnouncements,
    subscribeToAuction
  } = useNotificationSignalR();

  return (
    <div>
      {notifications.map(notification => (
        <div key={notification.id}>{notification.message}</div>
      ))}
    </div>
  );
}
```

### 3. Components

#### AuctionLiveView Component
```typescript
import { AuctionLiveView } from './components/AuctionLiveView';

<AuctionLiveView
  auctionId={1}
  auctionTitle="Vintage Rolex"
  currentBid={1250}
  endTime="2025-01-01T12:00:00Z"
  onBidPlaced={(amount) => console.log('Bid placed:', amount)}
  onError={(error) => console.error('Error:', error)}
/>
```

#### NotificationCenter Component
```typescript
import { NotificationCenter } from './components/NotificationCenter';

<NotificationCenter
  onNotificationClick={(notification) => {
    console.log('Notification clicked:', notification);
  }}
  maxNotifications={10}
/>
```

#### ConnectionStatus Component
```typescript
import { ConnectionStatus, StatusBadge } from './components/ConnectionStatus';

// Simple status badge
<StatusBadge />

// Detailed status panel
<ConnectionStatus showDetails={true} />
```

## 🔌 SignalR Hub Methods

### AuctionHub Methods
- `joinAuction(auctionId)` - Join auction room
- `leaveAuction(auctionId)` - Leave auction room
- `placeBid(auctionId, amount)` - Place a bid
- `joinUserNotifications()` - Join personal notifications
- `sendAuctionMessage(auctionId, message)` - Send chat message
- `getAuctionStats()` - Get auction statistics

### NotificationHub Methods
- `joinGeneralNotifications()` - Join general notifications
- `joinAdminNotifications()` - Join admin notifications
- `subscribeToAuction(auctionId)` - Subscribe to auction updates
- `subscribeToCategory(categoryId)` - Subscribe to category updates
- `subscribeToEndingSoon()` - Subscribe to ending soon alerts

## 📡 SignalR Events

### Auction Events
- `BidPlaced` - New bid placed
- `AuctionBidUpdate` - Bid amount updated
- `AuctionJoined` - User joined auction
- `UserJoinedAuction` - Another user joined
- `AuctionStarting` - Auction is starting
- `AuctionEnded` - Auction has ended
- `AuctionUpdated` - Auction details updated
- `AuctionCancelled` - Auction cancelled
- `AuctionEndingSoon` - Auction ending soon
- `AuctionMessage` - Chat message received
- `AuctionStats` - Auction statistics updated
- `BidFailed` - Bid placement failed

### Notification Events
- `SystemAnnouncement` - System-wide announcements
- `AdminAlert` - Admin-only alerts
- `AuctionNotification` - Auction-specific notifications
- `NewAuctionInCategory` - New auction in category
- `AuctionEndingSoon` - Auction ending soon notification
- `AuctionWon` - User won auction
- `AuctionLost` - User lost auction
- `TestMessage` - Test message received

## ⚙️ Configuration

### Environment Variables
```env
REACT_APP_API_URL=http://localhost:5000
REACT_APP_SIGNALR_HUB_URL=/auctionHub
REACT_APP_NOTIFICATION_HUB_URL=/notificationHub
```

### SignalR Service Configuration
```typescript
const config: SignalRConfig = {
  baseUrl: process.env.REACT_APP_API_URL || 'http://localhost:5000',
  auctionHubUrl: '/auctionHub',
  notificationHubUrl: '/notificationHub',
  accessToken: 'your-jwt-token'
};
```

## 🔒 Authentication

The SignalR client supports JWT authentication:

```typescript
// Update access token
signalRService.updateAccessToken('new-jwt-token');

// Or pass token during initialization
const { connectionStatus } = useSignalR({
  accessToken: 'your-jwt-token'
});
```

## 🔄 Connection Management

### Automatic Reconnection
The SignalR service includes automatic reconnection with exponential backoff:

```typescript
// Reconnection settings
.withAutomaticReconnect({
  nextRetryDelayInMilliseconds: (retryContext) => {
    if (retryContext.previousRetryCount < 3) {
      return 2000; // 2 seconds
    } else if (retryContext.previousRetryCount < 6) {
      return 10000; // 10 seconds
    } else {
      return 30000; // 30 seconds
    }
  }
})
```

### Manual Connection Control
```typescript
// Reconnect manually
await signalRService.reconnect();

// Disconnect
await signalRService.disconnect();

// Check connection status
const status = signalRService.getConnectionStatus();
const isConnected = signalRService.isConnected();
```

## 🎨 UI Components

### Real-Time Features Display
- **Live Status Indicators**: Green/red dots showing connection status
- **Bid History**: Real-time list of recent bids
- **Auction Chat**: Live messaging between bidders
- **Notification Toasts**: Pop-up notifications for important events
- **Connection Status Panel**: Detailed connection information

### Responsive Design
- **Mobile-First**: Optimized for mobile devices
- **Tailwind CSS**: Modern, responsive styling
- **Accessibility**: ARIA labels and keyboard navigation
- **Dark Mode Ready**: CSS variables for theme switching

## 🧪 Testing

### Manual Testing
1. Start the backend API with SignalR hubs
2. Run the React frontend: `npm start`
3. Open browser to `http://localhost:3000`
4. Click "Try Live Demo" to test SignalR features
5. Monitor browser console for SignalR events

### Test Scenarios
- **Connection**: Verify SignalR hubs connect successfully
- **Bidding**: Test real-time bid placement and updates
- **Notifications**: Test notification delivery and display
- **Reconnection**: Test automatic reconnection on network issues
- **Authentication**: Test JWT token authentication

## 🚨 Error Handling

### Connection Errors
```typescript
signalRService.on('error', (error) => {
  console.error('SignalR error:', error);
  // Handle error (show notification, retry, etc.)
});
```

### Bid Errors
```typescript
signalRService.on('bidFailed', (data) => {
  console.error('Bid failed:', data.message);
  // Show error message to user
});
```

### Network Issues
- Automatic reconnection with exponential backoff
- Connection status monitoring
- Error notifications to users
- Graceful degradation when offline

## 📊 Performance

### Optimization Features
- **Connection Pooling**: Efficient WebSocket connection management
- **Event Debouncing**: Prevents excessive UI updates
- **Memory Management**: Automatic cleanup of event listeners
- **Lazy Loading**: Components load SignalR only when needed

### Monitoring
- Connection status tracking
- Event frequency monitoring
- Error rate tracking
- Performance metrics collection

## 🔧 Development

### Local Development
```bash
# Install dependencies
npm install --legacy-peer-deps

# Start development server
npm start

# Build for production
npm run build
```

### Debugging
- Enable SignalR logging: `signalR.LogLevel.Debug`
- Monitor browser Network tab for WebSocket connections
- Check browser Console for SignalR events
- Use React DevTools for component state

## 📚 API Reference

### SignalRService Class
- `initialize()`: Initialize SignalR connections
- `disconnect()`: Disconnect all hubs
- `reconnect()`: Reconnect all hubs
- `updateAccessToken(token)`: Update JWT token
- `getConnectionStatus()`: Get connection status
- `isConnected()`: Check if all hubs are connected

### React Hooks
- `useSignalR(options)`: Basic SignalR hook
- `useAuctionSignalR(auctionId)`: Auction-specific hook
- `useNotificationSignalR()`: Notification-specific hook

### Components
- `AuctionLiveView`: Live auction interface
- `NotificationCenter`: Notification management
- `ConnectionStatus`: Connection status display
- `NotificationToast`: Pop-up notifications

## 🎯 Next Steps

### Planned Features
1. **Push Notifications**: Browser push notification support
2. **Offline Support**: Service worker for offline functionality
3. **Voice Notifications**: Audio alerts for important events
4. **Video Streaming**: Live video feeds for auctions
5. **Mobile App**: React Native mobile application

### Integration Opportunities
1. **Payment Processing**: Real-time payment status updates
2. **Inventory Management**: Live inventory updates
3. **User Analytics**: Real-time user behavior tracking
4. **A/B Testing**: Real-time feature flag updates

---

## 📞 Support

For questions or issues with the SignalR client implementation:

1. Check browser console for error messages
2. Verify SignalR hub endpoints are accessible
3. Ensure JWT authentication is working
4. Test network connectivity and firewall settings
5. Review SignalR server logs for connection issues

The SignalR client provides a robust foundation for real-time auction functionality with comprehensive error handling, automatic reconnection, and modern React patterns.

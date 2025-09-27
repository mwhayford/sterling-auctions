import * as signalR from '@microsoft/signalr';
import { HubConnection, HubConnectionState } from '@microsoft/signalr';

export interface SignalRConfig {
  baseUrl: string;
  auctionHubUrl: string;
  notificationHubUrl: string;
  accessToken?: string;
}

export interface AuctionBidUpdate {
  auctionId: number;
  newBid: number;
  bidderName: string;
  timestamp: string;
}

export interface AuctionNotification {
  type: string;
  auctionId: number;
  message: string;
  data?: any;
  timestamp: string;
}

export interface SystemAnnouncement {
  message: string;
  timestamp: string;
  type: string;
}

export interface AdminAlert {
  message: string;
  data?: any;
  timestamp: string;
  type: string;
}

export interface AuctionJoinedData {
  auctionId: number;
  userId: string;
  timestamp: string;
}

export interface BidPlacedData {
  auctionId: number;
  bid: any;
  newCurrentBid: number;
  bidderId: string;
  timestamp: string;
}

export interface AuctionEndingSoonData {
  auctionId: number;
  timeRemaining: string;
  minutesRemaining: number;
  timestamp: string;
}

export class SignalRService {
  private auctionHub: HubConnection | null = null;
  private notificationHub: HubConnection | null = null;
  private config: SignalRConfig;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private reconnectDelay = 1000;

  constructor(config: SignalRConfig) {
    this.config = config;
  }

  // Initialize SignalR connections
  async initialize(): Promise<void> {
    try {
      await this.createAuctionHubConnection();
      await this.createNotificationHubConnection();
      console.log('SignalR connections initialized successfully');
    } catch (error) {
      console.error('Failed to initialize SignalR connections:', error);
      throw error;
    }
  }

  // Create auction hub connection
  private async createAuctionHubConnection(): Promise<void> {
    const url = `${this.config.baseUrl}${this.config.auctionHubUrl}`;
    
    this.auctionHub = new signalR.HubConnectionBuilder()
      .withUrl(url, {
        accessTokenFactory: () => this.config.accessToken || '',
        transport: signalR.HttpTransportType.WebSockets,
        skipNegotiation: false
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          if (retryContext.previousRetryCount < 3) {
            return 2000;
          } else if (retryContext.previousRetryCount < 6) {
            return 10000;
          } else {
            return 30000;
          }
        }
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Set up event handlers
    this.setupAuctionHubEventHandlers();

    // Start connection
    await this.auctionHub.start();
    console.log('Auction Hub connected');
  }

  // Create notification hub connection
  private async createNotificationHubConnection(): Promise<void> {
    const url = `${this.config.baseUrl}${this.config.notificationHubUrl}`;
    
    this.notificationHub = new signalR.HubConnectionBuilder()
      .withUrl(url, {
        accessTokenFactory: () => this.config.accessToken || '',
        transport: signalR.HttpTransportType.WebSockets,
        skipNegotiation: false
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          if (retryContext.previousRetryCount < 3) {
            return 2000;
          } else if (retryContext.previousRetryCount < 6) {
            return 10000;
          } else {
            return 30000;
          }
        }
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Set up event handlers
    this.setupNotificationHubEventHandlers();

    // Start connection
    await this.notificationHub.start();
    console.log('Notification Hub connected');
  }

  // Setup auction hub event handlers
  private setupAuctionHubEventHandlers(): void {
    if (!this.auctionHub) return;

    // Connection events
    this.auctionHub.onclose((error) => {
      console.log('Auction Hub connection closed:', error);
      this.handleReconnection('auction');
    });

    this.auctionHub.onreconnecting((error) => {
      console.log('Auction Hub reconnecting:', error);
    });

    this.auctionHub.onreconnected((connectionId) => {
      console.log('Auction Hub reconnected:', connectionId);
      this.reconnectAttempts = 0;
    });

    // Auction events
    this.auctionHub.on('BidPlaced', (data: BidPlacedData) => {
      console.log('Bid placed:', data);
      this.emitEvent('bidPlaced', data);
    });

    this.auctionHub.on('AuctionBidUpdate', (data: AuctionBidUpdate) => {
      console.log('Auction bid update:', data);
      this.emitEvent('auctionBidUpdate', data);
    });

    this.auctionHub.on('AuctionJoined', (data: any) => {
      console.log('Auction joined:', data);
      this.emitEvent('auctionJoined', data);
    });

    this.auctionHub.on('UserJoinedAuction', (data: AuctionJoinedData) => {
      console.log('User joined auction:', data);
      this.emitEvent('userJoinedAuction', data);
    });

    this.auctionHub.on('UserLeftAuction', (data: AuctionJoinedData) => {
      console.log('User left auction:', data);
      this.emitEvent('userLeftAuction', data);
    });

    this.auctionHub.on('AuctionStarting', (data: any) => {
      console.log('Auction starting:', data);
      this.emitEvent('auctionStarting', data);
    });

    this.auctionHub.on('AuctionEnded', (data: any) => {
      console.log('Auction ended:', data);
      this.emitEvent('auctionEnded', data);
    });

    this.auctionHub.on('AuctionUpdated', (data: any) => {
      console.log('Auction updated:', data);
      this.emitEvent('auctionUpdated', data);
    });

    this.auctionHub.on('AuctionCancelled', (data: any) => {
      console.log('Auction cancelled:', data);
      this.emitEvent('auctionCancelled', data);
    });

    this.auctionHub.on('AuctionEndingSoon', (data: AuctionEndingSoonData) => {
      console.log('Auction ending soon:', data);
      this.emitEvent('auctionEndingSoon', data);
    });

    this.auctionHub.on('AuctionMessage', (data: any) => {
      console.log('Auction message:', data);
      this.emitEvent('auctionMessage', data);
    });

    this.auctionHub.on('AuctionStats', (data: any) => {
      console.log('Auction stats:', data);
      this.emitEvent('auctionStats', data);
    });

    this.auctionHub.on('BidFailed', (data: any) => {
      console.log('Bid failed:', data);
      this.emitEvent('bidFailed', data);
    });

    this.auctionHub.on('Connected', (data: any) => {
      console.log('Auction Hub connected:', data);
      this.emitEvent('connected', data);
    });

    this.auctionHub.on('Error', (error: any) => {
      console.error('Auction Hub error:', error);
      this.emitEvent('error', error);
    });
  }

  // Setup notification hub event handlers
  private setupNotificationHubEventHandlers(): void {
    if (!this.notificationHub) return;

    // Connection events
    this.notificationHub.onclose((error) => {
      console.log('Notification Hub connection closed:', error);
      this.handleReconnection('notification');
    });

    this.notificationHub.onreconnecting((error) => {
      console.log('Notification Hub reconnecting:', error);
    });

    this.notificationHub.onreconnected((connectionId) => {
      console.log('Notification Hub reconnected:', connectionId);
      this.reconnectAttempts = 0;
    });

    // Notification events
    this.notificationHub.on('SystemAnnouncement', (data: SystemAnnouncement) => {
      console.log('System announcement:', data);
      this.emitEvent('systemAnnouncement', data);
    });

    this.notificationHub.on('AdminAlert', (data: AdminAlert) => {
      console.log('Admin alert:', data);
      this.emitEvent('adminAlert', data);
    });

    this.notificationHub.on('AuctionNotification', (data: AuctionNotification) => {
      console.log('Auction notification:', data);
      this.emitEvent('auctionNotification', data);
    });

    this.notificationHub.on('NewAuctionInCategory', (data: any) => {
      console.log('New auction in category:', data);
      this.emitEvent('newAuctionInCategory', data);
    });

    this.notificationHub.on('AuctionEndingSoon', (data: AuctionEndingSoonData) => {
      console.log('Auction ending soon notification:', data);
      this.emitEvent('auctionEndingSoonNotification', data);
    });

    this.notificationHub.on('AuctionWon', (data: any) => {
      console.log('Auction won:', data);
      this.emitEvent('auctionWon', data);
    });

    this.notificationHub.on('AuctionLost', (data: any) => {
      console.log('Auction lost:', data);
      this.emitEvent('auctionLost', data);
    });

    this.notificationHub.on('TestMessage', (data: any) => {
      console.log('Test message:', data);
      this.emitEvent('testMessage', data);
    });

    this.notificationHub.on('Connected', (data: any) => {
      console.log('Notification Hub connected:', data);
      this.emitEvent('notificationConnected', data);
    });

    this.notificationHub.on('Error', (error: any) => {
      console.error('Notification Hub error:', error);
      this.emitEvent('notificationError', error);
    });
  }

  // Event emitter functionality
  private eventListeners: { [key: string]: Function[] } = {};

  on(event: string, callback: Function): void {
    if (!this.eventListeners[event]) {
      this.eventListeners[event] = [];
    }
    this.eventListeners[event].push(callback);
  }

  off(event: string, callback: Function): void {
    if (this.eventListeners[event]) {
      this.eventListeners[event] = this.eventListeners[event].filter(cb => cb !== callback);
    }
  }

  private emitEvent(event: string, data: any): void {
    if (this.eventListeners[event]) {
      this.eventListeners[event].forEach(callback => callback(data));
    }
  }

  // Auction Hub Methods
  async joinAuction(auctionId: number): Promise<void> {
    if (this.auctionHub?.state === HubConnectionState.Connected) {
      await this.auctionHub.invoke('JoinAuction', auctionId);
    } else {
      throw new Error('Auction Hub not connected');
    }
  }

  async leaveAuction(auctionId: number): Promise<void> {
    if (this.auctionHub?.state === HubConnectionState.Connected) {
      await this.auctionHub.invoke('LeaveAuction', auctionId);
    } else {
      throw new Error('Auction Hub not connected');
    }
  }

  async placeBid(auctionId: number, amount: number): Promise<void> {
    if (this.auctionHub?.state === HubConnectionState.Connected) {
      await this.auctionHub.invoke('PlaceBid', auctionId, amount);
    } else {
      throw new Error('Auction Hub not connected');
    }
  }

  async joinUserNotifications(): Promise<void> {
    if (this.auctionHub?.state === HubConnectionState.Connected) {
      await this.auctionHub.invoke('JoinUserNotifications');
    } else {
      throw new Error('Auction Hub not connected');
    }
  }

  async sendAuctionMessage(auctionId: number, message: string): Promise<void> {
    if (this.auctionHub?.state === HubConnectionState.Connected) {
      await this.auctionHub.invoke('SendAuctionMessage', auctionId, message);
    } else {
      throw new Error('Auction Hub not connected');
    }
  }

  async getAuctionStats(): Promise<void> {
    if (this.auctionHub?.state === HubConnectionState.Connected) {
      await this.auctionHub.invoke('GetAuctionStats');
    } else {
      throw new Error('Auction Hub not connected');
    }
  }

  // Notification Hub Methods
  async joinGeneralNotifications(): Promise<void> {
    if (this.notificationHub?.state === HubConnectionState.Connected) {
      await this.notificationHub.invoke('JoinGeneralNotifications');
    } else {
      throw new Error('Notification Hub not connected');
    }
  }

  async joinAdminNotifications(): Promise<void> {
    if (this.notificationHub?.state === HubConnectionState.Connected) {
      await this.notificationHub.invoke('JoinAdminNotifications');
    } else {
      throw new Error('Notification Hub not connected');
    }
  }

  async subscribeToAuction(auctionId: number): Promise<void> {
    if (this.notificationHub?.state === HubConnectionState.Connected) {
      await this.notificationHub.invoke('SubscribeToAuction', auctionId);
    } else {
      throw new Error('Notification Hub not connected');
    }
  }

  async subscribeToCategory(categoryId: number): Promise<void> {
    if (this.notificationHub?.state === HubConnectionState.Connected) {
      await this.notificationHub.invoke('SubscribeToCategory', categoryId);
    } else {
      throw new Error('Notification Hub not connected');
    }
  }

  async subscribeToEndingSoon(): Promise<void> {
    if (this.notificationHub?.state === HubConnectionState.Connected) {
      await this.notificationHub.invoke('SubscribeToEndingSoon');
    } else {
      throw new Error('Notification Hub not connected');
    }
  }

  // Connection management
  async disconnect(): Promise<void> {
    if (this.auctionHub) {
      await this.auctionHub.stop();
      this.auctionHub = null;
    }
    if (this.notificationHub) {
      await this.notificationHub.stop();
      this.notificationHub = null;
    }
    console.log('SignalR connections disconnected');
  }

  async reconnect(): Promise<void> {
    await this.disconnect();
    await this.initialize();
  }

  private async handleReconnection(hubType: 'auction' | 'notification'): Promise<void> {
    if (this.reconnectAttempts < this.maxReconnectAttempts) {
      this.reconnectAttempts++;
      console.log(`Attempting to reconnect ${hubType} hub (attempt ${this.reconnectAttempts})`);
      
      setTimeout(async () => {
        try {
          if (hubType === 'auction' && this.auctionHub) {
            await this.auctionHub.start();
          } else if (hubType === 'notification' && this.notificationHub) {
            await this.notificationHub.start();
          }
          this.reconnectAttempts = 0;
        } catch (error) {
          console.error(`Failed to reconnect ${hubType} hub:`, error);
          this.handleReconnection(hubType);
        }
      }, this.reconnectDelay * this.reconnectAttempts);
    } else {
      console.error(`Max reconnection attempts reached for ${hubType} hub`);
      this.emitEvent('maxReconnectAttemptsReached', { hubType });
    }
  }

  // Update access token
  updateAccessToken(token: string): void {
    this.config.accessToken = token;
    // Note: In a real implementation, you might need to reconnect with the new token
  }

  // Get connection status
  getConnectionStatus(): { auction: HubConnectionState; notification: HubConnectionState } {
    return {
      auction: this.auctionHub?.state || HubConnectionState.Disconnected,
      notification: this.notificationHub?.state || HubConnectionState.Disconnected
    };
  }

  // Check if both hubs are connected
  isConnected(): boolean {
    return (
      this.auctionHub?.state === HubConnectionState.Connected &&
      this.notificationHub?.state === HubConnectionState.Connected
    );
  }
}

// Export singleton instance
export const signalRService = new SignalRService({
  baseUrl: process.env.REACT_APP_API_URL || 'http://localhost:5000',
  auctionHubUrl: '/auctionHub',
  notificationHubUrl: '/notificationHub'
});

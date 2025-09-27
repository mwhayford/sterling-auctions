// WebSocket Fallback Service for Sterling Auctions
export interface ConnectionConfig {
  url: string;
  authToken?: string;
  reconnectInterval?: number;
  maxReconnectAttempts?: number;
  fallbackTransports?: TransportType[];
  heartbeatInterval?: number;
  connectionTimeout?: number;
}

export enum TransportType {
  WebSockets = 'WebSockets',
  ServerSentEvents = 'ServerSentEvents',
  LongPolling = 'LongPolling',
  Polling = 'Polling'
}

export enum ConnectionState {
  Disconnected = 'Disconnected',
  Connecting = 'Connecting',
  Connected = 'Connected',
  Reconnecting = 'Reconnecting',
  Failed = 'Failed'
}

export interface ConnectionStatus {
  state: ConnectionState;
  transport: TransportType | null;
  reconnectAttempts: number;
  lastConnectedAt: Date | null;
  lastError: string | null;
  latency: number | null;
}

export interface FallbackOptions {
  enableWebSockets: boolean;
  enableServerSentEvents: boolean;
  enableLongPolling: boolean;
  enablePolling: boolean;
  autoReconnect: boolean;
  maxReconnectAttempts: number;
  reconnectInterval: number;
  heartbeatInterval: number;
  connectionTimeout: number;
  fallbackDelay: number;
}

export interface MessageHandler {
  (data: any): void;
}

export interface ConnectionEventHandler {
  (status: ConnectionStatus): void;
}

class WebSocketFallbackService {
  private config: ConnectionConfig;
  private options: FallbackOptions;
  private currentTransport: TransportType | null = null;
  private connectionState: ConnectionState = ConnectionState.Disconnected;
  private reconnectAttempts: number = 0;
  private lastConnectedAt: Date | null = null;
  private lastError: string | null = null;
  private latency: number | null = null;
  private reconnectTimer: NodeJS.Timeout | null = null;
  private heartbeatTimer: NodeJS.Timeout | null = null;
  private connectionTimeoutTimer: NodeJS.Timeout | null = null;
  private messageHandlers: Map<string, MessageHandler[]> = new Map();
  private connectionEventHandlers: ConnectionEventHandler[] = [];
  private connection: any = null;
  private isDestroyed: boolean = false;

  constructor(config: ConnectionConfig, options?: Partial<FallbackOptions>) {
    this.config = config;
    this.options = {
      enableWebSockets: true,
      enableServerSentEvents: true,
      enableLongPolling: true,
      enablePolling: true,
      autoReconnect: true,
      maxReconnectAttempts: 5,
      reconnectInterval: 5000,
      heartbeatInterval: 30000,
      connectionTimeout: 10000,
      fallbackDelay: 2000,
      ...options
    };
  }

  public async connect(): Promise<void> {
    if (this.isDestroyed) {
      throw new Error('Service has been destroyed');
    }

    this.setState(ConnectionState.Connecting);
    
    const transports = this.getAvailableTransports();
    
    for (const transport of transports) {
      try {
        await this.attemptConnection(transport);
        if (this.connectionState === ConnectionState.Connected) {
          this.currentTransport = transport;
          this.reconnectAttempts = 0;
          this.lastConnectedAt = new Date();
          this.lastError = null;
          this.startHeartbeat();
          return;
        }
      } catch (error) {
        console.warn(`Failed to connect using ${transport}:`, error);
        this.lastError = error instanceof Error ? error.message : 'Unknown error';
        
        // Wait before trying next transport
        if (transport !== transports[transports.length - 1]) {
          await this.delay(this.options.fallbackDelay);
        }
      }
    }

    // All transports failed
    this.setState(ConnectionState.Failed);
    this.handleReconnect();
  }

  public disconnect(): void {
    this.isDestroyed = true;
    this.clearTimers();
    this.setState(ConnectionState.Disconnected);
    
    if (this.connection) {
      try {
        if (typeof this.connection.close === 'function') {
          this.connection.close();
        } else if (typeof this.connection.disconnect === 'function') {
          this.connection.disconnect();
        }
      } catch (error) {
        console.warn('Error closing connection:', error);
      }
      this.connection = null;
    }
  }

  public send(message: string, data?: any): boolean {
    if (this.connectionState !== ConnectionState.Connected || !this.connection) {
      console.warn('Cannot send message: not connected');
      return false;
    }

    try {
      if (typeof this.connection.send === 'function') {
        this.connection.send(message, data);
        return true;
      } else if (typeof this.connection.invoke === 'function') {
        this.connection.invoke(message, data);
        return true;
      }
    } catch (error) {
      console.error('Error sending message:', error);
      this.handleConnectionError(error);
    }

    return false;
  }

  public subscribe(event: string, handler: MessageHandler): void {
    if (!this.messageHandlers.has(event)) {
      this.messageHandlers.set(event, []);
    }
    this.messageHandlers.get(event)!.push(handler);
  }

  public unsubscribe(event: string, handler?: MessageHandler): void {
    if (!this.messageHandlers.has(event)) return;

    if (handler) {
      const handlers = this.messageHandlers.get(event)!;
      const index = handlers.indexOf(handler);
      if (index > -1) {
        handlers.splice(index, 1);
      }
    } else {
      this.messageHandlers.delete(event);
    }
  }

  public onConnectionChange(handler: ConnectionEventHandler): void {
    this.connectionEventHandlers.push(handler);
  }

  public getStatus(): ConnectionStatus {
    return {
      state: this.connectionState,
      transport: this.currentTransport,
      reconnectAttempts: this.reconnectAttempts,
      lastConnectedAt: this.lastConnectedAt,
      lastError: this.lastError,
      latency: this.latency
    };
  }

  public isConnected(): boolean {
    return this.connectionState === ConnectionState.Connected;
  }

  public getCurrentTransport(): TransportType | null {
    return this.currentTransport;
  }

  private async attemptConnection(transport: TransportType): Promise<void> {
    return new Promise((resolve, reject) => {
      const timeout = setTimeout(() => {
        reject(new Error(`Connection timeout for ${transport}`));
      }, this.options.connectionTimeout);

      try {
        switch (transport) {
          case TransportType.WebSockets:
            this.connectWebSocket(resolve, reject, timeout);
            break;
          case TransportType.ServerSentEvents:
            this.connectServerSentEvents(resolve, reject, timeout);
            break;
          case TransportType.LongPolling:
            this.connectLongPolling(resolve, reject, timeout);
            break;
          case TransportType.Polling:
            this.connectPolling(resolve, reject, timeout);
            break;
          default:
            reject(new Error(`Unsupported transport: ${transport}`));
        }
      } catch (error) {
        clearTimeout(timeout);
        reject(error instanceof Error ? error : new Error(String(error)));
      }
    });
  }

  private connectWebSocket(resolve: () => void, reject: (error: Error) => void, timeout: NodeJS.Timeout): void {
    try {
      const wsUrl = this.config.url.replace(/^http/, 'ws');
      const ws = new WebSocket(wsUrl);

      ws.onopen = () => {
        clearTimeout(timeout);
        this.connection = ws;
        this.setState(ConnectionState.Connected);
        resolve();
      };

      ws.onmessage = (event) => {
        this.handleMessage(event.data);
      };

      ws.onclose = (event) => {
        this.handleConnectionError(new Error(`WebSocket closed: ${event.code} ${event.reason}`));
      };

      ws.onerror = (error) => {
        clearTimeout(timeout);
        reject(new Error('WebSocket connection failed'));
      };
    } catch (error) {
      clearTimeout(timeout);
      reject(error instanceof Error ? error : new Error(String(error)));
    }
  }

  private connectServerSentEvents(resolve: () => void, reject: (error: Error) => void, timeout: NodeJS.Timeout): void {
    try {
      const eventSource = new EventSource(this.config.url);

      eventSource.onopen = () => {
        clearTimeout(timeout);
        this.connection = eventSource;
        this.setState(ConnectionState.Connected);
        resolve();
      };

      eventSource.onmessage = (event) => {
        this.handleMessage(event.data);
      };

      eventSource.onerror = (error) => {
        clearTimeout(timeout);
        reject(new Error('Server-Sent Events connection failed'));
      };
    } catch (error) {
      clearTimeout(timeout);
      reject(error instanceof Error ? error : new Error(String(error)));
    }
  }

  private connectLongPolling(resolve: () => void, reject: (error: Error) => void, timeout: NodeJS.Timeout): void {
    // Simulate long polling with fetch
    this.startLongPolling(resolve, reject, timeout);
  }

  private connectPolling(resolve: () => void, reject: (error: Error) => void, timeout: NodeJS.Timeout): void {
    // Simulate polling with periodic fetch
    this.startPolling(resolve, reject, timeout);
  }

  private async startLongPolling(resolve: () => void, reject: (error: Error) => void, timeout: NodeJS.Timeout): Promise<void> {
    try {
      const response = await fetch(`${this.config.url}/poll`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...(this.config.authToken && { 'Authorization': `Bearer ${this.config.authToken}` })
        },
        body: JSON.stringify({ timeout: 30000 })
      });

      if (response.ok) {
        clearTimeout(timeout);
        this.connection = { type: 'longpolling' };
        this.setState(ConnectionState.Connected);
        resolve();
      } else {
        throw new Error(`Long polling failed: ${response.status}`);
      }
    } catch (error) {
      clearTimeout(timeout);
      reject(error instanceof Error ? error : new Error(String(error)));
    }
  }

  private async startPolling(resolve: () => void, reject: (error: Error) => void, timeout: NodeJS.Timeout): Promise<void> {
    try {
      const response = await fetch(`${this.config.url}/status`, {
        headers: {
          ...(this.config.authToken && { 'Authorization': `Bearer ${this.config.authToken}` })
        }
      });

      if (response.ok) {
        clearTimeout(timeout);
        this.connection = { type: 'polling' };
        this.setState(ConnectionState.Connected);
        this.startPollingLoop();
        resolve();
      } else {
        throw new Error(`Polling failed: ${response.status}`);
      }
    } catch (error) {
      clearTimeout(timeout);
      reject(error instanceof Error ? error : new Error(String(error)));
    }
  }

  private startPollingLoop(): void {
    if (this.connection?.type !== 'polling') return;

    const poll = async () => {
      if (this.connectionState !== ConnectionState.Connected) return;

      try {
        const response = await fetch(`${this.config.url}/messages`, {
          headers: {
            ...(this.config.authToken && { 'Authorization': `Bearer ${this.config.authToken}` })
          }
        });

        if (response.ok) {
          const data = await response.json();
          if (data.messages) {
            data.messages.forEach((message: any) => {
              this.handleMessage(message);
            });
          }
        } else {
          this.handleConnectionError(new Error(`Polling request failed: ${response.status}`));
        }
      } catch (error) {
        this.handleConnectionError(error);
      }

      // Schedule next poll
      setTimeout(poll, 5000);
    };

    poll();
  }

  private handleMessage(data: string | any): void {
    try {
      let messageData: any;
      
      if (typeof data === 'string') {
        messageData = JSON.parse(data);
      } else {
        messageData = data;
      }

      const event = messageData.type || messageData.event || 'message';
      const handlers = this.messageHandlers.get(event);
      
      if (handlers) {
        handlers.forEach(handler => {
          try {
            handler(messageData);
          } catch (error) {
            console.error('Error in message handler:', error);
          }
        });
      }
    } catch (error) {
      console.error('Error handling message:', error);
    }
  }

  private handleConnectionError(error: any): void {
    console.warn('Connection error:', error);
    this.lastError = error instanceof Error ? error.message : 'Connection error';
    this.setState(ConnectionState.Disconnected);
    this.clearTimers();
    
    if (this.connection) {
      try {
        if (typeof this.connection.close === 'function') {
          this.connection.close();
        }
      } catch (e) {
        // Ignore close errors
      }
      this.connection = null;
    }

    this.handleReconnect();
  }

  private handleReconnect(): void {
    if (!this.options.autoReconnect || this.isDestroyed) return;
    
    if (this.reconnectAttempts >= this.options.maxReconnectAttempts) {
      this.setState(ConnectionState.Failed);
      return;
    }

    this.reconnectAttempts++;
    this.setState(ConnectionState.Reconnecting);

    this.reconnectTimer = setTimeout(() => {
      if (!this.isDestroyed) {
        this.connect().catch(error => {
          console.error('Reconnection failed:', error);
        });
      }
    }, this.options.reconnectInterval);
  }

  private startHeartbeat(): void {
    this.heartbeatTimer = setInterval(() => {
      if (this.connectionState === ConnectionState.Connected) {
        this.sendHeartbeat();
      }
    }, this.options.heartbeatInterval);
  }

  private sendHeartbeat(): void {
    const startTime = Date.now();
    
    if (this.send('ping', { timestamp: startTime })) {
      // Simulate latency calculation
      setTimeout(() => {
        this.latency = Date.now() - startTime;
      }, 100);
    }
  }

  private setState(state: ConnectionState): void {
    if (this.connectionState !== state) {
      this.connectionState = state;
      this.notifyConnectionChange();
    }
  }

  private notifyConnectionChange(): void {
    const status = this.getStatus();
    this.connectionEventHandlers.forEach(handler => {
      try {
        handler(status);
      } catch (error) {
        console.error('Error in connection event handler:', error);
      }
    });
  }

  private getAvailableTransports(): TransportType[] {
    const transports: TransportType[] = [];

    if (this.options.enableWebSockets && this.isWebSocketSupported()) {
      transports.push(TransportType.WebSockets);
    }

    if (this.options.enableServerSentEvents && this.isServerSentEventsSupported()) {
      transports.push(TransportType.ServerSentEvents);
    }

    if (this.options.enableLongPolling) {
      transports.push(TransportType.LongPolling);
    }

    if (this.options.enablePolling) {
      transports.push(TransportType.Polling);
    }

    return transports;
  }

  private isWebSocketSupported(): boolean {
    return typeof WebSocket !== 'undefined';
  }

  private isServerSentEventsSupported(): boolean {
    return typeof EventSource !== 'undefined';
  }

  private clearTimers(): void {
    if (this.reconnectTimer) {
      clearTimeout(this.reconnectTimer);
      this.reconnectTimer = null;
    }

    if (this.heartbeatTimer) {
      clearInterval(this.heartbeatTimer);
      this.heartbeatTimer = null;
    }

    if (this.connectionTimeoutTimer) {
      clearTimeout(this.connectionTimeoutTimer);
      this.connectionTimeoutTimer = null;
    }
  }

  private delay(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }
}

export default WebSocketFallbackService;

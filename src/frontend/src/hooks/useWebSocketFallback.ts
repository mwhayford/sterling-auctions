import { useState, useEffect, useCallback, useRef } from 'react';
import WebSocketFallbackService, { 
  ConnectionConfig, 
  ConnectionStatus, 
  TransportType, 
  ConnectionState,
  FallbackOptions 
} from '../services/websocketFallbackService';

interface UseWebSocketFallbackReturn {
  // State
  connectionStatus: ConnectionStatus;
  isConnected: boolean;
  currentTransport: TransportType | null;
  error: string | null;
  latency: number | null;

  // Actions
  connect: () => Promise<void>;
  disconnect: () => void;
  send: (message: string, data?: any) => boolean;
  subscribe: (event: string, handler: (data: any) => void) => void;
  unsubscribe: (event: string, handler?: (data: any) => void) => void;
  reconnect: () => Promise<void>;
  switchTransport: (transport: TransportType) => Promise<void>;

  // Configuration
  updateConfig: (config: Partial<ConnectionConfig>) => void;
  updateOptions: (options: Partial<FallbackOptions>) => void;
}

export const useWebSocketFallback = (
  initialConfig: ConnectionConfig,
  initialOptions?: Partial<FallbackOptions>
): UseWebSocketFallbackReturn => {
  const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>({
    state: ConnectionState.Disconnected,
    transport: null,
    reconnectAttempts: 0,
    lastConnectedAt: null,
    lastError: null,
    latency: null
  });

  const [error, setError] = useState<string | null>(null);
  const serviceRef = useRef<WebSocketFallbackService | null>(null);
  const configRef = useRef<ConnectionConfig>(initialConfig);
  const optionsRef = useRef<FallbackOptions>({
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
    ...initialOptions
  });

  // Initialize service
  useEffect(() => {
    if (!serviceRef.current) {
      serviceRef.current = new WebSocketFallbackService(configRef.current, optionsRef.current);
      
      // Listen for connection status changes
      serviceRef.current.onConnectionChange((status) => {
        setConnectionStatus(status);
        setError(status.lastError);
      });
    }

    return () => {
      if (serviceRef.current) {
        serviceRef.current.disconnect();
        serviceRef.current = null;
      }
    };
  }, []);

  // Auto-connect on mount if configured
  useEffect(() => {
    if (optionsRef.current.autoReconnect && serviceRef.current) {
      connect();
    }
  }, []);

  const connect = useCallback(async (): Promise<void> => {
    if (!serviceRef.current) {
      throw new Error('Service not initialized');
    }

    try {
      setError(null);
      await serviceRef.current.connect();
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Connection failed';
      setError(errorMessage);
      throw err;
    }
  }, []);

  const disconnect = useCallback((): void => {
    if (serviceRef.current) {
      serviceRef.current.disconnect();
    }
  }, []);

  const send = useCallback((message: string, data?: any): boolean => {
    if (!serviceRef.current) {
      console.warn('Service not initialized');
      return false;
    }

    return serviceRef.current.send(message, data);
  }, []);

  const subscribe = useCallback((event: string, handler: (data: any) => void): void => {
    if (!serviceRef.current) {
      console.warn('Service not initialized');
      return;
    }

    serviceRef.current.subscribe(event, handler);
  }, []);

  const unsubscribe = useCallback((event: string, handler?: (data: any) => void): void => {
    if (!serviceRef.current) {
      console.warn('Service not initialized');
      return;
    }

    serviceRef.current.unsubscribe(event, handler);
  }, []);

  const reconnect = useCallback(async (): Promise<void> => {
    if (!serviceRef.current) {
      throw new Error('Service not initialized');
    }

    disconnect();
    await new Promise(resolve => setTimeout(resolve, 1000)); // Wait 1 second
    await connect();
  }, [connect, disconnect]);

  const switchTransport = useCallback(async (transport: TransportType): Promise<void> => {
    if (!serviceRef.current) {
      throw new Error('Service not initialized');
    }

    // Disconnect current connection
    disconnect();
    
    // Update options to only use the specified transport
    const newOptions = { ...optionsRef.current };
    newOptions.enableWebSockets = transport === TransportType.WebSockets;
    newOptions.enableServerSentEvents = transport === TransportType.ServerSentEvents;
    newOptions.enableLongPolling = transport === TransportType.LongPolling;
    newOptions.enablePolling = transport === TransportType.Polling;
    
    // Create new service with updated options
    serviceRef.current.disconnect();
    serviceRef.current = new WebSocketFallbackService(configRef.current, newOptions);
    
    // Listen for connection status changes
    serviceRef.current.onConnectionChange((status) => {
      setConnectionStatus(status);
      setError(status.lastError);
    });

    // Connect with new transport
    await connect();
  }, [connect, disconnect]);

  const updateConfig = useCallback((newConfig: Partial<ConnectionConfig>): void => {
    configRef.current = { ...configRef.current, ...newConfig };
    
    if (serviceRef.current) {
      // Recreate service with new config
      serviceRef.current.disconnect();
      serviceRef.current = new WebSocketFallbackService(configRef.current, optionsRef.current);
      
      serviceRef.current.onConnectionChange((status) => {
        setConnectionStatus(status);
        setError(status.lastError);
      });
    }
  }, []);

  const updateOptions = useCallback((newOptions: Partial<FallbackOptions>): void => {
    optionsRef.current = { ...optionsRef.current, ...newOptions };
    
    if (serviceRef.current) {
      // Recreate service with new options
      serviceRef.current.disconnect();
      serviceRef.current = new WebSocketFallbackService(configRef.current, optionsRef.current);
      
      serviceRef.current.onConnectionChange((status) => {
        setConnectionStatus(status);
        setError(status.lastError);
      });
    }
  }, []);

  return {
    connectionStatus,
    isConnected: connectionStatus.state === ConnectionState.Connected,
    currentTransport: connectionStatus.transport,
    error,
    latency: connectionStatus.latency,
    connect,
    disconnect,
    send,
    subscribe,
    unsubscribe,
    reconnect,
    switchTransport,
    updateConfig,
    updateOptions
  };
};

// Enhanced SignalR hook with fallback support
export const useSignalRWithFallback = (
  hubUrl: string,
  authToken?: string,
  options?: Partial<FallbackOptions>
) => {
  const config: ConnectionConfig = {
    url: hubUrl,
    authToken,
    reconnectInterval: 5000,
    maxReconnectAttempts: 5,
    heartbeatInterval: 30000,
    connectionTimeout: 10000
  };

  const {
    connectionStatus,
    isConnected,
    currentTransport,
    error,
    latency,
    connect,
    disconnect,
    send,
    subscribe,
    unsubscribe,
    reconnect,
    switchTransport
  } = useWebSocketFallback(config, options);

  // SignalR-specific methods
  const joinGroup = useCallback((groupName: string): boolean => {
    return send('JoinGroup', { groupName });
  }, [send]);

  const leaveGroup = useCallback((groupName: string): boolean => {
    return send('LeaveGroup', { groupName });
  }, [send]);

  const invoke = useCallback((methodName: string, ...args: any[]): boolean => {
    return send(methodName, args);
  }, [send]);

  const on = useCallback((eventName: string, handler: (data: any) => void): void => {
    subscribe(eventName, handler);
  }, [subscribe]);

  const off = useCallback((eventName: string, handler?: (data: any) => void): void => {
    unsubscribe(eventName, handler);
  }, [unsubscribe]);

  return {
    // Connection state
    connectionStatus,
    isConnected,
    currentTransport,
    error,
    latency,

    // Connection management
    connect,
    disconnect,
    reconnect,
    switchTransport,

    // SignalR methods
    joinGroup,
    leaveGroup,
    invoke,
    on,
    off,

    // Transport information
    getAvailableTransports: () => [
      TransportType.WebSockets,
      TransportType.ServerSentEvents,
      TransportType.LongPolling,
      TransportType.Polling
    ],
    isTransportSupported: (transport: TransportType) => {
      switch (transport) {
        case TransportType.WebSockets:
          return typeof WebSocket !== 'undefined';
        case TransportType.ServerSentEvents:
          return typeof EventSource !== 'undefined';
        case TransportType.LongPolling:
        case TransportType.Polling:
          return true;
        default:
          return false;
      }
    }
  };
};

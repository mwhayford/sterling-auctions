import { useEffect, useRef, useState, useCallback } from 'react';
import { HubConnectionState } from '@microsoft/signalr';
import { signalRService, SignalRService } from '../services/signalRService';

export interface SignalRConnectionStatus {
  auction: HubConnectionState;
  notification: HubConnectionState;
  isConnected: boolean;
  isConnecting: boolean;
  error: string | null;
}

export interface UseSignalROptions {
  autoConnect?: boolean;
  accessToken?: string;
  onConnected?: () => void;
  onDisconnected?: () => void;
  onError?: (error: string) => void;
}

export const useSignalR = (options: UseSignalROptions = {}) => {
  const {
    autoConnect = true,
    accessToken,
    onConnected,
    onDisconnected,
    onError
  } = options;

  const [connectionStatus, setConnectionStatus] = useState<SignalRConnectionStatus>({
    auction: HubConnectionState.Disconnected,
    notification: HubConnectionState.Disconnected,
    isConnected: false,
    isConnecting: false,
    error: null
  });

  const serviceRef = useRef<SignalRService>(signalRService);
  const isInitializedRef = useRef(false);

  // Update connection status
  const updateConnectionStatus = useCallback(() => {
    const status = serviceRef.current.getConnectionStatus();
    const isConnected = serviceRef.current.isConnected();
    
    setConnectionStatus(prev => ({
      ...prev,
      auction: status.auction,
      notification: status.notification,
      isConnected,
      isConnecting: status.auction === HubConnectionState.Connecting || 
                   status.notification === HubConnectionState.Connecting,
      error: null
    }));
  }, []);

  // Initialize SignalR service
  const initialize = useCallback(async () => {
    if (isInitializedRef.current) return;

    try {
      setConnectionStatus(prev => ({ ...prev, isConnecting: true, error: null }));
      
      // Update access token if provided
      if (accessToken) {
        serviceRef.current.updateAccessToken(accessToken);
      }

      // Initialize connections
      await serviceRef.current.initialize();
      
      isInitializedRef.current = true;
      updateConnectionStatus();
      
      if (onConnected) {
        onConnected();
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to initialize SignalR';
      setConnectionStatus(prev => ({
        ...prev,
        isConnecting: false,
        error: errorMessage
      }));
      
      if (onError) {
        onError(errorMessage);
      }
    }
  }, [accessToken, onConnected, onError, updateConnectionStatus]);

  // Disconnect SignalR service
  const disconnect = useCallback(async () => {
    try {
      await serviceRef.current.disconnect();
      isInitializedRef.current = false;
      updateConnectionStatus();
      
      if (onDisconnected) {
        onDisconnected();
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to disconnect SignalR';
      setConnectionStatus(prev => ({ ...prev, error: errorMessage }));
      
      if (onError) {
        onError(errorMessage);
      }
    }
  }, [onDisconnected, onError, updateConnectionStatus]);

  // Reconnect SignalR service
  const reconnect = useCallback(async () => {
    try {
      setConnectionStatus(prev => ({ ...prev, isConnecting: true, error: null }));
      await serviceRef.current.reconnect();
      updateConnectionStatus();
      
      if (onConnected) {
        onConnected();
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to reconnect SignalR';
      setConnectionStatus(prev => ({
        ...prev,
        isConnecting: false,
        error: errorMessage
      }));
      
      if (onError) {
        onError(errorMessage);
      }
    }
  }, [onConnected, onError, updateConnectionStatus]);

  // Auto-connect on mount
  useEffect(() => {
    if (autoConnect && !isInitializedRef.current) {
      initialize();
    }

    // Cleanup on unmount
    return () => {
      if (isInitializedRef.current) {
        disconnect();
      }
    };
  }, [autoConnect, initialize, disconnect]);

  // Update access token when it changes
  useEffect(() => {
    if (accessToken && isInitializedRef.current) {
      serviceRef.current.updateAccessToken(accessToken);
    }
  }, [accessToken]);

  return {
    connectionStatus,
    initialize,
    disconnect,
    reconnect,
    service: serviceRef.current
  };
};

// Hook for auction-specific SignalR functionality
export const useAuctionSignalR = (auctionId?: number) => {
  const { connectionStatus, service } = useSignalR();
  const [isJoined, setIsJoined] = useState(false);
  const [currentBid, setCurrentBid] = useState<number | null>(null);
  const [bidHistory, setBidHistory] = useState<any[]>([]);
  const [auctionStats, setAuctionStats] = useState<any>(null);

  // Join auction room
  const joinAuction = useCallback(async () => {
    if (!auctionId || !service) return;
    
    try {
      await service.joinAuction(auctionId);
      setIsJoined(true);
    } catch (error) {
      console.error('Failed to join auction:', error);
    }
  }, [auctionId, service]);

  // Leave auction room
  const leaveAuction = useCallback(async () => {
    if (!auctionId || !service) return;
    
    try {
      await service.leaveAuction(auctionId);
      setIsJoined(false);
    } catch (error) {
      console.error('Failed to leave auction:', error);
    }
  }, [auctionId, service]);

  // Place a bid
  const placeBid = useCallback(async (amount: number) => {
    if (!auctionId || !service) return;
    
    try {
      await service.placeBid(auctionId, amount);
    } catch (error) {
      console.error('Failed to place bid:', error);
      throw error;
    }
  }, [auctionId, service]);

  // Send auction message
  const sendMessage = useCallback(async (message: string) => {
    if (!auctionId || !service) return;
    
    try {
      await service.sendAuctionMessage(auctionId, message);
    } catch (error) {
      console.error('Failed to send message:', error);
    }
  }, [auctionId, service]);

  // Get auction stats
  const getStats = useCallback(async () => {
    if (!service) return;
    
    try {
      await service.getAuctionStats();
    } catch (error) {
      console.error('Failed to get auction stats:', error);
    }
  }, [service]);

  // Set up event listeners
  useEffect(() => {
    if (!service) return;

    const handleBidPlaced = (data: any) => {
      setCurrentBid(data.newCurrentBid);
      setBidHistory(prev => [...prev, data.bid]);
    };

    const handleBidUpdate = (data: any) => {
      setCurrentBid(data.newBid);
    };

    const handleAuctionStats = (data: any) => {
      setAuctionStats(data);
    };

    const handleBidFailed = (data: any) => {
      console.error('Bid failed:', data.message);
    };

    // Register event listeners
    service.on('bidPlaced', handleBidPlaced);
    service.on('auctionBidUpdate', handleBidUpdate);
    service.on('auctionStats', handleAuctionStats);
    service.on('bidFailed', handleBidFailed);

    // Cleanup
    return () => {
      service.off('bidPlaced', handleBidPlaced);
      service.off('auctionBidUpdate', handleBidUpdate);
      service.off('auctionStats', handleAuctionStats);
      service.off('bidFailed', handleBidFailed);
    };
  }, [service]);

  // Auto-join auction when auctionId changes
  useEffect(() => {
    if (auctionId && connectionStatus.isConnected && !isJoined) {
      joinAuction();
    }
  }, [auctionId, connectionStatus.isConnected, isJoined, joinAuction]);

  return {
    connectionStatus,
    isJoined,
    currentBid,
    bidHistory,
    auctionStats,
    joinAuction,
    leaveAuction,
    placeBid,
    sendMessage,
    getStats
  };
};

// Hook for notification-specific SignalR functionality
export const useNotificationSignalR = () => {
  const { connectionStatus, service } = useSignalR();
  const [notifications, setNotifications] = useState<any[]>([]);
  const [systemAnnouncements, setSystemAnnouncements] = useState<any[]>([]);
  const [adminAlerts, setAdminAlerts] = useState<any[]>([]);

  // Join notification groups
  const joinGeneralNotifications = useCallback(async () => {
    if (!service) return;
    
    try {
      await service.joinGeneralNotifications();
    } catch (error) {
      console.error('Failed to join general notifications:', error);
    }
  }, [service]);

  const joinAdminNotifications = useCallback(async () => {
    if (!service) return;
    
    try {
      await service.joinAdminNotifications();
    } catch (error) {
      console.error('Failed to join admin notifications:', error);
    }
  }, [service]);

  const subscribeToAuction = useCallback(async (auctionId: number) => {
    if (!service) return;
    
    try {
      await service.subscribeToAuction(auctionId);
    } catch (error) {
      console.error('Failed to subscribe to auction:', error);
    }
  }, [service]);

  const subscribeToCategory = useCallback(async (categoryId: number) => {
    if (!service) return;
    
    try {
      await service.subscribeToCategory(categoryId);
    } catch (error) {
      console.error('Failed to subscribe to category:', error);
    }
  }, [service]);

  const subscribeToEndingSoon = useCallback(async () => {
    if (!service) return;
    
    try {
      await service.subscribeToEndingSoon();
    } catch (error) {
      console.error('Failed to subscribe to ending soon:', error);
    }
  }, [service]);

  // Set up event listeners
  useEffect(() => {
    if (!service) return;

    const handleSystemAnnouncement = (data: any) => {
      setSystemAnnouncements(prev => [data, ...prev.slice(0, 9)]); // Keep last 10
    };

    const handleAdminAlert = (data: any) => {
      setAdminAlerts(prev => [data, ...prev.slice(0, 9)]); // Keep last 10
    };

    const handleAuctionNotification = (data: any) => {
      setNotifications(prev => [data, ...prev.slice(0, 19)]); // Keep last 20
    };

    const handleTestMessage = (data: any) => {
      console.log('Test message received:', data);
    };

    // Register event listeners
    service.on('systemAnnouncement', handleSystemAnnouncement);
    service.on('adminAlert', handleAdminAlert);
    service.on('auctionNotification', handleAuctionNotification);
    service.on('testMessage', handleTestMessage);

    // Cleanup
    return () => {
      service.off('systemAnnouncement', handleSystemAnnouncement);
      service.off('adminAlert', handleAdminAlert);
      service.off('auctionNotification', handleAuctionNotification);
      service.off('testMessage', handleTestMessage);
    };
  }, [service]);

  // Auto-join general notifications when connected
  useEffect(() => {
    if (connectionStatus.isConnected) {
      joinGeneralNotifications();
    }
  }, [connectionStatus.isConnected, joinGeneralNotifications]);

  return {
    connectionStatus,
    notifications,
    systemAnnouncements,
    adminAlerts,
    joinGeneralNotifications,
    joinAdminNotifications,
    subscribeToAuction,
    subscribeToCategory,
    subscribeToEndingSoon
  };
};

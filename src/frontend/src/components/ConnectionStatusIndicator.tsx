import React, { useState } from 'react';
import { TransportType, ConnectionState } from '../services/websocketFallbackService';

interface ConnectionStatusProps {
  connectionStatus: {
    state: string;
    transport: TransportType | null;
    reconnectAttempts: number;
    lastConnectedAt: Date | null;
    lastError: string | null;
    latency: number | null;
  };
  availableTransports: TransportType[];
  onSwitchTransport: (transport: TransportType) => Promise<void>;
  onReconnect: () => Promise<void>;
  className?: string;
}

export const ConnectionStatusIndicator: React.FC<ConnectionStatusProps> = ({
  connectionStatus,
  availableTransports,
  onSwitchTransport,
  onReconnect,
  className = ''
}) => {
  const [isExpanded, setIsExpanded] = useState(false);
  const [isSwitching, setIsSwitching] = useState(false);
  const [isReconnecting, setIsReconnecting] = useState(false);

  const getStatusColor = (state: string) => {
    switch (state) {
      case ConnectionState.Connected:
        return 'bg-green-500';
      case ConnectionState.Connecting:
      case ConnectionState.Reconnecting:
        return 'bg-yellow-500';
      case ConnectionState.Failed:
        return 'bg-red-500';
      default:
        return 'bg-gray-500';
    }
  };

  const getStatusText = (state: string) => {
    switch (state) {
      case ConnectionState.Connected:
        return 'Connected';
      case ConnectionState.Connecting:
        return 'Connecting';
      case ConnectionState.Reconnecting:
        return 'Reconnecting';
      case ConnectionState.Failed:
        return 'Failed';
      default:
        return 'Disconnected';
    }
  };

  const getTransportIcon = (transport: TransportType) => {
    switch (transport) {
      case TransportType.WebSockets:
        return '🔌';
      case TransportType.ServerSentEvents:
        return '📡';
      case TransportType.LongPolling:
        return '⏳';
      case TransportType.Polling:
        return '🔄';
      default:
        return '❓';
    }
  };

  const getTransportName = (transport: TransportType) => {
    switch (transport) {
      case TransportType.WebSockets:
        return 'WebSocket';
      case TransportType.ServerSentEvents:
        return 'Server-Sent Events';
      case TransportType.LongPolling:
        return 'Long Polling';
      case TransportType.Polling:
        return 'Polling';
      default:
        return 'Unknown';
    }
  };

  const formatLastConnected = (date: Date | null) => {
    if (!date) return 'Never';
    
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const minutes = Math.floor(diff / 60000);
    const hours = Math.floor(diff / 3600000);
    const days = Math.floor(diff / 86400000);

    if (minutes < 1) return 'Just now';
    if (minutes < 60) return `${minutes}m ago`;
    if (hours < 24) return `${hours}h ago`;
    return `${days}d ago`;
  };

  const handleSwitchTransport = async (transport: TransportType) => {
    if (transport === connectionStatus.transport) return;

    setIsSwitching(true);
    try {
      await onSwitchTransport(transport);
    } catch (error) {
      console.error('Failed to switch transport:', error);
    } finally {
      setIsSwitching(false);
    }
  };

  const handleReconnect = async () => {
    setIsReconnecting(true);
    try {
      await onReconnect();
    } catch (error) {
      console.error('Failed to reconnect:', error);
    } finally {
      setIsReconnecting(false);
    }
  };

  return (
    <div className={`connection-status ${className}`}>
      {/* Status Indicator */}
      <div className="flex items-center space-x-2">
        <div className={`w-3 h-3 rounded-full ${getStatusColor(connectionStatus.state)}`}></div>
        <span className="text-sm font-medium text-gray-700">
          {getStatusText(connectionStatus.state)}
        </span>
        {connectionStatus.transport && (
          <span className="text-xs text-gray-500">
            via {getTransportIcon(connectionStatus.transport)} {getTransportName(connectionStatus.transport)}
          </span>
        )}
        {connectionStatus.latency && (
          <span className="text-xs text-gray-500">
            ({connectionStatus.latency}ms)
          </span>
        )}
        <button
          onClick={() => setIsExpanded(!isExpanded)}
          className="text-xs text-blue-600 hover:text-blue-800"
        >
          {isExpanded ? 'Less' : 'More'}
        </button>
      </div>

      {/* Expanded Details */}
      {isExpanded && (
        <div className="mt-3 p-3 bg-gray-50 rounded-lg border">
          <div className="space-y-2">
            {/* Connection Details */}
            <div className="text-sm">
              <div className="flex justify-between">
                <span className="text-gray-600">Status:</span>
                <span className="font-medium">{getStatusText(connectionStatus.state)}</span>
              </div>
              {connectionStatus.transport && (
                <div className="flex justify-between">
                  <span className="text-gray-600">Transport:</span>
                  <span className="font-medium">
                    {getTransportIcon(connectionStatus.transport)} {getTransportName(connectionStatus.transport)}
                  </span>
                </div>
              )}
              <div className="flex justify-between">
                <span className="text-gray-600">Reconnect Attempts:</span>
                <span className="font-medium">{connectionStatus.reconnectAttempts}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Last Connected:</span>
                <span className="font-medium">{formatLastConnected(connectionStatus.lastConnectedAt)}</span>
              </div>
              {connectionStatus.latency && (
                <div className="flex justify-between">
                  <span className="text-gray-600">Latency:</span>
                  <span className="font-medium">{connectionStatus.latency}ms</span>
                </div>
              )}
            </div>

            {/* Error Message */}
            {connectionStatus.lastError && (
              <div className="text-sm text-red-600 bg-red-50 p-2 rounded">
                <strong>Error:</strong> {connectionStatus.lastError}
              </div>
            )}

            {/* Transport Selection */}
            <div className="text-sm">
              <div className="text-gray-600 mb-2">Available Transports:</div>
              <div className="flex flex-wrap gap-2">
                {availableTransports.map((transport) => (
                  <button
                    key={transport}
                    onClick={() => handleSwitchTransport(transport)}
                    disabled={isSwitching || transport === connectionStatus.transport}
                    className={`px-3 py-1 text-xs rounded-full border ${
                      transport === connectionStatus.transport
                        ? 'bg-blue-100 border-blue-300 text-blue-700'
                        : 'bg-white border-gray-300 text-gray-700 hover:bg-gray-50'
                    } ${isSwitching ? 'opacity-50 cursor-not-allowed' : ''}`}
                  >
                    {getTransportIcon(transport)} {getTransportName(transport)}
                  </button>
                ))}
              </div>
            </div>

            {/* Actions */}
            <div className="flex space-x-2 pt-2">
              <button
                onClick={handleReconnect}
                disabled={isReconnecting || connectionStatus.state === ConnectionState.Connected}
                className={`px-3 py-1 text-xs rounded ${
                  isReconnecting || connectionStatus.state === ConnectionState.Connected
                    ? 'bg-gray-100 text-gray-400 cursor-not-allowed'
                    : 'bg-blue-600 text-white hover:bg-blue-700'
                }`}
              >
                {isReconnecting ? 'Reconnecting...' : 'Reconnect'}
              </button>
              
              {connectionStatus.state === ConnectionState.Failed && (
                <button
                  onClick={() => window.location.reload()}
                  className="px-3 py-1 text-xs rounded bg-red-600 text-white hover:bg-red-700"
                >
                  Reload Page
                </button>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

// Compact version for headers/toolbars
export const CompactConnectionStatus: React.FC<{
  connectionStatus: {
    state: string;
    transport: TransportType | null;
    latency: number | null;
  };
  className?: string;
}> = ({ connectionStatus, className = '' }) => {
  const getStatusColor = (state: string) => {
    switch (state) {
      case ConnectionState.Connected:
        return 'bg-green-500';
      case ConnectionState.Connecting:
      case ConnectionState.Reconnecting:
        return 'bg-yellow-500';
      case ConnectionState.Failed:
        return 'bg-red-500';
      default:
        return 'bg-gray-500';
    }
  };

  const getTransportIcon = (transport: TransportType) => {
    switch (transport) {
      case TransportType.WebSockets:
        return '🔌';
      case TransportType.ServerSentEvents:
        return '📡';
      case TransportType.LongPolling:
        return '⏳';
      case TransportType.Polling:
        return '🔄';
      default:
        return '❓';
    }
  };

  return (
    <div className={`flex items-center space-x-1 ${className}`}>
      <div className={`w-2 h-2 rounded-full ${getStatusColor(connectionStatus.state)}`}></div>
      {connectionStatus.transport && (
        <span className="text-xs text-gray-500">
          {getTransportIcon(connectionStatus.transport)}
        </span>
      )}
      {connectionStatus.latency && (
        <span className="text-xs text-gray-500">
          {connectionStatus.latency}ms
        </span>
      )}
    </div>
  );
};

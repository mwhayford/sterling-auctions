import React from 'react';
import { HubConnectionState } from '@microsoft/signalr';
import { useSignalR } from '../hooks/useSignalR';

interface ConnectionStatusProps {
  showDetails?: boolean;
  className?: string;
}

export const ConnectionStatus: React.FC<ConnectionStatusProps> = ({
  showDetails = false,
  className = ''
}) => {
  const { connectionStatus, reconnect, disconnect } = useSignalR();

  const getStatusColor = (state: HubConnectionState) => {
    switch (state) {
      case HubConnectionState.Connected:
        return 'text-green-600';
      case HubConnectionState.Connecting:
        return 'text-yellow-600';
      case HubConnectionState.Disconnected:
        return 'text-red-600';
      case HubConnectionState.Reconnecting:
        return 'text-yellow-600';
      default:
        return 'text-gray-600';
    }
  };

  const getStatusText = (state: HubConnectionState) => {
    switch (state) {
      case HubConnectionState.Connected:
        return 'Connected';
      case HubConnectionState.Connecting:
        return 'Connecting';
      case HubConnectionState.Disconnected:
        return 'Disconnected';
      case HubConnectionState.Reconnecting:
        return 'Reconnecting';
      default:
        return 'Unknown';
    }
  };

  const getStatusIcon = (state: HubConnectionState) => {
    switch (state) {
      case HubConnectionState.Connected:
        return (
          <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
          </svg>
        );
      case HubConnectionState.Connecting:
      case HubConnectionState.Reconnecting:
        return (
          <svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
        );
      case HubConnectionState.Disconnected:
        return (
          <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
          </svg>
        );
      default:
        return (
          <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
          </svg>
        );
    }
  };

  const handleReconnect = async () => {
    try {
      await reconnect();
    } catch (error) {
      console.error('Failed to reconnect:', error);
    }
  };

  const handleDisconnect = async () => {
    try {
      await disconnect();
    } catch (error) {
      console.error('Failed to disconnect:', error);
    }
  };

  if (showDetails) {
    return (
      <div className={`bg-white rounded-lg shadow p-4 ${className}`}>
        <h3 className="text-lg font-semibold text-gray-800 mb-3">Connection Status</h3>
        
        {/* Overall Status */}
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center space-x-2">
            {getStatusIcon(connectionStatus.isConnected ? HubConnectionState.Connected : HubConnectionState.Disconnected)}
            <span className={`font-medium ${getStatusColor(connectionStatus.isConnected ? HubConnectionState.Connected : HubConnectionState.Disconnected)}`}>
              {connectionStatus.isConnected ? 'All Systems Connected' : 'Connection Issues'}
            </span>
          </div>
          <div className="flex space-x-2">
            {!connectionStatus.isConnected && (
              <button
                onClick={handleReconnect}
                disabled={connectionStatus.isConnecting}
                className="px-3 py-1 text-sm bg-blue-600 text-white rounded hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed"
              >
                {connectionStatus.isConnecting ? 'Reconnecting...' : 'Reconnect'}
              </button>
            )}
            {connectionStatus.isConnected && (
              <button
                onClick={handleDisconnect}
                className="px-3 py-1 text-sm bg-red-600 text-white rounded hover:bg-red-700"
              >
                Disconnect
              </button>
            )}
          </div>
        </div>

        {/* Individual Hub Status */}
        <div className="space-y-2">
          <div className="flex items-center justify-between">
            <span className="text-sm text-gray-600">Auction Hub</span>
            <div className="flex items-center space-x-2">
              {getStatusIcon(connectionStatus.auction)}
              <span className={`text-sm ${getStatusColor(connectionStatus.auction)}`}>
                {getStatusText(connectionStatus.auction)}
              </span>
            </div>
          </div>
          
          <div className="flex items-center justify-between">
            <span className="text-sm text-gray-600">Notification Hub</span>
            <div className="flex items-center space-x-2">
              {getStatusIcon(connectionStatus.notification)}
              <span className={`text-sm ${getStatusColor(connectionStatus.notification)}`}>
                {getStatusText(connectionStatus.notification)}
              </span>
            </div>
          </div>
        </div>

        {/* Error Display */}
        {connectionStatus.error && (
          <div className="mt-4 p-3 bg-red-100 border border-red-300 text-red-700 rounded">
            <p className="text-sm font-medium">Connection Error:</p>
            <p className="text-sm">{connectionStatus.error}</p>
          </div>
        )}

        {/* Connection Info */}
        <div className="mt-4 pt-4 border-t border-gray-200">
          <div className="text-xs text-gray-500 space-y-1">
            <p>Real-time bidding and notifications</p>
            <p>WebSocket connection to SignalR hubs</p>
            <p>Automatic reconnection enabled</p>
          </div>
        </div>
      </div>
    );
  }

  // Simple status indicator
  return (
    <div className={`flex items-center space-x-2 ${className}`}>
      {getStatusIcon(connectionStatus.isConnected ? HubConnectionState.Connected : HubConnectionState.Disconnected)}
      <span className={`text-sm ${getStatusColor(connectionStatus.isConnected ? HubConnectionState.Connected : HubConnectionState.Disconnected)}`}>
        {connectionStatus.isConnected ? 'Live' : 'Offline'}
      </span>
      {connectionStatus.isConnecting && (
        <span className="text-xs text-gray-500">Connecting...</span>
      )}
    </div>
  );
};

// Simple status badge component
export const StatusBadge: React.FC<{ className?: string }> = ({ className = '' }) => {
  const { connectionStatus } = useSignalR();

  return (
    <div className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${
      connectionStatus.isConnected 
        ? 'bg-green-100 text-green-800' 
        : 'bg-red-100 text-red-800'
    } ${className}`}>
      <div className={`w-2 h-2 rounded-full mr-1 ${
        connectionStatus.isConnected ? 'bg-green-500' : 'bg-red-500'
      }`}></div>
      {connectionStatus.isConnected ? 'Live' : 'Offline'}
    </div>
  );
};

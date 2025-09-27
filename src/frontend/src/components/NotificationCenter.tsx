import React, { useState, useEffect } from 'react';
import { useNotificationSignalR } from '../hooks/useSignalR';

interface NotificationCenterProps {
  onNotificationClick?: (notification: any) => void;
  maxNotifications?: number;
}

export const NotificationCenter: React.FC<NotificationCenterProps> = ({
  onNotificationClick,
  maxNotifications = 10
}) => {
  const {
    connectionStatus,
    notifications,
    systemAnnouncements,
    adminAlerts,
    subscribeToAuction,
    subscribeToCategory,
    subscribeToEndingSoon
  } = useNotificationSignalR();

  const [isOpen, setIsOpen] = useState(false);
  const [activeTab, setActiveTab] = useState<'all' | 'system' | 'admin'>('all');
  const [unreadCount, setUnreadCount] = useState(0);

  // Calculate unread count
  useEffect(() => {
    const total = notifications.length + systemAnnouncements.length + adminAlerts.length;
    setUnreadCount(total);
  }, [notifications, systemAnnouncements, adminAlerts]);

  // Get notification icon based on type
  const getNotificationIcon = (type: string) => {
    switch (type) {
      case 'BidPlaced':
        return '💰';
      case 'AuctionEnded':
        return '🏁';
      case 'AuctionStarting':
        return '🚀';
      case 'AuctionEndingSoon':
        return '⏰';
      case 'SystemAnnouncement':
        return '📢';
      case 'AdminAlert':
        return '⚠️';
      case 'AuctionWon':
        return '🎉';
      case 'AuctionLost':
        return '😞';
      default:
        return '📋';
    }
  };

  // Get notification color based on type
  const getNotificationColor = (type: string) => {
    switch (type) {
      case 'BidPlaced':
        return 'bg-green-100 border-green-300';
      case 'AuctionEnded':
        return 'bg-gray-100 border-gray-300';
      case 'AuctionStarting':
        return 'bg-blue-100 border-blue-300';
      case 'AuctionEndingSoon':
        return 'bg-yellow-100 border-yellow-300';
      case 'SystemAnnouncement':
        return 'bg-purple-100 border-purple-300';
      case 'AdminAlert':
        return 'bg-red-100 border-red-300';
      case 'AuctionWon':
        return 'bg-green-100 border-green-300';
      case 'AuctionLost':
        return 'bg-red-100 border-red-300';
      default:
        return 'bg-gray-100 border-gray-300';
    }
  };

  // Format timestamp
  const formatTimestamp = (timestamp: string) => {
    const date = new Date(timestamp);
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

  // Get all notifications combined
  const getAllNotifications = () => {
    const all = [
      ...notifications.map(n => ({ ...n, source: 'auction' })),
      ...systemAnnouncements.map(n => ({ ...n, source: 'system' })),
      ...adminAlerts.map(n => ({ ...n, source: 'admin' }))
    ];
    return all.sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime());
  };

  // Get filtered notifications based on active tab
  const getFilteredNotifications = () => {
    const all = getAllNotifications();
    switch (activeTab) {
      case 'system':
        return all.filter(n => n.source === 'system');
      case 'admin':
        return all.filter(n => n.source === 'admin');
      default:
        return all;
    }
  };

  const filteredNotifications = getFilteredNotifications().slice(0, maxNotifications);

  return (
    <div className="notification-center relative">
      {/* Notification Bell */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="relative p-2 text-gray-600 hover:text-gray-800 focus:outline-none focus:ring-2 focus:ring-blue-500 rounded-full"
      >
        <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
        </svg>
        {unreadCount > 0 && (
          <span className="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full h-5 w-5 flex items-center justify-center">
            {unreadCount > 99 ? '99+' : unreadCount}
          </span>
        )}
      </button>

      {/* Notification Panel */}
      {isOpen && (
        <div className="absolute right-0 mt-2 w-96 bg-white rounded-lg shadow-lg border border-gray-200 z-50">
          {/* Header */}
          <div className="p-4 border-b border-gray-200">
            <div className="flex items-center justify-between">
              <h3 className="text-lg font-semibold text-gray-800">Notifications</h3>
              <div className="flex items-center space-x-2">
                <div className={`w-2 h-2 rounded-full ${
                  connectionStatus.isConnected ? 'bg-green-500' : 'bg-red-500'
                }`}></div>
                <span className="text-sm text-gray-600">
                  {connectionStatus.isConnected ? 'Live' : 'Offline'}
                </span>
              </div>
            </div>
            
            {/* Tabs */}
            <div className="flex space-x-1 mt-3">
              <button
                onClick={() => setActiveTab('all')}
                className={`px-3 py-1 text-sm rounded-md ${
                  activeTab === 'all' 
                    ? 'bg-blue-100 text-blue-700' 
                    : 'text-gray-600 hover:text-gray-800'
                }`}
              >
                All ({getAllNotifications().length})
              </button>
              <button
                onClick={() => setActiveTab('system')}
                className={`px-3 py-1 text-sm rounded-md ${
                  activeTab === 'system' 
                    ? 'bg-blue-100 text-blue-700' 
                    : 'text-gray-600 hover:text-gray-800'
                }`}
              >
                System ({systemAnnouncements.length})
              </button>
              <button
                onClick={() => setActiveTab('admin')}
                className={`px-3 py-1 text-sm rounded-md ${
                  activeTab === 'admin' 
                    ? 'bg-blue-100 text-blue-700' 
                    : 'text-gray-600 hover:text-gray-800'
                }`}
              >
                Admin ({adminAlerts.length})
              </button>
            </div>
          </div>

          {/* Notifications List */}
          <div className="max-h-96 overflow-y-auto">
            {filteredNotifications.length === 0 ? (
              <div className="p-4 text-center text-gray-500">
                <p>No notifications yet</p>
                <p className="text-sm mt-1">You'll see real-time updates here</p>
              </div>
            ) : (
              <div className="divide-y divide-gray-200">
                {filteredNotifications.map((notification, index) => (
                  <div
                    key={index}
                    onClick={() => onNotificationClick?.(notification)}
                    className={`p-4 hover:bg-gray-50 cursor-pointer border-l-4 ${getNotificationColor(notification.type)}`}
                  >
                    <div className="flex items-start space-x-3">
                      <span className="text-lg">{getNotificationIcon(notification.type)}</span>
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium text-gray-900">
                          {notification.message || notification.type}
                        </p>
                        {notification.data && (
                          <p className="text-xs text-gray-600 mt-1">
                            Auction #{notification.auctionId}
                          </p>
                        )}
                        <p className="text-xs text-gray-500 mt-1">
                          {formatTimestamp(notification.timestamp)}
                        </p>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Footer */}
          <div className="p-4 border-t border-gray-200 bg-gray-50">
            <div className="flex justify-between items-center">
              <div className="text-xs text-gray-600">
                {connectionStatus.isConnected ? 'Real-time updates active' : 'Connection lost'}
              </div>
              <button
                onClick={() => setIsOpen(false)}
                className="text-xs text-blue-600 hover:text-blue-800"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

// Quick notification toast component
interface NotificationToastProps {
  notification: any;
  onClose: () => void;
  duration?: number;
}

export const NotificationToast: React.FC<NotificationToastProps> = ({
  notification,
  onClose,
  duration = 5000
}) => {
  useEffect(() => {
    const timer = setTimeout(onClose, duration);
    return () => clearTimeout(timer);
  }, [onClose, duration]);

  const getNotificationIcon = (type: string) => {
    switch (type) {
      case 'BidPlaced':
        return '💰';
      case 'AuctionEnded':
        return '🏁';
      case 'AuctionStarting':
        return '🚀';
      case 'AuctionEndingSoon':
        return '⏰';
      case 'SystemAnnouncement':
        return '📢';
      case 'AdminAlert':
        return '⚠️';
      case 'AuctionWon':
        return '🎉';
      case 'AuctionLost':
        return '😞';
      default:
        return '📋';
    }
  };

  const getNotificationColor = (type: string) => {
    switch (type) {
      case 'BidPlaced':
        return 'bg-green-500';
      case 'AuctionEnded':
        return 'bg-gray-500';
      case 'AuctionStarting':
        return 'bg-blue-500';
      case 'AuctionEndingSoon':
        return 'bg-yellow-500';
      case 'SystemAnnouncement':
        return 'bg-purple-500';
      case 'AdminAlert':
        return 'bg-red-500';
      case 'AuctionWon':
        return 'bg-green-500';
      case 'AuctionLost':
        return 'bg-red-500';
      default:
        return 'bg-gray-500';
    }
  };

  return (
    <div className={`fixed top-4 right-4 max-w-sm w-full ${getNotificationColor(notification.type)} text-white rounded-lg shadow-lg p-4 z-50`}>
      <div className="flex items-start space-x-3">
        <span className="text-lg">{getNotificationIcon(notification.type)}</span>
        <div className="flex-1">
          <p className="text-sm font-medium">{notification.message || notification.type}</p>
          {notification.data && (
            <p className="text-xs opacity-90 mt-1">
              Auction #{notification.auctionId}
            </p>
          )}
        </div>
        <button
          onClick={onClose}
          className="text-white hover:text-gray-200 focus:outline-none"
        >
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>
    </div>
  );
};

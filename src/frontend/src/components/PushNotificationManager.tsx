import React, { useState, useEffect } from 'react';
import { usePushNotifications } from '../hooks/usePushNotifications';

interface PushNotificationManagerProps {
  authToken?: string;
  onNotificationClick?: (notification: any) => void;
}

export const PushNotificationManager: React.FC<PushNotificationManagerProps> = ({
  authToken,
  onNotificationClick
}) => {
  const {
    isSupported,
    isSubscribed,
    permission,
    preferences,
    notificationHistory,
    statistics,
    isLoading,
    error,
    requestPermission,
    subscribe,
    unsubscribe,
    updatePreferences,
    refreshHistory,
    markAsClicked,
    refreshStatistics
  } = usePushNotifications(authToken);

  const [showPreferences, setShowPreferences] = useState(false);
  const [showHistory, setShowHistory] = useState(false);
  const [showStatistics, setShowStatistics] = useState(false);

  // Handle notification clicks
  useEffect(() => {
    if (onNotificationClick) {
      // Set up notification click handler
      const handleNotificationClick = (event: any) => {
        onNotificationClick(event.detail);
      };

      window.addEventListener('notificationclick', handleNotificationClick);
      return () => window.removeEventListener('notificationclick', handleNotificationClick);
    }
  }, [onNotificationClick]);

  const handleSubscribe = async () => {
    if (permission === 'denied') {
      alert('Notification permission is denied. Please enable it in your browser settings.');
      return;
    }

    if (permission === 'default') {
      const newPermission = await requestPermission();
      if (newPermission !== 'granted') {
        alert('Notification permission is required to enable push notifications.');
        return;
      }
    }

    await subscribe();
  };

  const handleUnsubscribe = async () => {
    await unsubscribe();
  };

  const handlePreferenceChange = async (key: string, value: boolean | number) => {
    if (!preferences) return;

    const updatedPreferences = { ...preferences, [key]: value };
    await updatePreferences(updatedPreferences);
  };

  const formatTimestamp = (timestamp: string) => {
    const date = new Date(timestamp);
    return date.toLocaleString();
  };

  const getNotificationIcon = (type: string) => {
    switch (type) {
      case 'AuctionStarting': return '🚀';
      case 'AuctionEndingSoon': return '⏰';
      case 'AuctionEnded': return '🏁';
      case 'BidPlaced': return '💰';
      case 'AuctionWon': return '🎉';
      case 'AuctionLost': return '😞';
      case 'PaymentReceived': return '💳';
      case 'PaymentFailed': return '❌';
      case 'SystemAnnouncement': return '📢';
      case 'AdminAlert': return '⚠️';
      default: return '📋';
    }
  };

  if (!isSupported) {
    return (
      <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
        <div className="flex items-center">
          <svg className="w-5 h-5 text-yellow-600 mr-2" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
          </svg>
          <span className="text-yellow-800">Push notifications are not supported in this browser.</span>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-lg p-6">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-xl font-semibold text-gray-800">Push Notifications</h2>
        <div className="flex items-center space-x-2">
          <div className={`w-3 h-3 rounded-full ${
            isSubscribed ? 'bg-green-500' : 'bg-gray-400'
          }`}></div>
          <span className="text-sm text-gray-600">
            {isSubscribed ? 'Subscribed' : 'Not Subscribed'}
          </span>
        </div>
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-4">
          <div className="flex items-center">
            <svg className="w-5 h-5 text-red-600 mr-2" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
            </svg>
            <span className="text-red-800">{error}</span>
          </div>
        </div>
      )}

      <div className="space-y-4">
        {/* Permission Status */}
        <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
          <div>
            <h3 className="font-medium text-gray-800">Permission Status</h3>
            <p className="text-sm text-gray-600">
              {permission === 'granted' ? 'Notifications are allowed' :
               permission === 'denied' ? 'Notifications are blocked' :
               'Permission not requested'}
            </p>
          </div>
          <div className={`px-3 py-1 rounded-full text-sm ${
            permission === 'granted' ? 'bg-green-100 text-green-800' :
            permission === 'denied' ? 'bg-red-100 text-red-800' :
            'bg-yellow-100 text-yellow-800'
          }`}>
            {permission}
          </div>
        </div>

        {/* Subscription Actions */}
        <div className="flex space-x-3">
          {!isSubscribed ? (
            <button
              onClick={handleSubscribe}
              disabled={isLoading || permission === 'denied'}
              className="flex-1 bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isLoading ? 'Subscribing...' : 'Enable Push Notifications'}
            </button>
          ) : (
            <button
              onClick={handleUnsubscribe}
              disabled={isLoading}
              className="flex-1 bg-red-600 text-white px-4 py-2 rounded-lg hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isLoading ? 'Unsubscribing...' : 'Disable Push Notifications'}
            </button>
          )}
        </div>

        {/* Action Buttons */}
        <div className="flex space-x-3">
          <button
            onClick={() => setShowPreferences(!showPreferences)}
            className="flex-1 bg-gray-600 text-white px-4 py-2 rounded-lg hover:bg-gray-700"
          >
            Preferences
          </button>
          <button
            onClick={() => setShowHistory(!showHistory)}
            className="flex-1 bg-gray-600 text-white px-4 py-2 rounded-lg hover:bg-gray-700"
          >
            History
          </button>
          <button
            onClick={() => setShowStatistics(!showStatistics)}
            className="flex-1 bg-gray-600 text-white px-4 py-2 rounded-lg hover:bg-gray-700"
          >
            Statistics
          </button>
        </div>

        {/* Preferences Panel */}
        {showPreferences && preferences && (
          <div className="border-t pt-4">
            <h3 className="font-medium text-gray-800 mb-4">Notification Preferences</h3>
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <label htmlFor="enablePushNotifications" className="text-sm text-gray-600">Enable Push Notifications</label>
                <input
                  id="enablePushNotifications"
                  type="checkbox"
                  checked={preferences.enablePushNotifications}
                  onChange={(e) => handlePreferenceChange('enablePushNotifications', e.target.checked)}
                  className="rounded"
                />
              </div>
              <div className="flex items-center justify-between">
                <label htmlFor="enableAuctionNotifications" className="text-sm text-gray-600">Auction Notifications</label>
                <input
                  id="enableAuctionNotifications"
                  type="checkbox"
                  checked={preferences.enableAuctionNotifications}
                  onChange={(e) => handlePreferenceChange('enableAuctionNotifications', e.target.checked)}
                  className="rounded"
                />
              </div>
              <div className="flex items-center justify-between">
                <label htmlFor="enableBidNotifications" className="text-sm text-gray-600">Bid Notifications</label>
                <input
                  id="enableBidNotifications"
                  type="checkbox"
                  checked={preferences.enableBidNotifications}
                  onChange={(e) => handlePreferenceChange('enableBidNotifications', e.target.checked)}
                  className="rounded"
                />
              </div>
              <div className="flex items-center justify-between">
                <label htmlFor="enablePaymentNotifications" className="text-sm text-gray-600">Payment Notifications</label>
                <input
                  id="enablePaymentNotifications"
                  type="checkbox"
                  checked={preferences.enablePaymentNotifications}
                  onChange={(e) => handlePreferenceChange('enablePaymentNotifications', e.target.checked)}
                  className="rounded"
                />
              </div>
              <div className="flex items-center justify-between">
                <label htmlFor="enableSystemNotifications" className="text-sm text-gray-600">System Notifications</label>
                <input
                  id="enableSystemNotifications"
                  type="checkbox"
                  checked={preferences.enableSystemNotifications}
                  onChange={(e) => handlePreferenceChange('enableSystemNotifications', e.target.checked)}
                  className="rounded"
                />
              </div>
              <div className="flex items-center justify-between">
                <label htmlFor="enableAdminAlerts" className="text-sm text-gray-600">Admin Alerts</label>
                <input
                  id="enableAdminAlerts"
                  type="checkbox"
                  checked={preferences.enableAdminAlerts}
                  onChange={(e) => handlePreferenceChange('enableAdminAlerts', e.target.checked)}
                  className="rounded"
                />
              </div>
            </div>
          </div>
        )}

        {/* History Panel */}
        {showHistory && (
          <div className="border-t pt-4">
            <div className="flex items-center justify-between mb-4">
              <h3 className="font-medium text-gray-800">Notification History</h3>
              <button
                onClick={() => refreshHistory()}
                className="text-sm text-blue-600 hover:text-blue-800"
              >
                Refresh
              </button>
            </div>
            <div className="space-y-2 max-h-64 overflow-y-auto">
              {notificationHistory.length === 0 ? (
                <p className="text-gray-500 text-center py-4">No notifications yet</p>
              ) : (
                notificationHistory.map((notification) => (
                  <div
                    key={notification.id}
                    className="flex items-start space-x-3 p-3 bg-gray-50 rounded-lg"
                  >
                    <span className="text-lg">{getNotificationIcon(notification.type)}</span>
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium text-gray-900">{notification.title}</p>
                      <p className="text-xs text-gray-600">{notification.body}</p>
                      <p className="text-xs text-gray-500">{formatTimestamp(notification.createdAt)}</p>
                    </div>
                    {notification.clickedAt && (
                      <span className="text-xs text-green-600">✓ Clicked</span>
                    )}
                  </div>
                ))
              )}
            </div>
          </div>
        )}

        {/* Statistics Panel */}
        {showStatistics && statistics && (
          <div className="border-t pt-4">
            <div className="flex items-center justify-between mb-4">
              <h3 className="font-medium text-gray-800">Statistics</h3>
              <button
                onClick={refreshStatistics}
                className="text-sm text-blue-600 hover:text-blue-800"
              >
                Refresh
              </button>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="bg-gray-50 p-3 rounded-lg">
                <p className="text-sm text-gray-600">Total Sent</p>
                <p className="text-lg font-semibold">{statistics.totalNotificationsSent}</p>
              </div>
              <div className="bg-gray-50 p-3 rounded-lg">
                <p className="text-sm text-gray-600">Delivered</p>
                <p className="text-lg font-semibold">{statistics.totalNotificationsDelivered}</p>
              </div>
              <div className="bg-gray-50 p-3 rounded-lg">
                <p className="text-sm text-gray-600">Clicked</p>
                <p className="text-lg font-semibold">{statistics.totalNotificationsClicked}</p>
              </div>
              <div className="bg-gray-50 p-3 rounded-lg">
                <p className="text-sm text-gray-600">Failed</p>
                <p className="text-lg font-semibold">{statistics.totalNotificationsFailed}</p>
              </div>
            </div>
            <div className="mt-4 grid grid-cols-2 gap-4">
              <div className="bg-gray-50 p-3 rounded-lg">
                <p className="text-sm text-gray-600">Delivery Rate</p>
                <p className="text-lg font-semibold">{(statistics.deliveryRate * 100).toFixed(1)}%</p>
              </div>
              <div className="bg-gray-50 p-3 rounded-lg">
                <p className="text-sm text-gray-600">Click Rate</p>
                <p className="text-lg font-semibold">{(statistics.clickRate * 100).toFixed(1)}%</p>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

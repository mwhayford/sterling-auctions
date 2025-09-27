import { useState, useEffect, useCallback } from 'react';
import PushNotificationService, { 
  PushSubscription, 
  PushNotificationPreferences, 
  PushNotificationDto,
  PushNotificationStatistics 
} from '../services/pushNotificationService';

interface UsePushNotificationsReturn {
  // State
  isSupported: boolean;
  isSubscribed: boolean;
  permission: NotificationPermission;
  preferences: PushNotificationPreferences | null;
  notificationHistory: PushNotificationDto[];
  statistics: PushNotificationStatistics | null;
  isLoading: boolean;
  error: string | null;

  // Actions
  requestPermission: () => Promise<NotificationPermission>;
  subscribe: () => Promise<boolean>;
  unsubscribe: () => Promise<boolean>;
  updatePreferences: (preferences: PushNotificationPreferences) => Promise<boolean>;
  refreshHistory: (page?: number, pageSize?: number) => Promise<void>;
  markAsClicked: (notificationId: number) => Promise<boolean>;
  refreshStatistics: () => Promise<void>;
}

export const usePushNotifications = (authToken?: string): UsePushNotificationsReturn => {
  const [isSupported, setIsSupported] = useState(false);
  const [isSubscribed, setIsSubscribed] = useState(false);
  const [permission, setPermission] = useState<NotificationPermission>('default');
  const [preferences, setPreferences] = useState<PushNotificationPreferences | null>(null);
  const [notificationHistory, setNotificationHistory] = useState<PushNotificationDto[]>([]);
  const [statistics, setStatistics] = useState<PushNotificationStatistics | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const pushService = new PushNotificationService();

  // Initialize
  useEffect(() => {
    const initialize = async () => {
      setIsSupported(pushService.isPushSupported());
      setPermission(Notification.permission);
      
      if (authToken) {
        await loadUserData();
      }
    };

    initialize();
  }, [authToken]);

  const loadUserData = async () => {
    if (!authToken) return;

    setIsLoading(true);
    setError(null);

    try {
      const [prefs, history, stats] = await Promise.all([
        pushService.getPreferences(authToken),
        pushService.getNotificationHistory(authToken),
        pushService.getStatistics(authToken)
      ]);

      setPreferences(prefs);
      setNotificationHistory(history);
      setStatistics(stats);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load user data');
    } finally {
      setIsLoading(false);
    }
  };

  const requestPermission = useCallback(async (): Promise<NotificationPermission> => {
    try {
      const newPermission = await pushService.requestPermission();
      setPermission(newPermission);
      return newPermission;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to request permission');
      return 'denied';
    }
  }, []);

  const subscribe = useCallback(async (): Promise<boolean> => {
    if (!authToken) {
      setError('Authentication token required');
      return false;
    }

    setIsLoading(true);
    setError(null);

    try {
      const subscription = await pushService.subscribe();
      if (!subscription) {
        setError('Failed to create push subscription');
        return false;
      }

      const success = await pushService.registerWithServer(subscription, authToken);
      if (success) {
        setIsSubscribed(true);
        await loadUserData(); // Refresh user data
      } else {
        setError('Failed to register subscription with server');
      }

      return success;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to subscribe');
      return false;
    } finally {
      setIsLoading(false);
    }
  }, [authToken]);

  const unsubscribe = useCallback(async (): Promise<boolean> => {
    if (!authToken) {
      setError('Authentication token required');
      return false;
    }

    setIsLoading(true);
    setError(null);

    try {
      const success = await pushService.unsubscribe();
      if (success) {
        setIsSubscribed(false);
        await loadUserData(); // Refresh user data
      }

      return success;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to unsubscribe');
      return false;
    } finally {
      setIsLoading(false);
    }
  }, [authToken]);

  const updatePreferences = useCallback(async (newPreferences: PushNotificationPreferences): Promise<boolean> => {
    if (!authToken) {
      setError('Authentication token required');
      return false;
    }

    setIsLoading(true);
    setError(null);

    try {
      const success = await pushService.updatePreferences(newPreferences, authToken);
      if (success) {
        setPreferences(newPreferences);
      } else {
        setError('Failed to update preferences');
      }

      return success;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update preferences');
      return false;
    } finally {
      setIsLoading(false);
    }
  }, [authToken]);

  const refreshHistory = useCallback(async (page: number = 1, pageSize: number = 20): Promise<void> => {
    if (!authToken) return;

    try {
      const history = await pushService.getNotificationHistory(authToken, page, pageSize);
      setNotificationHistory(history);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to refresh history');
    }
  }, [authToken]);

  const markAsClicked = useCallback(async (notificationId: number): Promise<boolean> => {
    if (!authToken) {
      setError('Authentication token required');
      return false;
    }

    try {
      const success = await pushService.markAsClicked(notificationId, authToken);
      if (success) {
        // Update local state
        setNotificationHistory(prev => 
          prev.map(notification => 
            notification.id === notificationId 
              ? { ...notification, clickedAt: new Date().toISOString() }
              : notification
          )
        );
      }

      return success;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to mark as clicked');
      return false;
    }
  }, [authToken]);

  const refreshStatistics = useCallback(async (): Promise<void> => {
    if (!authToken) return;

    try {
      const stats = await pushService.getStatistics(authToken);
      setStatistics(stats);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to refresh statistics');
    }
  }, [authToken]);

  return {
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
  };
};

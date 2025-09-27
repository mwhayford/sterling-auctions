// Push Notification Service for Sterling Auctions
export interface PushSubscription {
  endpoint: string;
  p256dh: string;
  auth: string;
  userAgent?: string;
  deviceInfo?: string;
}

export interface PushNotificationPreferences {
  enablePushNotifications: boolean;
  enableAuctionNotifications: boolean;
  enableBidNotifications: boolean;
  enablePaymentNotifications: boolean;
  enableSystemNotifications: boolean;
  enableAdminAlerts: boolean;
  enableSound: boolean;
  enableVibration: boolean;
  quietHoursStart: number;
  quietHoursEnd: number;
  enableQuietHours: boolean;
}

export interface PushNotificationDto {
  id: number;
  userId: string;
  title: string;
  body: string;
  icon?: string;
  badge?: string;
  image?: string;
  tag?: string;
  data?: string;
  url?: string;
  type: string;
  status: string;
  createdAt: string;
  sentAt?: string;
  deliveredAt?: string;
  clickedAt?: string;
  errorMessage?: string;
  retryCount: number;
}

export interface PushNotificationStatistics {
  totalSubscriptions: number;
  activeSubscriptions: number;
  totalNotificationsSent: number;
  totalNotificationsDelivered: number;
  totalNotificationsClicked: number;
  totalNotificationsFailed: number;
  deliveryRate: number;
  clickRate: number;
  notificationsByType: Record<string, number>;
  notificationsByStatus: Record<string, number>;
  notificationsByDay: Record<string, number>;
}

class PushNotificationService {
  private baseUrl: string;
  private isSupported: boolean = false;
  private registration: ServiceWorkerRegistration | null = null;

  constructor(baseUrl: string = 'http://localhost:5000') {
    this.baseUrl = baseUrl;
    this.checkSupport();
  }

  private async checkSupport(): Promise<void> {
    if ('serviceWorker' in navigator && 'PushManager' in window) {
      this.isSupported = true;
      try {
        this.registration = await navigator.serviceWorker.ready;
      } catch (error) {
        console.error('Error getting service worker registration:', error);
        this.isSupported = false;
      }
    }
  }

  public isPushSupported(): boolean {
    return this.isSupported;
  }

  public async requestPermission(): Promise<NotificationPermission> {
    if (!this.isSupported) {
      throw new Error('Push notifications are not supported');
    }

    return await Notification.requestPermission();
  }

  public async subscribe(): Promise<PushSubscription | null> {
    if (!this.isSupported || !this.registration) {
      throw new Error('Push notifications are not supported');
    }

    try {
      const permission = await this.requestPermission();
      if (permission !== 'granted') {
        throw new Error('Notification permission denied');
      }

      const subscription = await this.registration.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: this.urlBase64ToUint8Array(
          'BEl62iUYgUivxIkv69yViEuiBIa40HI0QYyQ9VQSvBeL6z5xZQHG6f7w5I8e-LdS7v6mKyT5I9Jd2hT2swEzE'
        )
      });

      const pushSubscription: PushSubscription = {
        endpoint: subscription.endpoint,
        p256dh: this.arrayBufferToBase64(subscription.getKey('p256dh')!),
        auth: this.arrayBufferToBase64(subscription.getKey('auth')!),
        userAgent: navigator.userAgent,
        deviceInfo: `${navigator.platform} - ${navigator.language}`
      };

      return pushSubscription;
    } catch (error) {
      console.error('Error subscribing to push notifications:', error);
      return null;
    }
  }

  public async unsubscribe(): Promise<boolean> {
    if (!this.isSupported || !this.registration) {
      return false;
    }

    try {
      const subscription = await this.registration.pushManager.getSubscription();
      if (subscription) {
        await subscription.unsubscribe();
        return true;
      }
      return false;
    } catch (error) {
      console.error('Error unsubscribing from push notifications:', error);
      return false;
    }
  }

  public async registerWithServer(subscription: PushSubscription, token: string): Promise<boolean> {
    try {
      const response = await fetch(`${this.baseUrl}/api/pushnotification/subscribe`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(subscription)
      });

      return response.ok;
    } catch (error) {
      console.error('Error registering subscription with server:', error);
      return false;
    }
  }

  public async unregisterFromServer(endpoint: string, token: string): Promise<boolean> {
    try {
      const response = await fetch(`${this.baseUrl}/api/pushnotification/unsubscribe`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(endpoint)
      });

      return response.ok;
    } catch (error) {
      console.error('Error unregistering from server:', error);
      return false;
    }
  }

  public async getPreferences(token: string): Promise<PushNotificationPreferences | null> {
    try {
      const response = await fetch(`${this.baseUrl}/api/pushnotification/preferences`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        return await response.json();
      }
      return null;
    } catch (error) {
      console.error('Error getting preferences:', error);
      return null;
    }
  }

  public async updatePreferences(preferences: PushNotificationPreferences, token: string): Promise<boolean> {
    try {
      const response = await fetch(`${this.baseUrl}/api/pushnotification/preferences`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(preferences)
      });

      return response.ok;
    } catch (error) {
      console.error('Error updating preferences:', error);
      return false;
    }
  }

  public async getNotificationHistory(token: string, page: number = 1, pageSize: number = 20): Promise<PushNotificationDto[]> {
    try {
      const response = await fetch(`${this.baseUrl}/api/pushnotification/history?page=${page}&pageSize=${pageSize}`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        return await response.json();
      }
      return [];
    } catch (error) {
      console.error('Error getting notification history:', error);
      return [];
    }
  }

  public async markAsClicked(notificationId: number, token: string): Promise<boolean> {
    try {
      const response = await fetch(`${this.baseUrl}/api/pushnotification/${notificationId}/click`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      return response.ok;
    } catch (error) {
      console.error('Error marking notification as clicked:', error);
      return false;
    }
  }

  public async getStatistics(token: string): Promise<PushNotificationStatistics | null> {
    try {
      const response = await fetch(`${this.baseUrl}/api/pushnotification/statistics/user`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        return await response.json();
      }
      return null;
    } catch (error) {
      console.error('Error getting statistics:', error);
      return null;
    }
  }

  public setupNotificationHandlers(): void {
    if (!this.isSupported) {
      return;
    }

    // Handle notification click
    navigator.serviceWorker.addEventListener('message', (event) => {
      if (event.data && event.data.type === 'NOTIFICATION_CLICK') {
        const notification = event.data.notification;
        this.handleNotificationClick(notification);
      }
    });

    // Handle push events
    navigator.serviceWorker.addEventListener('push', (event: any) => {
      if (event.data) {
        const data = event.data.json();
        this.handlePushEvent(data);
      }
    });
  }

  private handleNotificationClick(notification: any): void {
    console.log('Notification clicked:', notification);
    
    // Mark as clicked on server
    if (notification.id) {
      this.markAsClicked(notification.id, this.getAuthToken());
    }

    // Navigate to relevant page if URL is provided
    if (notification.url) {
      window.location.href = notification.url;
    }
  }

  private handlePushEvent(data: any): void {
    console.log('Push event received:', data);
    
    // Show notification
    this.showNotification(data);
  }

  private showNotification(data: any): void {
    if (!this.registration) {
      return;
    }

    const options: NotificationOptions = {
      body: data.body,
      icon: data.icon || '/icons/icon-192x192.png',
      badge: data.badge || '/icons/badge-72x72.png',
      tag: data.tag,
      data: data.data,
      requireInteraction: data.requireInteraction || false,
      silent: data.silent || false
    };

    this.registration.showNotification(data.title, options);
  }

  private getAuthToken(): string {
    // Get token from localStorage or context
    return localStorage.getItem('authToken') || '';
  }

  private urlBase64ToUint8Array(base64String: string): ArrayBuffer {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding)
      .replace(/-/g, '+')
      .replace(/_/g, '/');

    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; ++i) {
      outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray.buffer;
  }

  private arrayBufferToBase64(buffer: ArrayBuffer): string {
    const bytes = new Uint8Array(buffer);
    let binary = '';
    for (let i = 0; i < bytes.byteLength; i++) {
      binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
  }
}

export default PushNotificationService;

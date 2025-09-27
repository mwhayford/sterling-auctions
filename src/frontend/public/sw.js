// Service Worker for Sterling Auctions Push Notifications
const CACHE_NAME = 'sterling-auctions-v1';
const urlsToCache = [
  '/',
  '/static/js/bundle.js',
  '/static/css/main.css',
  '/manifest.json',
  '/icons/icon-192x192.png',
  '/icons/icon-512x512.png'
];

// Install event
self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then((cache) => {
        console.log('Opened cache');
        return cache.addAll(urlsToCache);
      })
  );
});

// Activate event
self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((cacheNames) => {
      return Promise.all(
        cacheNames.map((cacheName) => {
          if (cacheName !== CACHE_NAME) {
            console.log('Deleting old cache:', cacheName);
            return caches.delete(cacheName);
          }
        })
      );
    })
  );
});

// Fetch event
self.addEventListener('fetch', (event) => {
  event.respondWith(
    caches.match(event.request)
      .then((response) => {
        // Return cached version or fetch from network
        return response || fetch(event.request);
      })
  );
});

// Push event
self.addEventListener('push', (event) => {
  console.log('Push event received:', event);

  let data = {};
  if (event.data) {
    try {
      data = event.data.json();
    } catch (e) {
      data = { title: 'Sterling Auctions', body: event.data.text() };
    }
  }

  const options = {
    body: data.body || 'You have a new notification',
    icon: data.icon || '/icons/icon-192x192.png',
    badge: data.badge || '/icons/badge-72x72.png',
    image: data.image,
    tag: data.tag || 'sterling-auctions',
    data: data.data || {},
    requireInteraction: data.requireInteraction || false,
    silent: data.silent || false,
    actions: data.actions || [
      {
        action: 'view',
        title: 'View',
        icon: '/icons/view-icon.png'
      },
      {
        action: 'dismiss',
        title: 'Dismiss',
        icon: '/icons/dismiss-icon.png'
      }
    ]
  };

  event.waitUntil(
    self.registration.showNotification(data.title || 'Sterling Auctions', options)
  );
});

// Notification click event
self.addEventListener('notificationclick', (event) => {
  console.log('Notification clicked:', event);

  event.notification.close();

  const data = event.notification.data || {};
  const action = event.action;

  if (action === 'dismiss') {
    // Just close the notification
    return;
  }

  // Handle notification click
  const urlToOpen = data.url || '/';
  
  event.waitUntil(
    clients.matchAll({ type: 'window', includeUncontrolled: true })
      .then((clientList) => {
        // Check if there's already a window/tab open with the target URL
        for (const client of clientList) {
          if (client.url === urlToOpen && 'focus' in client) {
            client.focus();
            return;
          }
        }
        
        // If no existing window, open a new one
        if (clients.openWindow) {
          return clients.openWindow(urlToOpen);
        }
      })
  );

  // Send message to main thread about notification click
  self.clients.matchAll().then((clients) => {
    clients.forEach((client) => {
      client.postMessage({
        type: 'NOTIFICATION_CLICK',
        notification: {
          id: data.id,
          title: event.notification.title,
          body: event.notification.body,
          data: data,
          url: urlToOpen
        }
      });
    });
  });
});

// Background sync event
self.addEventListener('sync', (event) => {
  console.log('Background sync event:', event.tag);

  if (event.tag === 'notification-sync') {
    event.waitUntil(
      // Sync notification data with server
      syncNotifications()
    );
  }
});

// Message event from main thread
self.addEventListener('message', (event) => {
  console.log('Service worker received message:', event.data);

  if (event.data && event.data.type === 'SKIP_WAITING') {
    self.skipWaiting();
  }
});

// Helper function to sync notifications
async function syncNotifications() {
  try {
    // Get stored notification data
    const notificationData = await getStoredNotificationData();
    
    if (notificationData && notificationData.length > 0) {
      // Send to server
      const response = await fetch('/api/pushnotification/sync', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(notificationData)
      });

      if (response.ok) {
        // Clear stored data after successful sync
        await clearStoredNotificationData();
        console.log('Notifications synced successfully');
      }
    }
  } catch (error) {
    console.error('Error syncing notifications:', error);
  }
}

// Helper function to get stored notification data
async function getStoredNotificationData() {
  try {
    const cache = await caches.open('notification-data');
    const response = await cache.match('pending-notifications');
    if (response) {
      return await response.json();
    }
  } catch (error) {
    console.error('Error getting stored notification data:', error);
  }
  return [];
}

// Helper function to clear stored notification data
async function clearStoredNotificationData() {
  try {
    const cache = await caches.open('notification-data');
    await cache.delete('pending-notifications');
  } catch (error) {
    console.error('Error clearing stored notification data:', error);
  }
}

// Helper function to store notification data for later sync
async function storeNotificationData(data) {
  try {
    const cache = await caches.open('notification-data');
    const existingData = await getStoredNotificationData();
    const newData = [...existingData, data];
    
    await cache.put('pending-notifications', new Response(JSON.stringify(newData)));
  } catch (error) {
    console.error('Error storing notification data:', error);
  }
}

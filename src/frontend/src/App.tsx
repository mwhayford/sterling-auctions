import React, { useState } from 'react';
import './index.css';
import { ConnectionStatus, StatusBadge } from './components/ConnectionStatus';
import { NotificationCenter, NotificationToast } from './components/NotificationCenter';
import { AuctionLiveView } from './components/AuctionLiveView';
import { useSignalR } from './hooks/useSignalR';

function App() {
  const [apiStatus, setApiStatus] = React.useState<string>('checking...');
  const [showLiveDemo, setShowLiveDemo] = useState(false);
  const [toastNotifications, setToastNotifications] = useState<any[]>([]);
  
  const { connectionStatus } = useSignalR({
    autoConnect: false, // Disable auto-connect to prevent rate limiting issues
    onConnected: () => {
      console.log('SignalR connected successfully');
    },
    onError: (error) => {
      console.error('SignalR error:', error);
      // If SignalR fails repeatedly, we can disable it
      if (error.includes('Failed to start the connection') || error.includes('timeout')) {
        console.warn('SignalR connection failed, disabling auto-connect');
      }
    }
  });

  React.useEffect(() => {
    fetch('http://localhost:5000/health')
      .then(response => response.json())
      .then(data => {
        setApiStatus(`API is ${data.status}`);
      })
      .catch(() => {
        setApiStatus('API connection failed');
      });
  }, []);

  // Handle notification toasts
  const addToastNotification = (notification: any) => {
    const toast = { ...notification, id: Date.now() };
    setToastNotifications(prev => [...prev, toast]);
  };

  const removeToastNotification = (id: number) => {
    setToastNotifications(prev => prev.filter(toast => toast.id !== id));
  };

  // Set up SignalR event listeners for toasts
  React.useEffect(() => {
    // This would be set up in a real app with the SignalR service
    // For demo purposes, we'll simulate some notifications
    if (connectionStatus.isConnected) {
      const timer = setTimeout(() => {
        addToastNotification({
          type: 'SystemAnnouncement',
          message: 'Welcome to Sterling Auctions! Real-time features are now active.',
          timestamp: new Date().toISOString()
        });
      }, 2000);

      return () => clearTimeout(timer);
    }
  }, [connectionStatus.isConnected]);

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
      {/* Toast Notifications */}
      {toastNotifications.map(toast => (
        <NotificationToast
          key={toast.id}
          notification={toast}
          onClose={() => removeToastNotification(toast.id)}
        />
      ))}

      {/* Header */}
      <header className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center py-6">
            <div className="flex items-center">
              <h1 className="text-2xl font-bold text-gray-900">Sterling Auctions</h1>
              <span className="ml-3 px-2 py-1 text-xs font-medium bg-blue-100 text-blue-800 rounded-full">
                Beta
              </span>
              <StatusBadge className="ml-3" />
            </div>
            <nav className="flex items-center space-x-8">
              <a href="#" className="text-gray-600 hover:text-gray-900">Auctions</a>
              <a href="#" className="text-gray-600 hover:text-gray-900">How it Works</a>
              <a href="#" className="text-gray-600 hover:text-gray-900">Login</a>
              <NotificationCenter />
              <button className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700">
                Sign Up
              </button>
            </nav>
          </div>
        </div>
      </header>

      {/* Hero Section */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="text-center">
          <h2 className="text-4xl font-extrabold text-gray-900 sm:text-5xl md:text-6xl">
            Premium Online 
            <span className="text-blue-600"> Auctions</span>
          </h2>
          <p className="mt-3 max-w-md mx-auto text-base text-gray-500 sm:text-lg md:mt-5 md:text-xl md:max-w-3xl">
            Discover exceptional items, place competitive bids, and win amazing deals at Sterling Auctions.
          </p>
          
          {/* API Status */}
          <div className="mt-8 p-4 bg-white rounded-lg shadow-sm">
            <p className="text-sm text-gray-600">
              Backend Status: <span className={`font-medium ${apiStatus.includes('healthy') ? 'text-green-600' : 'text-red-600'}`}>
                {apiStatus}
              </span>
            </p>
            <div className="mt-2">
              <ConnectionStatus />
            </div>
          </div>

          {/* Action Buttons */}
          <div className="mt-10 flex justify-center space-x-6">
            <button 
              onClick={() => setShowLiveDemo(!showLiveDemo)}
              className="bg-blue-600 text-white px-8 py-3 rounded-md text-lg font-medium hover:bg-blue-700"
            >
              {showLiveDemo ? 'Hide Live Demo' : 'Try Live Demo'}
            </button>
            <button className="border border-gray-300 text-gray-700 px-8 py-3 rounded-md text-lg font-medium hover:bg-gray-50">
              Learn More
            </button>
          </div>
        </div>

        {/* Live Demo Section */}
        {showLiveDemo && (
          <div className="mt-12">
            <div className="text-center mb-8">
              <h3 className="text-2xl font-bold text-gray-900">Live Auction Demo</h3>
              <p className="text-gray-600 mt-2">Experience real-time bidding with SignalR</p>
            </div>
            
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
              {/* Live Auction View */}
              <div className="lg:col-span-2">
                <AuctionLiveView
                  auctionId={1}
                  auctionTitle="Vintage Rolex Submariner"
                  currentBid={1250}
                  endTime={new Date(Date.now() + 2 * 60 * 60 * 1000).toISOString()} // 2 hours from now
                  onBidPlaced={(amount) => {
                    console.log('Bid placed:', amount);
                    addToastNotification({
                      type: 'BidPlaced',
                      message: `Your bid of $${amount} has been placed!`,
                      timestamp: new Date().toISOString()
                    });
                  }}
                  onError={(error) => {
                    console.error('Bid error:', error);
                    addToastNotification({
                      type: 'BidFailed',
                      message: error,
                      timestamp: new Date().toISOString()
                    });
                  }}
                />
              </div>

              {/* Connection Status Details */}
              <div>
                <ConnectionStatus showDetails={true} />
              </div>
            </div>
          </div>
        )}

        {/* Features */}
        <div className="mt-20">
          <div className="grid grid-cols-1 gap-8 sm:grid-cols-2 lg:grid-cols-3">
            <div className="bg-white p-6 rounded-lg shadow-sm">
              <div className="text-blue-600 mb-3">
                <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <h3 className="text-lg font-medium text-gray-900">Real-time Bidding</h3>
              <p className="mt-2 text-gray-600">Experience live auction excitement with real-time bid updates and notifications powered by SignalR.</p>
            </div>
            
            <div className="bg-white p-6 rounded-lg shadow-sm">
              <div className="text-blue-600 mb-3">
                <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <h3 className="text-lg font-medium text-gray-900">Secure Payments</h3>
              <p className="mt-2 text-gray-600">Safe and secure transactions with industry-leading payment processing.</p>
            </div>
            
            <div className="bg-white p-6 rounded-lg shadow-sm">
              <div className="text-blue-600 mb-3">
                <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                </svg>
              </div>
              <h3 className="text-lg font-medium text-gray-900">Fast & Reliable</h3>
              <p className="mt-2 text-gray-600">Built with modern technology including SignalR for real-time communication, Redis caching, and Docker deployment.</p>
            </div>
          </div>
        </div>

        {/* SignalR Features */}
        <div className="mt-16">
          <div className="text-center mb-8">
            <h3 className="text-2xl font-bold text-gray-900">Real-Time Features</h3>
            <p className="text-gray-600 mt-2">Powered by SignalR WebSocket connections</p>
          </div>
          
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
            <div className="bg-white p-6 rounded-lg shadow-sm border-l-4 border-green-500">
              <div className="flex items-center">
                <div className="w-3 h-3 bg-green-500 rounded-full mr-3"></div>
                <h4 className="font-medium text-gray-900">Live Bidding</h4>
              </div>
              <p className="mt-2 text-sm text-gray-600">Real-time bid updates and notifications</p>
            </div>
            
            <div className="bg-white p-6 rounded-lg shadow-sm border-l-4 border-blue-500">
              <div className="flex items-center">
                <div className="w-3 h-3 bg-blue-500 rounded-full mr-3"></div>
                <h4 className="font-medium text-gray-900">Auction Chat</h4>
              </div>
              <p className="mt-2 text-sm text-gray-600">Communicate with other bidders in real-time</p>
            </div>
            
            <div className="bg-white p-6 rounded-lg shadow-sm border-l-4 border-yellow-500">
              <div className="flex items-center">
                <div className="w-3 h-3 bg-yellow-500 rounded-full mr-3"></div>
                <h4 className="font-medium text-gray-900">Notifications</h4>
              </div>
              <p className="mt-2 text-sm text-gray-600">Instant alerts for auction events</p>
            </div>
            
            <div className="bg-white p-6 rounded-lg shadow-sm border-l-4 border-purple-500">
              <div className="flex items-center">
                <div className="w-3 h-3 bg-purple-500 rounded-full mr-3"></div>
                <h4 className="font-medium text-gray-900">Auto-Reconnect</h4>
              </div>
              <p className="mt-2 text-sm text-gray-600">Automatic reconnection on connection loss</p>
            </div>
          </div>
        </div>
      </main>

      {/* Footer */}
      <footer className="bg-white border-t mt-20">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="flex justify-between items-center">
            <p className="text-gray-500">© 2025 Sterling Auctions. All rights reserved.</p>
            <div className="flex space-x-6">
              <a href="#" className="text-gray-400 hover:text-gray-500">Privacy</a>
              <a href="#" className="text-gray-400 hover:text-gray-500">Terms</a>
              <a href="#" className="text-gray-400 hover:text-gray-500">Support</a>
            </div>
          </div>
        </div>
      </footer>
    </div>
  );
}

export default App;

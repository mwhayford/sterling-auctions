import React, { useState, useEffect } from 'react';
import { useAuctionSignalR } from '../hooks/useSignalR';

interface AuctionLiveViewProps {
  auctionId: number;
  auctionTitle: string;
  currentBid?: number;
  endTime?: string;
  onBidPlaced?: (amount: number) => void;
  onError?: (error: string) => void;
}

export const AuctionLiveView: React.FC<AuctionLiveViewProps> = ({
  auctionId,
  auctionTitle,
  currentBid: initialBid,
  endTime,
  onBidPlaced,
  onError
}) => {
  const {
    connectionStatus,
    isJoined,
    currentBid,
    bidHistory,
    auctionStats,
    placeBid,
    sendMessage,
    getStats
  } = useAuctionSignalR(auctionId);

  const [bidAmount, setBidAmount] = useState<string>('');
  const [message, setMessage] = useState<string>('');
  const [isPlacingBid, setIsPlacingBid] = useState(false);
  const [timeRemaining, setTimeRemaining] = useState<string>('');

  // Calculate time remaining
  useEffect(() => {
    if (endTime) {
      const updateTimeRemaining = () => {
        const now = new Date().getTime();
        const end = new Date(endTime).getTime();
        const diff = end - now;

        if (diff > 0) {
          const days = Math.floor(diff / (1000 * 60 * 60 * 24));
          const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
          const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
          const seconds = Math.floor((diff % (1000 * 60)) / 1000);

          if (days > 0) {
            setTimeRemaining(`${days}d ${hours}h ${minutes}m ${seconds}s`);
          } else if (hours > 0) {
            setTimeRemaining(`${hours}h ${minutes}m ${seconds}s`);
          } else if (minutes > 0) {
            setTimeRemaining(`${minutes}m ${seconds}s`);
          } else {
            setTimeRemaining(`${seconds}s`);
          }
        } else {
          setTimeRemaining('Ended');
        }
      };

      updateTimeRemaining();
      const interval = setInterval(updateTimeRemaining, 1000);

      return () => clearInterval(interval);
    }
  }, [endTime]);

  // Handle bid placement
  const handlePlaceBid = async () => {
    const amount = parseFloat(bidAmount);
    if (isNaN(amount) || amount <= 0) {
      onError?.('Please enter a valid bid amount');
      return;
    }

    if (currentBid && amount <= currentBid) {
      onError?.('Bid must be higher than current bid');
      return;
    }

    setIsPlacingBid(true);
    try {
      await placeBid(amount);
      setBidAmount('');
      onBidPlaced?.(amount);
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to place bid';
      onError?.(errorMessage);
    } finally {
      setIsPlacingBid(false);
    }
  };

  // Handle sending message
  const handleSendMessage = async () => {
    if (!message.trim()) return;

    try {
      await sendMessage(message.trim());
      setMessage('');
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to send message';
      onError?.(errorMessage);
    }
  };

  // Get auction stats
  useEffect(() => {
    if (isJoined) {
      getStats();
    }
  }, [isJoined, getStats]);

  const displayBid = currentBid || initialBid || 0;

  return (
    <div className="auction-live-view bg-white rounded-lg shadow-lg p-6">
      {/* Connection Status */}
      <div className="mb-4">
        <div className="flex items-center justify-between">
          <h2 className="text-2xl font-bold text-gray-800">{auctionTitle}</h2>
          <div className="flex items-center space-x-2">
            <div className={`w-3 h-3 rounded-full ${
              connectionStatus.isConnected ? 'bg-green-500' : 'bg-red-500'
            }`}></div>
            <span className="text-sm text-gray-600">
              {connectionStatus.isConnected ? 'Connected' : 'Disconnected'}
            </span>
            {isJoined && (
              <span className="text-sm text-green-600 font-medium">Joined</span>
            )}
          </div>
        </div>
      </div>

      {/* Auction Info */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <div className="bg-gray-50 p-4 rounded-lg">
          <h3 className="text-sm font-medium text-gray-600">Current Bid</h3>
          <p className="text-2xl font-bold text-green-600">${displayBid.toFixed(2)}</p>
        </div>
        <div className="bg-gray-50 p-4 rounded-lg">
          <h3 className="text-sm font-medium text-gray-600">Time Remaining</h3>
          <p className="text-xl font-bold text-red-600">{timeRemaining}</p>
        </div>
        <div className="bg-gray-50 p-4 rounded-lg">
          <h3 className="text-sm font-medium text-gray-600">Total Bids</h3>
          <p className="text-xl font-bold text-blue-600">{bidHistory.length}</p>
        </div>
      </div>

      {/* Bid Section */}
      <div className="mb-6">
        <h3 className="text-lg font-semibold mb-3">Place a Bid</h3>
        <div className="flex space-x-2">
          <input
            type="number"
            value={bidAmount}
            onChange={(e) => setBidAmount(e.target.value)}
            placeholder="Enter bid amount"
            className="flex-1 px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            disabled={isPlacingBid || !connectionStatus.isConnected}
          />
          <button
            onClick={handlePlaceBid}
            disabled={isPlacingBid || !connectionStatus.isConnected || !bidAmount}
            className="px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed"
          >
            {isPlacingBid ? 'Placing...' : 'Place Bid'}
          </button>
        </div>
      </div>

      {/* Auction Chat */}
      <div className="mb-6">
        <h3 className="text-lg font-semibold mb-3">Auction Chat</h3>
        <div className="flex space-x-2">
          <input
            type="text"
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            placeholder="Type a message..."
            className="flex-1 px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            disabled={!connectionStatus.isConnected}
            onKeyPress={(e) => e.key === 'Enter' && handleSendMessage()}
          />
          <button
            onClick={handleSendMessage}
            disabled={!connectionStatus.isConnected || !message.trim()}
            className="px-4 py-2 bg-gray-600 text-white rounded-md hover:bg-gray-700 disabled:bg-gray-400 disabled:cursor-not-allowed"
          >
            Send
          </button>
        </div>
      </div>

      {/* Recent Bids */}
      <div className="mb-6">
        <h3 className="text-lg font-semibold mb-3">Recent Bids</h3>
        <div className="max-h-48 overflow-y-auto">
          {bidHistory.length === 0 ? (
            <p className="text-gray-500 text-center py-4">No bids yet</p>
          ) : (
            <div className="space-y-2">
              {bidHistory.slice(0, 10).map((bid, index) => (
                <div key={index} className="flex justify-between items-center p-2 bg-gray-50 rounded">
                  <div>
                    <span className="font-medium">{bid.bidderName}</span>
                    <span className="text-gray-600 ml-2">${bid.amount.toFixed(2)}</span>
                  </div>
                  <span className="text-sm text-gray-500">
                    {new Date(bid.bidTime).toLocaleTimeString()}
                  </span>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Auction Stats */}
      {auctionStats && (
        <div className="bg-blue-50 p-4 rounded-lg">
          <h3 className="text-lg font-semibold mb-3">Auction Statistics</h3>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div>
              <p className="text-sm text-gray-600">Total Auctions</p>
              <p className="text-xl font-bold">{auctionStats.totalAuctions}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600">Active Auctions</p>
              <p className="text-xl font-bold">{auctionStats.activeAuctions}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600">Total Bids</p>
              <p className="text-xl font-bold">{auctionStats.totalBids}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600">Total Users</p>
              <p className="text-xl font-bold">{auctionStats.totalUsers}</p>
            </div>
          </div>
        </div>
      )}

      {/* Connection Error */}
      {connectionStatus.error && (
        <div className="mt-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded">
          <p className="font-medium">Connection Error:</p>
          <p>{connectionStatus.error}</p>
        </div>
      )}
    </div>
  );
};

import React, { useState, useEffect } from 'react';

interface SignalRPerformanceMetricsProps {
  authToken?: string;
  refreshInterval?: number;
}

interface PerformanceSummary {
  totalConnections: number;
  activeConnections: number;
  disconnectedConnections: number;
  totalMessages: number;
  messagesPerSecond: number;
  totalBytes: number;
  bytesPerSecond: number;
  averageLatency: number;
  maxLatency: number;
  minLatency: number;
  totalErrors: number;
  totalReconnects: number;
  errorRate: number;
  reconnectRate: number;
  connectionsByTransport: Record<string, number>;
  messagesByType: Record<string, number>;
  errorsByType: Record<string, number>;
  topConnections: Array<{
    connectionId: string;
    userId: string;
    transport: string;
    messagesSent: number;
    messagesReceived: number;
    averageLatency: number;
    errorCount: number;
  }>;
  hubMetrics: Array<{
    hubName: string;
    activeConnections: number;
    totalConnections: number;
    messagesPerSecond: number;
    averageLatency: number;
    errorRate: number;
    reconnectRate: number;
  }>;
}

interface ConnectionHealth {
  connectionId: string;
  userId: string;
  transport: string;
  isHealthy: boolean;
  latency: number;
  lastHeartbeat: string;
  messageCount: number;
  errorCount: number;
  healthStatus: string;
  issues: string[];
}

interface PerformanceAlert {
  id: number;
  alertType: string;
  severity: string;
  title: string;
  description: string;
  connectionId?: string;
  userId?: string;
  hubName?: string;
  triggeredAt: string;
  isResolved: boolean;
}

export const SignalRPerformanceDashboard: React.FC<SignalRPerformanceMetricsProps> = ({
  authToken,
  refreshInterval = 30000
}) => {
  const [summary, setSummary] = useState<PerformanceSummary | null>(null);
  const [connectionHealth, setConnectionHealth] = useState<ConnectionHealth[]>([]);
  const [alerts, setAlerts] = useState<PerformanceAlert[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'overview' | 'connections' | 'alerts' | 'hubs'>('overview');

  const fetchPerformanceData = async () => {
    if (!authToken) return;

    try {
      setIsLoading(true);
      setError(null);

      const [summaryResponse, healthResponse, alertsResponse] = await Promise.all([
        fetch('/api/signalrperformance/summary', {
          headers: { 'Authorization': `Bearer ${authToken}` }
        }),
        fetch('/api/signalrperformance/health', {
          headers: { 'Authorization': `Bearer ${authToken}` }
        }),
        fetch('/api/signalrperformance/alerts', {
          headers: { 'Authorization': `Bearer ${authToken}` }
        })
      ]);

      if (summaryResponse.ok) {
        const summaryData = await summaryResponse.json();
        setSummary(summaryData);
      }

      if (healthResponse.ok) {
        const healthData = await healthResponse.json();
        setConnectionHealth(healthData);
      }

      if (alertsResponse.ok) {
        const alertsData = await alertsResponse.json();
        setAlerts(alertsData);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch performance data');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchPerformanceData();
    
    if (refreshInterval > 0) {
      const interval = setInterval(fetchPerformanceData, refreshInterval);
      return () => clearInterval(interval);
    }
  }, [authToken, refreshInterval]);

  const getHealthStatusColor = (status: string) => {
    switch (status) {
      case 'healthy': return 'text-green-600 bg-green-100';
      case 'warning': return 'text-yellow-600 bg-yellow-100';
      case 'critical': return 'text-red-600 bg-red-100';
      default: return 'text-gray-600 bg-gray-100';
    }
  };

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'low': return 'text-blue-600 bg-blue-100';
      case 'medium': return 'text-yellow-600 bg-yellow-100';
      case 'high': return 'text-orange-600 bg-orange-100';
      case 'critical': return 'text-red-600 bg-red-100';
      default: return 'text-gray-600 bg-gray-100';
    }
  };

  const formatBytes = (bytes: number) => {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const formatLatency = (latency: number) => {
    return `${latency.toFixed(1)}ms`;
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        <span className="ml-2 text-gray-600">Loading performance data...</span>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-red-50 border border-red-200 rounded-lg p-4">
        <div className="flex items-center">
          <svg className="w-5 h-5 text-red-600 mr-2" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
          </svg>
          <span className="text-red-800">{error}</span>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-lg p-6">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-xl font-semibold text-gray-800">SignalR Performance Dashboard</h2>
        <div className="flex items-center space-x-2">
          <div className={`w-3 h-3 rounded-full ${
            summary && summary.errorRate < 5 ? 'bg-green-500' : 'bg-red-500'
          }`}></div>
          <span className="text-sm text-gray-600">
            {summary && summary.errorRate < 5 ? 'Healthy' : 'Issues Detected'}
          </span>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex space-x-1 mb-6">
        {[
          { id: 'overview', label: 'Overview' },
          { id: 'connections', label: 'Connections' },
          { id: 'alerts', label: 'Alerts' },
          { id: 'hubs', label: 'Hubs' }
        ].map((tab) => (
          <button
            key={tab.id}
            onClick={() => setActiveTab(tab.id as any)}
            className={`px-4 py-2 text-sm rounded-md ${
              activeTab === tab.id
                ? 'bg-blue-100 text-blue-700'
                : 'text-gray-600 hover:text-gray-800'
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* Overview Tab */}
      {activeTab === 'overview' && summary && (
        <div className="space-y-6">
          {/* Key Metrics */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div className="bg-blue-50 p-4 rounded-lg">
              <div className="text-2xl font-bold text-blue-600">{summary.activeConnections}</div>
              <div className="text-sm text-blue-800">Active Connections</div>
            </div>
            <div className="bg-green-50 p-4 rounded-lg">
              <div className="text-2xl font-bold text-green-600">{summary.messagesPerSecond}</div>
              <div className="text-sm text-green-800">Messages/sec</div>
            </div>
            <div className="bg-yellow-50 p-4 rounded-lg">
              <div className="text-2xl font-bold text-yellow-600">{formatLatency(summary.averageLatency)}</div>
              <div className="text-sm text-yellow-800">Avg Latency</div>
            </div>
            <div className="bg-red-50 p-4 rounded-lg">
              <div className="text-2xl font-bold text-red-600">{summary.errorRate.toFixed(1)}%</div>
              <div className="text-sm text-red-800">Error Rate</div>
            </div>
          </div>

          {/* Transport Distribution */}
          <div className="bg-gray-50 p-4 rounded-lg">
            <h3 className="text-lg font-semibold mb-3">Connections by Transport</h3>
            <div className="grid grid-cols-2 gap-4">
              {Object.entries(summary.connectionsByTransport).map(([transport, count]) => (
                <div key={transport} className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">{transport}</span>
                  <span className="font-semibold">{count}</span>
                </div>
              ))}
            </div>
          </div>

          {/* Message Types */}
          <div className="bg-gray-50 p-4 rounded-lg">
            <h3 className="text-lg font-semibold mb-3">Messages by Type</h3>
            <div className="grid grid-cols-2 gap-4">
              {Object.entries(summary.messagesByType).map(([type, count]) => (
                <div key={type} className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">{type}</span>
                  <span className="font-semibold">{count}</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* Connections Tab */}
      {activeTab === 'connections' && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h3 className="text-lg font-semibold">Connection Health</h3>
            <span className="text-sm text-gray-600">{connectionHealth.length} connections</span>
          </div>
          
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Connection</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Transport</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Latency</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Messages</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Errors</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {connectionHealth.map((health) => (
                  <tr key={health.connectionId}>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">{health.userId}</div>
                      <div className="text-sm text-gray-500">{health.connectionId.substring(0, 8)}...</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {health.transport}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getHealthStatusColor(health.healthStatus)}`}>
                        {health.healthStatus}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {formatLatency(health.latency)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {health.messageCount}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {health.errorCount}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Alerts Tab */}
      {activeTab === 'alerts' && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h3 className="text-lg font-semibold">Performance Alerts</h3>
            <span className="text-sm text-gray-600">{alerts.length} active alerts</span>
          </div>
          
          <div className="space-y-3">
            {alerts.map((alert) => (
              <div key={alert.id} className="border border-gray-200 rounded-lg p-4">
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center space-x-2 mb-2">
                      <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getSeverityColor(alert.severity)}`}>
                        {alert.severity}
                      </span>
                      <span className="text-sm font-medium text-gray-900">{alert.title}</span>
                    </div>
                    <p className="text-sm text-gray-600 mb-2">{alert.description}</p>
                    <div className="text-xs text-gray-500">
                      {alert.connectionId && `Connection: ${alert.connectionId.substring(0, 8)}...`}
                      {alert.userId && ` | User: ${alert.userId}`}
                      {alert.hubName && ` | Hub: ${alert.hubName}`}
                    </div>
                  </div>
                  <div className="text-xs text-gray-500">
                    {new Date(alert.triggeredAt).toLocaleString()}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Hubs Tab */}
      {activeTab === 'hubs' && summary && (
        <div className="space-y-4">
          <h3 className="text-lg font-semibold">Hub Metrics</h3>
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {summary.hubMetrics.map((hub) => (
              <div key={hub.hubName} className="bg-gray-50 p-4 rounded-lg">
                <h4 className="font-semibold text-gray-900 mb-3">{hub.hubName}</h4>
                <div className="grid grid-cols-2 gap-2 text-sm">
                  <div>
                    <span className="text-gray-600">Active:</span>
                    <span className="font-semibold ml-1">{hub.activeConnections}</span>
                  </div>
                  <div>
                    <span className="text-gray-600">Total:</span>
                    <span className="font-semibold ml-1">{hub.totalConnections}</span>
                  </div>
                  <div>
                    <span className="text-gray-600">Msg/sec:</span>
                    <span className="font-semibold ml-1">{hub.messagesPerSecond}</span>
                  </div>
                  <div>
                    <span className="text-gray-600">Latency:</span>
                    <span className="font-semibold ml-1">{formatLatency(hub.averageLatency)}</span>
                  </div>
                  <div>
                    <span className="text-gray-600">Error Rate:</span>
                    <span className="font-semibold ml-1">{hub.errorRate}%</span>
                  </div>
                  <div>
                    <span className="text-gray-600">Reconnect Rate:</span>
                    <span className="font-semibold ml-1">{hub.reconnectRate}%</span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

import React, { useState, useEffect } from 'react';

interface SignalRLoadTestDashboardProps {
  authToken?: string;
  refreshInterval?: number;
}

interface LoadTestConfig {
  id: number;
  name: string;
  description: string;
  testType: string;
  scenario: string;
  concurrentUsers: number;
  durationMinutes: number;
  messagesPerSecond: number;
  messageSizeBytes: number;
  hubUrl: string;
  transportTypes: string[];
  isActive: boolean;
  createdAt: string;
}

interface LoadTestExecution {
  id: number;
  executionId: string;
  status: string;
  testType: string;
  startedAt: string;
  completedAt?: string;
  plannedUsers: number;
  actualUsers: number;
  successfulConnections: number;
  failedConnections: number;
  totalMessages: number;
  averageLatency: number;
  maxLatency: number;
  minLatency: number;
  messagesPerSecond: number;
  errorRate: number;
  connectionSuccessRate: number;
}

interface LoadTestSummary {
  totalExecutions: number;
  successfulExecutions: number;
  failedExecutions: number;
  cancelledExecutions: number;
  averageLatency: number;
  maxLatency: number;
  minLatency: number;
  averageMessagesPerSecond: number;
  averageErrorRate: number;
  averageConnectionSuccessRate: number;
  executionsByType: Record<string, number>;
  executionsByScenario: Record<string, number>;
  executionsByStatus: Record<string, number>;
  recentExecutions: LoadTestExecution[];
}

interface RealTimeMetrics {
  executionId: string;
  status: string;
  timestamp: string;
  activeConnections: number;
  totalConnections: number;
  failedConnections: number;
  messagesInLastSecond: number;
  messagesInLastMinute: number;
  totalMessages: number;
  currentLatency: number;
  averageLatency: number;
  maxLatency: number;
  currentErrorRate: number;
  averageErrorRate: number;
  connectionsByTransport: Record<string, number>;
  messagesByType: Record<string, number>;
  errorsByType: Record<string, number>;
}

export const SignalRLoadTestDashboard: React.FC<SignalRLoadTestDashboardProps> = ({
  authToken,
  refreshInterval = 10000
}) => {
  const [configs, setConfigs] = useState<LoadTestConfig[]>([]);
  const [executions, setExecutions] = useState<LoadTestExecution[]>([]);
  const [summary, setSummary] = useState<LoadTestSummary | null>(null);
  const [realTimeMetrics, setRealTimeMetrics] = useState<RealTimeMetrics | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'overview' | 'configs' | 'executions' | 'realtime'>('overview');
  const [selectedExecution, setSelectedExecution] = useState<string | null>(null);

  const fetchData = async () => {
    if (!authToken) return;

    try {
      setIsLoading(true);
      setError(null);

      const [configsResponse, executionsResponse, summaryResponse] = await Promise.all([
        fetch('/api/signalrloadtest/configs', {
          headers: { 'Authorization': `Bearer ${authToken}` }
        }),
        fetch('/api/signalrloadtest/executions', {
          headers: { 'Authorization': `Bearer ${authToken}` }
        }),
        fetch('/api/signalrloadtest/summary', {
          headers: { 'Authorization': `Bearer ${authToken}` }
        })
      ]);

      if (configsResponse.ok) {
        const configsData = await configsResponse.json();
        setConfigs(configsData);
      }

      if (executionsResponse.ok) {
        const executionsData = await executionsResponse.json();
        setExecutions(executionsData);
      }

      if (summaryResponse.ok) {
        const summaryData = await summaryResponse.json();
        setSummary(summaryData);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch load test data');
    } finally {
      setIsLoading(false);
    }
  };

  const fetchRealTimeMetrics = async (executionId: string) => {
    if (!authToken) return;

    try {
      const response = await fetch(`/api/signalrloadtest/executions/${executionId}/metrics`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });

      if (response.ok) {
        const metrics = await response.json();
        setRealTimeMetrics(metrics);
      }
    } catch (err) {
      console.error('Error fetching real-time metrics:', err);
    }
  };

  useEffect(() => {
    fetchData();
    
    if (refreshInterval > 0) {
      const interval = setInterval(fetchData, refreshInterval);
      return () => clearInterval(interval);
    }
  }, [authToken, refreshInterval]);

  useEffect(() => {
    if (selectedExecution && refreshInterval > 0) {
      const interval = setInterval(() => fetchRealTimeMetrics(selectedExecution), 2000);
      return () => clearInterval(interval);
    }
  }, [selectedExecution, refreshInterval]);

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'completed': return 'text-green-600 bg-green-100';
      case 'running': return 'text-blue-600 bg-blue-100';
      case 'failed': return 'text-red-600 bg-red-100';
      case 'cancelled': return 'text-gray-600 bg-gray-100';
      case 'paused': return 'text-yellow-600 bg-yellow-100';
      default: return 'text-gray-600 bg-gray-100';
    }
  };

  const getTestTypeColor = (testType: string) => {
    switch (testType.toLowerCase()) {
      case 'load': return 'text-blue-600 bg-blue-100';
      case 'stress': return 'text-red-600 bg-red-100';
      case 'spike': return 'text-orange-600 bg-orange-100';
      case 'volume': return 'text-purple-600 bg-purple-100';
      case 'endurance': return 'text-green-600 bg-green-100';
      case 'scalability': return 'text-indigo-600 bg-indigo-100';
      default: return 'text-gray-600 bg-gray-100';
    }
  };

  const formatDuration = (startedAt: string, completedAt?: string) => {
    const start = new Date(startedAt);
    const end = completedAt ? new Date(completedAt) : new Date();
    const duration = end.getTime() - start.getTime();
    
    const minutes = Math.floor(duration / 60000);
    const seconds = Math.floor((duration % 60000) / 1000);
    
    return `${minutes}m ${seconds}s`;
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

  const runQuickTest = async (testType: string, users: number, duration: number, messagesPerSecond?: number) => {
    if (!authToken) return;

    try {
      let url = `/api/signalrloadtest/quick/${testType}?users=${users}&durationMinutes=${duration}`;
      if (messagesPerSecond) {
        url += `&messagesPerSecond=${messagesPerSecond}`;
      }

      const response = await fetch(url, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${authToken}` }
      });

      if (response.ok) {
        const execution = await response.json();
        setSelectedExecution(execution.executionId);
        setActiveTab('realtime');
        await fetchData(); // Refresh data
      } else {
        const error = await response.json();
        setError(error.message || 'Failed to start test');
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to start test');
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        <span className="ml-2 text-gray-600">Loading load test data...</span>
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
        <h2 className="text-xl font-semibold text-gray-800">SignalR Load Testing Dashboard</h2>
        <div className="flex items-center space-x-2">
          <div className={`w-3 h-3 rounded-full ${
            summary && summary.averageErrorRate < 5 ? 'bg-green-500' : 'bg-red-500'
          }`}></div>
          <span className="text-sm text-gray-600">
            {summary && summary.averageErrorRate < 5 ? 'System Healthy' : 'Issues Detected'}
          </span>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex space-x-1 mb-6">
        {[
          { id: 'overview', label: 'Overview' },
          { id: 'configs', label: 'Configurations' },
          { id: 'executions', label: 'Executions' },
          { id: 'realtime', label: 'Real-time' }
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
          {/* Quick Test Buttons */}
          <div className="bg-gray-50 p-4 rounded-lg">
            <h3 className="text-lg font-semibold mb-3">Quick Tests</h3>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <button
                onClick={() => runQuickTest('auction-bidding', 100, 10)}
                className="bg-blue-500 text-white px-4 py-2 rounded-md hover:bg-blue-600"
              >
                Auction Bidding<br/><span className="text-xs">100 users, 10min</span>
              </button>
              <button
                onClick={() => runQuickTest('connection-stress', 500, 5)}
                className="bg-red-500 text-white px-4 py-2 rounded-md hover:bg-red-600"
              >
                Connection Stress<br/><span className="text-xs">500 users, 5min</span>
              </button>
              <button
                onClick={() => runQuickTest('message-flood', 50, 5, 100)}
                className="bg-purple-500 text-white px-4 py-2 rounded-md hover:bg-purple-600"
              >
                Message Flood<br/><span className="text-xs">50 users, 100 msg/s</span>
              </button>
              <button
                onClick={() => runQuickTest('mixed-workload', 200, 15)}
                className="bg-green-500 text-white px-4 py-2 rounded-md hover:bg-green-600"
              >
                Mixed Workload<br/><span className="text-xs">200 users, 15min</span>
              </button>
            </div>
          </div>

          {/* Key Metrics */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div className="bg-blue-50 p-4 rounded-lg">
              <div className="text-2xl font-bold text-blue-600">{summary.totalExecutions}</div>
              <div className="text-sm text-blue-800">Total Tests</div>
            </div>
            <div className="bg-green-50 p-4 rounded-lg">
              <div className="text-2xl font-bold text-green-600">{summary.successfulExecutions}</div>
              <div className="text-sm text-green-800">Successful</div>
            </div>
            <div className="bg-yellow-50 p-4 rounded-lg">
              <div className="text-2xl font-bold text-yellow-600">{formatLatency(summary.averageLatency)}</div>
              <div className="text-sm text-yellow-800">Avg Latency</div>
            </div>
            <div className="bg-red-50 p-4 rounded-lg">
              <div className="text-2xl font-bold text-red-600">{summary.averageErrorRate.toFixed(1)}%</div>
              <div className="text-sm text-red-800">Error Rate</div>
            </div>
          </div>

          {/* Test Types Distribution */}
          <div className="bg-gray-50 p-4 rounded-lg">
            <h3 className="text-lg font-semibold mb-3">Tests by Type</h3>
            <div className="grid grid-cols-2 gap-4">
              {Object.entries(summary.executionsByType).map(([type, count]) => (
                <div key={type} className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">{type}</span>
                  <span className="font-semibold">{count}</span>
                </div>
              ))}
            </div>
          </div>

          {/* Recent Executions */}
          <div className="bg-gray-50 p-4 rounded-lg">
            <h3 className="text-lg font-semibold mb-3">Recent Executions</h3>
            <div className="space-y-2">
              {summary.recentExecutions.slice(0, 5).map((execution) => (
                <div key={execution.id} className="flex justify-between items-center p-2 bg-white rounded">
                  <div>
                    <span className="font-medium">{execution.executionId.substring(0, 8)}...</span>
                    <span className={`ml-2 px-2 py-1 text-xs rounded-full ${getStatusColor(execution.status)}`}>
                      {execution.status}
                    </span>
                  </div>
                  <div className="text-sm text-gray-600">
                    {execution.actualUsers} users, {formatLatency(execution.averageLatency)}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* Configurations Tab */}
      {activeTab === 'configs' && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h3 className="text-lg font-semibold">Test Configurations</h3>
            <span className="text-sm text-gray-600">{configs.length} configurations</span>
          </div>
          
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Name</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Type</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Scenario</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Users</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Duration</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Messages/s</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {configs.map((config) => (
                  <tr key={config.id}>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">{config.name}</div>
                      <div className="text-sm text-gray-500">{config.description}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getTestTypeColor(config.testType)}`}>
                        {config.testType}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {config.scenario}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {config.concurrentUsers}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {config.durationMinutes}m
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {config.messagesPerSecond}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${
                        config.isActive ? 'text-green-600 bg-green-100' : 'text-gray-600 bg-gray-100'
                      }`}>
                        {config.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Executions Tab */}
      {activeTab === 'executions' && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h3 className="text-lg font-semibold">Test Executions</h3>
            <span className="text-sm text-gray-600">{executions.length} executions</span>
          </div>
          
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Execution</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Type</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Users</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Duration</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Latency</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Error Rate</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {executions.map((execution) => (
                  <tr key={execution.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">
                        {execution.executionId.substring(0, 8)}...
                      </div>
                      <div className="text-sm text-gray-500">
                        {new Date(execution.startedAt).toLocaleString()}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getStatusColor(execution.status)}`}>
                        {execution.status}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getTestTypeColor(execution.testType)}`}>
                        {execution.testType}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {execution.actualUsers}/{execution.plannedUsers}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {formatDuration(execution.startedAt, execution.completedAt)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {formatLatency(execution.averageLatency)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {execution.errorRate.toFixed(1)}%
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Real-time Tab */}
      {activeTab === 'realtime' && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h3 className="text-lg font-semibold">Real-time Metrics</h3>
            <div className="flex items-center space-x-2">
              <label htmlFor="execution-select" className="text-sm text-gray-600">Execution:</label>
              <select
                id="execution-select"
                value={selectedExecution || ''}
                onChange={(e) => setSelectedExecution(e.target.value)}
                className="px-3 py-1 border border-gray-300 rounded-md text-sm"
              >
                <option value="">Select execution...</option>
                {executions.filter(e => e.status === 'Running').map((execution) => (
                  <option key={execution.id} value={execution.executionId}>
                    {execution.executionId.substring(0, 8)}... - {execution.testType}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {realTimeMetrics ? (
            <div className="space-y-4">
              {/* Current Metrics */}
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <div className="bg-blue-50 p-4 rounded-lg">
                  <div className="text-2xl font-bold text-blue-600">{realTimeMetrics.activeConnections}</div>
                  <div className="text-sm text-blue-800">Active Connections</div>
                </div>
                <div className="bg-green-50 p-4 rounded-lg">
                  <div className="text-2xl font-bold text-green-600">{realTimeMetrics.messagesInLastSecond}</div>
                  <div className="text-sm text-green-800">Messages/sec</div>
                </div>
                <div className="bg-yellow-50 p-4 rounded-lg">
                  <div className="text-2xl font-bold text-yellow-600">{formatLatency(realTimeMetrics.currentLatency)}</div>
                  <div className="text-sm text-yellow-800">Current Latency</div>
                </div>
                <div className="bg-red-50 p-4 rounded-lg">
                  <div className="text-2xl font-bold text-red-600">{realTimeMetrics.currentErrorRate.toFixed(1)}%</div>
                  <div className="text-sm text-red-800">Error Rate</div>
                </div>
              </div>

              {/* Transport Distribution */}
              <div className="bg-gray-50 p-4 rounded-lg">
                <h4 className="text-md font-semibold mb-3">Connections by Transport</h4>
                <div className="grid grid-cols-2 gap-4">
                  {Object.entries(realTimeMetrics.connectionsByTransport).map(([transport, count]) => (
                    <div key={transport} className="flex justify-between items-center">
                      <span className="text-sm text-gray-600">{transport}</span>
                      <span className="font-semibold">{count}</span>
                    </div>
                  ))}
                </div>
              </div>

              {/* Message Types */}
              <div className="bg-gray-50 p-4 rounded-lg">
                <h4 className="text-md font-semibold mb-3">Messages by Type</h4>
                <div className="grid grid-cols-2 gap-4">
                  {Object.entries(realTimeMetrics.messagesByType).map(([type, count]) => (
                    <div key={type} className="flex justify-between items-center">
                      <span className="text-sm text-gray-600">{type}</span>
                      <span className="font-semibold">{count}</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          ) : (
            <div className="text-center py-8 text-gray-500">
              <p>Select a running execution to view real-time metrics</p>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

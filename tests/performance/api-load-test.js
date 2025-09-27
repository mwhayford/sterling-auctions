import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');

export let options = {
  stages: [
    { duration: '2m', target: 10 }, // Ramp up
    { duration: '5m', target: 10 }, // Stay at 10 users
    { duration: '2m', target: 20 }, // Ramp up to 20 users
    { duration: '5m', target: 20 }, // Stay at 20 users
    { duration: '2m', target: 0 }, // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<2000'], // 95% of requests must complete below 2s
    http_req_failed: ['rate<0.1'], // Error rate must be below 10%
    errors: ['rate<0.1'], // Custom error rate must be below 10%
  },
};

const BASE_URL = 'http://localhost:5000';

export default function () {
  // Test API health endpoints
  let healthResponse = http.get(`${BASE_URL}/swagger`);
  check(healthResponse, {
    'health check status is 200': (r) => r.status === 200,
  }) || errorRate.add(1);

  sleep(1);

  // Test auction endpoints
  let auctionsResponse = http.get(`${BASE_URL}/api/auctions`);
  check(auctionsResponse, {
    'auctions endpoint status is 200': (r) => r.status === 200,
    'auctions response time < 1000ms': (r) => r.timings.duration < 1000,
  }) || errorRate.add(1);

  sleep(1);

  // Test specific auction
  let auctionResponse = http.get(`${BASE_URL}/api/auctions/1`);
  check(auctionResponse, {
    'auction detail status is 200': (r) => r.status === 200,
    'auction response time < 500ms': (r) => r.timings.duration < 500,
  }) || errorRate.add(1);

  sleep(1);

  // Test authentication endpoint
  let authPayload = JSON.stringify({
    email: 'test@example.com',
    password: 'TestPassword123!'
  });

  let authResponse = http.post(`${BASE_URL}/api/auth/login`, authPayload, {
    headers: { 'Content-Type': 'application/json' },
  });

  check(authResponse, {
    'auth endpoint status is 200 or 400': (r) => r.status === 200 || r.status === 400,
    'auth response time < 2000ms': (r) => r.timings.duration < 2000,
  }) || errorRate.add(1);

  sleep(2);
}

export function handleSummary(data) {
  return {
    'api-performance-results.json': JSON.stringify(data),
  };
}

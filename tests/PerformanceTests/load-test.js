import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');
const apiDuration = new Trend('api_duration');

// Test configuration
export const options = {
  stages: [
    { duration: '30s', target: 10 },   // Ramp up to 10 users
    { duration: '1m', target: 50 },    // Ramp up to 50 users
    { duration: '2m', target: 100 },   // Ramp up to 100 users
    { duration: '2m', target: 100 },   // Stay at 100 users
    { duration: '30s', target: 0 },    // Ramp down to 0 users
  ],
  thresholds: {
    'http_req_duration': ['p(95)<500', 'p(99)<1000'], // 95% of requests should be below 500ms, 99% below 1s
    'http_req_failed': ['rate<0.01'],   // Error rate should be less than 1%
    'errors': ['rate<0.1'],             // Custom error rate should be less than 10%
  },
};

const BASE_URL = __ENV.API_URL || 'http://localhost:5000';

// Test data
const testCategories = [
  { name: 'Electronics', slug: 'electronics' },
  { name: 'Clothing', slug: 'clothing' },
  { name: 'Books', slug: 'books' },
];

const testProducts = [
  { name: 'Laptop', price: 999.99, currency: 'USD' },
  { name: 'Smartphone', price: 699.99, currency: 'USD' },
  { name: 'Tablet', price: 499.99, currency: 'USD' },
];

export function setup() {
  // Create test categories
  const categories = [];
  for (const category of testCategories) {
    const res = http.post(
      `${BASE_URL}/api/v1/categories`,
      JSON.stringify(category),
      {
        headers: { 'Content-Type': 'application/json' },
      }
    );

    if (res.status === 201) {
      const categoryId = res.json();
      categories.push({ id: categoryId, ...category });
    }
  }

  return { categories };
}

export default function (data) {
  // Test 1: Health check
  testHealthCheck();
  sleep(1);

  // Test 2: Get categories
  testGetCategories();
  sleep(1);

  // Test 3: Create and manage products
  if (data.categories && data.categories.length > 0) {
    const categoryId = data.categories[0].id;
    testProductOperations(categoryId);
    sleep(1);
  }

  // Test 4: Search products
  testSearchProducts();
  sleep(1);
}

function testHealthCheck() {
  const res = http.get(`${BASE_URL}/health`);

  const success = check(res, {
    'health check status is 200': (r) => r.status === 200,
    'health check is healthy': (r) => {
      const body = JSON.parse(r.body);
      return body.status === 'Healthy';
    },
  });

  errorRate.add(!success);
  apiDuration.add(res.timings.duration);
}

function testGetCategories() {
  const res = http.get(`${BASE_URL}/api/v1/categories`);

  const success = check(res, {
    'get categories status is 200': (r) => r.status === 200,
    'get categories returns array': (r) => {
      try {
        const body = JSON.parse(r.body);
        return Array.isArray(body);
      } catch {
        return false;
      }
    },
  });

  errorRate.add(!success);
  apiDuration.add(res.timings.duration);
}

function testProductOperations(categoryId) {
  // Create product
  const product = {
    name: `Test Product ${Date.now()}`,
    description: 'Performance test product',
    price: 99.99,
    currency: 'USD',
    categoryId: categoryId,
  };

  const createRes = http.post(
    `${BASE_URL}/api/v1/products`,
    JSON.stringify(product),
    {
      headers: { 'Content-Type': 'application/json' },
    }
  );

  const createSuccess = check(createRes, {
    'create product status is 201': (r) => r.status === 201,
  });

  errorRate.add(!createSuccess);
  apiDuration.add(createRes.timings.duration);

  if (createRes.status === 201) {
    const productId = createRes.json();

    // Get product
    const getRes = http.get(`${BASE_URL}/api/v1/products/${productId}`);

    const getSuccess = check(getRes, {
      'get product status is 200': (r) => r.status === 200,
      'get product returns correct data': (r) => {
        try {
          const body = JSON.parse(r.body);
          return body.id === productId;
        } catch {
          return false;
        }
      },
    });

    errorRate.add(!getSuccess);
    apiDuration.add(getRes.timings.duration);

    // Update product price
    const updateRes = http.put(
      `${BASE_URL}/api/v1/products/${productId}/price`,
      JSON.stringify({ price: 149.99, currency: 'USD' }),
      {
        headers: { 'Content-Type': 'application/json' },
      }
    );

    const updateSuccess = check(updateRes, {
      'update product price status is 204': (r) => r.status === 204,
    });

    errorRate.add(!updateSuccess);
    apiDuration.add(updateRes.timings.duration);
  }
}

function testSearchProducts() {
  const res = http.get(`${BASE_URL}/api/v1/products/search?pageNumber=1&pageSize=10`);

  const success = check(res, {
    'search products status is 200': (r) => r.status === 200,
    'search products returns paged result': (r) => {
      try {
        const body = JSON.parse(r.body);
        return body.hasOwnProperty('items') &&
               body.hasOwnProperty('pageNumber') &&
               body.hasOwnProperty('pageSize');
      } catch {
        return false;
      }
    },
  });

  errorRate.add(!success);
  apiDuration.add(res.timings.duration);
}

export function teardown(data) {
  // Cleanup is handled by database reset in test environment
  console.log('Performance test completed');
}

export function handleSummary(data) {
  return {
    'stdout': textSummary(data, { indent: ' ', enableColors: true }),
    'performance-summary.json': JSON.stringify(data),
  };
}

function textSummary(data, options) {
  // Basic text summary
  let summary = '\n';
  summary += '='.repeat(80) + '\n';
  summary += '  Performance Test Summary\n';
  summary += '='.repeat(80) + '\n';
  summary += `  Total Requests: ${data.metrics.http_reqs.values.count}\n`;
  summary += `  Failed Requests: ${data.metrics.http_req_failed.values.passes}\n`;
  summary += `  Request Duration (avg): ${data.metrics.http_req_duration.values.avg.toFixed(2)}ms\n`;
  summary += `  Request Duration (p95): ${data.metrics.http_req_duration.values['p(95)'].toFixed(2)}ms\n`;
  summary += `  Request Duration (p99): ${data.metrics.http_req_duration.values['p(99)'].toFixed(2)}ms\n`;
  summary += `  Requests/sec: ${data.metrics.http_reqs.values.rate.toFixed(2)}\n`;
  summary += '='.repeat(80) + '\n';

  return summary;
}

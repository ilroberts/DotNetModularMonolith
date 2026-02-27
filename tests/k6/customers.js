import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend, Rate } from 'k6/metrics';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.2/index.js';
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------
const BASE_URL = __ENV.BASE_URL || 'http://127.0.0.1:5173';
const USERNAME = __ENV.K6_USERNAME || 'bilbo.baggins';

// ---------------------------------------------------------------------------
// Custom metrics
// ---------------------------------------------------------------------------
const postDuration = new Trend('customer_post_duration', true);
const getDuration  = new Trend('customer_get_duration', true);
const postErrors   = new Rate('customer_post_errors');
const getErrors    = new Rate('customer_get_errors');

// ---------------------------------------------------------------------------
// Test options
// ---------------------------------------------------------------------------
export const options = {
  scenarios: {
    create_customers: {
      executor: 'ramping-vus',
      startVUs: 1,
      stages: [
        { duration: '10s', target: 5 },
        { duration: '20s', target: 5 },
        { duration: '10s', target: 0 },
      ],
      exec: 'createCustomer',
      tags: { scenario: 'create_customers' },
    },
    get_customers: {
      executor: 'ramping-vus',
      startVUs: 1,
      stages: [
        { duration: '10s', target: 10 },
        { duration: '20s', target: 10 },
        { duration: '10s', target: 0  },
      ],
      exec: 'getCustomers',
      tags: { scenario: 'get_customers' },
    },
  },
  thresholds: {
    customer_post_duration: ['p(95)<500'],
    customer_get_duration:  ['p(95)<300'],
    customer_post_errors:   ['rate<0.01'],
    customer_get_errors:    ['rate<0.01'],
  },
};

// ---------------------------------------------------------------------------
// Customer schema definition (v1)
// ---------------------------------------------------------------------------
const CUSTOMER_SCHEMA = {
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://example.com/schemas/customer/v1",
  "title": "Customer",
  "description": "A customer entity",
  "type": "object",
  "properties": {
    "Id":          { "type": "string",  "description": "Unique customer identifier",        "x-metadata": true },
    "Name":        { "type": "string",  "description": "Customer's name",                   "x-metadata": true },
    "Email":       { "type": "string",  "format": "email", "description": "Customer's email address", "x-metadata": true },
    "Phone":       { "type": "string",  "description": "Customer's phone number" },
    "DateOfBirth": { "type": "string",  "format": "date-time", "description": "Customer's date of birth" },
    "CreatedAt":   { "type": "string",  "format": "date-time", "description": "When the customer was created",      "x-metadata": true },
    "UpdatedAt":   { "type": "string",  "format": "date-time", "description": "When the customer was last updated", "x-metadata": true },
  },
  "required": ["Id", "Name", "Email"],
  "additionalProperties": false,
};

// ---------------------------------------------------------------------------
// Setup: generate token and register customer schema before tests start
// ---------------------------------------------------------------------------
export function setup() {
  // 1. Generate token
  const tokenRes = http.post(
    `${BASE_URL}/admin/generateToken`,
    JSON.stringify({ user_name: USERNAME }),
    { headers: { 'Content-Type': 'application/json' } }
  );

  if (tokenRes.status !== 200) {
    throw new Error(`Failed to generate token: HTTP ${tokenRes.status} - ${tokenRes.body}`);
  }

  const token = tokenRes.body.trim();
  console.log(`Token generated for user '${USERNAME}'`);

  // 2. Register customer schema (ignore 409 if it already exists)
  const authHeaders = {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
  };

  const schemaRes = http.post(
    `${BASE_URL}/schemas`,
    JSON.stringify({
      EntityType:       'Customer',
      Version:          1,
      SchemaDefinition: JSON.stringify(CUSTOMER_SCHEMA),
    }),
    authHeaders
  );

  if (schemaRes.status === 201) {
    console.log('Customer schema v1 registered successfully');
  } else if (schemaRes.status === 409) {
    console.log('Customer schema v1 already exists, continuing');
  } else {
    throw new Error(`Failed to register customer schema: HTTP ${schemaRes.status} - ${schemaRes.body}`);
  }

  return { token };
}

// ---------------------------------------------------------------------------
// Scenario: POST /customers
// ---------------------------------------------------------------------------
export function createCustomer({ token }) {
  const headers = {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
  };

  const payload = JSON.stringify({
    Name:  `Test User ${Date.now()}`,
    Email: `testuser+${Date.now()}@example.com`,
  });

  const res = http.post(`${BASE_URL}/customers`, payload, headers);

  postDuration.add(res.timings.duration);
  const ok = check(res, {
    'POST /customers status is 201': (r) => r.status === 201,
    'POST /customers returns id':    (r) => r.json('id') !== undefined,
  });
  postErrors.add(!ok);

  if (!ok) {
    console.error(`POST /customers failed: HTTP ${res.status} - ${res.body}`);
  }

  sleep(1);
}

// ---------------------------------------------------------------------------
// Scenario: GET /customers and GET /customers/{id}
// ---------------------------------------------------------------------------
export function getCustomers({ token }) {
  const headers = {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
  };

  const listRes = http.get(`${BASE_URL}/customers`, headers);

  getDuration.add(listRes.timings.duration);
  const listOk = check(listRes, {
    'GET /customers status is 200': (r) => r.status === 200,
    'GET /customers returns array': (r) => Array.isArray(r.json()),
  });
  getErrors.add(!listOk);

  if (!listOk) {
    console.error(`GET /customers failed: HTTP ${listRes.status} - ${listRes.body}`);
  }

  const customers = listRes.json();
  if (Array.isArray(customers) && customers.length > 0) {
    const id = customers[0].id;
    const byIdRes = http.get(`${BASE_URL}/customers/${id}`, headers);

    getDuration.add(byIdRes.timings.duration);
    const byIdOk = check(byIdRes, {
      'GET /customers/{id} status is 200': (r) => r.status === 200,
      'GET /customers/{id} correct id':    (r) => r.json('id') === id,
    });
    getErrors.add(!byIdOk);

    if (!byIdOk) {
      console.error(`GET /customers/${id} failed: HTTP ${byIdRes.status} - ${byIdRes.body}`);
    }
  }

  sleep(1);
}

// ---------------------------------------------------------------------------
// Summary: write results to stdout, JSON and HTML after the run
// ---------------------------------------------------------------------------
export function handleSummary(data) {
  const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
  const reportDir = __ENV.REPORT_DIR || 'tests/k6/reports';
  const jsonPath  = `${reportDir}/customers-${timestamp}.json`;
  const htmlPath  = `${reportDir}/customers-${timestamp}.html`;

  return {
    'stdout':   textSummary(data, { indent: ' ', enableColors: true }),
    [jsonPath]: JSON.stringify(data, null, 2),
    [htmlPath]: htmlReport(data, { title: 'Customer Endpoint Performance Report' }),
  };
}

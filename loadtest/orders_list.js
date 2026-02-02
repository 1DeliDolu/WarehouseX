import http from "k6/http";
import { check, fail, sleep } from "k6";

export const options = {
  summaryTrendStats: ["avg", "min", "med", "max", "p(90)", "p(95)", "p(99)"],
  scenarios: {
    orders_list: {
      executor: "constant-vus",
      vus: 50,
      duration: "60s",
    },
  },
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<600", "p(99)<1200"],
  },
};

const BASE = __ENV.BASE_URL || "http://localhost:5057";
const SEED_IDS = loadSeedIds(__ENV.SEED_IDS_FILE);
const STATUSES = (__ENV.STATUSES || "Created,Picked,Shipped,Cancelled")
  .split(",")
  .map((s) => s.trim())
  .filter(Boolean);
const PAGE_MAX = toInt(__ENV.PAGE_MAX, 50, 1);
const PAGE_SIZE = toInt(__ENV.PAGE_SIZE, 20, 1);
const DAYS_BACK = toInt(__ENV.DAYS_BACK, 30, 0);
const USE_CUSTOMER_FILTER = ["1", "true", "yes"].includes(
  String(__ENV.CUSTOMER_FILTER || "false").toLowerCase()
);

export function setup() {
  const envWarehouseId = String(__ENV.WAREHOUSE_ID || "").trim();
  const envCustomerId = String(__ENV.CUSTOMER_ID || "").trim();
  if (envWarehouseId) {
    return { wid: envWarehouseId, cid: envCustomerId || null };
  }

  if (SEED_IDS?.warehouseId) {
    return { wid: SEED_IDS.warehouseId, cid: SEED_IDS.customerId || null };
  }

  if (SEED_IDS) {
    fail(`SEED_IDS_FILE is missing warehouseId. Regenerate the file or provide WAREHOUSE_ID.`);
  }

  const url = `${BASE}/debug/sample-ids`;
  const res = http.get(url);

  if (res.status !== 200) {
    const detail = res.error ? ` error="${res.error}"` : "";
    fail(
      `GET ${url} failed (status=${res.status}). Provide WAREHOUSE_ID/SEED_IDS_FILE or enable debug endpoint. Is the API running on ${BASE}?${detail}`
    );
  }

  let data;
  try {
    data = res.json();
  } catch (err) {
    fail(`GET ${url} returned invalid JSON: ${err}`);
  }

  if (!data?.warehouseId) {
    fail(`GET ${url} returned no warehouseId. Ensure seed data exists.`);
  }

  check(res, { "sample ids 200": (r) => r.status === 200 });
  check(data, { "warehouseId exists": (d) => !!d.warehouseId });

  return { wid: data.warehouseId, cid: data.customerId || null };
}

export default function (data) {
  const page = Math.floor(Math.random() * PAGE_MAX) + 1;
  const status =
    STATUSES.length > 0
      ? STATUSES[Math.floor(Math.random() * STATUSES.length)]
      : null;
  const params = [
    `warehouseId=${encodeURIComponent(data.wid)}`,
    `page=${page}`,
    `pageSize=${PAGE_SIZE}`,
  ];

  if (status) {
    params.push(`status=${encodeURIComponent(status)}`);
  }

  if (DAYS_BACK > 0) {
    const now = new Date();
    const from = new Date(now.getTime() - DAYS_BACK * 24 * 60 * 60 * 1000);
    params.push(`from=${encodeURIComponent(from.toISOString())}`);
    params.push(`to=${encodeURIComponent(now.toISOString())}`);
  }

  if (USE_CUSTOMER_FILTER && data.cid) {
    params.push(`customerId=${encodeURIComponent(data.cid)}`);
  }

  const url = `${BASE}/orders?${params.join("&")}`;

  const res = http.get(url);
  check(res, { "status 200": (r) => r.status === 200 });

  sleep(0.1);
}

function toInt(value, fallback, min) {
  const parsed = parseInt(value || "", 10);
  if (!Number.isFinite(parsed)) {
    return fallback;
  }
  return Math.max(min, parsed);
}

function loadSeedIds(path) {
  if (!path) {
    return null;
  }

  try {
    return JSON.parse(open(path));
  } catch (err) {
    throw new Error(`SEED_IDS_FILE '${path}' could not be read: ${err}`);
  }
}

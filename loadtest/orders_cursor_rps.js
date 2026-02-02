import http from "k6/http";
import { check, fail } from "k6";

const RATE = toInt(__ENV.RATE, 600, 1);
const DURATION = String(__ENV.DURATION || "90s");
const PRE_ALLOC_VUS = toInt(__ENV.PRE_ALLOC_VUS, 200, 1);
const MAX_VUS = toInt(__ENV.MAX_VUS, 400, 1);

export const options = {
  summaryTrendStats: ["avg", "min", "med", "max", "p(90)", "p(95)", "p(99)"],
  scenarios: {
    rps_test: {
      executor: "constant-arrival-rate",
      rate: RATE,
      timeUnit: "1s",
      duration: DURATION,
      preAllocatedVUs: PRE_ALLOC_VUS,
      maxVUs: MAX_VUS,
    },
  },
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<50", "p(99)<150"],
  },
};

const BASE = __ENV.BASE_URL || "http://127.0.0.1:5057";
const SEED_IDS = loadSeedIds(__ENV.SEED_IDS_FILE);
const PAGE_SIZE = toInt(__ENV.PAGE_SIZE, 20, 1);
const INCLUDE_ITEM_COUNT = ["1", "true", "yes"].includes(
  String(__ENV.INCLUDE_ITEM_COUNT || "false").toLowerCase()
);

export function setup() {
  const envWarehouseId = String(__ENV.WAREHOUSE_ID || "").trim();
  if (envWarehouseId) {
    return { wid: envWarehouseId };
  }

  if (SEED_IDS?.warehouseId) {
    return { wid: SEED_IDS.warehouseId };
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

  return { wid: data.warehouseId };
}

export default function (data) {
  const params = [
    `warehouseId=${encodeURIComponent(data.wid)}`,
    `pageSize=${PAGE_SIZE}`,
  ];

  if (INCLUDE_ITEM_COUNT) {
    params.push("includeItemCount=true");
  }

  const url = `${BASE}/orders/cursor?${params.join("&")}`;
  const res = http.get(url);
  check(res, { "200": (r) => r.status === 200 });
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

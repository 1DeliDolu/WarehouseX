import http from "k6/http";
import { check, fail, sleep } from "k6";

const VUS = toInt(__ENV.VUS, 50, 1);
const DURATION = String(__ENV.DURATION || "60s");
const GRACEFUL_STOP = String(__ENV.GRACEFUL_STOP || "30s");

export const options = {
  summaryTrendStats: ["avg", "min", "med", "max", "p(90)", "p(95)", "p(99)"],
  scenarios: {
    cursor_list: {
      executor: "constant-vus",
      vus: VUS,
      duration: DURATION,
      gracefulStop: GRACEFUL_STOP,
    },
  },
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<400", "p(99)<800"],
  },
};

const BASE = __ENV.BASE_URL || "http://127.0.0.1:5057";
const SEED_IDS = loadSeedIds(__ENV.SEED_IDS_FILE);
const PAGE_SIZE = toInt(__ENV.PAGE_SIZE, 20, 1);
const SCROLL_PAGES = toInt(__ENV.SCROLL_PAGES, 3, 1);
const STATUS = String(__ENV.STATUS || "").trim();
const DAYS_BACK = toInt(__ENV.DAYS_BACK, 0, 0);
const INCLUDE_ITEM_COUNT = ["1", "true", "yes"].includes(
  String(__ENV.INCLUDE_ITEM_COUNT || "false").toLowerCase()
);
const THINK_TIME = toFloat(__ENV.THINK_TIME, 0.1, 0);

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
  let cursorCreatedAt = null;
  let cursorId = null;

  for (let i = 0; i < SCROLL_PAGES; i++) {
    const params = [
      `warehouseId=${encodeURIComponent(data.wid)}`,
      `pageSize=${PAGE_SIZE}`,
    ];

    if (STATUS) {
      params.push(`status=${encodeURIComponent(STATUS)}`);
    }

    if (DAYS_BACK > 0) {
      const now = new Date();
      const from = new Date(now.getTime() - DAYS_BACK * 24 * 60 * 60 * 1000);
      params.push(`from=${encodeURIComponent(from.toISOString())}`);
      params.push(`to=${encodeURIComponent(now.toISOString())}`);
    }

    if (INCLUDE_ITEM_COUNT) {
      params.push("includeItemCount=true");
    }

    if (cursorCreatedAt && cursorId) {
      params.push(`cursorCreatedAt=${encodeURIComponent(cursorCreatedAt)}`);
      params.push(`cursorId=${cursorId}`);
    }

    const url = `${BASE}/orders/cursor?${params.join("&")}`;
    const res = http.get(url);
    check(res, { "200": (r) => r.status === 200 });

    let body;
    try {
      body = res.json();
    } catch {
      break;
    }

    if (!body?.hasMore || !body?.next) {
      break;
    }

    cursorCreatedAt = body.next.cursorCreatedAt;
    cursorId = body.next.cursorId;
  }

  sleep(THINK_TIME);
}

function toInt(value, fallback, min) {
  const parsed = parseInt(value || "", 10);
  if (!Number.isFinite(parsed)) {
    return fallback;
  }
  return Math.max(min, parsed);
}

function toFloat(value, fallback, min) {
  const parsed = parseFloat(value || "");
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

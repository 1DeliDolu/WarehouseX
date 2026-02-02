import http from "k6/http";
import { check } from "k6";

export const options = {
  scenarios: {
    gate: {
      executor: "constant-arrival-rate",
      rate: 1500,
      timeUnit: "1s",
      duration: "60s",
      preAllocatedVUs: 200,
      maxVUs: 400,
    },
  },
  thresholds: {
    http_req_failed: ["rate<0.001"],
    http_req_duration: ["p(95)<120", "p(99)<200"],
  },
};

const BASE = __ENV.BASE_URL || "http://127.0.0.1:5057";
const WID = __ENV.WAREHOUSE_ID || "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";

export default function () {
  const res = http.get(`${BASE}/orders/cursor?warehouseId=${WID}&pageSize=20`);
  check(res, { "200": (r) => r.status === 200 });
}

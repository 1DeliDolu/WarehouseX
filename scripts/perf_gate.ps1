param(
  [string]$BaseUrl = "http://127.0.0.1:5057",
  [string]$WarehouseId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"
)

Write-Host "Running perf gate..."
Write-Host "BASE_URL=$BaseUrl"
Write-Host "WAREHOUSE_ID=$WarehouseId"

k6 run .\loadtest\orders_cursor_gate.js `
  --env BASE_URL=$BaseUrl `
  --env WAREHOUSE_ID=$WarehouseId `
  --summary-trend-stats "avg,min,med,max,p(90),p(95),p(99)"

if ($LASTEXITCODE -ne 0) {
  throw "Performance gate failed (thresholds crossed)."
}

Write-Host "Performance gate passed."

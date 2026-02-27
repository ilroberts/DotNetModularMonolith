#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPORT_DIR="${REPORT_DIR:-$SCRIPT_DIR/reports}"
BASE_URL="${BASE_URL:-http://localhost:5173}"
TEST_SCRIPT="${1:-$SCRIPT_DIR/customers.js}"

mkdir -p "$REPORT_DIR"

echo "Running k6 test: $TEST_SCRIPT"
echo "Target:          $BASE_URL"
echo "Reports dir:     $REPORT_DIR"
echo "Dashboard:       http://localhost:5665"
echo ""

K6_WEB_DASHBOARD=true \
K6_WEB_DASHBOARD_EXPORT="$REPORT_DIR/dashboard-$(date -u +%Y-%m-%dT%H-%M-%SZ).html" \
k6 run \
  -e BASE_URL="$BASE_URL" \
  -e REPORT_DIR="$REPORT_DIR" \
  "$TEST_SCRIPT"

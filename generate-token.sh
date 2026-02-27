#!/bin/bash

USERNAME="${1:-bilbo.baggins}"

echo "Generating bearer token for user '$USERNAME'..."

RESPONSE=$(curl -s -w "\n%{http_code}" -X 'POST' \
  'http://127.0.0.1:5173/admin/generateToken' \
  -H 'Content-Type: application/json' \
  --data "{\"user_name\": \"$USERNAME\"}")

HTTP_STATUS=$(echo "$RESPONSE" | tail -n1)
RESPONSE_BODY=$(echo "$RESPONSE" | sed '$d')

if [ "$HTTP_STATUS" -ge 200 ] && [ "$HTTP_STATUS" -lt 300 ]; then
  echo "HTTP Status: $HTTP_STATUS"
  echo ""
  echo "Token:"
  echo "$RESPONSE_BODY"
  echo ""
  echo "To use this token, run:"
  echo "  export ECOMMERCE_BEARER_TOKEN=\"$RESPONSE_BODY\""
else
  echo "Error: failed to generate token (HTTP $HTTP_STATUS)" >&2
  echo "$RESPONSE_BODY" >&2
  exit 1
fi

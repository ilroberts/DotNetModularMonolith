#!/bin/bash

if [ -z "$ECOMMERCE_BEARER_TOKEN" ]; then
  echo "Error: ECOMMERCE_BEARER_TOKEN environment variable is not set" >&2
  exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SCHEMA_FILE="$SCRIPT_DIR/ECommerce.BusinessEvents/Resources/Schemas/customer.v1.schema.json"

# The /schemas endpoint expects { EntityType, Version, SchemaDefinition } where
# SchemaDefinition is the schema JSON serialised as a string.
SCHEMA_DEFINITION=$(cat "$SCHEMA_FILE")

PAYLOAD=$(jq -n \
  --arg entityType "Customer" \
  --argjson version 1 \
  --arg schemaDefinition "$SCHEMA_DEFINITION" \
  '{EntityType: $entityType, Version: $version, SchemaDefinition: $schemaDefinition}')

echo "Posting customer schema to http://127.0.0.1:5173/schemas..."
echo "Payload:"
echo "$PAYLOAD" | jq .
echo ""

RESPONSE=$(curl -s -w "\n%{http_code}" -X 'POST' \
  'http://127.0.0.1:5173/schemas' \
  -H 'accept: */*' \
  -H "Authorization: Bearer $ECOMMERCE_BEARER_TOKEN" \
  -H 'Content-Type: application/json' \
  --data "$PAYLOAD")

HTTP_STATUS=$(echo "$RESPONSE" | tail -n1)
RESPONSE_BODY=$(echo "$RESPONSE" | sed '$d')

echo "HTTP Status: $HTTP_STATUS"
echo "Response body:"
echo "$RESPONSE_BODY" | jq . 2>/dev/null || echo "$RESPONSE_BODY"

if [ "$HTTP_STATUS" -ge 200 ] && [ "$HTTP_STATUS" -lt 300 ]; then
  echo ""
  echo "Success."
else
  echo ""
  echo "Error: request failed with status $HTTP_STATUS." >&2
  exit 1
fi

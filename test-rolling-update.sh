#!/bin/bash

# Script to test zero downtime during rolling updates
# Usage: ./test-rolling-update.sh [base_url] [interval] [duration] [--no-log]
# Example: ./test-rolling-update.sh "http://localhost:5000" 0.5 300
# Example with no logging: ./test-rolling-update.sh "http://localhost:5000" 0.5 300 --no-log

# Set default values
LOGGING_ENABLED=true

# Process arguments
BASE_URL=""
INTERVAL=""
DURATION=""

for arg in "$@"; do
  if [ "$arg" == "--no-log" ]; then
    LOGGING_ENABLED=false
  elif [ -z "$BASE_URL" ]; then
    BASE_URL="$arg"
  elif [ -z "$INTERVAL" ]; then
    INTERVAL="$arg"
  elif [ -z "$DURATION" ]; then
    DURATION="$arg"
  fi
done

# Set defaults if not provided
BASE_URL="${BASE_URL:-http://localhost:5000}"
INTERVAL="${INTERVAL:-0.5}"  # Interval between requests in seconds
DURATION="${DURATION:-300}"  # Total duration of the test in seconds

echo "Starting rolling update test for $BASE_URL"
echo "- Sending requests every $INTERVAL seconds"
echo "- Test will run for $DURATION seconds"
echo "- Logging enabled: $LOGGING_ENABLED"
echo "- Press Ctrl+C to stop the test"
echo

start_time=$(date +%s)
end_time=$((start_time + DURATION))
success_count=0
failure_count=0
total_count=0
max_response_time=0
min_response_time=999999
total_response_time=0

# Create a log file if logging is enabled
if $LOGGING_ENABLED; then
  log_file="rolling_update_test_$(date +%Y%m%d_%H%M%S).log"
  echo "Timestamp,Status,Response Time (ms),Response" > "$log_file"
else
  log_file=""
fi

# Function to convert seconds to a human-readable format
function format_time() {
  local total_seconds=$1
  local hours=$((total_seconds / 3600))
  local minutes=$(( (total_seconds % 3600) / 60 ))
  local seconds=$((total_seconds % 60))
  printf "%02d:%02d:%02d" $hours $minutes $seconds
}

# Main test loop
while [ "$(date +%s)" -lt "$end_time" ]; do
  timestamp=$(date +"%Y-%m-%d %H:%M:%S")
  start_request=$(date +%s.%N)

  # Send request and capture both status code and response
  response=$(curl -s -w "\n%{http_code},%{time_total}" -o /tmp/curl_response.txt "$BASE_URL" 2>/dev/null)

  # Extract status code and response time
  response_body=$(cat /tmp/curl_response.txt)
  IFS="," read -r status_code response_time_sec <<< "$(echo "$response" | tail -n1)"

  # Calculate response time in milliseconds
  response_time=$(echo "$response_time_sec * 1000" | bc)
  response_time=${response_time%.*}  # Remove decimal part

  # Update statistics
  total_count=$((total_count + 1))
  total_response_time=$(echo "$total_response_time + $response_time" | bc)

  if [ "$min_response_time" -gt "$response_time" ]; then
    min_response_time=$response_time
  fi

  if [ "$max_response_time" -lt "$response_time" ]; then
    max_response_time=$response_time
  fi

  # Check if the request was successful (2xx status code)
  if [[ "$status_code" =~ ^2[0-9][0-9]$ ]]; then
    status="✅ Success"
    success_count=$((success_count + 1))
  else
    status="❌ Failed"
    failure_count=$((failure_count + 1))
  fi

  # Log the result if logging is enabled
  if $LOGGING_ENABLED; then
    echo "$timestamp,$status,$response_time,$response_body" >> "$log_file"
  fi

  # Display progress
  elapsed_time=$(($(date +%s) - start_time))
  remaining_time=$((DURATION - elapsed_time))
  avg_response_time=$(echo "scale=2; $total_response_time / $total_count" | bc)

  # Clear the line and print the updated stats
  echo -ne "\r\033[K"
  echo -ne "Runtime: $(format_time $elapsed_time) | Remaining: $(format_time $remaining_time) | "
  echo -ne "Requests: $total_count | Success: $success_count | Failed: $failure_count | "
  echo -ne "Avg: ${avg_response_time}ms | Min: ${min_response_time}ms | Max: ${max_response_time}ms"

  # Sleep for the specified interval
  sleep "$INTERVAL"
done

# Final summary
echo
echo
echo "===== Test Complete ====="
echo "Total requests: $total_count"
echo "Successful requests: $success_count"
echo "Failed requests: $failure_count"
echo "Success rate: $(echo "scale=2; ($success_count * 100) / $total_count" | bc)%"
echo "Average response time: $(echo "scale=2; $total_response_time / $total_count" | bc) ms"
echo "Minimum response time: $min_response_time ms"
echo "Maximum response time: $max_response_time ms"
if $LOGGING_ENABLED; then
  echo "Results saved to: $log_file"
fi

#!/bin/bash

API="http://localhost:5003/api/timecards"
COUNT=${1:-25}   # default to 100 if no parameter passed

for i in $(seq 1 $COUNT)
do
  # Generate random GUIDs using PowerShell (works in Git Bash on Windows)
  workerId=$(powershell -Command "[guid]::NewGuid().ToString()")
  projectId=$(powershell -Command "[guid]::NewGuid().ToString()")

  # Random totalHours between 1 and 24
  totalHours=$((RANDOM % 24 + 1))

  # Random weekStart date in 2024 (any day of the year)
  daysAhead=$((RANDOM % 365))
  weekStart=$(date -d "2024-01-01 +$daysAhead days" +%Y-%m-%d)

  # Send POST request asynchronously (background job)
  curl -s -X POST "$API" \
    -H "Content-Type: application/json" \
    -d "{
      \"workerId\": \"$workerId\",
      \"projectId\": \"$projectId\",
      \"weekStart\": \"$weekStart\",
      \"totalHours\": $totalHours
    }" >/dev/null &

  # Print statement for each timecard
  echo "Triggered timecard $i: workerId=$workerId, projectId=$projectId, weekStart=$weekStart, totalHours=$totalHours"
done

# Wait for all background jobs to finish
wait

echo "Created $COUNT timecards (async)"
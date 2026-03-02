#!/usr/bin/env bash
set -euo pipefail

export DOTNET_ROOT=/usr/share/dotnet
export PATH=$DOTNET_ROOT:$PATH

# Run all tests and produce test-results.json
set +e
dotnet test \
  --no-build \
  -- --report-ctrf --report-ctrf-filename test-results.json
set -e

# Find all test-results.json files
FILES=$(find . -type f -name "test-results.json")
if [[ -z "$FILES" ]]; then
  echo "Error: No test-results.json files found"
  exit 1
fi

# Collect all tests into one array
ALL_TESTS="[]"
TOTAL_TESTS=0
PASSED_TESTS=0

for FILE in $FILES; do
  # Extract each test case
  TESTS=$(jq -c '.results.tests[]' "$FILE")

  while IFS= read -r TEST; do
    NAME=$(echo "$TEST" | jq -r '.name')
    STATUS=$(echo "$TEST" | jq -r '.status')
    MESSAGE=$(echo "$TEST" | jq -r '.message // empty')
    TRACE=$(echo "$TEST" | jq -r '.trace // empty')
    LINE=$(echo "$TEST" | jq '.line // null')


    # Normalize status
    if [[ "$STATUS" == "passed" ]]; then
      STATUS_OUT="pass"
    else
      STATUS_OUT="fail"
    fi

    TEST_OBJ=$(jq -n \
      --arg name "$NAME" \
      --arg status "$STATUS_OUT" \
      --arg msg "$MESSAGE" \
      --arg trace "$TRACE" \
      --argjson line "$LINE" \
      '{
          name: $name,
          status: $status,
          message: (if $msg != "" then $msg else null end),
          test_code: (if $trace != "" then $trace else null end),
          line_no: $line,
          task_id: 0,
          score: 0
      }')

    ALL_TESTS=$(echo "$ALL_TESTS" | jq --argjson t "$TEST_OBJ" '. + [$t]')
    TOTAL_TESTS=$((TOTAL_TESTS + 1))
    if [[ "$STATUS" == "passed" ]]; then
      PASSED_TESTS=$((PASSED_TESTS + 1))
    fi
  done <<< "$TESTS"
done

if [[ "$TOTAL_TESTS" -eq 0 ]]; then
  echo "Error: No tests found in any result file"
  exit 1
fi

# Score distribution
BASE_POINTS=$((100 / TOTAL_TESTS))
REMAINDER=$((100 % TOTAL_TESTS))

# Assign scores
i=0
UPDATED_TESTS=$(echo "$ALL_TESTS" | jq --argjson base $BASE_POINTS --argjson rem $REMAINDER '
  to_entries
  | map(
      .value
      + {
          score: (
            if .value.status == "pass" then
              $base
            else
              0
            end
          )
        }
    )
')

# Give remainder points to the first N passed tests
if [[ $REMAINDER -gt 0 ]]; then
  UPDATED_TESTS=$(echo "$UPDATED_TESTS" | jq --argjson rem $REMAINDER '
    reduce range(0; length) as $i
      (.; if $i < $rem and .[$i].status == "pass" then
            .[$i].score += 1
          else
            .
          end
      )
  ')
fi

# Global status
GLOBAL_STATUS="pass"
if [[ "$PASSED_TESTS" -lt "$TOTAL_TESTS" ]]; then
  GLOBAL_STATUS="fail"
fi

# Build compact JSON for GitHub Actions
RESULT=$(jq -n \
  --argjson version 3 \
  --arg status "$GLOBAL_STATUS" \
  --argjson tests "$UPDATED_TESTS" \
  '{version: $version, status: $status, max_score: 100, tests: $tests}' \
  | jq -c '.')  # compact representation

echo "$RESULT" > results.json


# Export for GitHub Actions (safe for multiline JSON)
if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
  b64=$(base64 -w0 results.json)
  echo "result=$b64" >> "$GITHUB_OUTPUT"
fi

echo "$RESULT"
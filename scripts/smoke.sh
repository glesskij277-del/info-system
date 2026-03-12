#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${1:-http://127.0.0.1:8080}"

echo "[smoke] BASE_URL=${BASE_URL}"

check_status() {
  local path="$1"
  local expected="$2"

  local status
  status=$(curl -s -o /dev/null -w "%{http_code}" "${BASE_URL}${path}")

  if [[ "${status}" != "${expected}" ]]; then
    echo "[smoke] FAIL ${path}: expected ${expected}, got ${status}"
    exit 1
  fi

  echo "[smoke] OK ${path}: ${status}"
}

check_header_contains() {
  local path="$1"
  local header_fragment="$2"

  local headers
  headers=$(curl -s -I "${BASE_URL}${path}")

  if ! grep -qi "${header_fragment}" <<< "${headers}"; then
    echo "[smoke] FAIL ${path}: header not found -> ${header_fragment}"
    exit 1
  fi

  echo "[smoke] OK ${path}: header contains '${header_fragment}'"
}

check_body_contains() {
  local path="$1"
  local body_fragment="$2"

  local body
  body=$(curl -s "${BASE_URL}${path}")

  if ! grep -qi "${body_fragment}" <<< "${body}"; then
    echo "[smoke] FAIL ${path}: body not found -> ${body_fragment}"
    exit 1
  fi

  echo "[smoke] OK ${path}: body contains '${body_fragment}'"
}

check_status "/" "200"
check_status "/Patients" "200"
check_status "/Appointments" "200"
check_status "/Reports" "200"
check_status "/health" "200"

check_body_contains "/health" "healthy"
check_header_contains "/Reports/ExportAppointmentsCsv" "Content-Type: text/csv"
check_header_contains "/Reports/ExportPatientsCsv" "Content-Type: text/csv"

echo "[smoke] SUCCESS: all checks passed"


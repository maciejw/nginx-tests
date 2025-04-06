#!/bin/sh
set -ex

update-ca-certificates

exec /docker-entrypoint.sh "$@"
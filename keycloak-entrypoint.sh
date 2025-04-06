#!/bin/sh

update-ca-trust extract

exec /opt/keycloak/bin/kc.sh "$@"
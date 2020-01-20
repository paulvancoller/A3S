#!/bin/sh
#
# *************************************************
# Copyright (c) 2019, Grindrod Bank Limited
# License MIT: https://opensource.org/licenses/MIT
# **************************************************
#

# Set some variables, and defaults in case they are not provided.

FLYWAY_CONNECTION_RETRIES=${FLYWAY_CONNECTION_RETRIES:-60}
FLYWAY_ENABLE=${FLYWAY_ENABLE:-false}
DATABASE_SERVER=${DATABASE_SERVER:-localhost}
DATABASE_PORT=${DATABASE_PORT:-5432}
DATABASE_NAME=${DATABASE_NAME:-identity_server}
FLYWAY_USER=${FLYWAY_USER:-postgres}
FLYWAY_PASSWORD=${FLYWAY_PASSWORD:-postgres}
FLYWAY_CONNECTION_RETRIES=${FLYWAY_CONNECTION_RETRIES:-10}
# fail on first error
set -e

echo "========================================================================="
echo " Flyway Migrations First  [$FLYWAY_ENABLE]"
echo "========================================================================="
if [ "$FLYWAY_ENABLE" = "true" ]; then
	echo "> Migrating A3S database with flyway..."
	/flyway/flyway -url=jdbc:postgresql://$DATABASE_SERVER:$DATABASE_PORT/$DATABASE_NAME -schemas=_a3s -baselineOnMigrate=true -baselineVersion=0 -locations=filesystem:/flyway/sql/a3s -user=$FLYWAY_USER -password=$FLYWAY_PASSWORD -connectRetries=$FLYWAY_CONNECTION_RETRIES -q migrate
	echo "> Done migrating A3S database with flyway..."
fi

echo ""
echo "========================================================================="
echo " Starting Application"
echo "========================================================================="

# NB! Run the actual application using 'exec', as this results in the correct PID being set for the actual dotnet application, thus enabling it to receive signals (such as sigterms) correctly
# This drastically reduces the time to correctly and safely bring down the container.
exec dotnet za.co.grindrodbank.a3s.dll
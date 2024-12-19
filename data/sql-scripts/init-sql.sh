#!/bin/bash
SQL_SCRIPTS_DIR="/sql-scripts"
SQLCMD="/opt/mssql-tools/bin/sqlcmd"
SA_PASSWORD="JWmTjvNnRfKcjvLo0r"

for script in $SQL_SCRIPTS_DIR/*.sql; do
    echo "Executing $script"
    until $SQLCMD -S localhost -U SA -P "$SA_PASSWORD" -i $script; do
        echo "SQL Server is starting up or not ready yet - waiting"
        sleep 2
    done
done
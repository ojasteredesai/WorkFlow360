\set ON_ERROR_STOP on

\c postgres_user;
CREATE EXTENSION IF NOT EXISTS pgcrypto;

\c postgres_monolith;
CREATE EXTENSION IF NOT EXISTS pgcrypto;

\c postgres_timecard;
CREATE EXTENSION IF NOT EXISTS pgcrypto;

\c postgres_expense;
CREATE EXTENSION IF NOT EXISTS pgcrypto;

\c postgres_invoice;
CREATE EXTENSION IF NOT EXISTS pgcrypto;

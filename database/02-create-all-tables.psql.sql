-- =========================================================
-- CREATE ALL TABLES ACROSS DATABASES
-- RE-RUNNABLE | psql ONLY
-- =========================================================

\set ON_ERROR_STOP on

-- =========================================================
-- TIMECARD SERVICE DATABASE
-- =========================================================
\c postgres_timecard;

CREATE TABLE IF NOT EXISTS timecards (
    id UUID PRIMARY KEY,
    worker_id UUID NOT NULL,
    project_id UUID NOT NULL,
    week_start DATE NOT NULL,
    total_hours INT CHECK (total_hours >= 0),
    status VARCHAR(20) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_timecards_worker
    ON timecards(worker_id);

CREATE INDEX IF NOT EXISTS idx_timecards_project
    ON timecards(project_id);
	
-- 1️⃣ Ensure table exists
CREATE TABLE IF NOT EXISTS event_outbox (
    id UUID PRIMARY KEY,
    event_type TEXT NOT NULL,
    payload JSONB NOT NULL,
    occurred_at TIMESTAMPTZ NOT NULL,
    processed_at TIMESTAMPTZ NULL,
    locked_at TIMESTAMPTZ NULL
);

-- 2️⃣ Index for unprocessed events (CRITICAL)
CREATE INDEX IF NOT EXISTS idx_event_outbox_unprocessed
ON event_outbox (occurred_at)
WHERE processed_at IS NULL AND locked_at IS NULL;

-- 3️⃣ Index for lock handling
CREATE INDEX IF NOT EXISTS idx_event_outbox_locked
ON event_outbox (locked_at);	

-- =========================================================
-- EXPENSE SERVICE DATABASE
-- =========================================================
\c postgres_expense;

CREATE TABLE IF NOT EXISTS expenses (
    id UUID PRIMARY KEY,
    worker_id UUID NOT NULL,
    amount NUMERIC(10,2) CHECK (amount >= 0),
    expense_date DATE NOT NULL,
    description TEXT,
    status VARCHAR(20) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_expenses_worker
    ON expenses(worker_id);

-- =========================================================
-- USER SERVICE DATABASE
-- =========================================================
\c postgres_user;

CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS roles (
    id UUID PRIMARY KEY,
    name VARCHAR(50) UNIQUE NOT NULL
);

CREATE TABLE IF NOT EXISTS user_roles (
    user_id UUID NOT NULL,
    role_id UUID NOT NULL,
    PRIMARY KEY (user_id, role_id),
    CONSTRAINT fk_user_roles_user
        FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_user_roles_role
        FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE
);

-- =========================================================
-- INVOICE & PAYROLL SERVICE DATABASE
-- =========================================================
\c postgres_invoice;

CREATE TABLE IF NOT EXISTS invoices (
    id UUID PRIMARY KEY,
    project_id UUID NOT NULL,
    invoice_month VARCHAR(7) NOT NULL,
    total_amount NUMERIC(12,2) CHECK (total_amount >= 0),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS payroll (
    id UUID PRIMARY KEY,
    worker_id UUID NOT NULL,
    payroll_month VARCHAR(7) NOT NULL,
    amount NUMERIC(10,2) CHECK (amount >= 0),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =========================================================
-- MONOLITH DATABASE
-- =========================================================
\c postgres_monolith;

CREATE TABLE IF NOT EXISTS workers (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS engagements (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    status VARCHAR(20) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS projects (
    id UUID PRIMARY KEY,
    engagement_id UUID,
    worker_id UUID,
    start_date DATE,
    end_date DATE,
    status VARCHAR(20),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_projects_engagement
        FOREIGN KEY (engagement_id) REFERENCES engagements(id),
    CONSTRAINT fk_projects_worker
        FOREIGN KEY (worker_id) REFERENCES workers(id)
);

CREATE TABLE IF NOT EXISTS requisitions (
    id UUID PRIMARY KEY,
    project_id UUID,
    role VARCHAR(50),
    status VARCHAR(20),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_requisitions_project
        FOREIGN KEY (project_id) REFERENCES projects(id)
);

-- =========================================================
-- COMPLETION
-- =========================================================
SELECT 'All schemas created successfully across all databases.' AS status;

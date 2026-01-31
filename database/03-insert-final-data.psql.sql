\set ON_ERROR_STOP on

-- =========================================================
-- USER SERVICE DATABASE
-- =========================================================
\c postgres_user;

TRUNCATE TABLE user_roles, users, roles CASCADE;

-- 25 users
INSERT INTO users (id, username, email, is_active)
SELECT
    gen_random_uuid(),
    'user_' || gs,
    'user_' || gs || '@example.com',
    true
FROM generate_series(1,25) gs;

-- roles
INSERT INTO roles (id, name)
VALUES
  (gen_random_uuid(), 'WORKER'),
  (gen_random_uuid(), 'MANAGER'),
  (gen_random_uuid(), 'ADMIN');

-- role assignment: 15 workers, 8 managers, rest admins
WITH ordered_users AS (
  SELECT id, row_number() OVER () rn FROM users
),
role_ids AS (
  SELECT id, name FROM roles
)
INSERT INTO user_roles (user_id, role_id)
SELECT
  u.id,
  r.id
FROM ordered_users u
JOIN role_ids r
ON (
     (u.rn <= 15 AND r.name = 'WORKER')
  OR (u.rn > 15 AND u.rn <= 23 AND r.name = 'MANAGER')
  OR (u.rn > 23 AND r.name = 'ADMIN')
);

-- =========================================================
-- MONOLITH DATABASE
-- =========================================================
\c postgres_monolith;

TRUNCATE TABLE requisitions, projects, engagements, workers CASCADE;

-- 25 workers (independent of users DB – correct)
INSERT INTO workers (id, name, email)
SELECT
    gen_random_uuid(),
    'Worker ' || gs,
    'worker_' || gs || '@example.com'
FROM generate_series(1,25) gs;

-- 75 engagements
INSERT INTO engagements (id, name, status)
SELECT
    gen_random_uuid(),
    'Engagement ' || gs,
    'ACTIVE'
FROM generate_series(1,75) gs;

-- 75 projects
INSERT INTO projects (id, engagement_id, worker_id, start_date, status)
SELECT
    gen_random_uuid(),
    (SELECT id FROM engagements ORDER BY random() LIMIT 1),
    (SELECT id FROM workers ORDER BY random() LIMIT 1),
    CURRENT_DATE - (random() * 300)::int,
    'ACTIVE'
FROM generate_series(1,75);

-- 75 requisitions (1 per project)
INSERT INTO requisitions (id, project_id, role, status)
SELECT
    gen_random_uuid(),
    id,
    'Developer',
    'APPROVED'
FROM projects;

-- =========================================================
-- TIMECARD SERVICE DATABASE
-- =========================================================
\c postgres_timecard;

TRUNCATE TABLE timecards CASCADE;

-- 500 timecards
INSERT INTO timecards (
    id, worker_id, project_id, week_start, total_hours, status
)
SELECT
    gen_random_uuid(),
    gen_random_uuid(),   -- logical worker reference
    gen_random_uuid(),   -- logical project reference
    CURRENT_DATE - (random() * 120)::int,
    (random() * 40 + 1)::int,
    CASE
      WHEN random() > 0.7 THEN 'APPROVED'
      WHEN random() > 0.4 THEN 'SUBMITTED'
      ELSE 'DRAFT'
    END
FROM generate_series(1,500);

-- =========================================================
-- EXPENSE SERVICE DATABASE
-- =========================================================
\c postgres_expense;

TRUNCATE TABLE expenses CASCADE;

-- 300 expenses
INSERT INTO expenses (
    id, worker_id, amount, expense_date, description, status
)
SELECT
    gen_random_uuid(),
    gen_random_uuid(),   -- logical worker reference
    round((random() * 8000 + 500)::numeric, 2),
    CURRENT_DATE - (random() * 180)::int,
    'Expense ' || gs,
    CASE WHEN random() > 0.5 THEN 'APPROVED' ELSE 'SUBMITTED' END
FROM generate_series(1,300) gs;

-- =========================================================
-- INVOICE & PAYROLL SERVICE DATABASE
-- =========================================================
\c postgres_invoice;

TRUNCATE TABLE payroll, invoices CASCADE;

-- 800 invoices
INSERT INTO invoices (
    id, project_id, invoice_month, total_amount
)
SELECT
    gen_random_uuid(),
    gen_random_uuid(),   -- logical project reference
    to_char(CURRENT_DATE - (random() * 720)::int, 'YYYY-MM'),
    round((random() * 150000 + 30000)::numeric, 2)
FROM generate_series(1,800);

-- payroll: each worker has at least 15 records (25 × 15 = 375)
INSERT INTO payroll (
    id, worker_id, payroll_month, amount
)
SELECT
    gen_random_uuid(),
    gen_random_uuid(),   -- logical worker reference
    to_char(CURRENT_DATE - (m * interval '1 month'), 'YYYY-MM'),
    round((random() * 90000 + 30000)::numeric, 2)
FROM generate_series(1,25) u
CROSS JOIN generate_series(1,15) m;

-- =========================================================
-- DONE
-- =========================================================
SELECT 'FINAL FIXED INSERT SCRIPT COMPLETED SUCCESSFULLY.' AS status;

CREATE TABLE IF NOT EXISTS event_outbox (
    id UUID PRIMARY KEY,
    event_type VARCHAR(100) NOT NULL,
    payload JSONB NOT NULL,
    occurred_at TIMESTAMP NOT NULL,
    processed_at TIMESTAMP NULL
);

CREATE INDEX IF NOT EXISTS idx_event_outbox_unprocessed
    ON event_outbox(processed_at)
    WHERE processed_at IS NULL;


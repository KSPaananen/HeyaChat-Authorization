-- Database: HeyaChat-Authorization
-- This database is designed to run in a linux docker
-- Install list:
-- - pg_cron

-- DROP DATABASE IF EXISTS "HeyaChat";

CREATE DATABASE "HeyaChat-Authorization"
    WITH
    OWNER = postgres
    ENCODING = 'UTF8'
	LC_COLLATE = 'en_US.UTF-8'
	LC_CTYPE = 'en_US.UTF-8'
    LOCALE_PROVIDER = 'libc'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;
	
CREATE TABLE IF NOT EXISTS users (
	user_id BIGSERIAL PRIMARY KEY,
	username VARCHAR(20) NOT NULL UNIQUE,
	password_hash VARCHAR NOT NULL,
	password_salt BYTEA NOT NULL,
	biometrics_key BYTEA NULL,
	email VARCHAR(100) NOT NULL UNIQUE,
	phone VARCHAR(30) NULL UNIQUE
); 

ALTER SEQUENCE users_user_id_seq RESTART WITH 1000;

CREATE TABLE IF NOT EXISTS user_details (
	detail_id BIGSERIAL PRIMARY KEY,
	email_verified BOOLEAN DEFAULT FALSE,
	phone_verified BOOLEAN DEFAULT FALSE,
	mfa_enabled BOOLEAN DEFAULT FALSE,
	created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP, -- only update on row create
	updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP, -- updated by userDetails_update_updated_at trigger
	user_id BIGINT REFERENCES users(user_id) ON DELETE CASCADE
);

ALTER SEQUENCE user_details_detail_id_seq RESTART WITH 1000;

CREATE TABLE IF NOT EXISTS devices (
	device_id BIGSERIAL PRIMARY KEY,
	device_name VARCHAR(50) NOT NULL,
	device_identifier UUID,
	country_tag VARCHAR(3) NOT NULL,
	used_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
	user_id BIGINT REFERENCES users(user_id) ON DELETE CASCADE
);

ALTER SEQUENCE devices_device_id_seq RESTART WITH 1000;

CREATE TABLE IF NOT EXISTS tokens (
	token_id BIGSERIAL PRIMARY KEY,
	identifier UUID UNIQUE NOT NULL,
	expires_at TIMESTAMPTZ NOT NULL,
	active BOOLEAN DEFAULT FALSE,
	device_id BIGINT REFERENCES devices(device_id) ON DELETE CASCADE
);

ALTER SEQUENCE tokens_token_id_seq RESTART WITH 1000;

CREATE TABLE IF NOT EXISTS audit_logs (
	log_id BIGSERIAL PRIMARY KEY,
	performed_action VARCHAR(100),
	performed_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
	device_id BIGINT REFERENCES devices(device_id) ON DELETE CASCADE
);

ALTER SEQUENCE audit_logs_log_id_seq RESTART WITH 1000;

CREATE TABLE IF NOT EXISTS codes (
	code_id BIGSERIAL PRIMARY KEY,
	code VARCHAR(8) NOT NULL,
	expires_at TIMESTAMPTZ NOT NULL,
	used BOOLEAN DEFAULT FALSE,
	user_id BIGINT REFERENCES users(user_id) ON DELETE CASCADE
);

ALTER SEQUENCE codes_code_id_seq RESTART WITH 1000;

CREATE TABLE IF NOT EXISTS suspensions (
	suspension_id BIGSERIAL PRIMARY KEY,
	reason VARCHAR(150),
	suspended_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
	expires_at TIMESTAMPTZ NULL,
	lifted_at TIMESTAMPTZ NULL,
	user_id BIGINT REFERENCES users(user_id) ON DELETE CASCADE
);

ALTER SEQUENCE suspensions_suspension_id_seq RESTART WITH 1000;

CREATE TABLE IF NOT EXISTS blocked_credentials (
	block_id BIGSERIAL PRIMARY KEY,
	email VARCHAR(100) NOT NULL,
	phone VARCHAR(30) NULL
);

ALTER SEQUENCE blocked_credentials_block_id_seq RESTART WITH 1000;

CREATE TABLE IF NOT EXISTS delete_requests (
	delete_id BIGSERIAL PRIMARY KEY,
	user_id BIGINT NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
	date_requested TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
	fulfilled BOOLEAN DEFAULT FALSE
);

ALTER SEQUENCE delete_requests_delete_id_seq RESTART WITH 1000;



			-- Indexes --
			
-- DROP INDEX name;
			
CREATE INDEX idx_users_user_id ON users(user_id);
CREATE INDEX idx_user_details_user_id on user_details(user_id);
CREATE INDEX idx_devices_user_id on devices(user_id);
CREATE INDEX idx_tokens_device_id on tokens(device_id);
CREATE INDEX idx_audit_logs_device_id on audit_logs(device_id);
CREATE INDEX idx_codes_user_id on codes(user_id);
CREATE INDEX idx_suspensions_user_id on suspensions(user_id);
CREATE INDEX idx_blocked_credentials_email on blocked_credentials(email);
CREATE INDEX idx_delete_requests_date_requested on delete_requests(date_requested);
		
			
			
			-- Functions --

-- Update updated_at in userDetails table on update
CREATE OR REPLACE FUNCTION update_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- update used_at in devices table on update
CREATE OR REPLACE FUNCTION update_used_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.used_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

--
CREATE OR REPLACE FUNCTION insert_blocked_credentials()
RETURNS TRIGGER AS $$
BEGIN
    IF EXISTS (SELECT 1 FROM suspensions WHERE user_id = OLD.user_id AND expires_at IS NULL) THEN
        INSERT INTO blocked_credentials (email, phone)
        VALUES (OLD.email, OLD.phone);
    END IF;
    RETURN OLD;
END;
$$ LANGUAGE plpgsql;



			-- Triggers --

-- Update updated_at in userDetails table on update
CREATE TRIGGER user_details_update_updated_at
BEFORE UPDATE ON user_details
FOR EACH ROW
EXECUTE FUNCTION update_updated_at();

-- update used_at in devices table on update
CREATE TRIGGER devices_update_used_at
BEFORE UPDATE ON devices
FOR EACH ROW
EXECUTE FUNCTION update_used_at();

-- insert users phone and email to blocked_credentials if suspensions expires_at is null
CREATE TRIGGER before_user_delete_insert_to_blocked_credentials
BEFORE DELETE ON users
FOR EACH ROW
EXECUTE FUNCTION insert_blocked_credentials();



			-- Scheduled jobs --
			
CREATE EXTENSION IF NOT EXISTS pg_cron;

-- Clear expired rows from mfaCodes at midnight
PERFORM cron.schedule('clear_expired_codes', '0 0 * * *',
    'DELETE FROM codes WHERE expires_at < NOW()');
	
-- Clear expired rows from tokens table. Run at 1 am
PERFORM cron.schedule('clear_expired_tokens', '0 1 * * *',
    'DELETE FROM tokens WHERE expires_at < NOW()');
	
-- Check if account deletion requests are 60 days old and delete if so. Run at 2 am
PERFORM cron.schedule('delete_requested_accounts', '0 2 * * *',
 'DELETE FROM delete_requests WHERE date_requested < NOW() - INTERVAL ''60 days''');
 
-- Clear rows in audit_logs where performed_at is older than 3 month. Run at 3 am
PERFORM cron.schedule('clear_old_audit_logs', '0 3 * * *',
 'DELETE FROM audit_logs WHERE performed_at < NOW() - INTERVAL ''4 months''');
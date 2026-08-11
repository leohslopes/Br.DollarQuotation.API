CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE currency_quotations (
    id uuid NOT NULL,
    currency_pair character varying(20) NOT NULL,
    bid_price numeric(20,8) NOT NULL,
    ask_price numeric(20,8) NOT NULL,
    high_price numeric(20,8) NOT NULL,
    low_price numeric(20,8) NOT NULL,
    variation numeric(20,8) NOT NULL,
    variation_percentage numeric(20,8) NOT NULL,
    quotation_date timestamp with time zone NOT NULL,
    created_at timestamp with time zone NOT NULL,
    CONSTRAINT "PK_currency_quotations" PRIMARY KEY (id)
);

CREATE TABLE users (
    id uuid NOT NULL,
    name character varying(150) NOT NULL,
    email character varying(200) NOT NULL,
    password_hash character varying(500) NOT NULL,
    photo_base64 text,
    photo_content_type character varying(100),
    is_active boolean NOT NULL DEFAULT TRUE,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    CONSTRAINT "PK_users" PRIMARY KEY (id)
);

CREATE INDEX ix_currency_quotations_currency_pair ON currency_quotations (currency_pair);

CREATE INDEX ix_currency_quotations_quotation_date ON currency_quotations (quotation_date);

CREATE UNIQUE INDEX ux_currency_quotations_pair_date ON currency_quotations (currency_pair, quotation_date);

CREATE UNIQUE INDEX ux_users_email ON users (email);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260807002507_InitialCreate', '9.0.18');

CREATE TABLE quotation_alerts (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    currency_pair character varying(20) NOT NULL,
    condition integer NOT NULL,
    target_price numeric(20,8) NOT NULL,
    is_active boolean NOT NULL,
    triggered_at timestamp with time zone,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    CONSTRAINT "PK_quotation_alerts" PRIMARY KEY (id),
    CONSTRAINT "FK_quotation_alerts_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE INDEX ix_quotation_alerts_currency_pair ON quotation_alerts (currency_pair);

CREATE INDEX ix_quotation_alerts_is_active ON quotation_alerts (is_active);

CREATE INDEX ix_quotation_alerts_user_id ON quotation_alerts (user_id);

CREATE INDEX ix_quotation_alerts_user_pair_active ON quotation_alerts (user_id, currency_pair, is_active);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260807195721_AddQuotationAlerts', '9.0.18');

CREATE TABLE password_reset_tokens (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    token_hash character varying(256) NOT NULL,
    expires_at timestamp with time zone NOT NULL,
    used_at timestamp with time zone,
    created_at timestamp with time zone NOT NULL,
    CONSTRAINT "PK_password_reset_tokens" PRIMARY KEY (id),
    CONSTRAINT "FK_password_reset_tokens_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE INDEX ix_password_reset_tokens_user_id ON password_reset_tokens (user_id);

CREATE INDEX ix_password_reset_tokens_user_status ON password_reset_tokens (user_id, expires_at, used_at);

CREATE UNIQUE INDEX ux_password_reset_tokens_token_hash ON password_reset_tokens (token_hash);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260810173523_AddPasswordResetTokens', '9.0.18');

COMMIT;


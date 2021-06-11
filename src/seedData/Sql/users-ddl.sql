CREATE TABLE "asp_net_roles" (
    "id" TEXT NOT NULL CONSTRAINT "pk_asp_net_roles" PRIMARY KEY,
    "name" TEXT NULL,
    "normalized_name" TEXT NULL,
    "concurrency_stamp" TEXT NULL
);

CREATE TABLE "asp_net_users" (
    "id" TEXT NOT NULL CONSTRAINT "pk_asp_net_users" PRIMARY KEY,
    "user_name" TEXT NULL,
    "normalized_user_name" TEXT NULL,
    "email" TEXT NULL,
    "normalized_email" TEXT NULL,
    "email_confirmed" INTEGER NOT NULL,
    "password_hash" TEXT NULL,
    "security_stamp" TEXT NULL,
    "concurrency_stamp" TEXT NULL,
    "phone_number" TEXT NULL,
    "phone_number_confirmed" INTEGER NOT NULL,
    "two_factor_enabled" INTEGER NOT NULL,
    "lockout_end" TEXT NULL,
    "lockout_enabled" INTEGER NOT NULL,
    "access_failed_count" INTEGER NOT NULL
);

CREATE TABLE "asp_net_role_claims" (
    "id" INTEGER NOT NULL CONSTRAINT "pk_asp_net_role_claims" PRIMARY KEY AUTOINCREMENT,
    "role_id" TEXT NOT NULL,
    "claim_type" TEXT NULL,
    "claim_value" TEXT NULL,
    CONSTRAINT "fk_asp_net_role_claims_asp_net_roles_role_id" FOREIGN KEY ("role_id") REFERENCES "asp_net_roles" ("id") ON DELETE CASCADE
);


CREATE TABLE "asp_net_user_claims" (
    "id" INTEGER NOT NULL CONSTRAINT "pk_asp_net_user_claims" PRIMARY KEY AUTOINCREMENT,
    "user_id" TEXT NOT NULL,
    "claim_type" TEXT NULL,
    "claim_value" TEXT NULL,
    CONSTRAINT "fk_asp_net_user_claims_asp_net_users_user_id" FOREIGN KEY ("user_id") REFERENCES "asp_net_users" ("id") ON DELETE CASCADE
);

CREATE TABLE "asp_net_user_logins" (
    "login_provider" TEXT NOT NULL,
    "provider_key" TEXT NOT NULL,
    "provider_display_name" TEXT NULL,
    "user_id" TEXT NOT NULL,
    CONSTRAINT "pk_asp_net_user_logins" PRIMARY KEY ("login_provider", "provider_key"),
    CONSTRAINT "fk_asp_net_user_logins_asp_net_users_user_id" FOREIGN KEY ("user_id") REFERENCES "asp_net_users" ("id") ON DELETE CASCADE
);


CREATE TABLE "asp_net_user_roles" (
    "user_id" TEXT NOT NULL,
    "role_id" TEXT NOT NULL,
    CONSTRAINT "pk_asp_net_user_roles" PRIMARY KEY ("user_id", "role_id"),
    CONSTRAINT "fk_asp_net_user_roles_asp_net_roles_role_id" FOREIGN KEY ("role_id") REFERENCES "asp_net_roles" ("id") ON DELETE CASCADE,
    CONSTRAINT "fk_asp_net_user_roles_asp_net_users_user_id" FOREIGN KEY ("user_id") REFERENCES "asp_net_users" ("id") ON DELETE CASCADE
);


CREATE TABLE "asp_net_user_tokens" (
    "user_id" TEXT NOT NULL,
    "login_provider" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "value" TEXT NULL,
    CONSTRAINT "pk_asp_net_user_tokens" PRIMARY KEY ("user_id", "login_provider", "name"),
    CONSTRAINT "fk_asp_net_user_tokens_asp_net_users_user_id" FOREIGN KEY ("user_id") REFERENCES "asp_net_users" ("id") ON DELETE CASCADE
);

CREATE INDEX "ix_asp_net_role_claims_role_id" ON "asp_net_role_claims" ("role_id");

CREATE UNIQUE INDEX "role_name_index" ON "asp_net_roles" ("normalized_name");

CREATE INDEX "ix_asp_net_user_claims_user_id" ON "asp_net_user_claims" ("user_id");

CREATE INDEX "ix_asp_net_user_logins_user_id" ON "asp_net_user_logins" ("user_id");

CREATE INDEX "ix_asp_net_user_roles_role_id" ON "asp_net_user_roles" ("role_id");

CREATE INDEX "email_index" ON "asp_net_users" ("normalized_email");

CREATE UNIQUE INDEX "user_name_index" ON "asp_net_users" ("normalized_user_name");
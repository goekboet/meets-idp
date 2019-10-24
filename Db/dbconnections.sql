create user ids_ef with password '';
CREATE DATABASE grants OWNER ids_ef;

create user ids with password '';
GRANT CONNECT ON DATABASE grants to ids;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO ids;

CREATE DATABASE users OWNER ids_ef;
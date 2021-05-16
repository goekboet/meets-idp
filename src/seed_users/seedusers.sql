CREATE TABLE IF NOT EXISTS seed_users(
   "id" TEXT NOT NULL,
   "user_name" TEXT NOT NULL,
   "password" TEXT NOT NULL,
   "password_hash" TEXT NOT NULL,
   "security_stamp" TEXT NOT NULL
);

insert into asp_net_users(
	id, 
	user_name, 
	normalized_user_name,
	email,
	normalized_email,
	email_confirmed, 
	password_hash,
	security_stamp,
	phone_number_confirmed,
	two_factor_enabled,
	lockout_enabled,
	access_failed_count)
select 
	id, 
	user_name,
	UPPER(user_name) as normalized_user_name, 
	user_name as email,
	UPPER(user_name) as normalized_email,
	true as email_confirmed,
	password_hash,
	security_stamp as security_stamp,
	false as phone_number_confirmed,
	false as two_factor_enabled,
	false as lockout_enabled,
	0 as access_failed_count
from seed_users;
CREATE TEMPORARY TABLE seed_users(
   "id" text,
   "user_name" character varying(256),
   "password" text,
   "password_hash" text
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
	md5(random()::text) as security_stamp,
	false as phone_number_confirmed,
	false as two_factor_enabled,
	false as lockout_enabled,
	0 as access_failed_count
from seed_users;
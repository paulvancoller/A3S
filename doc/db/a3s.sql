-- Database generated with pgModeler (PostgreSQL Database Modeler).
-- pgModeler  version: 0.9.2-beta
-- PostgreSQL version: 11.0
-- Project Site: pgmodeler.io
-- Model Author: ---


-- Database creation must be done outside a multicommand file.
-- These commands were put in this file only as a convenience.
-- -- object: identity_server | type: DATABASE --
-- -- DROP DATABASE IF EXISTS identity_server;
-- CREATE DATABASE identity_server
-- 	ENCODING = 'UTF8'
-- 	LC_COLLATE = 'en_US.utf8'
-- 	LC_CTYPE = 'en_US.utf8'
-- 	TABLESPACE = pg_default
-- 	OWNER = postgres;
-- -- ddl-end --
-- 

-- object: _a3s | type: SCHEMA --
-- DROP SCHEMA IF EXISTS _a3s CASCADE;
CREATE SCHEMA _a3s;
-- ddl-end --
ALTER SCHEMA _a3s OWNER TO postgres;
-- ddl-end --

SET search_path TO pg_catalog,public,_a3s;
-- ddl-end --

-- object: _a3s.application | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.application CASCADE;
CREATE TABLE _a3s.application (
	id uuid NOT NULL,
	name text NOT NULL,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT pk_application PRIMARY KEY (id),
	CONSTRAINT uk_application_name UNIQUE (name)

);
-- ddl-end --
COMMENT ON TABLE _a3s.application IS 'Application resources that are protected by IDS4';
-- ddl-end --
ALTER TABLE _a3s.application OWNER TO postgres;
-- ddl-end --

-- object: _a3s.application_function | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.application_function CASCADE;
CREATE TABLE _a3s.application_function (
	id uuid NOT NULL,
	name text NOT NULL,
	description text,
	application_id uuid NOT NULL,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT pk_application_function PRIMARY KEY (id),
	CONSTRAINT uk_application_function_name UNIQUE (name)

);
-- ddl-end --
COMMENT ON TABLE _a3s.application_function IS 'A grouping of permissions belonging to a specific application, as configured by the service developers.';
-- ddl-end --
ALTER TABLE _a3s.application_function OWNER TO postgres;
-- ddl-end --

-- object: _a3s.application_function_permission | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.application_function_permission CASCADE;
CREATE TABLE _a3s.application_function_permission (
	application_function_id uuid NOT NULL,
	permission_id uuid NOT NULL,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT pk_application_function_permission PRIMARY KEY (permission_id,application_function_id)

);
-- ddl-end --
COMMENT ON TABLE _a3s.application_function_permission IS 'Application Function and Permission link';
-- ddl-end --
ALTER TABLE _a3s.application_function_permission OWNER TO postgres;
-- ddl-end --

-- object: _a3s.application_user | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.application_user CASCADE;
CREATE TABLE _a3s.application_user (
	id text NOT NULL,
	user_name text NOT NULL,
	normalized_user_name text NOT NULL,
	email text NOT NULL,
	normalized_email text NOT NULL,
	email_confirmed boolean NOT NULL,
	password_hash text NOT NULL,
	security_stamp text NOT NULL,
	concurrency_stamp text NOT NULL,
	phone_number text,
	phone_number_confirmed boolean NOT NULL,
	two_factor_enabled boolean NOT NULL,
	lockout_end timestamp with time zone,
	lockout_enabled boolean NOT NULL,
	access_failed_count integer NOT NULL,
	ldap_authentication_mode_id uuid,
	first_name text NOT NULL,
	surname text NOT NULL,
	avatar bytea,
	is_deleted boolean NOT NULL DEFAULT false,
	deleted_time timestamp with time zone,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT pk_application_user PRIMARY KEY (id),
	CONSTRAINT uk_application_user_username UNIQUE (user_name),
	CONSTRAINT uk_application_user_normalized_user_name UNIQUE (normalized_user_name)

);
-- ddl-end --
COMMENT ON TABLE _a3s.application_user IS 'Asp,net Identity User profile table.';
-- ddl-end --
COMMENT ON COLUMN _a3s.application_user.id IS 'Asp.Net Identity User ID. Must be string, although Guid is saved here.';
-- ddl-end --
COMMENT ON COLUMN _a3s.application_user.ldap_authentication_mode_id IS 'Link to a LdapAuthenticationMode if applicable';
-- ddl-end --
COMMENT ON COLUMN _a3s.application_user.avatar IS 'Byte array of avatar image';
-- ddl-end --
COMMENT ON COLUMN _a3s.application_user.is_deleted IS 'Indicates that the user has been marked as deleted. The system will treat this as a deleted record.';
-- ddl-end --
ALTER TABLE _a3s.application_user OWNER TO postgres;
-- ddl-end --

-- object: _a3s.application_user_claim_id_seq | type: SEQUENCE --
-- DROP SEQUENCE IF EXISTS _a3s.application_user_claim_id_seq CASCADE;
CREATE SEQUENCE _a3s.application_user_claim_id_seq
	INCREMENT BY 1
	MINVALUE 1
	MAXVALUE 2147483647
	START WITH 1
	CACHE 1
	NO CYCLE
	OWNED BY NONE;
-- ddl-end --
ALTER SEQUENCE _a3s.application_user_claim_id_seq OWNER TO postgres;
-- ddl-end --

-- object: _a3s.application_user_claim | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.application_user_claim CASCADE;
CREATE TABLE _a3s.application_user_claim (
	id integer NOT NULL DEFAULT nextval('_a3s.application_user_claim_id_seq'::regclass),
	claim_type text,
	claim_value text,
	user_id text,
	discriminator text,
	CONSTRAINT pk_application_user_claim PRIMARY KEY (id)

);
-- ddl-end --
COMMENT ON TABLE _a3s.application_user_claim IS 'Asp.Net identity user claims table.';
-- ddl-end --
ALTER TABLE _a3s.application_user_claim OWNER TO postgres;
-- ddl-end --

-- object: _a3s.aspnet_role | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.aspnet_role CASCADE;
CREATE TABLE _a3s.aspnet_role (
	id text NOT NULL,
	name character varying(256),
	normalized_name character varying(256),
	concurrency_stamp text,
	CONSTRAINT pk_aspnet_role PRIMARY KEY (id)

);
-- ddl-end --
COMMENT ON TABLE _a3s.aspnet_role IS 'Asp.Net identity default table. Not Used, but has to exist.';
-- ddl-end --
ALTER TABLE _a3s.aspnet_role OWNER TO postgres;
-- ddl-end --

-- object: _a3s.aspnet_role_claim_id_seq | type: SEQUENCE --
-- DROP SEQUENCE IF EXISTS _a3s.aspnet_role_claim_id_seq CASCADE;
CREATE SEQUENCE _a3s.aspnet_role_claim_id_seq
	INCREMENT BY 1
	MINVALUE 1
	MAXVALUE 2147483647
	START WITH 1
	CACHE 1
	NO CYCLE
	OWNED BY NONE;
-- ddl-end --
ALTER SEQUENCE _a3s.aspnet_role_claim_id_seq OWNER TO postgres;
-- ddl-end --

-- object: _a3s.aspnet_role_claim | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.aspnet_role_claim CASCADE;
CREATE TABLE _a3s.aspnet_role_claim (
	id integer NOT NULL DEFAULT nextval('_a3s.aspnet_role_claim_id_seq'::regclass),
	role_id text NOT NULL,
	claim_type text,
	claim_value text,
	CONSTRAINT pk_aspnet_role_claim PRIMARY KEY (id)

);
-- ddl-end --
COMMENT ON TABLE _a3s.aspnet_role_claim IS 'Asp.Net identity default table. Not Used, but has to exist.';
-- ddl-end --
ALTER TABLE _a3s.aspnet_role_claim OWNER TO postgres;
-- ddl-end --

-- object: _a3s.aspnet_user_login | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.aspnet_user_login CASCADE;
CREATE TABLE _a3s.aspnet_user_login (
	login_provider text NOT NULL,
	provider_key text NOT NULL,
	provider_display_name text,
	user_id text NOT NULL,
	CONSTRAINT pk_aspnet_user_login PRIMARY KEY (login_provider,provider_key)

);
-- ddl-end --
COMMENT ON TABLE _a3s.aspnet_user_login IS 'Asp.Net identity default table. Not Used, but has to exist.';
-- ddl-end --
ALTER TABLE _a3s.aspnet_user_login OWNER TO postgres;
-- ddl-end --

-- object: _a3s.aspnet_user_role | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.aspnet_user_role CASCADE;
CREATE TABLE _a3s.aspnet_user_role (
	user_id text NOT NULL,
	role_id text NOT NULL,
	CONSTRAINT pk_aspnet_user_role PRIMARY KEY (user_id,role_id)

);
-- ddl-end --
COMMENT ON TABLE _a3s.aspnet_user_role IS 'Asp.Net identity default table. Not Used, but has to exist.';
-- ddl-end --
ALTER TABLE _a3s.aspnet_user_role OWNER TO postgres;
-- ddl-end --

-- object: _a3s.application_user_token | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.application_user_token CASCADE;
CREATE TABLE _a3s.application_user_token (
	user_id text NOT NULL,
	login_provider text NOT NULL,
	name text NOT NULL,
	value text,
	is_verified boolean DEFAULT false,
	CONSTRAINT pk_aspnet_user_token PRIMARY KEY (user_id,login_provider,name)

);
-- ddl-end --
COMMENT ON TABLE _a3s.application_user_token IS 'Token provider link to Users';
-- ddl-end --
COMMENT ON COLUMN _a3s.application_user_token.is_verified IS 'Indicates that the user token provider lin has been verified by a user OTP';
-- ddl-end --
ALTER TABLE _a3s.application_user_token OWNER TO postgres;
-- ddl-end --

-- object: _a3s.function | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.function CASCADE;
CREATE TABLE _a3s.function (
	id uuid NOT NULL,
	name text NOT NULL,
	description text,
	application_id uuid NOT NULL,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	sub_realm_id uuid,
	CONSTRAINT pk_function PRIMARY KEY (id),
	CONSTRAINT uk_function_name UNIQUE (name)

);
-- ddl-end --
COMMENT ON TABLE _a3s.function IS 'A grouping of permissions belonging to a specific application, as created by business users within A3S. These are functions that are assigned to roles which result in users receiving the contained permissions.';
-- ddl-end --
ALTER TABLE _a3s.function OWNER TO postgres;
-- ddl-end --

-- object: _a3s.function_permission | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.function_permission CASCADE;
CREATE TABLE _a3s.function_permission (
	function_id uuid NOT NULL,
	permission_id uuid NOT NULL,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT pk_function_permission PRIMARY KEY (permission_id,function_id)

);
-- ddl-end --
COMMENT ON TABLE _a3s.function_permission IS 'Function and Permission link';
-- ddl-end --
ALTER TABLE _a3s.function_permission OWNER TO postgres;
-- ddl-end --

-- object: _a3s.permission | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.permission CASCADE;
CREATE TABLE _a3s.permission (
	id uuid NOT NULL,
	name text NOT NULL,
	description text,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT pk_permission PRIMARY KEY (id),
	CONSTRAINT uk_permission_name UNIQUE (name)

);
-- ddl-end --
COMMENT ON TABLE _a3s.permission IS ' Specific permission inside an application, like read, write or delete';
-- ddl-end --
ALTER TABLE _a3s.permission OWNER TO postgres;
-- ddl-end --

-- object: _a3s.role | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.role CASCADE;
CREATE TABLE _a3s.role (
	id uuid NOT NULL,
	name text NOT NULL,
	description text,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	sub_realm_id uuid,
	CONSTRAINT pk_role PRIMARY KEY (id),
	CONSTRAINT uk_role_name UNIQUE (name)

);
-- ddl-end --
COMMENT ON TABLE _a3s.role IS 'A role a user belongs to';
-- ddl-end --
ALTER TABLE _a3s.role OWNER TO postgres;
-- ddl-end --

-- object: _a3s.role_function | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.role_function CASCADE;
CREATE TABLE _a3s.role_function (
	role_id uuid NOT NULL,
	function_id uuid NOT NULL,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT pk_role_function PRIMARY KEY (role_id,function_id)

);
-- ddl-end --
COMMENT ON TABLE _a3s.role_function IS 'Role and Function link';
-- ddl-end --
ALTER TABLE _a3s.role_function OWNER TO postgres;
-- ddl-end --

-- object: _a3s.team | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.team CASCADE;
CREATE TABLE _a3s.team (
	id uuid NOT NULL,
	name text NOT NULL,
	description text,
	terms_of_service_id uuid,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	sub_realm_id uuid,
	CONSTRAINT pk_team PRIMARY KEY (id),
	CONSTRAINT uk_team_name UNIQUE (name)

);
-- ddl-end --
COMMENT ON TABLE _a3s.team IS 'Team that users belong to';
-- ddl-end --
ALTER TABLE _a3s.team OWNER TO postgres;
-- ddl-end --

-- object: _a3s.user_role | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.user_role CASCADE;
CREATE TABLE _a3s.user_role (
	user_id text NOT NULL,
	role_id uuid NOT NULL,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT pk_user_role PRIMARY KEY (role_id,user_id)

);
-- ddl-end --
COMMENT ON TABLE _a3s.user_role IS 'User and Role link';
-- ddl-end --
ALTER TABLE _a3s.user_role OWNER TO postgres;
-- ddl-end --

-- object: _a3s.user_team | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.user_team CASCADE;
CREATE TABLE _a3s.user_team (
	user_id text NOT NULL,
	team_id uuid NOT NULL,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT pk_user_team PRIMARY KEY (team_id,user_id)

);
-- ddl-end --
COMMENT ON TABLE _a3s.user_team IS 'Users and Teams link';
-- ddl-end --
ALTER TABLE _a3s.user_team OWNER TO postgres;
-- ddl-end --

-- object: ix_application_function_application_id | type: INDEX --
-- DROP INDEX IF EXISTS _a3s.ix_application_function_application_id CASCADE;
CREATE INDEX ix_application_function_application_id ON _a3s.application_function
	USING btree
	(
	  application_id
	)
	WITH (FILLFACTOR = 90);
-- ddl-end --

-- object: ix_application_function_permission_application_function_id | type: INDEX --
-- DROP INDEX IF EXISTS _a3s.ix_application_function_permission_application_function_id CASCADE;
CREATE INDEX ix_application_function_permission_application_function_id ON _a3s.application_function_permission
	USING btree
	(
	  application_function_id
	)
	WITH (FILLFACTOR = 90);
-- ddl-end --

-- object: ix_application_user_claim_user_id | type: INDEX --
-- DROP INDEX IF EXISTS _a3s.ix_application_user_claim_user_id CASCADE;
CREATE INDEX ix_application_user_claim_user_id ON _a3s.application_user_claim
	USING btree
	(
	  user_id
	)
	WITH (FILLFACTOR = 90);
-- ddl-end --

-- object: ix_aspnet_role_claim_role_id | type: INDEX --
-- DROP INDEX IF EXISTS _a3s.ix_aspnet_role_claim_role_id CASCADE;
CREATE INDEX ix_aspnet_role_claim_role_id ON _a3s.aspnet_role_claim
	USING btree
	(
	  role_id
	)
	WITH (FILLFACTOR = 90);
-- ddl-end --

-- object: ix_aspnet_user_login_user_id | type: INDEX --
-- DROP INDEX IF EXISTS _a3s.ix_aspnet_user_login_user_id CASCADE;
CREATE INDEX ix_aspnet_user_login_user_id ON _a3s.aspnet_user_login
	USING btree
	(
	  user_id
	)
	WITH (FILLFACTOR = 90);
-- ddl-end --

-- object: ix_aspnet_user_role_role_id | type: INDEX --
-- DROP INDEX IF EXISTS _a3s.ix_aspnet_user_role_role_id CASCADE;
CREATE INDEX ix_aspnet_user_role_role_id ON _a3s.aspnet_user_role
	USING btree
	(
	  role_id
	)
	WITH (FILLFACTOR = 90);
-- ddl-end --

-- object: ix_function_application_id | type: INDEX --
-- DROP INDEX IF EXISTS _a3s.ix_function_application_id CASCADE;
CREATE INDEX ix_function_application_id ON _a3s.function
	USING btree
	(
	  application_id
	)
	WITH (FILLFACTOR = 90);
-- ddl-end --

-- object: ix_function_permission_function_id | type: INDEX --
-- DROP INDEX IF EXISTS _a3s.ix_function_permission_function_id CASCADE;
CREATE INDEX ix_function_permission_function_id ON _a3s.function_permission
	USING btree
	(
	  function_id
	)
	WITH (FILLFACTOR = 90);
-- ddl-end --

-- object: ix_role_function_function_id | type: INDEX --
-- DROP INDEX IF EXISTS _a3s.ix_role_function_function_id CASCADE;
CREATE INDEX ix_role_function_function_id ON _a3s.role_function
	USING btree
	(
	  function_id
	)
	WITH (FILLFACTOR = 90);
-- ddl-end --

-- object: ix_role_name | type: INDEX --
-- DROP INDEX IF EXISTS _a3s.ix_role_name CASCADE;
CREATE UNIQUE INDEX ix_role_name ON _a3s.role
	USING btree
	(
	  name
	)
	WITH (FILLFACTOR = 90);
-- ddl-end --

-- object: ix_user_role_user_id | type: INDEX --
-- DROP INDEX IF EXISTS _a3s.ix_user_role_user_id CASCADE;
CREATE INDEX ix_user_role_user_id ON _a3s.user_role
	USING btree
	(
	  user_id
	)
	WITH (FILLFACTOR = 90);
-- ddl-end --

-- object: ix_user_team_user_id | type: INDEX --
-- DROP INDEX IF EXISTS _a3s.ix_user_team_user_id CASCADE;
CREATE INDEX ix_user_team_user_id ON _a3s.user_team
	USING btree
	(
	  user_id
	)
	WITH (FILLFACTOR = 90);
-- ddl-end --

-- object: role_name_index | type: INDEX --
-- DROP INDEX IF EXISTS _a3s.role_name_index CASCADE;
CREATE UNIQUE INDEX role_name_index ON _a3s.aspnet_role
	USING btree
	(
	  normalized_name
	)
	WITH (FILLFACTOR = 90);
-- ddl-end --

-- object: _a3s.team_team | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.team_team CASCADE;
CREATE TABLE _a3s.team_team (
	parent_team_id uuid NOT NULL,
	child_team_id uuid NOT NULL,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT pk_team_team PRIMARY KEY (parent_team_id,child_team_id)

);
-- ddl-end --
COMMENT ON TABLE _a3s.team_team IS 'Team of Teams (compound teams) definition';
-- ddl-end --
ALTER TABLE _a3s.team_team OWNER TO postgres;
-- ddl-end --

-- object: ix_team_team_child_team_id | type: INDEX --
-- DROP INDEX IF EXISTS _a3s.ix_team_team_child_team_id CASCADE;
CREATE INDEX ix_team_team_child_team_id ON _a3s.team_team
	USING btree
	(
	  child_team_id
	)
	WITH (FILLFACTOR = 90);
-- ddl-end --

-- object: _a3s.role_role | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.role_role CASCADE;
CREATE TABLE _a3s.role_role (
	parent_role_id uuid NOT NULL,
	child_role_id uuid NOT NULL,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT pk_role_role PRIMARY KEY (parent_role_id,child_role_id)

);
-- ddl-end --
COMMENT ON TABLE _a3s.role_role IS 'Role of Roles (compound role) definition';
-- ddl-end --
ALTER TABLE _a3s.role_role OWNER TO postgres;
-- ddl-end --

-- object: ix_role_role_child_role_id | type: INDEX --
-- DROP INDEX IF EXISTS _a3s.ix_role_role_child_role_id CASCADE;
CREATE INDEX ix_role_role_child_role_id ON _a3s.role_role
	USING btree
	(
	  child_role_id
	)
	WITH (FILLFACTOR = 90);
-- ddl-end --

-- object: _a3s.ldap_authentication_mode | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.ldap_authentication_mode CASCADE;
CREATE TABLE _a3s.ldap_authentication_mode (
	id uuid NOT NULL,
	name text NOT NULL,
	host_name text NOT NULL,
	port integer NOT NULL,
	is_ldaps boolean NOT NULL DEFAULT true,
	account text NOT NULL,
	password text NOT NULL,
	base_dn text NOT NULL,
	changed_by uuid NOT NULL,
	sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT ldap_authentication_mode_pkey PRIMARY KEY (id),
	CONSTRAINT uk_ldap_authentication_mode_name UNIQUE (name)

);
-- ddl-end --
COMMENT ON TABLE _a3s.ldap_authentication_mode IS 'LDAP Profile definitions';
-- ddl-end --
COMMENT ON COLUMN _a3s.ldap_authentication_mode.is_ldaps IS 'Indicates that this utulizes a secure LDAP connection';
-- ddl-end --
COMMENT ON COLUMN _a3s.ldap_authentication_mode.account IS 'Ldap admin username
';
-- ddl-end --
COMMENT ON COLUMN _a3s.ldap_authentication_mode.password IS 'Ldap admin password';
-- ddl-end --
COMMENT ON COLUMN _a3s.ldap_authentication_mode.base_dn IS 'The LDAP Base DN Address';
-- ddl-end --
ALTER TABLE _a3s.ldap_authentication_mode OWNER TO postgres;
-- ddl-end --

-- object: _a3s.ldap_authentication_mode_ldap_attribute_id_seq | type: SEQUENCE --
-- DROP SEQUENCE IF EXISTS _a3s.ldap_authentication_mode_ldap_attribute_id_seq CASCADE;
CREATE SEQUENCE _a3s.ldap_authentication_mode_ldap_attribute_id_seq
	INCREMENT BY 1
	MINVALUE 1
	MAXVALUE 2147483647
	START WITH 1
	CACHE 1
	NO CYCLE
	OWNED BY NONE;
-- ddl-end --
ALTER SEQUENCE _a3s.ldap_authentication_mode_ldap_attribute_id_seq OWNER TO postgres;
-- ddl-end --

-- object: _a3s.ldap_authentication_mode_ldap_attribute | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.ldap_authentication_mode_ldap_attribute CASCADE;
CREATE TABLE _a3s.ldap_authentication_mode_ldap_attribute (
	id integer NOT NULL DEFAULT nextval('_a3s.ldap_authentication_mode_ldap_attribute_id_seq'::regclass),
	ldap_authentication_mode_id uuid NOT NULL,
	user_field text NOT NULL,
	ldap_field text NOT NULL,
	changed_by uuid NOT NULL,
	sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT ldap_authentication_mode_ldap_attribute_pkey PRIMARY KEY (id),
	CONSTRAINT uk_ldap_authentication_mode_ldap_attribute_1 UNIQUE (ldap_authentication_mode_id,user_field,ldap_field)

);
-- ddl-end --
COMMENT ON TABLE _a3s.ldap_authentication_mode_ldap_attribute IS 'Attribute to User field mappings for Ldap detail synchronisation';
-- ddl-end --
COMMENT ON COLUMN _a3s.ldap_authentication_mode_ldap_attribute.user_field IS 'The field in the ApplicationUser table';
-- ddl-end --
COMMENT ON COLUMN _a3s.ldap_authentication_mode_ldap_attribute.ldap_field IS 'The field in the LDAP directory';
-- ddl-end --
ALTER TABLE _a3s.ldap_authentication_mode_ldap_attribute OWNER TO postgres;
-- ddl-end --

-- object: pgcrypto | type: EXTENSION --
-- DROP EXTENSION IF EXISTS pgcrypto CASCADE;
CREATE EXTENSION pgcrypto
WITH SCHEMA _a3s
VERSION '1.3';
-- ddl-end --
COMMENT ON EXTENSION pgcrypto IS 'cryptographic functions';
-- ddl-end --

-- object: _a3s.application_data_policy | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.application_data_policy CASCADE;
CREATE TABLE _a3s.application_data_policy (
	id uuid NOT NULL,
	name text,
	description text,
	application_id uuid NOT NULL,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT pk_application_data_policy PRIMARY KEY (id),
	CONSTRAINT uk_application_data_policy_name UNIQUE (name)

);
-- ddl-end --
COMMENT ON TABLE _a3s.application_data_policy IS 'Data policies defined for an application';
-- ddl-end --
ALTER TABLE _a3s.application_data_policy OWNER TO postgres;
-- ddl-end --

-- object: ix_application_data_policy_name | type: INDEX --
-- DROP INDEX IF EXISTS _a3s.ix_application_data_policy_name CASCADE;
CREATE INDEX ix_application_data_policy_name ON _a3s.application_data_policy
	USING btree
	(
	  name
	)
	WITH (FILLFACTOR = 90);
-- ddl-end --

-- object: _a3s.team_application_data_policy | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.team_application_data_policy CASCADE;
CREATE TABLE _a3s.team_application_data_policy (
	team_id uuid NOT NULL,
	application_data_policy_id uuid NOT NULL,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT pk_team_application_data_policy PRIMARY KEY (team_id,application_data_policy_id)

);
-- ddl-end --
COMMENT ON TABLE _a3s.team_application_data_policy IS 'Data policies and Teams link';
-- ddl-end --
ALTER TABLE _a3s.team_application_data_policy OWNER TO postgres;
-- ddl-end --

-- object: _a3s.terms_of_service | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.terms_of_service CASCADE;
CREATE TABLE _a3s.terms_of_service (
	id uuid NOT NULL,
	agreement_name text NOT NULL,
	version text NOT NULL,
	agreement_file bytea NOT NULL,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	sub_realm_id uuid,
	CONSTRAINT terms_of_service_pk PRIMARY KEY (id),
	CONSTRAINT uk_terms_of_service_agreement_name_version UNIQUE (agreement_name,version)

);
-- ddl-end --
COMMENT ON TABLE _a3s.terms_of_service IS 'Terms of service agreement entries that users agree to.';
-- ddl-end --
COMMENT ON COLUMN _a3s.terms_of_service.version IS 'The version of the agreement. Format is {year}.{number}, i.e. 2019.6.';
-- ddl-end --
COMMENT ON COLUMN _a3s.terms_of_service.agreement_file IS 'A .tar.gz file, containing two files with the terms agreement: 

- terms_of_service.html
- terms_of_service.css';
-- ddl-end --
ALTER TABLE _a3s.terms_of_service OWNER TO postgres;
-- ddl-end --

-- object: _a3s.terms_of_service_user_acceptance | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.terms_of_service_user_acceptance CASCADE;
CREATE TABLE _a3s.terms_of_service_user_acceptance (
	terms_of_service_id uuid NOT NULL,
	user_id text NOT NULL,
	acceptance_time tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT terms_of_service_user_acceptance_pk PRIMARY KEY (terms_of_service_id,user_id)

);
-- ddl-end --
COMMENT ON TABLE _a3s.terms_of_service_user_acceptance IS 'This records the acceptance of terms of service entries by users.';
-- ddl-end --
COMMENT ON COLUMN _a3s.terms_of_service_user_acceptance.acceptance_time IS 'The date and time the user accepted the specific agreement.';
-- ddl-end --
ALTER TABLE _a3s.terms_of_service_user_acceptance OWNER TO postgres;
-- ddl-end --

-- object: _a3s.terms_of_service_user_acceptance_history | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.terms_of_service_user_acceptance_history CASCADE;
CREATE TABLE _a3s.terms_of_service_user_acceptance_history (
	terms_of_service_id uuid NOT NULL,
	user_id text NOT NULL,
	acceptance_time tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT terms_of_service_user_acceptance_history_pk PRIMARY KEY (terms_of_service_id,user_id)

);
-- ddl-end --
COMMENT ON TABLE _a3s.terms_of_service_user_acceptance_history IS 'This stores the history of the acceptance of terms of service entries by users.
On every update of a terms of service agreement version for a team, all user acceptance records get copied from ''terms_of_service_user_acceptance'' to ''terms_of_service_user_acceptance_history''.';
-- ddl-end --
COMMENT ON COLUMN _a3s.terms_of_service_user_acceptance_history.acceptance_time IS 'The date and time the user accepted the specific agreement.';
-- ddl-end --
ALTER TABLE _a3s.terms_of_service_user_acceptance_history OWNER TO postgres;
-- ddl-end --

-- object: _a3s.profile | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.profile CASCADE;
CREATE TABLE _a3s.profile (
	id uuid NOT NULL,
	name text NOT NULL,
	description text,
	user_id text NOT NULL,
	sub_realm_id uuid NOT NULL,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT pk_profile PRIMARY KEY (id)

);
-- ddl-end --
COMMENT ON COLUMN _a3s.profile.id IS 'The UUID identifier for a profile.';
-- ddl-end --
COMMENT ON COLUMN _a3s.profile.name IS 'The name of the profile. This must be unique for each user.';
-- ddl-end --
COMMENT ON COLUMN _a3s.profile.description IS 'A brief description of the profile and it''s purpose.';
-- ddl-end --
COMMENT ON COLUMN _a3s.profile.changed_by IS 'The UUID of the user that last changed this record.';
-- ddl-end --
COMMENT ON COLUMN _a3s.profile.sys_period IS 'The temporal data for when this record was changed.';
-- ddl-end --
ALTER TABLE _a3s.profile OWNER TO postgres;
-- ddl-end --

-- object: fk_profile_application_user_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.profile DROP CONSTRAINT IF EXISTS fk_profile_application_user_id CASCADE;
ALTER TABLE _a3s.profile ADD CONSTRAINT fk_profile_application_user_id FOREIGN KEY (user_id)
REFERENCES _a3s.application_user (id) MATCH FULL
ON DELETE CASCADE ON UPDATE CASCADE;
-- ddl-end --

-- object: _a3s.sub_realm | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.sub_realm CASCADE;
CREATE TABLE _a3s.sub_realm (
	id uuid NOT NULL,
	name text NOT NULL,
	description text,
	changed_by uuid,
	sys_preriod tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT uk_sub_realm_name UNIQUE (name),
	CONSTRAINT pk_sub_realm PRIMARY KEY (id)

);
-- ddl-end --
COMMENT ON TABLE _a3s.sub_realm IS 'Table modelling a sub realm within A3S.';
-- ddl-end --
COMMENT ON COLUMN _a3s.sub_realm.id IS 'The UUID identifier for a sub realm.';
-- ddl-end --
COMMENT ON COLUMN _a3s.sub_realm.name IS 'The name of the sub realm. This is a human readable name and must be unique within an A3S instance.';
-- ddl-end --
COMMENT ON COLUMN _a3s.sub_realm.description IS 'A brief description of the sub-realm and it''s intent. ';
-- ddl-end --
COMMENT ON COLUMN _a3s.sub_realm.changed_by IS 'UUID of user that last changed the record.';
-- ddl-end --
COMMENT ON COLUMN _a3s.sub_realm.sys_preriod IS 'Temporal data for this record.';
-- ddl-end --
COMMENT ON CONSTRAINT uk_sub_realm_name ON _a3s.sub_realm  IS 'A uniqueness contraint ensuring that a sub realm''s name is always unique.';
-- ddl-end --
ALTER TABLE _a3s.sub_realm OWNER TO postgres;
-- ddl-end --

-- object: fk_profile_sub_realm_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.profile DROP CONSTRAINT IF EXISTS fk_profile_sub_realm_id CASCADE;
ALTER TABLE _a3s.profile ADD CONSTRAINT fk_profile_sub_realm_id FOREIGN KEY (sub_realm_id)
REFERENCES _a3s.sub_realm (id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: uq_profile | type: CONSTRAINT --
-- ALTER TABLE _a3s.profile DROP CONSTRAINT IF EXISTS uq_profile CASCADE;
ALTER TABLE _a3s.profile ADD CONSTRAINT uq_profile UNIQUE (sub_realm_id);
-- ddl-end --

-- object: _a3s.sub_realm_permission | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.sub_realm_permission CASCADE;
CREATE TABLE _a3s.sub_realm_permission (
	sub_realm_id uuid NOT NULL,
	permission_id uuid NOT NULL,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT pk_sub_realm_permission PRIMARY KEY (sub_realm_id,permission_id)

);
-- ddl-end --
COMMENT ON COLUMN _a3s.sub_realm_permission.changed_by IS 'The UUID of the user that last changed the record.';
-- ddl-end --
COMMENT ON COLUMN _a3s.sub_realm_permission.sys_period IS 'The temporal data for changes to this table.';
-- ddl-end --

-- object: fk_sub_realm_permission_sub_realm_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.sub_realm_permission DROP CONSTRAINT IF EXISTS fk_sub_realm_permission_sub_realm_id CASCADE;
ALTER TABLE _a3s.sub_realm_permission ADD CONSTRAINT fk_sub_realm_permission_sub_realm_id FOREIGN KEY (sub_realm_id)
REFERENCES _a3s.sub_realm (id) MATCH FULL
ON DELETE CASCADE ON UPDATE CASCADE;
-- ddl-end --

-- object: fk_sub_realm_permission_permission_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.sub_realm_permission DROP CONSTRAINT IF EXISTS fk_sub_realm_permission_permission_id CASCADE;
ALTER TABLE _a3s.sub_realm_permission ADD CONSTRAINT fk_sub_realm_permission_permission_id FOREIGN KEY (permission_id)
REFERENCES _a3s.permission (id) MATCH FULL
ON DELETE CASCADE ON UPDATE CASCADE;
-- ddl-end --

-- object: fk_role_sub_realm_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.role DROP CONSTRAINT IF EXISTS fk_role_sub_realm_id CASCADE;
ALTER TABLE _a3s.role ADD CONSTRAINT fk_role_sub_realm_id FOREIGN KEY (sub_realm_id)
REFERENCES _a3s.sub_realm (id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: fk_function_sub_realm_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.function DROP CONSTRAINT IF EXISTS fk_function_sub_realm_id CASCADE;
ALTER TABLE _a3s.function ADD CONSTRAINT fk_function_sub_realm_id FOREIGN KEY (sub_realm_id)
REFERENCES _a3s.sub_realm (id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: fk_team_sub_realm_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.team DROP CONSTRAINT IF EXISTS fk_team_sub_realm_id CASCADE;
ALTER TABLE _a3s.team ADD CONSTRAINT fk_team_sub_realm_id FOREIGN KEY (sub_realm_id)
REFERENCES _a3s.sub_realm (id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: _a3s.profile_role | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.profile_role CASCADE;
CREATE TABLE _a3s.profile_role (
	profile_id uuid NOT NULL,
	role_id uuid NOT NULL,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT profile_role_pk PRIMARY KEY (profile_id,role_id)

);
-- ddl-end --
COMMENT ON COLUMN _a3s.profile_role.changed_by IS 'Stores the UUID of the user that last changed this record.';
-- ddl-end --
COMMENT ON COLUMN _a3s.profile_role.sys_period IS 'Temporal data for this record.';
-- ddl-end --

-- object: fk_profile_role_profile_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.profile_role DROP CONSTRAINT IF EXISTS fk_profile_role_profile_id CASCADE;
ALTER TABLE _a3s.profile_role ADD CONSTRAINT fk_profile_role_profile_id FOREIGN KEY (profile_id)
REFERENCES _a3s.profile (id) MATCH FULL
ON DELETE CASCADE ON UPDATE CASCADE;
-- ddl-end --

-- object: fk_profile_role_role_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.profile_role DROP CONSTRAINT IF EXISTS fk_profile_role_role_id CASCADE;
ALTER TABLE _a3s.profile_role ADD CONSTRAINT fk_profile_role_role_id FOREIGN KEY (role_id)
REFERENCES _a3s.role (id) MATCH FULL
ON DELETE CASCADE ON UPDATE CASCADE;
-- ddl-end --

-- object: _a3s.profile_team | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.profile_team CASCADE;
CREATE TABLE _a3s.profile_team (
	profile_id uuid NOT NULL,
	team_id uuid NOT NULL,
	changed_by uuid,
	sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT profile_team_pk PRIMARY KEY (profile_id,team_id)

);
-- ddl-end --
COMMENT ON COLUMN _a3s.profile_team.changed_by IS 'UUID of user that last modified the record.';
-- ddl-end --
COMMENT ON COLUMN _a3s.profile_team.sys_period IS 'Temporal data for this record.';
-- ddl-end --

-- object: fk_profile_team_profile_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.profile_team DROP CONSTRAINT IF EXISTS fk_profile_team_profile_id CASCADE;
ALTER TABLE _a3s.profile_team ADD CONSTRAINT fk_profile_team_profile_id FOREIGN KEY (profile_id)
REFERENCES _a3s.profile (id) MATCH FULL
ON DELETE CASCADE ON UPDATE CASCADE;
-- ddl-end --

-- object: fk_profile_team_team_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.profile_team DROP CONSTRAINT IF EXISTS fk_profile_team_team_id CASCADE;
ALTER TABLE _a3s.profile_team ADD CONSTRAINT fk_profile_team_team_id FOREIGN KEY (team_id)
REFERENCES _a3s.team (id) MATCH FULL
ON DELETE CASCADE ON UPDATE CASCADE;
-- ddl-end --

-- object: fk_terms_of_service_sub_realm_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.terms_of_service DROP CONSTRAINT IF EXISTS fk_terms_of_service_sub_realm_id CASCADE;
ALTER TABLE _a3s.terms_of_service ADD CONSTRAINT fk_terms_of_service_sub_realm_id FOREIGN KEY (sub_realm_id)
REFERENCES _a3s.sub_realm (id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: fk_application_function_application_application_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.application_function DROP CONSTRAINT IF EXISTS fk_application_function_application_application_id CASCADE;
ALTER TABLE _a3s.application_function ADD CONSTRAINT fk_application_function_application_application_id FOREIGN KEY (application_id)
REFERENCES _a3s.application (id) MATCH SIMPLE
ON DELETE RESTRICT ON UPDATE NO ACTION;
-- ddl-end --

-- object: "fk_application_function_permission_application_function_applic~" | type: CONSTRAINT --
-- ALTER TABLE _a3s.application_function_permission DROP CONSTRAINT IF EXISTS "fk_application_function_permission_application_function_applic~" CASCADE;
ALTER TABLE _a3s.application_function_permission ADD CONSTRAINT "fk_application_function_permission_application_function_applic~" FOREIGN KEY (application_function_id)
REFERENCES _a3s.application_function (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_application_function_permission_permission_permission_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.application_function_permission DROP CONSTRAINT IF EXISTS fk_application_function_permission_permission_permission_id CASCADE;
ALTER TABLE _a3s.application_function_permission ADD CONSTRAINT fk_application_function_permission_permission_permission_id FOREIGN KEY (permission_id)
REFERENCES _a3s.permission (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: application_user_ldap_authentication_mode_id_fkey | type: CONSTRAINT --
-- ALTER TABLE _a3s.application_user DROP CONSTRAINT IF EXISTS application_user_ldap_authentication_mode_id_fkey CASCADE;
ALTER TABLE _a3s.application_user ADD CONSTRAINT application_user_ldap_authentication_mode_id_fkey FOREIGN KEY (ldap_authentication_mode_id)
REFERENCES _a3s.ldap_authentication_mode (id) MATCH SIMPLE
ON DELETE RESTRICT ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_application_user_claim_application_user_user_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.application_user_claim DROP CONSTRAINT IF EXISTS fk_application_user_claim_application_user_user_id CASCADE;
ALTER TABLE _a3s.application_user_claim ADD CONSTRAINT fk_application_user_claim_application_user_user_id FOREIGN KEY (user_id)
REFERENCES _a3s.application_user (id) MATCH SIMPLE
ON DELETE RESTRICT ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_aspnet_role_claim_aspnet_role_role_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.aspnet_role_claim DROP CONSTRAINT IF EXISTS fk_aspnet_role_claim_aspnet_role_role_id CASCADE;
ALTER TABLE _a3s.aspnet_role_claim ADD CONSTRAINT fk_aspnet_role_claim_aspnet_role_role_id FOREIGN KEY (role_id)
REFERENCES _a3s.aspnet_role (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_aspnet_user_login_application_user_user_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.aspnet_user_login DROP CONSTRAINT IF EXISTS fk_aspnet_user_login_application_user_user_id CASCADE;
ALTER TABLE _a3s.aspnet_user_login ADD CONSTRAINT fk_aspnet_user_login_application_user_user_id FOREIGN KEY (user_id)
REFERENCES _a3s.application_user (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_aspnet_user_role_application_user_user_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.aspnet_user_role DROP CONSTRAINT IF EXISTS fk_aspnet_user_role_application_user_user_id CASCADE;
ALTER TABLE _a3s.aspnet_user_role ADD CONSTRAINT fk_aspnet_user_role_application_user_user_id FOREIGN KEY (user_id)
REFERENCES _a3s.application_user (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_aspnet_user_role_aspnet_role_role_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.aspnet_user_role DROP CONSTRAINT IF EXISTS fk_aspnet_user_role_aspnet_role_role_id CASCADE;
ALTER TABLE _a3s.aspnet_user_role ADD CONSTRAINT fk_aspnet_user_role_aspnet_role_role_id FOREIGN KEY (role_id)
REFERENCES _a3s.aspnet_role (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_aspnet_user_token_application_user_user_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.application_user_token DROP CONSTRAINT IF EXISTS fk_aspnet_user_token_application_user_user_id CASCADE;
ALTER TABLE _a3s.application_user_token ADD CONSTRAINT fk_aspnet_user_token_application_user_user_id FOREIGN KEY (user_id)
REFERENCES _a3s.application_user (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_function_application_application_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.function DROP CONSTRAINT IF EXISTS fk_function_application_application_id CASCADE;
ALTER TABLE _a3s.function ADD CONSTRAINT fk_function_application_application_id FOREIGN KEY (application_id)
REFERENCES _a3s.application (id) MATCH SIMPLE
ON DELETE RESTRICT ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_function_permission_function_function_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.function_permission DROP CONSTRAINT IF EXISTS fk_function_permission_function_function_id CASCADE;
ALTER TABLE _a3s.function_permission ADD CONSTRAINT fk_function_permission_function_function_id FOREIGN KEY (function_id)
REFERENCES _a3s.function (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_function_permission_permission_permission_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.function_permission DROP CONSTRAINT IF EXISTS fk_function_permission_permission_permission_id CASCADE;
ALTER TABLE _a3s.function_permission ADD CONSTRAINT fk_function_permission_permission_permission_id FOREIGN KEY (permission_id)
REFERENCES _a3s.permission (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_role_function_function_function_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.role_function DROP CONSTRAINT IF EXISTS fk_role_function_function_function_id CASCADE;
ALTER TABLE _a3s.role_function ADD CONSTRAINT fk_role_function_function_function_id FOREIGN KEY (function_id)
REFERENCES _a3s.function (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_role_function_role_role_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.role_function DROP CONSTRAINT IF EXISTS fk_role_function_role_role_id CASCADE;
ALTER TABLE _a3s.role_function ADD CONSTRAINT fk_role_function_role_role_id FOREIGN KEY (role_id)
REFERENCES _a3s.role (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_team_terms_of_service_terms_of_service_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.team DROP CONSTRAINT IF EXISTS fk_team_terms_of_service_terms_of_service_id CASCADE;
ALTER TABLE _a3s.team ADD CONSTRAINT fk_team_terms_of_service_terms_of_service_id FOREIGN KEY (terms_of_service_id)
REFERENCES _a3s.terms_of_service (id) MATCH FULL
ON DELETE RESTRICT ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_user_role_application_user_user_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.user_role DROP CONSTRAINT IF EXISTS fk_user_role_application_user_user_id CASCADE;
ALTER TABLE _a3s.user_role ADD CONSTRAINT fk_user_role_application_user_user_id FOREIGN KEY (user_id)
REFERENCES _a3s.application_user (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_user_role_role_role_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.user_role DROP CONSTRAINT IF EXISTS fk_user_role_role_role_id CASCADE;
ALTER TABLE _a3s.user_role ADD CONSTRAINT fk_user_role_role_role_id FOREIGN KEY (role_id)
REFERENCES _a3s.role (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_user_team_application_user_user_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.user_team DROP CONSTRAINT IF EXISTS fk_user_team_application_user_user_id CASCADE;
ALTER TABLE _a3s.user_team ADD CONSTRAINT fk_user_team_application_user_user_id FOREIGN KEY (user_id)
REFERENCES _a3s.application_user (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_user_team_team_team_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.user_team DROP CONSTRAINT IF EXISTS fk_user_team_team_team_id CASCADE;
ALTER TABLE _a3s.user_team ADD CONSTRAINT fk_user_team_team_team_id FOREIGN KEY (team_id)
REFERENCES _a3s.team (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_team_team_team_child_team_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.team_team DROP CONSTRAINT IF EXISTS fk_team_team_team_child_team_id CASCADE;
ALTER TABLE _a3s.team_team ADD CONSTRAINT fk_team_team_team_child_team_id FOREIGN KEY (child_team_id)
REFERENCES _a3s.team (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_team_team_team_parent_team_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.team_team DROP CONSTRAINT IF EXISTS fk_team_team_team_parent_team_id CASCADE;
ALTER TABLE _a3s.team_team ADD CONSTRAINT fk_team_team_team_parent_team_id FOREIGN KEY (parent_team_id)
REFERENCES _a3s.team (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_role_role_role_child_role_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.role_role DROP CONSTRAINT IF EXISTS fk_role_role_role_child_role_id CASCADE;
ALTER TABLE _a3s.role_role ADD CONSTRAINT fk_role_role_role_child_role_id FOREIGN KEY (child_role_id)
REFERENCES _a3s.role (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_role_role_role_parent_role_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.role_role DROP CONSTRAINT IF EXISTS fk_role_role_role_parent_role_id CASCADE;
ALTER TABLE _a3s.role_role ADD CONSTRAINT fk_role_role_role_parent_role_id FOREIGN KEY (parent_role_id)
REFERENCES _a3s.role (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: ldap_authentication_mode_ldap__ldap_authentication_mode_id_fkey | type: CONSTRAINT --
-- ALTER TABLE _a3s.ldap_authentication_mode_ldap_attribute DROP CONSTRAINT IF EXISTS ldap_authentication_mode_ldap__ldap_authentication_mode_id_fkey CASCADE;
ALTER TABLE _a3s.ldap_authentication_mode_ldap_attribute ADD CONSTRAINT ldap_authentication_mode_ldap__ldap_authentication_mode_id_fkey FOREIGN KEY (ldap_authentication_mode_id)
REFERENCES _a3s.ldap_authentication_mode (id) MATCH SIMPLE
ON DELETE NO ACTION ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_application_data_policy_application_application_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.application_data_policy DROP CONSTRAINT IF EXISTS fk_application_data_policy_application_application_id CASCADE;
ALTER TABLE _a3s.application_data_policy ADD CONSTRAINT fk_application_data_policy_application_application_id FOREIGN KEY (application_id)
REFERENCES _a3s.application (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_team_application_data_policy_team_team_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.team_application_data_policy DROP CONSTRAINT IF EXISTS fk_team_application_data_policy_team_team_id CASCADE;
ALTER TABLE _a3s.team_application_data_policy ADD CONSTRAINT fk_team_application_data_policy_team_team_id FOREIGN KEY (team_id)
REFERENCES _a3s.team (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_team_application_data_policy_application_data_policy_applica | type: CONSTRAINT --
-- ALTER TABLE _a3s.team_application_data_policy DROP CONSTRAINT IF EXISTS fk_team_application_data_policy_application_data_policy_applica CASCADE;
ALTER TABLE _a3s.team_application_data_policy ADD CONSTRAINT fk_team_application_data_policy_application_data_policy_applica FOREIGN KEY (application_data_policy_id)
REFERENCES _a3s.application_data_policy (id) MATCH SIMPLE
ON DELETE CASCADE ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_terms_of_service_user_acceptance_terms_of_service_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.terms_of_service_user_acceptance DROP CONSTRAINT IF EXISTS fk_terms_of_service_user_acceptance_terms_of_service_id CASCADE;
ALTER TABLE _a3s.terms_of_service_user_acceptance ADD CONSTRAINT fk_terms_of_service_user_acceptance_terms_of_service_id FOREIGN KEY (terms_of_service_id)
REFERENCES _a3s.terms_of_service (id) MATCH FULL
ON DELETE NO ACTION ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_terms_of_service_user_acceptance_user_user_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.terms_of_service_user_acceptance DROP CONSTRAINT IF EXISTS fk_terms_of_service_user_acceptance_user_user_id CASCADE;
ALTER TABLE _a3s.terms_of_service_user_acceptance ADD CONSTRAINT fk_terms_of_service_user_acceptance_user_user_id FOREIGN KEY (user_id)
REFERENCES _a3s.application_user (id) MATCH FULL
ON DELETE NO ACTION ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_terms_of_service_user_acceptance_history_terms_of_service_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.terms_of_service_user_acceptance_history DROP CONSTRAINT IF EXISTS fk_terms_of_service_user_acceptance_history_terms_of_service_id CASCADE;
ALTER TABLE _a3s.terms_of_service_user_acceptance_history ADD CONSTRAINT fk_terms_of_service_user_acceptance_history_terms_of_service_id FOREIGN KEY (terms_of_service_id)
REFERENCES _a3s.terms_of_service (id) MATCH FULL
ON DELETE NO ACTION ON UPDATE NO ACTION;
-- ddl-end --

-- object: fk_terms_of_service_user_acceptance_history_user_user_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.terms_of_service_user_acceptance_history DROP CONSTRAINT IF EXISTS fk_terms_of_service_user_acceptance_history_user_user_id CASCADE;
ALTER TABLE _a3s.terms_of_service_user_acceptance_history ADD CONSTRAINT fk_terms_of_service_user_acceptance_history_user_user_id FOREIGN KEY (user_id)
REFERENCES _a3s.application_user (id) MATCH FULL
ON DELETE NO ACTION ON UPDATE NO ACTION;
-- ddl-end --



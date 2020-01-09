--
-- PostgreSQL database dump
--

-- Dumped from database version 10.7
-- Dumped by pg_dump version 10.7

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: _a3s; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA _a3s;


ALTER SCHEMA _a3s OWNER TO postgres;

--
-- Name: _ids4; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA _ids4;


ALTER SCHEMA _ids4 OWNER TO postgres;

--
-- Name: plpgsql; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;


--
-- Name: EXTENSION plpgsql; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';


--
-- Name: pgcrypto; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA _a3s;


--
-- Name: EXTENSION pgcrypto; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION pgcrypto IS 'cryptographic functions';


SET default_tablespace = '';

SET default_with_oids = false;

--
-- Name: application; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.application (
    id uuid NOT NULL,
    name text NOT NULL,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
);


ALTER TABLE _a3s.application OWNER TO postgres;

--
-- Name: TABLE application; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.application IS 'Application resources that are protected by IDS4';


--
-- Name: application_data_policy; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.application_data_policy (
    id uuid NOT NULL,
    name text,
    description text,
    application_id uuid NOT NULL,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
);


ALTER TABLE _a3s.application_data_policy OWNER TO postgres;

--
-- Name: TABLE application_data_policy; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.application_data_policy IS 'Data policies defined for an application';


--
-- Name: application_function; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.application_function (
    id uuid NOT NULL,
    name text NOT NULL,
    description text,
    application_id uuid NOT NULL,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
);


ALTER TABLE _a3s.application_function OWNER TO postgres;

--
-- Name: TABLE application_function; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.application_function IS 'A grouping of permissions belonging to a specific application, as configured by the service developers.';


--
-- Name: application_function_permission; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.application_function_permission (
    application_function_id uuid NOT NULL,
    permission_id uuid NOT NULL,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
);


ALTER TABLE _a3s.application_function_permission OWNER TO postgres;

--
-- Name: TABLE application_function_permission; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.application_function_permission IS 'Application Function and Permission link';


--
-- Name: application_user; Type: TABLE; Schema: _a3s; Owner: postgres
--

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
    is_deleted boolean DEFAULT false NOT NULL,
    deleted_time timestamp with time zone,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
);


ALTER TABLE _a3s.application_user OWNER TO postgres;

--
-- Name: TABLE application_user; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.application_user IS 'Asp,net Identity User profile table.';


--
-- Name: COLUMN application_user.id; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.application_user.id IS 'Asp.Net Identity User ID. Must be string, although Guid is saved here.';


--
-- Name: COLUMN application_user.ldap_authentication_mode_id; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.application_user.ldap_authentication_mode_id IS 'Link to a LdapAuthenticationMode if applicable';


--
-- Name: COLUMN application_user.avatar; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.application_user.avatar IS 'Byte array of avatar image';


--
-- Name: COLUMN application_user.is_deleted; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.application_user.is_deleted IS 'Indicates that the user has been marked as deleted. The system will treat this as a deleted record.';


--
-- Name: application_user_claim_id_seq; Type: SEQUENCE; Schema: _a3s; Owner: postgres
--

CREATE SEQUENCE _a3s.application_user_claim_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE _a3s.application_user_claim_id_seq OWNER TO postgres;

--
-- Name: application_user_claim; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.application_user_claim (
    id integer DEFAULT nextval('_a3s.application_user_claim_id_seq'::regclass) NOT NULL,
    claim_type text,
    claim_value text,
    user_id text,
    discriminator text
);


ALTER TABLE _a3s.application_user_claim OWNER TO postgres;

--
-- Name: TABLE application_user_claim; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.application_user_claim IS 'Asp.Net identity user claims table.';


--
-- Name: application_user_token; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.application_user_token (
    user_id text NOT NULL,
    login_provider text NOT NULL,
    name text NOT NULL,
    value text,
    is_verified boolean DEFAULT false
);


ALTER TABLE _a3s.application_user_token OWNER TO postgres;

--
-- Name: TABLE application_user_token; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.application_user_token IS 'Token provider link to Users';


--
-- Name: COLUMN application_user_token.is_verified; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.application_user_token.is_verified IS 'Indicates that the user token provider lin has been verified by a user OTP';


--
-- Name: aspnet_role; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.aspnet_role (
    id text NOT NULL,
    name character varying(256),
    normalized_name character varying(256),
    concurrency_stamp text
);


ALTER TABLE _a3s.aspnet_role OWNER TO postgres;

--
-- Name: TABLE aspnet_role; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.aspnet_role IS 'Asp.Net identity default table. Not Used, but has to exist.';


--
-- Name: aspnet_role_claim_id_seq; Type: SEQUENCE; Schema: _a3s; Owner: postgres
--

CREATE SEQUENCE _a3s.aspnet_role_claim_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE _a3s.aspnet_role_claim_id_seq OWNER TO postgres;

--
-- Name: aspnet_role_claim; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.aspnet_role_claim (
    id integer DEFAULT nextval('_a3s.aspnet_role_claim_id_seq'::regclass) NOT NULL,
    role_id text NOT NULL,
    claim_type text,
    claim_value text
);


ALTER TABLE _a3s.aspnet_role_claim OWNER TO postgres;

--
-- Name: TABLE aspnet_role_claim; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.aspnet_role_claim IS 'Asp.Net identity default table. Not Used, but has to exist.';


--
-- Name: aspnet_user_login; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.aspnet_user_login (
    login_provider text NOT NULL,
    provider_key text NOT NULL,
    provider_display_name text,
    user_id text NOT NULL
);


ALTER TABLE _a3s.aspnet_user_login OWNER TO postgres;

--
-- Name: TABLE aspnet_user_login; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.aspnet_user_login IS 'Asp.Net identity default table. Not Used, but has to exist.';


--
-- Name: aspnet_user_role; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.aspnet_user_role (
    user_id text NOT NULL,
    role_id text NOT NULL
);


ALTER TABLE _a3s.aspnet_user_role OWNER TO postgres;

--
-- Name: TABLE aspnet_user_role; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.aspnet_user_role IS 'Asp.Net identity default table. Not Used, but has to exist.';


--
-- Name: flyway_schema_history; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.flyway_schema_history (
    installed_rank integer NOT NULL,
    version character varying(50),
    description character varying(200) NOT NULL,
    type character varying(20) NOT NULL,
    script character varying(1000) NOT NULL,
    checksum integer,
    installed_by character varying(100) NOT NULL,
    installed_on timestamp without time zone DEFAULT now() NOT NULL,
    execution_time integer NOT NULL,
    success boolean NOT NULL
);


ALTER TABLE _a3s.flyway_schema_history OWNER TO postgres;

--
-- Name: function; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.function (
    id uuid NOT NULL,
    name text NOT NULL,
    description text,
    application_id uuid NOT NULL,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL,
    sub_realm_id uuid
);


ALTER TABLE _a3s.function OWNER TO postgres;

--
-- Name: TABLE function; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.function IS 'A grouping of permissions belonging to a specific application, as created by business users within A3S. These are functions that are assigned to roles which result in users receiving the contained permissions.';


--
-- Name: function_permission; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.function_permission (
    function_id uuid NOT NULL,
    permission_id uuid NOT NULL,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
);


ALTER TABLE _a3s.function_permission OWNER TO postgres;

--
-- Name: TABLE function_permission; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.function_permission IS 'Function and Permission link';


--
-- Name: ldap_authentication_mode; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.ldap_authentication_mode (
    id uuid NOT NULL,
    name text NOT NULL,
    host_name text NOT NULL,
    port integer NOT NULL,
    is_ldaps boolean DEFAULT true NOT NULL,
    account text NOT NULL,
    password text NOT NULL,
    base_dn text NOT NULL,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone)
);


ALTER TABLE _a3s.ldap_authentication_mode OWNER TO postgres;

--
-- Name: TABLE ldap_authentication_mode; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.ldap_authentication_mode IS 'LDAP Profile definitions';


--
-- Name: COLUMN ldap_authentication_mode.is_ldaps; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.ldap_authentication_mode.is_ldaps IS 'Indicates that this utulizes a secure LDAP connection';


--
-- Name: COLUMN ldap_authentication_mode.account; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.ldap_authentication_mode.account IS 'Ldap admin username
';


--
-- Name: COLUMN ldap_authentication_mode.password; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.ldap_authentication_mode.password IS 'Ldap admin password';


--
-- Name: COLUMN ldap_authentication_mode.base_dn; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.ldap_authentication_mode.base_dn IS 'The LDAP Base DN Address';


--
-- Name: ldap_authentication_mode_ldap_attribute_id_seq; Type: SEQUENCE; Schema: _a3s; Owner: postgres
--

CREATE SEQUENCE _a3s.ldap_authentication_mode_ldap_attribute_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE _a3s.ldap_authentication_mode_ldap_attribute_id_seq OWNER TO postgres;

--
-- Name: ldap_authentication_mode_ldap_attribute; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.ldap_authentication_mode_ldap_attribute (
    id integer DEFAULT nextval('_a3s.ldap_authentication_mode_ldap_attribute_id_seq'::regclass) NOT NULL,
    ldap_authentication_mode_id uuid NOT NULL,
    user_field text NOT NULL,
    ldap_field text NOT NULL,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone)
);


ALTER TABLE _a3s.ldap_authentication_mode_ldap_attribute OWNER TO postgres;

--
-- Name: TABLE ldap_authentication_mode_ldap_attribute; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.ldap_authentication_mode_ldap_attribute IS 'Attribute to User field mappings for Ldap detail synchronisation';


--
-- Name: COLUMN ldap_authentication_mode_ldap_attribute.user_field; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.ldap_authentication_mode_ldap_attribute.user_field IS 'The field in the ApplicationUser table';


--
-- Name: COLUMN ldap_authentication_mode_ldap_attribute.ldap_field; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.ldap_authentication_mode_ldap_attribute.ldap_field IS 'The field in the LDAP directory';


--
-- Name: permission; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.permission (
    id uuid NOT NULL,
    name text NOT NULL,
    description text,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
);


ALTER TABLE _a3s.permission OWNER TO postgres;

--
-- Name: TABLE permission; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.permission IS ' Specific permission inside an application, like read, write or delete';


--
-- Name: profile; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.profile (
    id uuid NOT NULL,
    name text NOT NULL,
    description text,
    user_id text NOT NULL,
    sub_realm_id uuid NOT NULL,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
);


ALTER TABLE _a3s.profile OWNER TO postgres;

--
-- Name: COLUMN profile.id; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.profile.id IS 'The UUID identifier for a profile.';


--
-- Name: COLUMN profile.name; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.profile.name IS 'The name of the profile. This must be unique for each user.';


--
-- Name: COLUMN profile.description; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.profile.description IS 'A brief description of the profile and it''s purpose.';


--
-- Name: COLUMN profile.changed_by; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.profile.changed_by IS 'The UUID of the user that last changed this record.';


--
-- Name: COLUMN profile.sys_period; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.profile.sys_period IS 'The temporal data for when this record was changed.';


--
-- Name: profile_role; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.profile_role (
    profile_id uuid NOT NULL,
    role_id uuid NOT NULL,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
);


ALTER TABLE _a3s.profile_role OWNER TO postgres;

--
-- Name: COLUMN profile_role.changed_by; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.profile_role.changed_by IS 'Stores the UUID of the user that last changed this record.';


--
-- Name: COLUMN profile_role.sys_period; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.profile_role.sys_period IS 'Temporal data for this record.';


--
-- Name: profile_team; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.profile_team (
    profile_id uuid NOT NULL,
    team_id uuid NOT NULL,
    changed_by uuid,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone)
);


ALTER TABLE _a3s.profile_team OWNER TO postgres;

--
-- Name: COLUMN profile_team.changed_by; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.profile_team.changed_by IS 'UUID of user that last modified the record.';


--
-- Name: COLUMN profile_team.sys_period; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.profile_team.sys_period IS 'Temporal data for this record.';


--
-- Name: role; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.role (
    id uuid NOT NULL,
    name text NOT NULL,
    description text,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL,
    sub_realm_id uuid
);


ALTER TABLE _a3s.role OWNER TO postgres;

--
-- Name: TABLE role; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.role IS 'A role a user belongs to';


--
-- Name: role_function; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.role_function (
    role_id uuid NOT NULL,
    function_id uuid NOT NULL,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
);


ALTER TABLE _a3s.role_function OWNER TO postgres;

--
-- Name: TABLE role_function; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.role_function IS 'Role and Function link';


--
-- Name: role_role; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.role_role (
    parent_role_id uuid NOT NULL,
    child_role_id uuid NOT NULL,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
);


ALTER TABLE _a3s.role_role OWNER TO postgres;

--
-- Name: TABLE role_role; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.role_role IS 'Role of Roles (compound role) definition';


--
-- Name: sub_realm; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.sub_realm (
    id uuid NOT NULL,
    name text NOT NULL,
    description text,
    changed_by uuid,
    sys_preriod tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone)
);


ALTER TABLE _a3s.sub_realm OWNER TO postgres;

--
-- Name: TABLE sub_realm; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.sub_realm IS 'Table modelling a sub realm within A3S.';


--
-- Name: COLUMN sub_realm.id; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.sub_realm.id IS 'The UUID identifier for a sub realm.';


--
-- Name: COLUMN sub_realm.name; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.sub_realm.name IS 'The name of the sub realm. This is a human readable name and must be unique within an A3S instance.';


--
-- Name: COLUMN sub_realm.description; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.sub_realm.description IS 'A brief description of the sub-realm and it''s intent. ';


--
-- Name: COLUMN sub_realm.changed_by; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.sub_realm.changed_by IS 'UUID of user that last changed the record.';


--
-- Name: COLUMN sub_realm.sys_preriod; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.sub_realm.sys_preriod IS 'Temporal data for this record.';


--
-- Name: sub_realm_permission; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.sub_realm_permission (
    sub_realm_id uuid NOT NULL,
    permission_id uuid NOT NULL,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
);


ALTER TABLE _a3s.sub_realm_permission OWNER TO postgres;

--
-- Name: COLUMN sub_realm_permission.changed_by; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.sub_realm_permission.changed_by IS 'The UUID of the user that last changed the record.';


--
-- Name: COLUMN sub_realm_permission.sys_period; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.sub_realm_permission.sys_period IS 'The temporal data for changes to this table.';


--
-- Name: team; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.team (
    id uuid NOT NULL,
    name text NOT NULL,
    description text,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL,
    terms_of_service_id uuid,
    sub_realm_id uuid
);


ALTER TABLE _a3s.team OWNER TO postgres;

--
-- Name: TABLE team; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.team IS 'Team that users belong to';


--
-- Name: team_application_data_policy; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.team_application_data_policy (
    team_id uuid NOT NULL,
    application_data_policy_id uuid NOT NULL,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
);


ALTER TABLE _a3s.team_application_data_policy OWNER TO postgres;

--
-- Name: TABLE team_application_data_policy; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.team_application_data_policy IS 'Data policies and Teams link';


--
-- Name: team_team; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.team_team (
    parent_team_id uuid NOT NULL,
    child_team_id uuid NOT NULL,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
);


ALTER TABLE _a3s.team_team OWNER TO postgres;

--
-- Name: TABLE team_team; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.team_team IS 'Team of Teams (compound teams) definition';


--
-- Name: terms_of_service; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.terms_of_service (
    id uuid NOT NULL,
    agreement_name text NOT NULL,
    version text NOT NULL,
    agreement_file bytea NOT NULL,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL,
    sub_realm_id uuid
);


ALTER TABLE _a3s.terms_of_service OWNER TO postgres;

--
-- Name: TABLE terms_of_service; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.terms_of_service IS 'Terms of service agreement entries that users agree to.';


--
-- Name: COLUMN terms_of_service.version; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.terms_of_service.version IS 'The version of the agreement. Format is {year}.{number}, i.e. 2019.6.';


--
-- Name: COLUMN terms_of_service.agreement_file; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.terms_of_service.agreement_file IS 'A .tar.gz file, containing two files with the terms agreement: 
- terms_of_service.html
- terms_of_service.css';


--
-- Name: terms_of_service_user_acceptance; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.terms_of_service_user_acceptance (
    terms_of_service_id uuid NOT NULL,
    user_id text NOT NULL,
    acceptance_time tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
);


ALTER TABLE _a3s.terms_of_service_user_acceptance OWNER TO postgres;

--
-- Name: TABLE terms_of_service_user_acceptance; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.terms_of_service_user_acceptance IS 'This records the acceptance of terms of service entries by users.';


--
-- Name: COLUMN terms_of_service_user_acceptance.acceptance_time; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.terms_of_service_user_acceptance.acceptance_time IS 'The date and time the user accepted the specific agreement.';


--
-- Name: terms_of_service_user_acceptance_history; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.terms_of_service_user_acceptance_history (
    terms_of_service_id uuid NOT NULL,
    user_id text NOT NULL,
    acceptance_time tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
);


ALTER TABLE _a3s.terms_of_service_user_acceptance_history OWNER TO postgres;

--
-- Name: TABLE terms_of_service_user_acceptance_history; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.terms_of_service_user_acceptance_history IS 'This stores the history of the acceptance of terms of service entries by users.
On every update of a terms of service agreement version for a team, all user acceptance records get copied from ''terms_of_service_user_acceptance'' to ''terms_of_service_user_acceptance_history''.';


--
-- Name: COLUMN terms_of_service_user_acceptance_history.acceptance_time; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.terms_of_service_user_acceptance_history.acceptance_time IS 'The date and time the user accepted the specific agreement.';


--
-- Name: user_role; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.user_role (
    user_id text NOT NULL,
    role_id uuid NOT NULL,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
);


ALTER TABLE _a3s.user_role OWNER TO postgres;

--
-- Name: TABLE user_role; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.user_role IS 'User and Role link';


--
-- Name: user_team; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.user_team (
    user_id text NOT NULL,
    team_id uuid NOT NULL,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
);


ALTER TABLE _a3s.user_team OWNER TO postgres;

--
-- Name: TABLE user_team; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.user_team IS 'Users and Teams link';


--
-- Name: ApiClaims; Type: TABLE; Schema: _ids4; Owner: postgres
--

CREATE TABLE _ids4."ApiClaims" (
    "Id" integer NOT NULL,
    "Type" character varying(200) NOT NULL,
    "ApiResourceId" integer NOT NULL
);


ALTER TABLE _ids4."ApiClaims" OWNER TO postgres;

--
-- Name: ApiClaims_Id_seq; Type: SEQUENCE; Schema: _ids4; Owner: postgres
--

CREATE SEQUENCE _ids4."ApiClaims_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE _ids4."ApiClaims_Id_seq" OWNER TO postgres;

--
-- Name: ApiClaims_Id_seq; Type: SEQUENCE OWNED BY; Schema: _ids4; Owner: postgres
--

ALTER SEQUENCE _ids4."ApiClaims_Id_seq" OWNED BY _ids4."ApiClaims"."Id";


--
-- Name: ApiProperties; Type: TABLE; Schema: _ids4; Owner: postgres
--

CREATE TABLE _ids4."ApiProperties" (
    "Id" integer NOT NULL,
    "Key" character varying(250) NOT NULL,
    "Value" character varying(2000) NOT NULL,
    "ApiResourceId" integer NOT NULL
);


ALTER TABLE _ids4."ApiProperties" OWNER TO postgres;

--
-- Name: ApiProperties_Id_seq; Type: SEQUENCE; Schema: _ids4; Owner: postgres
--

CREATE SEQUENCE _ids4."ApiProperties_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE _ids4."ApiProperties_Id_seq" OWNER TO postgres;

--
-- Name: ApiProperties_Id_seq; Type: SEQUENCE OWNED BY; Schema: _ids4; Owner: postgres
--

ALTER SEQUENCE _ids4."ApiProperties_Id_seq" OWNED BY _ids4."ApiProperties"."Id";


--
-- Name: ApiResources; Type: TABLE; Schema: _ids4; Owner: postgres
--

CREATE TABLE _ids4."ApiResources" (
    "Id" integer NOT NULL,
    "Enabled" boolean NOT NULL,
    "Name" character varying(200) NOT NULL,
    "DisplayName" character varying(200),
    "Description" character varying(1000),
    "Created" timestamp without time zone NOT NULL,
    "Updated" timestamp without time zone,
    "LastAccessed" timestamp without time zone,
    "NonEditable" boolean NOT NULL
);


ALTER TABLE _ids4."ApiResources" OWNER TO postgres;

--
-- Name: ApiResources_Id_seq; Type: SEQUENCE; Schema: _ids4; Owner: postgres
--

CREATE SEQUENCE _ids4."ApiResources_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE _ids4."ApiResources_Id_seq" OWNER TO postgres;

--
-- Name: ApiResources_Id_seq; Type: SEQUENCE OWNED BY; Schema: _ids4; Owner: postgres
--

ALTER SEQUENCE _ids4."ApiResources_Id_seq" OWNED BY _ids4."ApiResources"."Id";


--
-- Name: ApiScopeClaims; Type: TABLE; Schema: _ids4; Owner: postgres
--

CREATE TABLE _ids4."ApiScopeClaims" (
    "Id" integer NOT NULL,
    "Type" character varying(200) NOT NULL,
    "ApiScopeId" integer NOT NULL
);


ALTER TABLE _ids4."ApiScopeClaims" OWNER TO postgres;

--
-- Name: ApiScopeClaims_Id_seq; Type: SEQUENCE; Schema: _ids4; Owner: postgres
--

CREATE SEQUENCE _ids4."ApiScopeClaims_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE _ids4."ApiScopeClaims_Id_seq" OWNER TO postgres;

--
-- Name: ApiScopeClaims_Id_seq; Type: SEQUENCE OWNED BY; Schema: _ids4; Owner: postgres
--

ALTER SEQUENCE _ids4."ApiScopeClaims_Id_seq" OWNED BY _ids4."ApiScopeClaims"."Id";


--
-- Name: ApiScopes; Type: TABLE; Schema: _ids4; Owner: postgres
--

CREATE TABLE _ids4."ApiScopes" (
    "Id" integer NOT NULL,
    "Name" character varying(200) NOT NULL,
    "DisplayName" character varying(200),
    "Description" character varying(1000),
    "Required" boolean NOT NULL,
    "Emphasize" boolean NOT NULL,
    "ShowInDiscoveryDocument" boolean NOT NULL,
    "ApiResourceId" integer NOT NULL
);


ALTER TABLE _ids4."ApiScopes" OWNER TO postgres;

--
-- Name: ApiScopes_Id_seq; Type: SEQUENCE; Schema: _ids4; Owner: postgres
--

CREATE SEQUENCE _ids4."ApiScopes_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE _ids4."ApiScopes_Id_seq" OWNER TO postgres;

--
-- Name: ApiScopes_Id_seq; Type: SEQUENCE OWNED BY; Schema: _ids4; Owner: postgres
--

ALTER SEQUENCE _ids4."ApiScopes_Id_seq" OWNED BY _ids4."ApiScopes"."Id";


--
-- Name: ApiSecrets; Type: TABLE; Schema: _ids4; Owner: postgres
--

CREATE TABLE _ids4."ApiSecrets" (
    "Id" integer NOT NULL,
    "Description" character varying(1000),
    "Value" character varying(4000) NOT NULL,
    "Expiration" timestamp without time zone,
    "Type" character varying(250) NOT NULL,
    "Created" timestamp without time zone NOT NULL,
    "ApiResourceId" integer NOT NULL
);


ALTER TABLE _ids4."ApiSecrets" OWNER TO postgres;

--
-- Name: ApiSecrets_Id_seq; Type: SEQUENCE; Schema: _ids4; Owner: postgres
--

CREATE SEQUENCE _ids4."ApiSecrets_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE _ids4."ApiSecrets_Id_seq" OWNER TO postgres;

--
-- Name: ApiSecrets_Id_seq; Type: SEQUENCE OWNED BY; Schema: _ids4; Owner: postgres
--

ALTER SEQUENCE _ids4."ApiSecrets_Id_seq" OWNED BY _ids4."ApiSecrets"."Id";


--
-- Name: ClientClaims; Type: TABLE; Schema: _ids4; Owner: postgres
--

CREATE TABLE _ids4."ClientClaims" (
    "Id" integer NOT NULL,
    "Type" character varying(250) NOT NULL,
    "Value" character varying(250) NOT NULL,
    "ClientId" integer NOT NULL
);


ALTER TABLE _ids4."ClientClaims" OWNER TO postgres;

--
-- Name: ClientClaims_Id_seq; Type: SEQUENCE; Schema: _ids4; Owner: postgres
--

CREATE SEQUENCE _ids4."ClientClaims_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE _ids4."ClientClaims_Id_seq" OWNER TO postgres;

--
-- Name: ClientClaims_Id_seq; Type: SEQUENCE OWNED BY; Schema: _ids4; Owner: postgres
--

ALTER SEQUENCE _ids4."ClientClaims_Id_seq" OWNED BY _ids4."ClientClaims"."Id";


--
-- Name: ClientCorsOrigins; Type: TABLE; Schema: _ids4; Owner: postgres
--

CREATE TABLE _ids4."ClientCorsOrigins" (
    "Id" integer NOT NULL,
    "Origin" character varying(150) NOT NULL,
    "ClientId" integer NOT NULL
);


ALTER TABLE _ids4."ClientCorsOrigins" OWNER TO postgres;

--
-- Name: ClientCorsOrigins_Id_seq; Type: SEQUENCE; Schema: _ids4; Owner: postgres
--

CREATE SEQUENCE _ids4."ClientCorsOrigins_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE _ids4."ClientCorsOrigins_Id_seq" OWNER TO postgres;

--
-- Name: ClientCorsOrigins_Id_seq; Type: SEQUENCE OWNED BY; Schema: _ids4; Owner: postgres
--

ALTER SEQUENCE _ids4."ClientCorsOrigins_Id_seq" OWNED BY _ids4."ClientCorsOrigins"."Id";


--
-- Name: ClientGrantTypes; Type: TABLE; Schema: _ids4; Owner: postgres
--

CREATE TABLE _ids4."ClientGrantTypes" (
    "Id" integer NOT NULL,
    "GrantType" character varying(250) NOT NULL,
    "ClientId" integer NOT NULL
);


ALTER TABLE _ids4."ClientGrantTypes" OWNER TO postgres;

--
-- Name: ClientGrantTypes_Id_seq; Type: SEQUENCE; Schema: _ids4; Owner: postgres
--

CREATE SEQUENCE _ids4."ClientGrantTypes_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE _ids4."ClientGrantTypes_Id_seq" OWNER TO postgres;

--
-- Name: ClientGrantTypes_Id_seq; Type: SEQUENCE OWNED BY; Schema: _ids4; Owner: postgres
--

ALTER SEQUENCE _ids4."ClientGrantTypes_Id_seq" OWNED BY _ids4."ClientGrantTypes"."Id";


--
-- Name: ClientIdPRestrictions; Type: TABLE; Schema: _ids4; Owner: postgres
--

CREATE TABLE _ids4."ClientIdPRestrictions" (
    "Id" integer NOT NULL,
    "Provider" character varying(200) NOT NULL,
    "ClientId" integer NOT NULL
);


ALTER TABLE _ids4."ClientIdPRestrictions" OWNER TO postgres;

--
-- Name: ClientIdPRestrictions_Id_seq; Type: SEQUENCE; Schema: _ids4; Owner: postgres
--

CREATE SEQUENCE _ids4."ClientIdPRestrictions_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE _ids4."ClientIdPRestrictions_Id_seq" OWNER TO postgres;

--
-- Name: ClientIdPRestrictions_Id_seq; Type: SEQUENCE OWNED BY; Schema: _ids4; Owner: postgres
--

ALTER SEQUENCE _ids4."ClientIdPRestrictions_Id_seq" OWNED BY _ids4."ClientIdPRestrictions"."Id";


--
-- Name: ClientPostLogoutRedirectUris; Type: TABLE; Schema: _ids4; Owner: postgres
--

CREATE TABLE _ids4."ClientPostLogoutRedirectUris" (
    "Id" integer NOT NULL,
    "PostLogoutRedirectUri" character varying(2000) NOT NULL,
    "ClientId" integer NOT NULL
);


ALTER TABLE _ids4."ClientPostLogoutRedirectUris" OWNER TO postgres;

--
-- Name: ClientPostLogoutRedirectUris_Id_seq; Type: SEQUENCE; Schema: _ids4; Owner: postgres
--

CREATE SEQUENCE _ids4."ClientPostLogoutRedirectUris_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE _ids4."ClientPostLogoutRedirectUris_Id_seq" OWNER TO postgres;

--
-- Name: ClientPostLogoutRedirectUris_Id_seq; Type: SEQUENCE OWNED BY; Schema: _ids4; Owner: postgres
--

ALTER SEQUENCE _ids4."ClientPostLogoutRedirectUris_Id_seq" OWNED BY _ids4."ClientPostLogoutRedirectUris"."Id";


--
-- Name: ClientProperties; Type: TABLE; Schema: _ids4; Owner: postgres
--

CREATE TABLE _ids4."ClientProperties" (
    "Id" integer NOT NULL,
    "Key" character varying(250) NOT NULL,
    "Value" character varying(2000) NOT NULL,
    "ClientId" integer NOT NULL
);


ALTER TABLE _ids4."ClientProperties" OWNER TO postgres;

--
-- Name: ClientProperties_Id_seq; Type: SEQUENCE; Schema: _ids4; Owner: postgres
--

CREATE SEQUENCE _ids4."ClientProperties_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE _ids4."ClientProperties_Id_seq" OWNER TO postgres;

--
-- Name: ClientProperties_Id_seq; Type: SEQUENCE OWNED BY; Schema: _ids4; Owner: postgres
--

ALTER SEQUENCE _ids4."ClientProperties_Id_seq" OWNED BY _ids4."ClientProperties"."Id";


--
-- Name: ClientRedirectUris; Type: TABLE; Schema: _ids4; Owner: postgres
--

CREATE TABLE _ids4."ClientRedirectUris" (
    "Id" integer NOT NULL,
    "RedirectUri" character varying(2000) NOT NULL,
    "ClientId" integer NOT NULL
);


ALTER TABLE _ids4."ClientRedirectUris" OWNER TO postgres;

--
-- Name: ClientRedirectUris_Id_seq; Type: SEQUENCE; Schema: _ids4; Owner: postgres
--

CREATE SEQUENCE _ids4."ClientRedirectUris_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE _ids4."ClientRedirectUris_Id_seq" OWNER TO postgres;

--
-- Name: ClientRedirectUris_Id_seq; Type: SEQUENCE OWNED BY; Schema: _ids4; Owner: postgres
--

ALTER SEQUENCE _ids4."ClientRedirectUris_Id_seq" OWNED BY _ids4."ClientRedirectUris"."Id";


--
-- Name: ClientScopes; Type: TABLE; Schema: _ids4; Owner: postgres
--

CREATE TABLE _ids4."ClientScopes" (
    "Id" integer NOT NULL,
    "Scope" character varying(200) NOT NULL,
    "ClientId" integer NOT NULL
);


ALTER TABLE _ids4."ClientScopes" OWNER TO postgres;

--
-- Name: ClientScopes_Id_seq; Type: SEQUENCE; Schema: _ids4; Owner: postgres
--

CREATE SEQUENCE _ids4."ClientScopes_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE _ids4."ClientScopes_Id_seq" OWNER TO postgres;

--
-- Name: ClientScopes_Id_seq; Type: SEQUENCE OWNED BY; Schema: _ids4; Owner: postgres
--

ALTER SEQUENCE _ids4."ClientScopes_Id_seq" OWNED BY _ids4."ClientScopes"."Id";


--
-- Name: ClientSecrets; Type: TABLE; Schema: _ids4; Owner: postgres
--

CREATE TABLE _ids4."ClientSecrets" (
    "Id" integer NOT NULL,
    "Description" character varying(2000),
    "Value" character varying(4000) NOT NULL,
    "Expiration" timestamp without time zone,
    "Type" character varying(250) NOT NULL,
    "Created" timestamp without time zone NOT NULL,
    "ClientId" integer NOT NULL
);


ALTER TABLE _ids4."ClientSecrets" OWNER TO postgres;

--
-- Name: ClientSecrets_Id_seq; Type: SEQUENCE; Schema: _ids4; Owner: postgres
--

CREATE SEQUENCE _ids4."ClientSecrets_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE _ids4."ClientSecrets_Id_seq" OWNER TO postgres;

--
-- Name: ClientSecrets_Id_seq; Type: SEQUENCE OWNED BY; Schema: _ids4; Owner: postgres
--

ALTER SEQUENCE _ids4."ClientSecrets_Id_seq" OWNED BY _ids4."ClientSecrets"."Id";


--
-- Name: Clients; Type: TABLE; Schema: _ids4; Owner: postgres
--

CREATE TABLE _ids4."Clients" (
    "Id" integer NOT NULL,
    "Enabled" boolean NOT NULL,
    "ClientId" character varying(200) NOT NULL,
    "ProtocolType" character varying(200) NOT NULL,
    "RequireClientSecret" boolean NOT NULL,
    "ClientName" character varying(200),
    "Description" character varying(1000),
    "ClientUri" character varying(2000),
    "LogoUri" character varying(2000),
    "RequireConsent" boolean NOT NULL,
    "AllowRememberConsent" boolean NOT NULL,
    "AlwaysIncludeUserClaimsInIdToken" boolean NOT NULL,
    "RequirePkce" boolean NOT NULL,
    "AllowPlainTextPkce" boolean NOT NULL,
    "AllowAccessTokensViaBrowser" boolean NOT NULL,
    "FrontChannelLogoutUri" character varying(2000),
    "FrontChannelLogoutSessionRequired" boolean NOT NULL,
    "BackChannelLogoutUri" character varying(2000),
    "BackChannelLogoutSessionRequired" boolean NOT NULL,
    "AllowOfflineAccess" boolean NOT NULL,
    "IdentityTokenLifetime" integer NOT NULL,
    "AccessTokenLifetime" integer NOT NULL,
    "AuthorizationCodeLifetime" integer NOT NULL,
    "ConsentLifetime" integer,
    "AbsoluteRefreshTokenLifetime" integer NOT NULL,
    "SlidingRefreshTokenLifetime" integer NOT NULL,
    "RefreshTokenUsage" integer NOT NULL,
    "UpdateAccessTokenClaimsOnRefresh" boolean NOT NULL,
    "RefreshTokenExpiration" integer NOT NULL,
    "AccessTokenType" integer NOT NULL,
    "EnableLocalLogin" boolean NOT NULL,
    "IncludeJwtId" boolean NOT NULL,
    "AlwaysSendClientClaims" boolean NOT NULL,
    "ClientClaimsPrefix" character varying(200),
    "PairWiseSubjectSalt" character varying(200),
    "Created" timestamp without time zone NOT NULL,
    "Updated" timestamp without time zone,
    "LastAccessed" timestamp without time zone,
    "UserSsoLifetime" integer,
    "UserCodeType" character varying(100),
    "DeviceCodeLifetime" integer NOT NULL,
    "NonEditable" boolean NOT NULL
);


ALTER TABLE _ids4."Clients" OWNER TO postgres;

--
-- Name: Clients_Id_seq; Type: SEQUENCE; Schema: _ids4; Owner: postgres
--

CREATE SEQUENCE _ids4."Clients_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE _ids4."Clients_Id_seq" OWNER TO postgres;

--
-- Name: Clients_Id_seq; Type: SEQUENCE OWNED BY; Schema: _ids4; Owner: postgres
--

ALTER SEQUENCE _ids4."Clients_Id_seq" OWNED BY _ids4."Clients"."Id";


--
-- Name: IdentityClaims; Type: TABLE; Schema: _ids4; Owner: postgres
--

CREATE TABLE _ids4."IdentityClaims" (
    "Id" integer NOT NULL,
    "Type" character varying(200) NOT NULL,
    "IdentityResourceId" integer NOT NULL
);


ALTER TABLE _ids4."IdentityClaims" OWNER TO postgres;

--
-- Name: IdentityClaims_Id_seq; Type: SEQUENCE; Schema: _ids4; Owner: postgres
--

CREATE SEQUENCE _ids4."IdentityClaims_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE _ids4."IdentityClaims_Id_seq" OWNER TO postgres;

--
-- Name: IdentityClaims_Id_seq; Type: SEQUENCE OWNED BY; Schema: _ids4; Owner: postgres
--

ALTER SEQUENCE _ids4."IdentityClaims_Id_seq" OWNED BY _ids4."IdentityClaims"."Id";


--
-- Name: IdentityProperties; Type: TABLE; Schema: _ids4; Owner: postgres
--

CREATE TABLE _ids4."IdentityProperties" (
    "Id" integer NOT NULL,
    "Key" character varying(250) NOT NULL,
    "Value" character varying(2000) NOT NULL,
    "IdentityResourceId" integer NOT NULL
);


ALTER TABLE _ids4."IdentityProperties" OWNER TO postgres;

--
-- Name: IdentityProperties_Id_seq; Type: SEQUENCE; Schema: _ids4; Owner: postgres
--

CREATE SEQUENCE _ids4."IdentityProperties_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE _ids4."IdentityProperties_Id_seq" OWNER TO postgres;

--
-- Name: IdentityProperties_Id_seq; Type: SEQUENCE OWNED BY; Schema: _ids4; Owner: postgres
--

ALTER SEQUENCE _ids4."IdentityProperties_Id_seq" OWNED BY _ids4."IdentityProperties"."Id";


--
-- Name: IdentityResources; Type: TABLE; Schema: _ids4; Owner: postgres
--

CREATE TABLE _ids4."IdentityResources" (
    "Id" integer NOT NULL,
    "Enabled" boolean NOT NULL,
    "Name" character varying(200) NOT NULL,
    "DisplayName" character varying(200),
    "Description" character varying(1000),
    "Required" boolean NOT NULL,
    "Emphasize" boolean NOT NULL,
    "ShowInDiscoveryDocument" boolean NOT NULL,
    "Created" timestamp without time zone NOT NULL,
    "Updated" timestamp without time zone,
    "NonEditable" boolean NOT NULL
);


ALTER TABLE _ids4."IdentityResources" OWNER TO postgres;

--
-- Name: IdentityResources_Id_seq; Type: SEQUENCE; Schema: _ids4; Owner: postgres
--

CREATE SEQUENCE _ids4."IdentityResources_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE _ids4."IdentityResources_Id_seq" OWNER TO postgres;

--
-- Name: IdentityResources_Id_seq; Type: SEQUENCE OWNED BY; Schema: _ids4; Owner: postgres
--

ALTER SEQUENCE _ids4."IdentityResources_Id_seq" OWNED BY _ids4."IdentityResources"."Id";


--
-- Name: flyway_schema_history; Type: TABLE; Schema: _ids4; Owner: postgres
--

CREATE TABLE _ids4.flyway_schema_history (
    installed_rank integer NOT NULL,
    version character varying(50),
    description character varying(200) NOT NULL,
    type character varying(20) NOT NULL,
    script character varying(1000) NOT NULL,
    checksum integer,
    installed_by character varying(100) NOT NULL,
    installed_on timestamp without time zone DEFAULT now() NOT NULL,
    execution_time integer NOT NULL,
    success boolean NOT NULL
);


ALTER TABLE _ids4.flyway_schema_history OWNER TO postgres;

--
-- Name: ApiClaims Id; Type: DEFAULT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ApiClaims" ALTER COLUMN "Id" SET DEFAULT nextval('_ids4."ApiClaims_Id_seq"'::regclass);


--
-- Name: ApiProperties Id; Type: DEFAULT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ApiProperties" ALTER COLUMN "Id" SET DEFAULT nextval('_ids4."ApiProperties_Id_seq"'::regclass);


--
-- Name: ApiResources Id; Type: DEFAULT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ApiResources" ALTER COLUMN "Id" SET DEFAULT nextval('_ids4."ApiResources_Id_seq"'::regclass);


--
-- Name: ApiScopeClaims Id; Type: DEFAULT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ApiScopeClaims" ALTER COLUMN "Id" SET DEFAULT nextval('_ids4."ApiScopeClaims_Id_seq"'::regclass);


--
-- Name: ApiScopes Id; Type: DEFAULT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ApiScopes" ALTER COLUMN "Id" SET DEFAULT nextval('_ids4."ApiScopes_Id_seq"'::regclass);


--
-- Name: ApiSecrets Id; Type: DEFAULT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ApiSecrets" ALTER COLUMN "Id" SET DEFAULT nextval('_ids4."ApiSecrets_Id_seq"'::regclass);


--
-- Name: ClientClaims Id; Type: DEFAULT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientClaims" ALTER COLUMN "Id" SET DEFAULT nextval('_ids4."ClientClaims_Id_seq"'::regclass);


--
-- Name: ClientCorsOrigins Id; Type: DEFAULT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientCorsOrigins" ALTER COLUMN "Id" SET DEFAULT nextval('_ids4."ClientCorsOrigins_Id_seq"'::regclass);


--
-- Name: ClientGrantTypes Id; Type: DEFAULT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientGrantTypes" ALTER COLUMN "Id" SET DEFAULT nextval('_ids4."ClientGrantTypes_Id_seq"'::regclass);


--
-- Name: ClientIdPRestrictions Id; Type: DEFAULT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientIdPRestrictions" ALTER COLUMN "Id" SET DEFAULT nextval('_ids4."ClientIdPRestrictions_Id_seq"'::regclass);


--
-- Name: ClientPostLogoutRedirectUris Id; Type: DEFAULT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientPostLogoutRedirectUris" ALTER COLUMN "Id" SET DEFAULT nextval('_ids4."ClientPostLogoutRedirectUris_Id_seq"'::regclass);


--
-- Name: ClientProperties Id; Type: DEFAULT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientProperties" ALTER COLUMN "Id" SET DEFAULT nextval('_ids4."ClientProperties_Id_seq"'::regclass);


--
-- Name: ClientRedirectUris Id; Type: DEFAULT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientRedirectUris" ALTER COLUMN "Id" SET DEFAULT nextval('_ids4."ClientRedirectUris_Id_seq"'::regclass);


--
-- Name: ClientScopes Id; Type: DEFAULT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientScopes" ALTER COLUMN "Id" SET DEFAULT nextval('_ids4."ClientScopes_Id_seq"'::regclass);


--
-- Name: ClientSecrets Id; Type: DEFAULT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientSecrets" ALTER COLUMN "Id" SET DEFAULT nextval('_ids4."ClientSecrets_Id_seq"'::regclass);


--
-- Name: Clients Id; Type: DEFAULT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."Clients" ALTER COLUMN "Id" SET DEFAULT nextval('_ids4."Clients_Id_seq"'::regclass);


--
-- Name: IdentityClaims Id; Type: DEFAULT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."IdentityClaims" ALTER COLUMN "Id" SET DEFAULT nextval('_ids4."IdentityClaims_Id_seq"'::regclass);


--
-- Name: IdentityProperties Id; Type: DEFAULT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."IdentityProperties" ALTER COLUMN "Id" SET DEFAULT nextval('_ids4."IdentityProperties_Id_seq"'::regclass);


--
-- Name: IdentityResources Id; Type: DEFAULT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."IdentityResources" ALTER COLUMN "Id" SET DEFAULT nextval('_ids4."IdentityResources_Id_seq"'::regclass);


--
-- Name: flyway_schema_history flyway_schema_history_pk; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.flyway_schema_history
    ADD CONSTRAINT flyway_schema_history_pk PRIMARY KEY (installed_rank);


--
-- Name: ldap_authentication_mode_ldap_attribute ldap_authentication_mode_ldap_attribute_pkey; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.ldap_authentication_mode_ldap_attribute
    ADD CONSTRAINT ldap_authentication_mode_ldap_attribute_pkey PRIMARY KEY (id);


--
-- Name: ldap_authentication_mode ldap_authentication_mode_pkey; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.ldap_authentication_mode
    ADD CONSTRAINT ldap_authentication_mode_pkey PRIMARY KEY (id);


--
-- Name: application pk_application; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.application
    ADD CONSTRAINT pk_application PRIMARY KEY (id);


--
-- Name: application_data_policy pk_application_data_policy; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.application_data_policy
    ADD CONSTRAINT pk_application_data_policy PRIMARY KEY (id);


--
-- Name: application_function pk_application_function; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.application_function
    ADD CONSTRAINT pk_application_function PRIMARY KEY (id);


--
-- Name: application_function_permission pk_application_function_permission; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.application_function_permission
    ADD CONSTRAINT pk_application_function_permission PRIMARY KEY (permission_id, application_function_id);


--
-- Name: application_user pk_application_user; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.application_user
    ADD CONSTRAINT pk_application_user PRIMARY KEY (id);


--
-- Name: application_user_claim pk_application_user_claim; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.application_user_claim
    ADD CONSTRAINT pk_application_user_claim PRIMARY KEY (id);


--
-- Name: aspnet_role pk_aspnet_role; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.aspnet_role
    ADD CONSTRAINT pk_aspnet_role PRIMARY KEY (id);


--
-- Name: aspnet_role_claim pk_aspnet_role_claim; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.aspnet_role_claim
    ADD CONSTRAINT pk_aspnet_role_claim PRIMARY KEY (id);


--
-- Name: aspnet_user_login pk_aspnet_user_login; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.aspnet_user_login
    ADD CONSTRAINT pk_aspnet_user_login PRIMARY KEY (login_provider, provider_key);


--
-- Name: aspnet_user_role pk_aspnet_user_role; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.aspnet_user_role
    ADD CONSTRAINT pk_aspnet_user_role PRIMARY KEY (user_id, role_id);


--
-- Name: application_user_token pk_aspnet_user_token; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.application_user_token
    ADD CONSTRAINT pk_aspnet_user_token PRIMARY KEY (user_id, login_provider, name);


--
-- Name: function pk_function; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.function
    ADD CONSTRAINT pk_function PRIMARY KEY (id);


--
-- Name: function_permission pk_function_permission; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.function_permission
    ADD CONSTRAINT pk_function_permission PRIMARY KEY (permission_id, function_id);


--
-- Name: permission pk_permission; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.permission
    ADD CONSTRAINT pk_permission PRIMARY KEY (id);


--
-- Name: profile pk_profile; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.profile
    ADD CONSTRAINT pk_profile PRIMARY KEY (id);


--
-- Name: role pk_role; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.role
    ADD CONSTRAINT pk_role PRIMARY KEY (id);


--
-- Name: role_function pk_role_function; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.role_function
    ADD CONSTRAINT pk_role_function PRIMARY KEY (role_id, function_id);


--
-- Name: role_role pk_role_role; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.role_role
    ADD CONSTRAINT pk_role_role PRIMARY KEY (parent_role_id, child_role_id);


--
-- Name: sub_realm pk_sub_realm; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.sub_realm
    ADD CONSTRAINT pk_sub_realm PRIMARY KEY (id);


--
-- Name: sub_realm_permission pk_sub_realm_permission; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.sub_realm_permission
    ADD CONSTRAINT pk_sub_realm_permission PRIMARY KEY (sub_realm_id, permission_id);


--
-- Name: team pk_team; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.team
    ADD CONSTRAINT pk_team PRIMARY KEY (id);


--
-- Name: team_application_data_policy pk_team_application_data_policy; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.team_application_data_policy
    ADD CONSTRAINT pk_team_application_data_policy PRIMARY KEY (team_id, application_data_policy_id);


--
-- Name: team_team pk_team_team; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.team_team
    ADD CONSTRAINT pk_team_team PRIMARY KEY (parent_team_id, child_team_id);


--
-- Name: user_role pk_user_role; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.user_role
    ADD CONSTRAINT pk_user_role PRIMARY KEY (role_id, user_id);


--
-- Name: user_team pk_user_team; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.user_team
    ADD CONSTRAINT pk_user_team PRIMARY KEY (team_id, user_id);


--
-- Name: profile_role profile_role_pk; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.profile_role
    ADD CONSTRAINT profile_role_pk PRIMARY KEY (profile_id, role_id);


--
-- Name: profile_team profile_team_pk; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.profile_team
    ADD CONSTRAINT profile_team_pk PRIMARY KEY (profile_id, team_id);


--
-- Name: terms_of_service terms_of_service_pk; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.terms_of_service
    ADD CONSTRAINT terms_of_service_pk PRIMARY KEY (id);


--
-- Name: terms_of_service_user_acceptance_history terms_of_service_user_acceptance_history_pk; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.terms_of_service_user_acceptance_history
    ADD CONSTRAINT terms_of_service_user_acceptance_history_pk PRIMARY KEY (terms_of_service_id, user_id);


--
-- Name: terms_of_service_user_acceptance terms_of_service_user_acceptance_pk; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.terms_of_service_user_acceptance
    ADD CONSTRAINT terms_of_service_user_acceptance_pk PRIMARY KEY (terms_of_service_id, user_id);


--
-- Name: application_data_policy uk_application_data_policy_name; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.application_data_policy
    ADD CONSTRAINT uk_application_data_policy_name UNIQUE (name);


--
-- Name: application_function uk_application_function_name; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.application_function
    ADD CONSTRAINT uk_application_function_name UNIQUE (name);


--
-- Name: application uk_application_name; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.application
    ADD CONSTRAINT uk_application_name UNIQUE (name);


--
-- Name: application_user uk_application_user_normalized_user_name; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.application_user
    ADD CONSTRAINT uk_application_user_normalized_user_name UNIQUE (normalized_user_name);


--
-- Name: application_user uk_application_user_username; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.application_user
    ADD CONSTRAINT uk_application_user_username UNIQUE (user_name);


--
-- Name: function uk_function_name; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.function
    ADD CONSTRAINT uk_function_name UNIQUE (name);


--
-- Name: ldap_authentication_mode_ldap_attribute uk_ldap_authentication_mode_ldap_attribute_1; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.ldap_authentication_mode_ldap_attribute
    ADD CONSTRAINT uk_ldap_authentication_mode_ldap_attribute_1 UNIQUE (ldap_authentication_mode_id, user_field, ldap_field);


--
-- Name: ldap_authentication_mode uk_ldap_authentication_mode_name; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.ldap_authentication_mode
    ADD CONSTRAINT uk_ldap_authentication_mode_name UNIQUE (name);


--
-- Name: permission uk_permission_name; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.permission
    ADD CONSTRAINT uk_permission_name UNIQUE (name);


--
-- Name: role uk_role_name; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.role
    ADD CONSTRAINT uk_role_name UNIQUE (name);


--
-- Name: sub_realm uk_sub_realm_name; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.sub_realm
    ADD CONSTRAINT uk_sub_realm_name UNIQUE (name);


--
-- Name: CONSTRAINT uk_sub_realm_name ON sub_realm; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON CONSTRAINT uk_sub_realm_name ON _a3s.sub_realm IS 'A uniqueness contraint ensuring that a sub realm''s name is always unique.';


--
-- Name: team uk_team_name; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.team
    ADD CONSTRAINT uk_team_name UNIQUE (name);


--
-- Name: terms_of_service uk_terms_of_service_agreement_name_version; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.terms_of_service
    ADD CONSTRAINT uk_terms_of_service_agreement_name_version UNIQUE (agreement_name, version);


--
-- Name: profile uq_profile; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.profile
    ADD CONSTRAINT uq_profile UNIQUE (sub_realm_id);


--
-- Name: ApiClaims PK_ApiClaims; Type: CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ApiClaims"
    ADD CONSTRAINT "PK_ApiClaims" PRIMARY KEY ("Id");


--
-- Name: ApiProperties PK_ApiProperties; Type: CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ApiProperties"
    ADD CONSTRAINT "PK_ApiProperties" PRIMARY KEY ("Id");


--
-- Name: ApiResources PK_ApiResources; Type: CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ApiResources"
    ADD CONSTRAINT "PK_ApiResources" PRIMARY KEY ("Id");


--
-- Name: ApiScopeClaims PK_ApiScopeClaims; Type: CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ApiScopeClaims"
    ADD CONSTRAINT "PK_ApiScopeClaims" PRIMARY KEY ("Id");


--
-- Name: ApiScopes PK_ApiScopes; Type: CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ApiScopes"
    ADD CONSTRAINT "PK_ApiScopes" PRIMARY KEY ("Id");


--
-- Name: ApiSecrets PK_ApiSecrets; Type: CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ApiSecrets"
    ADD CONSTRAINT "PK_ApiSecrets" PRIMARY KEY ("Id");


--
-- Name: ClientClaims PK_ClientClaims; Type: CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientClaims"
    ADD CONSTRAINT "PK_ClientClaims" PRIMARY KEY ("Id");


--
-- Name: ClientCorsOrigins PK_ClientCorsOrigins; Type: CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientCorsOrigins"
    ADD CONSTRAINT "PK_ClientCorsOrigins" PRIMARY KEY ("Id");


--
-- Name: ClientGrantTypes PK_ClientGrantTypes; Type: CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientGrantTypes"
    ADD CONSTRAINT "PK_ClientGrantTypes" PRIMARY KEY ("Id");


--
-- Name: ClientIdPRestrictions PK_ClientIdPRestrictions; Type: CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientIdPRestrictions"
    ADD CONSTRAINT "PK_ClientIdPRestrictions" PRIMARY KEY ("Id");


--
-- Name: ClientPostLogoutRedirectUris PK_ClientPostLogoutRedirectUris; Type: CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientPostLogoutRedirectUris"
    ADD CONSTRAINT "PK_ClientPostLogoutRedirectUris" PRIMARY KEY ("Id");


--
-- Name: ClientProperties PK_ClientProperties; Type: CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientProperties"
    ADD CONSTRAINT "PK_ClientProperties" PRIMARY KEY ("Id");


--
-- Name: ClientRedirectUris PK_ClientRedirectUris; Type: CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientRedirectUris"
    ADD CONSTRAINT "PK_ClientRedirectUris" PRIMARY KEY ("Id");


--
-- Name: ClientScopes PK_ClientScopes; Type: CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientScopes"
    ADD CONSTRAINT "PK_ClientScopes" PRIMARY KEY ("Id");


--
-- Name: ClientSecrets PK_ClientSecrets; Type: CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientSecrets"
    ADD CONSTRAINT "PK_ClientSecrets" PRIMARY KEY ("Id");


--
-- Name: Clients PK_Clients; Type: CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."Clients"
    ADD CONSTRAINT "PK_Clients" PRIMARY KEY ("Id");


--
-- Name: IdentityClaims PK_IdentityClaims; Type: CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."IdentityClaims"
    ADD CONSTRAINT "PK_IdentityClaims" PRIMARY KEY ("Id");


--
-- Name: IdentityProperties PK_IdentityProperties; Type: CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."IdentityProperties"
    ADD CONSTRAINT "PK_IdentityProperties" PRIMARY KEY ("Id");


--
-- Name: IdentityResources PK_IdentityResources; Type: CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."IdentityResources"
    ADD CONSTRAINT "PK_IdentityResources" PRIMARY KEY ("Id");


--
-- Name: flyway_schema_history flyway_schema_history_pk; Type: CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4.flyway_schema_history
    ADD CONSTRAINT flyway_schema_history_pk PRIMARY KEY (installed_rank);


--
-- Name: flyway_schema_history_s_idx; Type: INDEX; Schema: _a3s; Owner: postgres
--

CREATE INDEX flyway_schema_history_s_idx ON _a3s.flyway_schema_history USING btree (success);


--
-- Name: ix_application_data_policy_name; Type: INDEX; Schema: _a3s; Owner: postgres
--

CREATE INDEX ix_application_data_policy_name ON _a3s.application_data_policy USING btree (name) WITH (fillfactor='90');


--
-- Name: ix_application_function_application_id; Type: INDEX; Schema: _a3s; Owner: postgres
--

CREATE INDEX ix_application_function_application_id ON _a3s.application_function USING btree (application_id) WITH (fillfactor='90');


--
-- Name: ix_application_function_permission_application_function_id; Type: INDEX; Schema: _a3s; Owner: postgres
--

CREATE INDEX ix_application_function_permission_application_function_id ON _a3s.application_function_permission USING btree (application_function_id) WITH (fillfactor='90');


--
-- Name: ix_application_user_claim_user_id; Type: INDEX; Schema: _a3s; Owner: postgres
--

CREATE INDEX ix_application_user_claim_user_id ON _a3s.application_user_claim USING btree (user_id) WITH (fillfactor='90');


--
-- Name: ix_aspnet_role_claim_role_id; Type: INDEX; Schema: _a3s; Owner: postgres
--

CREATE INDEX ix_aspnet_role_claim_role_id ON _a3s.aspnet_role_claim USING btree (role_id) WITH (fillfactor='90');


--
-- Name: ix_aspnet_user_login_user_id; Type: INDEX; Schema: _a3s; Owner: postgres
--

CREATE INDEX ix_aspnet_user_login_user_id ON _a3s.aspnet_user_login USING btree (user_id) WITH (fillfactor='90');


--
-- Name: ix_aspnet_user_role_role_id; Type: INDEX; Schema: _a3s; Owner: postgres
--

CREATE INDEX ix_aspnet_user_role_role_id ON _a3s.aspnet_user_role USING btree (role_id) WITH (fillfactor='90');


--
-- Name: ix_function_application_id; Type: INDEX; Schema: _a3s; Owner: postgres
--

CREATE INDEX ix_function_application_id ON _a3s.function USING btree (application_id) WITH (fillfactor='90');


--
-- Name: ix_function_permission_function_id; Type: INDEX; Schema: _a3s; Owner: postgres
--

CREATE INDEX ix_function_permission_function_id ON _a3s.function_permission USING btree (function_id) WITH (fillfactor='90');


--
-- Name: ix_role_function_function_id; Type: INDEX; Schema: _a3s; Owner: postgres
--

CREATE INDEX ix_role_function_function_id ON _a3s.role_function USING btree (function_id) WITH (fillfactor='90');


--
-- Name: ix_role_name; Type: INDEX; Schema: _a3s; Owner: postgres
--

CREATE UNIQUE INDEX ix_role_name ON _a3s.role USING btree (name) WITH (fillfactor='90');


--
-- Name: ix_role_role_child_role_id; Type: INDEX; Schema: _a3s; Owner: postgres
--

CREATE INDEX ix_role_role_child_role_id ON _a3s.role_role USING btree (child_role_id) WITH (fillfactor='90');


--
-- Name: ix_team_team_child_team_id; Type: INDEX; Schema: _a3s; Owner: postgres
--

CREATE INDEX ix_team_team_child_team_id ON _a3s.team_team USING btree (child_team_id) WITH (fillfactor='90');


--
-- Name: ix_user_role_user_id; Type: INDEX; Schema: _a3s; Owner: postgres
--

CREATE INDEX ix_user_role_user_id ON _a3s.user_role USING btree (user_id) WITH (fillfactor='90');


--
-- Name: ix_user_team_user_id; Type: INDEX; Schema: _a3s; Owner: postgres
--

CREATE INDEX ix_user_team_user_id ON _a3s.user_team USING btree (user_id) WITH (fillfactor='90');


--
-- Name: role_name_index; Type: INDEX; Schema: _a3s; Owner: postgres
--

CREATE UNIQUE INDEX role_name_index ON _a3s.aspnet_role USING btree (normalized_name) WITH (fillfactor='90');


--
-- Name: IX_ApiClaims_ApiResourceId; Type: INDEX; Schema: _ids4; Owner: postgres
--

CREATE INDEX "IX_ApiClaims_ApiResourceId" ON _ids4."ApiClaims" USING btree ("ApiResourceId");


--
-- Name: IX_ApiProperties_ApiResourceId; Type: INDEX; Schema: _ids4; Owner: postgres
--

CREATE INDEX "IX_ApiProperties_ApiResourceId" ON _ids4."ApiProperties" USING btree ("ApiResourceId");


--
-- Name: IX_ApiResources_Name; Type: INDEX; Schema: _ids4; Owner: postgres
--

CREATE UNIQUE INDEX "IX_ApiResources_Name" ON _ids4."ApiResources" USING btree ("Name");


--
-- Name: IX_ApiScopeClaims_ApiScopeId; Type: INDEX; Schema: _ids4; Owner: postgres
--

CREATE INDEX "IX_ApiScopeClaims_ApiScopeId" ON _ids4."ApiScopeClaims" USING btree ("ApiScopeId");


--
-- Name: IX_ApiScopes_ApiResourceId; Type: INDEX; Schema: _ids4; Owner: postgres
--

CREATE INDEX "IX_ApiScopes_ApiResourceId" ON _ids4."ApiScopes" USING btree ("ApiResourceId");


--
-- Name: IX_ApiScopes_Name; Type: INDEX; Schema: _ids4; Owner: postgres
--

CREATE UNIQUE INDEX "IX_ApiScopes_Name" ON _ids4."ApiScopes" USING btree ("Name");


--
-- Name: IX_ApiSecrets_ApiResourceId; Type: INDEX; Schema: _ids4; Owner: postgres
--

CREATE INDEX "IX_ApiSecrets_ApiResourceId" ON _ids4."ApiSecrets" USING btree ("ApiResourceId");


--
-- Name: IX_ClientClaims_ClientId; Type: INDEX; Schema: _ids4; Owner: postgres
--

CREATE INDEX "IX_ClientClaims_ClientId" ON _ids4."ClientClaims" USING btree ("ClientId");


--
-- Name: IX_ClientCorsOrigins_ClientId; Type: INDEX; Schema: _ids4; Owner: postgres
--

CREATE INDEX "IX_ClientCorsOrigins_ClientId" ON _ids4."ClientCorsOrigins" USING btree ("ClientId");


--
-- Name: IX_ClientGrantTypes_ClientId; Type: INDEX; Schema: _ids4; Owner: postgres
--

CREATE INDEX "IX_ClientGrantTypes_ClientId" ON _ids4."ClientGrantTypes" USING btree ("ClientId");


--
-- Name: IX_ClientIdPRestrictions_ClientId; Type: INDEX; Schema: _ids4; Owner: postgres
--

CREATE INDEX "IX_ClientIdPRestrictions_ClientId" ON _ids4."ClientIdPRestrictions" USING btree ("ClientId");


--
-- Name: IX_ClientPostLogoutRedirectUris_ClientId; Type: INDEX; Schema: _ids4; Owner: postgres
--

CREATE INDEX "IX_ClientPostLogoutRedirectUris_ClientId" ON _ids4."ClientPostLogoutRedirectUris" USING btree ("ClientId");


--
-- Name: IX_ClientProperties_ClientId; Type: INDEX; Schema: _ids4; Owner: postgres
--

CREATE INDEX "IX_ClientProperties_ClientId" ON _ids4."ClientProperties" USING btree ("ClientId");


--
-- Name: IX_ClientRedirectUris_ClientId; Type: INDEX; Schema: _ids4; Owner: postgres
--

CREATE INDEX "IX_ClientRedirectUris_ClientId" ON _ids4."ClientRedirectUris" USING btree ("ClientId");


--
-- Name: IX_ClientScopes_ClientId; Type: INDEX; Schema: _ids4; Owner: postgres
--

CREATE INDEX "IX_ClientScopes_ClientId" ON _ids4."ClientScopes" USING btree ("ClientId");


--
-- Name: IX_ClientSecrets_ClientId; Type: INDEX; Schema: _ids4; Owner: postgres
--

CREATE INDEX "IX_ClientSecrets_ClientId" ON _ids4."ClientSecrets" USING btree ("ClientId");


--
-- Name: IX_Clients_ClientId; Type: INDEX; Schema: _ids4; Owner: postgres
--

CREATE UNIQUE INDEX "IX_Clients_ClientId" ON _ids4."Clients" USING btree ("ClientId");


--
-- Name: IX_IdentityClaims_IdentityResourceId; Type: INDEX; Schema: _ids4; Owner: postgres
--

CREATE INDEX "IX_IdentityClaims_IdentityResourceId" ON _ids4."IdentityClaims" USING btree ("IdentityResourceId");


--
-- Name: IX_IdentityProperties_IdentityResourceId; Type: INDEX; Schema: _ids4; Owner: postgres
--

CREATE INDEX "IX_IdentityProperties_IdentityResourceId" ON _ids4."IdentityProperties" USING btree ("IdentityResourceId");


--
-- Name: IX_IdentityResources_Name; Type: INDEX; Schema: _ids4; Owner: postgres
--

CREATE UNIQUE INDEX "IX_IdentityResources_Name" ON _ids4."IdentityResources" USING btree ("Name");


--
-- Name: flyway_schema_history_s_idx; Type: INDEX; Schema: _ids4; Owner: postgres
--

CREATE INDEX flyway_schema_history_s_idx ON _ids4.flyway_schema_history USING btree (success);


--
-- Name: application_user application_user_ldap_authentication_mode_id_fkey; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.application_user
    ADD CONSTRAINT application_user_ldap_authentication_mode_id_fkey FOREIGN KEY (ldap_authentication_mode_id) REFERENCES _a3s.ldap_authentication_mode(id) ON DELETE RESTRICT;


--
-- Name: application_data_policy fk_application_data_policy_application_application_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.application_data_policy
    ADD CONSTRAINT fk_application_data_policy_application_application_id FOREIGN KEY (application_id) REFERENCES _a3s.application(id) ON DELETE CASCADE;


--
-- Name: application_function fk_application_function_application_application_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.application_function
    ADD CONSTRAINT fk_application_function_application_application_id FOREIGN KEY (application_id) REFERENCES _a3s.application(id) ON DELETE RESTRICT;


--
-- Name: application_function_permission fk_application_function_permission_application_function_applic~; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.application_function_permission
    ADD CONSTRAINT "fk_application_function_permission_application_function_applic~" FOREIGN KEY (application_function_id) REFERENCES _a3s.application_function(id) ON DELETE CASCADE;


--
-- Name: application_function_permission fk_application_function_permission_permission_permission_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.application_function_permission
    ADD CONSTRAINT fk_application_function_permission_permission_permission_id FOREIGN KEY (permission_id) REFERENCES _a3s.permission(id) ON DELETE CASCADE;


--
-- Name: application_user_claim fk_application_user_claim_application_user_user_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.application_user_claim
    ADD CONSTRAINT fk_application_user_claim_application_user_user_id FOREIGN KEY (user_id) REFERENCES _a3s.application_user(id) ON DELETE RESTRICT;


--
-- Name: aspnet_role_claim fk_aspnet_role_claim_aspnet_role_role_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.aspnet_role_claim
    ADD CONSTRAINT fk_aspnet_role_claim_aspnet_role_role_id FOREIGN KEY (role_id) REFERENCES _a3s.aspnet_role(id) ON DELETE CASCADE;


--
-- Name: aspnet_user_login fk_aspnet_user_login_application_user_user_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.aspnet_user_login
    ADD CONSTRAINT fk_aspnet_user_login_application_user_user_id FOREIGN KEY (user_id) REFERENCES _a3s.application_user(id) ON DELETE CASCADE;


--
-- Name: aspnet_user_role fk_aspnet_user_role_application_user_user_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.aspnet_user_role
    ADD CONSTRAINT fk_aspnet_user_role_application_user_user_id FOREIGN KEY (user_id) REFERENCES _a3s.application_user(id) ON DELETE CASCADE;


--
-- Name: aspnet_user_role fk_aspnet_user_role_aspnet_role_role_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.aspnet_user_role
    ADD CONSTRAINT fk_aspnet_user_role_aspnet_role_role_id FOREIGN KEY (role_id) REFERENCES _a3s.aspnet_role(id) ON DELETE CASCADE;


--
-- Name: application_user_token fk_aspnet_user_token_application_user_user_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.application_user_token
    ADD CONSTRAINT fk_aspnet_user_token_application_user_user_id FOREIGN KEY (user_id) REFERENCES _a3s.application_user(id) ON DELETE CASCADE;


--
-- Name: function fk_function_application_application_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.function
    ADD CONSTRAINT fk_function_application_application_id FOREIGN KEY (application_id) REFERENCES _a3s.application(id) ON DELETE RESTRICT;


--
-- Name: function_permission fk_function_permission_function_function_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.function_permission
    ADD CONSTRAINT fk_function_permission_function_function_id FOREIGN KEY (function_id) REFERENCES _a3s.function(id) ON DELETE CASCADE;


--
-- Name: function_permission fk_function_permission_permission_permission_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.function_permission
    ADD CONSTRAINT fk_function_permission_permission_permission_id FOREIGN KEY (permission_id) REFERENCES _a3s.permission(id) ON DELETE CASCADE;


--
-- Name: function fk_function_sub_realm_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.function
    ADD CONSTRAINT fk_function_sub_realm_id FOREIGN KEY (sub_realm_id) REFERENCES _a3s.sub_realm(id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: profile fk_profile_application_user_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.profile
    ADD CONSTRAINT fk_profile_application_user_id FOREIGN KEY (user_id) REFERENCES _a3s.application_user(id) MATCH FULL ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: profile_role fk_profile_role_profile_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.profile_role
    ADD CONSTRAINT fk_profile_role_profile_id FOREIGN KEY (profile_id) REFERENCES _a3s.profile(id) MATCH FULL ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: profile_role fk_profile_role_role_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.profile_role
    ADD CONSTRAINT fk_profile_role_role_id FOREIGN KEY (role_id) REFERENCES _a3s.role(id) MATCH FULL ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: profile fk_profile_sub_realm_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.profile
    ADD CONSTRAINT fk_profile_sub_realm_id FOREIGN KEY (sub_realm_id) REFERENCES _a3s.sub_realm(id) MATCH FULL ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: profile_team fk_profile_team_profile_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.profile_team
    ADD CONSTRAINT fk_profile_team_profile_id FOREIGN KEY (profile_id) REFERENCES _a3s.profile(id) MATCH FULL ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: profile_team fk_profile_team_team_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.profile_team
    ADD CONSTRAINT fk_profile_team_team_id FOREIGN KEY (team_id) REFERENCES _a3s.team(id) MATCH FULL ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: role_function fk_role_function_function_function_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.role_function
    ADD CONSTRAINT fk_role_function_function_function_id FOREIGN KEY (function_id) REFERENCES _a3s.function(id) ON DELETE CASCADE;


--
-- Name: role_function fk_role_function_role_role_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.role_function
    ADD CONSTRAINT fk_role_function_role_role_id FOREIGN KEY (role_id) REFERENCES _a3s.role(id) ON DELETE CASCADE;


--
-- Name: role_role fk_role_role_role_child_role_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.role_role
    ADD CONSTRAINT fk_role_role_role_child_role_id FOREIGN KEY (child_role_id) REFERENCES _a3s.role(id) ON DELETE CASCADE;


--
-- Name: role_role fk_role_role_role_parent_role_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.role_role
    ADD CONSTRAINT fk_role_role_role_parent_role_id FOREIGN KEY (parent_role_id) REFERENCES _a3s.role(id) ON DELETE CASCADE;


--
-- Name: role fk_role_sub_realm_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.role
    ADD CONSTRAINT fk_role_sub_realm_id FOREIGN KEY (sub_realm_id) REFERENCES _a3s.sub_realm(id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: sub_realm_permission fk_sub_realm_permission_permission_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.sub_realm_permission
    ADD CONSTRAINT fk_sub_realm_permission_permission_id FOREIGN KEY (permission_id) REFERENCES _a3s.permission(id) MATCH FULL ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: sub_realm_permission fk_sub_realm_permission_sub_realm_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.sub_realm_permission
    ADD CONSTRAINT fk_sub_realm_permission_sub_realm_id FOREIGN KEY (sub_realm_id) REFERENCES _a3s.sub_realm(id) MATCH FULL ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: team_application_data_policy fk_team_application_data_policy_application_data_policy_applica; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.team_application_data_policy
    ADD CONSTRAINT fk_team_application_data_policy_application_data_policy_applica FOREIGN KEY (application_data_policy_id) REFERENCES _a3s.application_data_policy(id) ON DELETE CASCADE;


--
-- Name: team_application_data_policy fk_team_application_data_policy_team_team_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.team_application_data_policy
    ADD CONSTRAINT fk_team_application_data_policy_team_team_id FOREIGN KEY (team_id) REFERENCES _a3s.team(id) ON DELETE CASCADE;


--
-- Name: team fk_team_sub_realm_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.team
    ADD CONSTRAINT fk_team_sub_realm_id FOREIGN KEY (sub_realm_id) REFERENCES _a3s.sub_realm(id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: team_team fk_team_team_team_child_team_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.team_team
    ADD CONSTRAINT fk_team_team_team_child_team_id FOREIGN KEY (child_team_id) REFERENCES _a3s.team(id) ON DELETE CASCADE;


--
-- Name: team_team fk_team_team_team_parent_team_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.team_team
    ADD CONSTRAINT fk_team_team_team_parent_team_id FOREIGN KEY (parent_team_id) REFERENCES _a3s.team(id) ON DELETE CASCADE;


--
-- Name: team fk_team_terms_of_service_terms_of_service_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.team
    ADD CONSTRAINT fk_team_terms_of_service_terms_of_service_id FOREIGN KEY (terms_of_service_id) REFERENCES _a3s.terms_of_service(id) MATCH FULL ON DELETE RESTRICT;


--
-- Name: terms_of_service fk_terms_of_service_sub_realm_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.terms_of_service
    ADD CONSTRAINT fk_terms_of_service_sub_realm_id FOREIGN KEY (sub_realm_id) REFERENCES _a3s.sub_realm(id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: terms_of_service_user_acceptance_history fk_terms_of_service_user_acceptance_history_terms_of_service_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.terms_of_service_user_acceptance_history
    ADD CONSTRAINT fk_terms_of_service_user_acceptance_history_terms_of_service_id FOREIGN KEY (terms_of_service_id) REFERENCES _a3s.terms_of_service(id) MATCH FULL;


--
-- Name: terms_of_service_user_acceptance_history fk_terms_of_service_user_acceptance_history_user_user_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.terms_of_service_user_acceptance_history
    ADD CONSTRAINT fk_terms_of_service_user_acceptance_history_user_user_id FOREIGN KEY (user_id) REFERENCES _a3s.application_user(id) MATCH FULL;


--
-- Name: terms_of_service_user_acceptance fk_terms_of_service_user_acceptance_terms_of_service_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.terms_of_service_user_acceptance
    ADD CONSTRAINT fk_terms_of_service_user_acceptance_terms_of_service_id FOREIGN KEY (terms_of_service_id) REFERENCES _a3s.terms_of_service(id) MATCH FULL;


--
-- Name: terms_of_service_user_acceptance fk_terms_of_service_user_acceptance_user_user_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.terms_of_service_user_acceptance
    ADD CONSTRAINT fk_terms_of_service_user_acceptance_user_user_id FOREIGN KEY (user_id) REFERENCES _a3s.application_user(id) MATCH FULL;


--
-- Name: user_role fk_user_role_application_user_user_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.user_role
    ADD CONSTRAINT fk_user_role_application_user_user_id FOREIGN KEY (user_id) REFERENCES _a3s.application_user(id) ON DELETE CASCADE;


--
-- Name: user_role fk_user_role_role_role_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.user_role
    ADD CONSTRAINT fk_user_role_role_role_id FOREIGN KEY (role_id) REFERENCES _a3s.role(id) ON DELETE CASCADE;


--
-- Name: user_team fk_user_team_application_user_user_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.user_team
    ADD CONSTRAINT fk_user_team_application_user_user_id FOREIGN KEY (user_id) REFERENCES _a3s.application_user(id) ON DELETE CASCADE;


--
-- Name: user_team fk_user_team_team_team_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.user_team
    ADD CONSTRAINT fk_user_team_team_team_id FOREIGN KEY (team_id) REFERENCES _a3s.team(id) ON DELETE CASCADE;


--
-- Name: ldap_authentication_mode_ldap_attribute ldap_authentication_mode_ldap__ldap_authentication_mode_id_fkey; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.ldap_authentication_mode_ldap_attribute
    ADD CONSTRAINT ldap_authentication_mode_ldap__ldap_authentication_mode_id_fkey FOREIGN KEY (ldap_authentication_mode_id) REFERENCES _a3s.ldap_authentication_mode(id);


--
-- Name: ApiClaims FK_ApiClaims_ApiResources_ApiResourceId; Type: FK CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ApiClaims"
    ADD CONSTRAINT "FK_ApiClaims_ApiResources_ApiResourceId" FOREIGN KEY ("ApiResourceId") REFERENCES _ids4."ApiResources"("Id") ON DELETE CASCADE;


--
-- Name: ApiProperties FK_ApiProperties_ApiResources_ApiResourceId; Type: FK CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ApiProperties"
    ADD CONSTRAINT "FK_ApiProperties_ApiResources_ApiResourceId" FOREIGN KEY ("ApiResourceId") REFERENCES _ids4."ApiResources"("Id") ON DELETE CASCADE;


--
-- Name: ApiScopeClaims FK_ApiScopeClaims_ApiScopes_ApiScopeId; Type: FK CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ApiScopeClaims"
    ADD CONSTRAINT "FK_ApiScopeClaims_ApiScopes_ApiScopeId" FOREIGN KEY ("ApiScopeId") REFERENCES _ids4."ApiScopes"("Id") ON DELETE CASCADE;


--
-- Name: ApiScopes FK_ApiScopes_ApiResources_ApiResourceId; Type: FK CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ApiScopes"
    ADD CONSTRAINT "FK_ApiScopes_ApiResources_ApiResourceId" FOREIGN KEY ("ApiResourceId") REFERENCES _ids4."ApiResources"("Id") ON DELETE CASCADE;


--
-- Name: ApiSecrets FK_ApiSecrets_ApiResources_ApiResourceId; Type: FK CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ApiSecrets"
    ADD CONSTRAINT "FK_ApiSecrets_ApiResources_ApiResourceId" FOREIGN KEY ("ApiResourceId") REFERENCES _ids4."ApiResources"("Id") ON DELETE CASCADE;


--
-- Name: ClientClaims FK_ClientClaims_Clients_ClientId; Type: FK CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientClaims"
    ADD CONSTRAINT "FK_ClientClaims_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES _ids4."Clients"("Id") ON DELETE CASCADE;


--
-- Name: ClientCorsOrigins FK_ClientCorsOrigins_Clients_ClientId; Type: FK CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientCorsOrigins"
    ADD CONSTRAINT "FK_ClientCorsOrigins_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES _ids4."Clients"("Id") ON DELETE CASCADE;


--
-- Name: ClientGrantTypes FK_ClientGrantTypes_Clients_ClientId; Type: FK CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientGrantTypes"
    ADD CONSTRAINT "FK_ClientGrantTypes_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES _ids4."Clients"("Id") ON DELETE CASCADE;


--
-- Name: ClientIdPRestrictions FK_ClientIdPRestrictions_Clients_ClientId; Type: FK CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientIdPRestrictions"
    ADD CONSTRAINT "FK_ClientIdPRestrictions_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES _ids4."Clients"("Id") ON DELETE CASCADE;


--
-- Name: ClientPostLogoutRedirectUris FK_ClientPostLogoutRedirectUris_Clients_ClientId; Type: FK CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientPostLogoutRedirectUris"
    ADD CONSTRAINT "FK_ClientPostLogoutRedirectUris_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES _ids4."Clients"("Id") ON DELETE CASCADE;


--
-- Name: ClientProperties FK_ClientProperties_Clients_ClientId; Type: FK CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientProperties"
    ADD CONSTRAINT "FK_ClientProperties_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES _ids4."Clients"("Id") ON DELETE CASCADE;


--
-- Name: ClientRedirectUris FK_ClientRedirectUris_Clients_ClientId; Type: FK CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientRedirectUris"
    ADD CONSTRAINT "FK_ClientRedirectUris_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES _ids4."Clients"("Id") ON DELETE CASCADE;


--
-- Name: ClientScopes FK_ClientScopes_Clients_ClientId; Type: FK CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientScopes"
    ADD CONSTRAINT "FK_ClientScopes_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES _ids4."Clients"("Id") ON DELETE CASCADE;


--
-- Name: ClientSecrets FK_ClientSecrets_Clients_ClientId; Type: FK CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."ClientSecrets"
    ADD CONSTRAINT "FK_ClientSecrets_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES _ids4."Clients"("Id") ON DELETE CASCADE;


--
-- Name: IdentityClaims FK_IdentityClaims_IdentityResources_IdentityResourceId; Type: FK CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."IdentityClaims"
    ADD CONSTRAINT "FK_IdentityClaims_IdentityResources_IdentityResourceId" FOREIGN KEY ("IdentityResourceId") REFERENCES _ids4."IdentityResources"("Id") ON DELETE CASCADE;


--
-- Name: IdentityProperties FK_IdentityProperties_IdentityResources_IdentityResourceId; Type: FK CONSTRAINT; Schema: _ids4; Owner: postgres
--

ALTER TABLE ONLY _ids4."IdentityProperties"
    ADD CONSTRAINT "FK_IdentityProperties_IdentityResources_IdentityResourceId" FOREIGN KEY ("IdentityResourceId") REFERENCES _ids4."IdentityResources"("Id") ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--


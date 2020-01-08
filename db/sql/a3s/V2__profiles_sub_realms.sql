--
-- *************************************************
-- Copyright (c) 2019, Grindrod Bank Limited
-- License MIT: https://opensource.org/licenses/MIT
-- **************************************************
--

-- [ Created objects ] --
-- object: _a3s.profile | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.profile CASCADE;
CREATE TABLE _a3s.profile (
	id uuid NOT NULL,
	name text NOT NULL,
	description text,
	user_id text NOT NULL,
	sub_realm_id uuid NOT NULL,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL,
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

-- object: _a3s.sub_realm | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.sub_realm CASCADE;
CREATE TABLE _a3s.sub_realm (
	id uuid NOT NULL,
	name text NOT NULL,
	description text,
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
COMMENT ON CONSTRAINT uk_sub_realm_name ON _a3s.sub_realm  IS 'A uniqueness contraint ensuring that a sub realm''s name is always unique.';
-- ddl-end --
ALTER TABLE _a3s.sub_realm OWNER TO postgres;
-- ddl-end --

-- object: _a3s.sub_realm_permission | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.sub_realm_permission CASCADE;
CREATE TABLE _a3s.sub_realm_permission (
	sub_realm_id uuid NOT NULL,
	permission_id uuid NOT NULL,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL,
	CONSTRAINT pk_sub_realm_permission PRIMARY KEY (sub_realm_id,permission_id)

);
-- ddl-end --
COMMENT ON COLUMN _a3s.sub_realm_permission.changed_by IS 'The UUID of the user that last changed the record.';
-- ddl-end --
COMMENT ON COLUMN _a3s.sub_realm_permission.sys_period IS 'The temporal data for changes to this table.';
-- ddl-end --

-- object: _a3s.profile_role | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.profile_role CASCADE;
CREATE TABLE _a3s.profile_role (
	profile_id uuid NOT NULL,
	role_id uuid NOT NULL,
	changed_by uuid NOT NULL,
	sys_period tstzrange NOT NULL,
	CONSTRAINT profile_role_pk PRIMARY KEY (profile_id,role_id)

);
-- ddl-end --
COMMENT ON COLUMN _a3s.profile_role.changed_by IS 'Stores the UUID of the user that last changed this record.';
-- ddl-end --
COMMENT ON COLUMN _a3s.profile_role.sys_period IS 'Temporal data for this record.';
-- ddl-end --

-- object: _a3s.profile_team | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.profile_team CASCADE;
CREATE TABLE _a3s.profile_team (
	profile_id uuid NOT NULL,
	team_id uuid NOT NULL,
	CONSTRAINT profile_team_pk PRIMARY KEY (profile_id,team_id)

);
-- ddl-end --

-- object: sub_realm_id | type: COLUMN --
-- ALTER TABLE _a3s.role DROP COLUMN IF EXISTS sub_realm_id CASCADE;
ALTER TABLE _a3s.role ADD COLUMN sub_realm_id uuid;
-- ddl-end --


-- object: sub_realm_id | type: COLUMN --
-- ALTER TABLE _a3s.function DROP COLUMN IF EXISTS sub_realm_id CASCADE;
ALTER TABLE _a3s.function ADD COLUMN sub_realm_id uuid;
-- ddl-end --


-- object: sub_realm_id | type: COLUMN --
-- ALTER TABLE _a3s.team DROP COLUMN IF EXISTS sub_realm_id CASCADE;
ALTER TABLE _a3s.team ADD COLUMN sub_realm_id uuid;
-- ddl-end -
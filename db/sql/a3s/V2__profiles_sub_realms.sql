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

-- object: _a3s.sub_realm | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.sub_realm CASCADE;
CREATE TABLE _a3s.sub_realm (
	id uuid NOT NULL,
	name text NOT NULL,
	description text,
	changed_by uuid,
	sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
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
COMMENT ON COLUMN _a3s.sub_realm.sys_period IS 'Temporal data for this record.';
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
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
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
	sys_period tstzrange NOT NULL DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT pk_profile_role PRIMARY KEY (profile_id,role_id)

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
	changed_by uuid,
	sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT pk_profile_team PRIMARY KEY (profile_id,team_id)

);
-- ddl-end --
COMMENT ON COLUMN _a3s.profile_team.changed_by IS 'UUID of user that last modified the record.';
-- ddl-end --
COMMENT ON COLUMN _a3s.profile_team.sys_period IS 'Temporal data for this record.';
-- ddl-end --

-- object: _a3s.sub_realm_application_data_policy | type: TABLE --
-- DROP TABLE IF EXISTS _a3s.sub_realm_application_data_policy CASCADE;
CREATE TABLE _a3s.sub_realm_application_data_policy (
	sub_realm_id uuid NOT NULL,
	application_data_policy_id uuid NOT NULL,
	changed_by uuid,
	sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone),
	CONSTRAINT pk_sub_realm_application_data_policy PRIMARY KEY (sub_realm_id,application_data_policy_id)

);
-- ddl-end --
COMMENT ON COLUMN _a3s.sub_realm_application_data_policy.changed_by IS 'The UUID of the user that last modified this record.';
-- ddl-end --
COMMENT ON COLUMN _a3s.sub_realm_application_data_policy.sys_period IS 'The temporal data for this record.';
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
-- ddl-end --


-- object: sub_realm_id | type: COLUMN --
-- ALTER TABLE _a3s.terms_of_service DROP COLUMN IF EXISTS sub_realm_id CASCADE;
ALTER TABLE _a3s.terms_of_service ADD COLUMN sub_realm_id uuid;
-- ddl-end --




-- [ Created constraints ] --
-- object: uq_profile | type: CONSTRAINT --
-- ALTER TABLE _a3s.profile DROP CONSTRAINT IF EXISTS uq_profile CASCADE;
ALTER TABLE _a3s.profile ADD CONSTRAINT uq_profile UNIQUE (sub_realm_id);
-- ddl-end --



-- [ Created foreign keys ] --
-- object: fk_profile_application_user_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.profile DROP CONSTRAINT IF EXISTS fk_profile_application_user_id CASCADE;
ALTER TABLE _a3s.profile ADD CONSTRAINT fk_profile_application_user_id FOREIGN KEY (user_id)
REFERENCES _a3s.application_user (id) MATCH FULL
ON DELETE CASCADE ON UPDATE CASCADE;
-- ddl-end --

-- object: fk_profile_sub_realm_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.profile DROP CONSTRAINT IF EXISTS fk_profile_sub_realm_id CASCADE;
ALTER TABLE _a3s.profile ADD CONSTRAINT fk_profile_sub_realm_id FOREIGN KEY (sub_realm_id)
REFERENCES _a3s.sub_realm (id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
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

-- object: fk_sub_realm_application_data_policy_sub_realm_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.sub_realm_application_data_policy DROP CONSTRAINT IF EXISTS fk_sub_realm_application_data_policy_sub_realm_id CASCADE;
ALTER TABLE _a3s.sub_realm_application_data_policy ADD CONSTRAINT fk_sub_realm_application_data_policy_sub_realm_id FOREIGN KEY (sub_realm_id)
REFERENCES _a3s.sub_realm (id) MATCH FULL
ON DELETE CASCADE ON UPDATE CASCADE;
-- ddl-end --

-- object: fk_sub_realm_application_data_policy_id | type: CONSTRAINT --
-- ALTER TABLE _a3s.sub_realm_application_data_policy DROP CONSTRAINT IF EXISTS fk_sub_realm_application_data_policy_id CASCADE;
ALTER TABLE _a3s.sub_realm_application_data_policy ADD CONSTRAINT fk_sub_realm_application_data_policy_id FOREIGN KEY (application_data_policy_id)
REFERENCES _a3s.application_data_policy (id) MATCH FULL
ON DELETE CASCADE ON UPDATE CASCADE;
-- ddl-end --
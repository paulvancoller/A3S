--
-- *************************************************
-- Copyright (c) 2019, Grindrod Bank Limited
-- License MIT: https://opensource.org/licenses/MIT
-- **************************************************
--

-- These updates relate to A3S v1.0.2 updates

--
-- Name: terms_of_service; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.terms_of_service (
    id uuid NOT NULL,
    agreement_name text NOT NULL,
    version text NOT NULL,
    agreement_file bytea NOT NULL,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
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
-- Name: terms_of_service terms_of_service_pk; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.terms_of_service
    ADD CONSTRAINT terms_of_service_pk PRIMARY KEY (id);

--
-- Name: terms_of_service uk_terms_of_service_agreement_name_version; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.terms_of_service
    ADD CONSTRAINT uk_terms_of_service_agreement_name_version UNIQUE (agreement_name, version);

--
-- Name: team; Type: TABLE; Schema: _a3s; Owner: postgres
--

ALTER TABLE _a3s.team
    ADD terms_of_service_id uuid NULL;

--
-- Name: team fk_team_terms_of_service_terms_of_service_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.team
    ADD CONSTRAINT fk_team_terms_of_service_terms_of_service_id FOREIGN KEY (terms_of_service_id) REFERENCES _a3s.terms_of_service(id) MATCH FULL ON DELETE RESTRICT;



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
-- Adding of not-null constraints

ALTER TABLE _a3s.application ALTER COLUMN name SET NOT NULL;
ALTER TABLE _a3s.application ALTER COLUMN sys_period SET NOT NULL;
ALTER TABLE _a3s.application_function ALTER COLUMN name SET NOT NULL;
ALTER TABLE _a3s.application_function ALTER COLUMN application_id SET NOT NULL;
ALTER TABLE _a3s.application_function ALTER COLUMN sys_period SET NOT NULL;
ALTER TABLE _a3s.application_function_permission ALTER COLUMN sys_period SET NOT NULL;
ALTER TABLE _a3s.function ALTER COLUMN name SET NOT NULL;
ALTER TABLE _a3s.function ALTER COLUMN application_id SET NOT NULL;
ALTER TABLE _a3s.function ALTER COLUMN sys_period SET NOT NULL;
ALTER TABLE _a3s.function_permission ALTER COLUMN sys_period SET NOT NULL;
ALTER TABLE _a3s.permission ALTER COLUMN name SET NOT NULL;
ALTER TABLE _a3s.permission ALTER COLUMN sys_period SET NOT NULL;
ALTER TABLE _a3s.role ALTER COLUMN name SET NOT NULL;
ALTER TABLE _a3s.role ALTER COLUMN sys_period SET NOT NULL;
ALTER TABLE _a3s.role_function ALTER COLUMN sys_period SET NOT NULL;
ALTER TABLE _a3s.team ALTER COLUMN name SET NOT NULL;
ALTER TABLE _a3s.team ALTER COLUMN sys_period SET NOT NULL;
ALTER TABLE _a3s.user_role ALTER COLUMN sys_period SET NOT NULL;
ALTER TABLE _a3s.user_team ALTER COLUMN sys_period SET NOT NULL;
ALTER TABLE _a3s.team_team ALTER COLUMN sys_period SET NOT NULL;
ALTER TABLE _a3s.role_role ALTER COLUMN sys_period SET NOT NULL;
ALTER TABLE _a3s.application_data_policy ALTER COLUMN sys_period SET NOT NULL;
ALTER TABLE _a3s.team_application_data_policy ALTER COLUMN sys_period SET NOT NULL;


-- FUNCTION: _a3s."terms_of_service_acceptance archive"(uuid)

-- DROP FUNCTION _a3s."terms_of_service_acceptance archive"(uuid);

CREATE OR REPLACE FUNCTION _a3s."terms_of_service_acceptance archive"(
	terms_of_service_guid uuid)
    RETURNS boolean
    LANGUAGE 'sql'

    COST 100
    VOLATILE 
AS $BODY$

-- Copy records and set upper bound acceptance_time
INSERT INTO _a3S.terms_of_service_user_acceptance_history
SELECT terms_of_service_id, user_id, tstzrange(lower(acceptance_time), CURRENT_TIMESTAMP) FROM _a3s.terms_of_service_user_acceptance
WHERE terms_of_service_id = terms_of_service_guid;

-- Remove copied records from source
DELETE FROM _a3s.terms_of_service_user_acceptance where terms_of_service_id = terms_of_service_guid;

-- All done
select true;
$BODY$;

ALTER FUNCTION _a3s."terms_of_service_acceptance archive"(uuid)
    OWNER TO postgres;

COMMENT ON FUNCTION _a3s."terms_of_service_acceptance archive"(uuid)
    IS 'Archives the Terms of Service acceptance records from the terms_of_service_user_acceptance table to the terms_of_service_user_acceptance_history table.';

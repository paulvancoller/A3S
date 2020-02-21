--
-- *************************************************
-- Copyright (c) 2019, Grindrod Bank Limited
-- License MIT: https://opensource.org/licenses/MIT
-- **************************************************
--

-- These updates relate to A3S v1.0.2 updates

--
-- Name: consent_of_service; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.consent_of_service (
    id uuid NOT NULL,
    consent_file bytea NOT NULL,
    changed_by uuid NOT NULL,
    sys_period tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
);


ALTER TABLE _a3s.consent_of_service OWNER TO postgres;

--
-- Name: TABLE consent_of_service; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.consent_of_service IS 'Consent of service entries.';


--
-- Name: COLUMN consent_of_service.consent_file; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.consent_of_service.consent_file IS 'A .tar.gz file, containing file with the consent css:
- consent_of_service.css';

--
-- Name: consent_of_service consent_of_service_pk; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.consent_of_service
    ADD CONSTRAINT consent_of_service_pk PRIMARY KEY (id);
	
------------ACCEPTANCE------------
--
-- Name: consent_of_service_user_acceptance; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.consent_of_service_user_acceptance (
    id uuid NOT NULL,
	permission_id uuid NOT NULL,
    user_id text NOT NULL,
    acceptance_time tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
);


ALTER TABLE _a3s.consent_of_service_user_acceptance OWNER TO postgres;

--
-- Name: TABLE consent_of_service_user_acceptance; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.consent_of_service_user_acceptance IS 'This records the acceptance of consent of service entries by users.';

--
-- Name: COLUMN consent_of_service_user_acceptance.permission_id; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.consent_of_service_user_acceptance.permission_id IS 'User accepted the specific consent permission.';

--
-- Name: COLUMN consent_of_service_user_acceptance.user_id; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.consent_of_service_user_acceptance.user_id IS 'User ID accepted the specific consent permission.';

--
-- Name: COLUMN consent_of_service_user_acceptance.acceptance_time; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.consent_of_service_user_acceptance.acceptance_time IS 'The date and time the user accepted the specific consent.';

--
-- Name: consent_of_service consent_of_service_pk; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--
ALTER TABLE ONLY _a3s.consent_of_service_user_acceptance
    ADD CONSTRAINT consent_of_service_user_acceptance_pk PRIMARY KEY (id);

--
-- Name: consent_of_service_user_acceptance fk_terms_of_service_user_acceptance_history_user_user_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.consent_of_service_user_acceptance
    ADD CONSTRAINT fk_consent_of_service_user_acceptance_user_user_id FOREIGN KEY (user_id) REFERENCES _a3s.application_user(id) MATCH FULL;
	
--
-- Name: consent_of_service_user_acceptance fk_terms_of_service_user_acceptance_history_user_user_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.consent_of_service_user_acceptance
    ADD CONSTRAINT fk_consent_of_service_user_acceptance_permission_permission_id FOREIGN KEY (permission_id) REFERENCES _a3s.permission(id) MATCH FULL;
	
------------ACCEPTANCE HISTORY------------
--
-- Name: consent_of_service_user_acceptance_history; Type: TABLE; Schema: _a3s; Owner: postgres
--

CREATE TABLE _a3s.consent_of_service_user_acceptance_history (
    id uuid NOT NULL,
	permission_id uuid NOT NULL,
    user_id text NOT NULL,
	action_type smallint NOT NULL,
    action_time tstzrange DEFAULT tstzrange(CURRENT_TIMESTAMP, NULL::timestamp with time zone) NOT NULL
);


ALTER TABLE _a3s.consent_of_service_user_acceptance_history OWNER TO postgres;

--
-- Name: TABLE consent_of_service_user_acceptance_history; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON TABLE _a3s.consent_of_service_user_acceptance_history IS 'This stores the history of the acceptance of consent of service entries by users.';

--
-- Name: COLUMN consent_of_service_user_acceptance_history.permission_id; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.consent_of_service_user_acceptance_history.permission_id IS 'User action by the specific consent permission.';

--
-- Name: COLUMN consent_of_service_user_acceptance_history.user_id; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.consent_of_service_user_acceptance_history.user_id IS 'User ID accepted the specific consent permission.';

--
-- Name: COLUMN consent_of_service_user_acceptance_history.action_type; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.consent_of_service_user_acceptance_history.action_type IS 'The date and time the user accepted the specific agreement.';

--
-- Name: COLUMN consent_of_service_user_acceptance_history.action_time; Type: COMMENT; Schema: _a3s; Owner: postgres
--

COMMENT ON COLUMN _a3s.consent_of_service_user_acceptance_history.action_time IS 'The date and time the user accepted the specific agreement.';

--
-- Name: consent_of_service_user_acceptance_history terms_of_service_user_acceptance_history_pk; Type: CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.consent_of_service_user_acceptance_history
    ADD CONSTRAINT consent_of_service_user_acceptance_history_pk PRIMARY KEY (id);
	
	--
-- Name: consent_of_service_user_acceptance_history fk_terms_of_service_user_acceptance_history_user_user_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.consent_of_service_user_acceptance_history
    ADD CONSTRAINT fk_consent_of_service_user_acceptance_history_user_user_id FOREIGN KEY (user_id) REFERENCES _a3s.application_user(id) MATCH FULL;
	
--
-- Name: consent_of_service_user_acceptance_history fk_terms_of_service_user_acceptance_history_user_user_id; Type: FK CONSTRAINT; Schema: _a3s; Owner: postgres
--

ALTER TABLE ONLY _a3s.consent_of_service_user_acceptance_history
    ADD CONSTRAINT fk_consent_of_service_user_acceptance_history_perm_perm_id FOREIGN KEY (permission_id) REFERENCES _a3s.permission(id) MATCH FULL;
	

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
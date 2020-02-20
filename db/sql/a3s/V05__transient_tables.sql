--
-- *************************************************
-- Copyright (c) 2019, Grindrod Bank Limited
-- License MIT: https://opensource.org/licenses/MIT
-- **************************************************
--

-- Table: _poc.role_transient
CREATE TABLE _a3s.role_transient
(
    id uuid NOT NULL,
    role_id uuid NOT NULL,
    name text  NOT NULL,
    description text NOT NULL,
    r_state text NOT NULL,
    changed_by text NOT NULL,
    approval_count int NOT NULL,
    action text NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_role_transient PRIMARY KEY (id)
);

ALTER TABLE _a3s.role_transient
    OWNER to postgres;


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
    changed_by uuid NOT NULL,
    approval_count int NOT NULL,
    action text NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_role_transient PRIMARY KEY (id)
);

ALTER TABLE _a3s.role_transient
    OWNER to postgres;

-- Remove superflous change related columns from the role table.

ALTER TABLE _a3s.role
DROP COLUMN changed_by,
DROP COLUMN sys_period;

-- Table: _poc.role_function_transient
CREATE TABLE _a3s.role_function_transient
(
    id uuid NOT NULL,
    role_id uuid,
    function_id uuid NOT NULL,
    r_state text NOT NULL,
    changed_by text NOT NULL,
    approval_count int NOT NULL,
    action text NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_role_function_transient PRIMARY KEY (id)
);
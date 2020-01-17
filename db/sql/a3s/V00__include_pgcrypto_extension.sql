--
-- *************************************************
-- Copyright (c) 2019, Grindrod Bank Limited
-- License MIT: https://opensource.org/licenses/MIT
-- **************************************************
--

DO $$
BEGIN
  CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA public;
  COMMENT ON EXTENSION pgcrypto IS 'cryptographic functions';
EXCEPTION
  WHEN SQLSTATE '23505' THEN 
	-- do nothing, the extension is already installed
END; 
$$

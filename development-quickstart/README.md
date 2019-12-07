# Development Docker Compose Usage Instructions

This docker-compose setup is intended for use when developing the A3S platform. It includes:
* An instance of [Postgresql](https://www.postgresql.org) database.
* An instance of [Open LDAP](https://www.openldap.org). A light weight LDAP implementation.
* An instanec of [PHP LDAP admin](https://www.openldap.org). A web-based LDAP client for accessing the Open LDAP instance.

## Pre-requisites

* [Docker v17.04.0+](http://docker.com).
* [Docker compose v1.13.0+](https://docs.docker.com/compose/).
* A command-line terminal for running the required Docker compose commands.

## Running

Using a CLI tool, navigate to the following directory relative to the root of the code repository: "docker-compose/a3s-development". Run ```docker-compose up -d``` to start all the services in a detached manner.

## Postgresql

* Configured username: "postgres"
* Configured password: "postgres"

## Open LDAP

* Configured domain: "bigbaobab.org"
* Configured admin password: "admin"

**Note:** There are some comments inside the docker-compose.yml file pertainig to configuring Open LDAP with data persistence.

## PHP LDAP Admin

After the ```docker-compose up``` command is run, PHP admin LDAP will be accessible at: "http://localhost:8085". Note: You will see a warning on the login screen pertaining to an HTTP only connection. if you require HTTPS, open the docker-compose file, and comment out the "PHPLDAPADMIN_HTTPS=false" environment variable within the docker-compose.yml file. This will result in PHP LDAP Admin being available at: "https://localhost:6443". This configuration uses self signed SSL certificates which will need to be accepted within the browser on warning.

**NOTE!** Please use the Chrome browser to access PHP admin LDAP, as there have been known issues using other browsers.

To log into the Open Ldap Admin instance from PHP Admin LDAP, click the login button on the UI. Note: The PHP admin LDAP instance has already been configured to connect to the Open LDAP instance, so a host it not required.

* Login DN: "cn=admin,dc=bigbaobab,dc=org"
* Password: "admin"

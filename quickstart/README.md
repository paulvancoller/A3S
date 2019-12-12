# A3S Quickstart

The purpose of the quickstart is to get the core A3S components and optional dependencies running together as an environment using Docker Compose and to test the A3S Management APIs and Identity Server token issuing components. A Postman collection has been developed for the purpose of exploring and experimenting with the deployed APIs. Later chapters of this quickstart guide a user through the process of bringing up the environment, obtaining an access token for the bootstrap user, configuring or bootstrapping A3S via the Security Contract API, and then obtaining a permission rich access token for a user that was configured with the Security Contract. This configured user can then be used to explore the current API using the provided Postman collection.

## Pre-requisites

* [Docker v17.04.0+](http://docker.com).
* [Docker compose v1.13.0+](https://docs.docker.com/compose/).
* A command-line terminal for running the required Docker compose commands.
* [Postman](https://www.getpostman.com). An API development and testing environment. A Postman collection and a corresponding Postman environment is provided within the repository for out the box interaction with the APIs.

# Bringing up the Quickstart Environment

* Clone this Git repository onto your local machine.
  ```bash
  git clone git@github.com:GrindrodBank/A3S.git
  ```
* Navigate to the `quickstart` folder.
  ```bash
  cd A3S/quickstart
  ```

* Pull the latest version of all the images required for the docker-compose environment.
  ```bash
  docker-compose pull
  ```  
  This step is especially important if you have run this quickstart before, as it will ensure that the latest versions of the images are used within the docker-compose.
* If you have previously deployed the quickstart docker-compose, clear any potential state from the previous deployment:
  ```bash
  docker-compose down
  ```
* Deploy the environment using `docker-compose`.
  ```bash
  docker-compose up -d
  ```

* **NB!** The quickstart docker-compose will attempt to bind the various services to local ports: `80`, `8081`, `8085`, `389`, `636`, `5478`. Please ensure that no other applications are bound to these ports on the host or the `docker-compose up` command will throw errors.

* The Identity Server and A3S modules will migrate their own database schemas into the supporting Postgres instance using [Flyway](https://flywaydb.org) migrations on initialisation.

## Components of Quickstart Environment

After bringing up the quickstart environment, the following components will be deployed:

* An [Identity Server 4](http://docs.identityserver.io/en/latest/) instance (IDS4).
* An [A3S](https://github.com/GrindrodBank/A3S) API instance.
* A [Postgresql](https://www.postgresql.org) instance.
* An instance of [Open LDAP](https://www.openldap.org) for providing an LDAP authentication source.
* An instance of [PHP LDAP Admin](http://phpldapadmin.sourceforge.net), which is a browser-based UI for accessing the Open LDAP instance. Added purely for convenience.

# Configure A3S for Quickstart Using the API

Once the containers are deployed, the following components will be accessible from your local machine:

* The [Identity Server](http://docs.identityserver.io/en/latest/) will be available at `http://a3s-identity-server`. **NB!** It is very important that a host file or local DNS entry is added for the domain `a3s-identity-server`. Please refer to the 'Add Host Entry For `a3s-identity-server`' section of this documentation for more details.
* If you run the server and navigate the browser to `http://a3s-identity-server/.well-known/openid-configuration`, you should see the so-called discovery document. The discovery document is a standard endpoint in identity servers. The discovery document will be used by your clients and APIs to download the necessary configuration data.
* The A3S management API will be available at `http://localhost:8081`.
* **NB!** These applications should never be deployed into a production environment without `https`. It is done so in the quickstart context to simplify deployment and interaction with the quickstart instances.
* If direct access to the Postgres database is desired, it can be accessed using a [Postgresql](https://www.postgresql.org) client at `localhost:5478`.

## Postman Collection

A [Postman](https://www.getpostman.com) collection and corresponding quickstart environment files are available within the `A3S/postman` folder.

* `A3S.postman_collection.json` for the collection.
* `A3S-quickstart.postman_environment.json` for the environment file.

**Note**: a third file, `A3S-development-quickstart.postman_environment.json` exists in this folder, but wil **not** be used in this guide.

Import these two files into Postman. Once `A3S` the collection is imported, you should see several folders, each pertaining to a portion of the API exposed by A3S and IDS4. Select the `A3S - Quickstart` environment from the environment drop-down in Postman. This is usually located in the top right of the screen when using the Postman client.

## Add DNS Host Entry For `a3s-identity-server`

When the JWT access token validity is assessed by A3S (standard JWT validation), the issuer URL in the token must match the URL of the configured authorisation server that A3S uses. This essentially asserts that the JWT was issued by the same Authorisation server that A3S trusts. because the quickstart is using docker-compose, the A3S instance has been configured to access the identity server module using it's configured docker-compose service name as the URL, which is `http://a3s-identity-server`. Therefore, for the JWT to be considered valid by A3S, it must be acquired using the same URL from Postman. This necessitates the creation of a local DNS entry that resolves `a3s-identity-server` to `localhost` or `127.0.0.1`.

**NB!** Failure to do this and obtaining the token directly from `http://localhost` will succeed, but attempts to use that token to access resources via the A3S API will result in `401 - not authenticated` response being returned from the API.

To add a host file entry on a Unix based host (Linux and Mac OS), open the file `/etc/hosts` and add the following line:

```
127.0.0.1 a3s-identity-server
```

## Security Contracts

Security Contracts are an important aspect of the A3S. They enable idempotent configuration of almost all aspects of A3S using YAML declarations, including:

* Registering applications (resource servers or micro-services). Micro-services declare the `Permissions`, `Application Functions` (`Permissions` grouped by functionality), and `Data Policies` that they enforce.
* Registering OAuth 2.0 and OpenID Connect clients.
* Configuring default `Functions` (These are business defined groups of `Permissions`), `Roles`, `Users` and the assignment of `Functions` to `Roles` and `Roles` to `Users`.
* Configuring `Teams` and assigning `Users` or child `Teams` to them.

**Note:** Security Contracts sit primarily within the DevOps domain and will most likely not even be seen by business users of A3S. However, to introduce some core A3S concepts within the quickstart, A3S will be configured with a security contract. The `Apply Quickstart Security Contract` section of this document describes how to do this.

A3S is pre-configured with a single user called `a3s-bootstrap-admin`. This user only has permission to upload security contracts, which corresponds to the `SecurityContracts/PutSecurityContractDefinition` Postman request.

## Apply Quickstart Security Contract

Security contracts are applied using the `Security Contracts` API. 

* Obtain an access token from the API. Open the `AccessToken` folder within the Postman collection and run the `Get Access Token - Bootstrap Admin - Password Grant` request. **Note:** The body of this request has been pre-configured to fetch a token for the 'a3s-bootstrap-admin' user, which only has permission to access the security contract API.
* Verify that the access token has the following `permission` user claims section:

```x-yamls
"permission": [
    "a3s.securityContracts.read",
    "a3s.securityContracts.update"
  ]
```

To do this, visit [this JWT decoding site](https://jwt.io) and paste the contents from the returned `access_token` field in the `Get Access Token` response.
* Open the `SecurityContracts` folder within the Postman collection.
* Execute the `PutSecurityContractDefinition` request within this folder. Note: The body of this request has been pre-populated with a security contract YAML definition tailored for the quickstart. It configures A3S with a superuser called `a3s-admin`. This user has been configured with all possible A3S permissions, enabling access to all the functions within the API.

# Explore the API.

## Get an Access Token for the Newly Created Admin User

* Once the security contract has been applied, get an access token for the `a3s-admin` user that was created during that operation. To do this, open the `AccessToken` folder within the Postman collection and run the `Get Access Token - A3S Admin - Password Grant` request.
* Verify that the access token has a `permission` user claims section and that it contains a list of permissions. To do this, visit [this JWT decoding site](https://jwt.io) and paste the contents from the returned `access_token` field in the `Get Access Token` response.
* Once you have obtained an access token for the `a3s-admin` user, you have access to the entire API. 

# Cleaning Up When Done

To stop all the containers, but still keep their state, simply run:

```bash
docker-compose stop
```

To completely clean up all trace of the environment, including any possible state, run:

```bash
docker-compose down
```

within the `./docker-compose/quickstart` folder.

# Bonus - Run Integration Postman Collection

If you would like to gain further insight into the API it may be useful to investigate and/or run the Postman collection that is used for integration testing of the A3S API. This collection contains many more requests, details and nuances about the API's functionality. Additionally, there are many more examples for understanding how the various APIs relate to one another. 

The requests in this collection are designed to run, in order, from top to bottom, and give insight into how the API is practically used. Many of the environment GUIDs used in URLs or request bodies of most requests are set in the execution of earlier requests in the collection, allowing for chaining of various API calls to test complex scenarios.

## Import the Integration Postman Collection

Import the `A3S-integration.postman_collection.json` Postman collection, found within the `A3S/tests/postman-integration-tests` folder of the A3S repository. If you have not already done so, import the `A3S-quickstart.postman_environment.json` environment file, as this will be used in conjunction with the integration collection.

## Run the Integration Postman Collection

### Deploy a Clean Quickstart Environment

Firstly, deploy a clean instance of the `Quickstart Environment`. Please refer to the `Bringing up the Quickstart Environment` section of this document for details on how to do this.

**Note!** The Postman Integration tests are not idempotent and are designed to run against a completely new instance of the environment. It is very important to start from a fresh instance every time the collection is run. Therefore, run:

```bash
docker-compose down
```

within the `quickstart` folder of the cloned repository, prior to brining up a new instance of the `Quickstart Environment`. This will ensure that all state from any potential prior deployments of the `Quickstart Environment` are cleared.

### Run the Integration Collection Using Postman Runner

Postman has a feature called the `Collection Runner`, which automatically runs all requests within a Postman collection. It can be accessed by clicking the `Runner` button in Postman. 

* Once the runner is open, select `A3S - Integration` as the collection to run.
* Select `A3S - Quickstart` from the Environment drop down.
* Click `Start Run`. This should result in the entire `A3S - Integration` Postman collection being executed against the `Quickstart Environment`.
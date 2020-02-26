# Integration Guide

This document is a guide for how to protect a resource server using [A3S](https://github.com/GrindrodBank/A3S). A resource server is the OAuth 2.0 term for your API server and is referred to as an [application](./glossary.md#application) within [A3S](https://github.com/GrindrodBank/A3S). The resource server or [application](./glossary.md#application) handles authenticated requests after an [access token](./glossary.md#access-token) has been obtained from [A3S](https://github.com/GrindrodBank/A3S).

# Table of Contents

- [Protecting an API/Resource Server with A3S](#protecting-an-apiresource-server-with-a3s)
  - [Permissions Based Access Control](#permissions-based-access-control)
  - [Registering an Application with A3S](#registering-an-application-with-a3s)
  - [Defining an Application with a Security Contract](#defining-an-application-with-a-security-contract)
  - [Applying the Security Contract](#aplpying-the-security-contract)
- [Enforcing Permissions and Data Policies in Applications](#enforcing-permissions-and-data-policies-in-applications)
  - [Protecting Resources in Java Spring/boot](#protecting-resources-in-java-springboot)
  - [Protecting Resources in C#](#protecting-resources-in-c)


# Protecting an API/Resource Server with A3S

## Permissions Based Access Control

[A3S](https://github.com/GrindrodBank/A3S) implements a [permissions](./glossary.md#permission) based access control system. [Applications](./glossary.md#application) are required to enforce that the accessing [user](./glossary.md#user) has the required [permissions](./glossary.md#permission) to access the [resources](./glossary.md#resource) the [user](./glossary.md#user) requested. At the moment of checking for [permissions](./glossary.md#permission), the [application](./glossary.md#application) only needs to know "does [user](./glossary.md#user) X have [permission](./glossary.md#permission) to access [resource](./glossary.md#resource) Y?". The [application](./glossary.md#application) does not care about, and should not be aware of relationships between [roles](./glossary.md#role), [functions](./glossary.md#function) and [permissions](./glossary.md#permission). This is very important, as it allows for the creation or editing of [functions](./glossary.md#function), [roles](./glossary.md#role) and the [users](./glossary.md#user) they are assigned to within [A3S](https://github.com/GrindrodBank/A3S) without the need to make any changes to the [application](./glossary.md#application). If a conventional Role Based Access Control (RBAC) system was employed, then the declaration or changing of any [roles](./glossary.md#role) or [functions](./glossary.md#function) would require a corresponding change to the [application's](./glossary.md#application) code, most likely requiring it to be restarted or re-deployed too.

[Roles](./glossary.md#role) and [functions]((./glossary.md#function)) are a business level concerns, and model business capabilities and groups of people within an organisation. Coupling a business level concern to a technical one (adding code and re-deploying an [appplication](./glossary.md#application)) is an impediment to the organisation. Permissions based access control allows for the business and technical levels of access control to be completely decoupled and enable much more flexibility aggregating [permissions](./glossary.md#permission) into [functions](./glossary.md#function) assigned to [roles](./glossary.md#role) and [users](./glossary.md#user).

## Registering an Application with A3S

[Permissions](./glossary.md#permission), [data policies](./glossary.md#data-policy) and protected [resources](./glossary.md#resource) are only understood and ultimately enforced within the actual [application](./glossary.md#application) itself. [Applications](./glossary.md#application) are also going to change and evolve over time, sometimes many times a day (within a CI/CD environment for example), and it must be assumed that the [permissions](./glossary.md#permission) and [data policies](./glossary.md#data-policy) are going to evolve accordingly. Therefore, [A3S](https://github.com/GrindrodBank/A3S) has been designed to enable [applications](./glossary.md#application) to declare themselves, their [permissions](./glossary.md#permission) and the [data policies](./glossary.md#data-policy) they enforce to [A3S]((https://github.com/GrindrodBank/A3S)) each time they are deployed. This enables [A3S](https://github.com/GrindrodBank/A3S) to always have the latest definition of all resource servers, their permissions and data policies at any given moment.

In OpenID Connect and OAuth 2.0, a [client](./glossary.md#client) is required to obtain an [access token](./glossary.md#access-token). These [clients](./glossary.md#client) are configured to have access to one or more [applications](./glossary.md#application). This requires the [application](./glossary.md#application) itself to be modelled so that these [client](./glossary.md#client) configurations can be done.

Both of these concerns are addressed by defining an [application](./glossary.md#application) within the `applications` section of an A3S [security contract](./glossary.md#security-contract). An [application](./glossary.md#application) defines [permissions](./glossary.md#permission) and [data policies](./glossary.md#data-policy) that it enforces. The following section details the components of a [security contract](./glossary.md#security-contract) for defining an [application](./glossary.md#application) and it's [permissions](./glossary.md#permission).

## Defining an Application with a Security Contract

A [security contract](./glossary.md#security-contract) is a YAML file allowing for configuration and declaration of almost all aspects of [A3S](https://github.com/GrindrodBank/A3S). This section describes the components of the [security contract](./glossary.md#security-contract) pertaining to declaring an [application](./glossary.md#application), the [permissions](./glossary.md#permission) and the [data policies](./glossary.md#data-policy) that it enforces. For more details on the [security contract](./glossary.md#security-contract), please read the [Security Contracts](./security-contracts.md) documentation.


```yaml
applications:
  - fullname: a3s   
    applicationFunctions:
      - name: a3s.users
        description: Functionality to maintain users within the A3S User store.
        permissions:
          - name: a3s.users.read
            description: View a list of users or a single user.
          - name: a3s.users.create
            description: Create a new user. Enables assigning roles to newly created users.
          - name: a3s.users.delete
            description: Removes a user, but only from the A3S User store.
          - name: a3s.users.update
            description: Updates a user. Also enables modifying roles assigned to the user.

      - name: a3s.teams
        description: Functionality to maintain teams.
        permissions:
          - name: a3s.teams.read
            description: View list of teams. View a single team.
          - name: a3s.teams.create
            description: Create a new team. Grants ability to assign users to this team.
          - name: a3s.teams.delete
            description: Remove a team.
          - name: a3s.teams.update
            description: Update a team. Change which users are assigned to the team.

       - name: a3s.securityContracts
        description: Functionality to apply security contracts for micro-services.
        permissions:
          - name: a3s.securityContracts.read
            description: Enables fetching of a security contract definition.
          - name: a3s.securityContracts.update
            description: Enables idempotently applying (creating or updating) a security contract definition. This includes creation or updating of permissions, functions, applications and the relationships between them.


    dataPolicies:
      - name: a3s.viewYourTeamsOnly
        description: Will only return teams that the accessing user is part of when retrieving lists of teams from the API.

```
*Figure 1: Simplified YAML excerpt from the [A3S](https://github.com/GrindrodBank/A3S) quickstart reference [security contract](./glossary.md#security-contract), defining the [A3S](https://github.com/GrindrodBank/A3S) [application](./glossary.md#application).*

The components of the excerpt are detailed below:

* `applications` - The top level element for defining [applications](./glossary.md#application). It is defined as a list, as multiple [applications](./glossary.md#application) can be defined within a single [security contract](./glossary.md#security-contract).
* `fullname` - The fullname of the [application](./glossary.md#application). **Note!** Currently, the `fullname` of the [application](./glossary.md#application) is used as the scope and audience values within [JWT's](./glossary.md#jwt) issued by [A3S](https://github.com/GrindrodBank/A3S). This requires the `fullname` value to be unique, lower case, and contain no spaces. There are plans to extend the [application's](./glossary.md#application) YAML definition to separate the [application](./glossary.md#application) name into two components, an `application_identifier` (which will behave precisely as the current `fullname` attribute with regards to being mapped into JWTs) and a new `application_display_name` attribute, which will be a more human readable (and changeable) name for the [application](./glossary.md#application) or resource server.
* `applicationFunctions` - These are developer defined groups of related permissions. **Note!** The `applicationFunction` entity's only purpose is to make long lists of [permissions](./glossary.md#permission) more human readable within [security contract](./glossary.md#security-contract) definitions! They are utterly distinct from, and not to be confused with [functions](./glossary.md#function).
* `permissions`- This is the list of related [permissions](./glossary.md#permission) contained within an `applicationFunction`. **Note!** Each [permission](./glossary.md#permission) name must be unique across the entire enterprise (all [applications](./glossary.md#application)), which is why a `<application>.<applicationFunction>.<permissionName>` naming convention is highly recommended! If any [permission](./glossary.md#permission) is found to already exist within [A3S](https://github.com/GrindrodBank/A3S) (potentially defined by another [application](./glossary.md#application)), the entire [security contract](./glossary.md#security-contract) will be rejected. A `description` should be defined for each [permission](./glossary.md#permission) for the purpose of giving context to [A3S](https://github.com/GrindrodBank/A3S) users that are adminstering them.
* `dataPolicies` - Enables defining [data policies](./glossary.md#data-policy) for the [application](./glossary.md#application), which are policies that modify the behaviour of returned data that a particular resource could return. Using the `a3s.viewYourTeamsOnly` [data policy](./glossary.md#data-policy), defined in *figure 1* above, as an example. When fetching a list of teams from the [A3S](https://github.com/GrindrodBank/A3S) API, if this [data policy](./glossary.md#data-policy) is present within the [JWT](./glossary.md#jwt), then only the teams that the accessing user is a member of will be returned. The absence of the `a3s.viewYourTeamsOnly` `dataPolicy` results in all the teams (regardless of whether the user is a member of the team or not) being returned. 

**Note!** [Permissions](./glossary.md#permission) and [Data Policies](./glossary.md#data-policy) can only be understood and enforced by the [application](./glossary.md#application) that defines them. This is why they are intended to be declared by the [application](./glossary.md#application) during it's deployment phase, using the [security contract](./glossary.md#security-contract) mechanism. [Permissions](./glossary.md#permission) and [data policies](./glossary.md#data-policy) are owned by the [application](./glossary.md#application) and cannot be created by any other means for this reason.

## Applying The Security Contract

After [defining your application in a security contract](#defining-an-application-with-a-security-contract), it will need to be applied to [A3S](https://github.com/GrindrodBank/A3S). This is done using the `SecurityContract` API that [A3S](https://github.com/GrindrodBank/A3S) exposes. Please refer to the documentation for viewing the [OAS3 API definition](./view-oas3-spec.md) for details on the easiest way to view the API documentation and get details on how to use the `SecurityContract` API.

# Enforcing Permissions and Data Policies in Applications

[A3S](https://github.com/GrindrodBank/A3S) enables authorization of the [user](./glossary.md#user) in addition to the [client](./glossary.md#client) that is used to access the [resource](./glossary.md#resource). [Applications](./glossary.md#application) are responsible for enforcing the [permissions](./glossary.md#permission) and [data policies](./glossary.md#data-policy) that they declare. Every [JWT](./glossary.md#jwt) issued to a [user](./glossary.md#user) will have a 2 claims pertaining to these aspects:

* `permission` - A list of the [effective permissions](./glossary.md#effective-permission-set) that the [user](./glossary.md#user) has.
* `dataPolicy` - A list of [data policies](./glossary.md#data-policy) that the [user](./glossary.md#user) has.

Figure 2 demostrates a [JWT](./glossary.md#jwt) issued by [A3S](https://github.com/GrindrodBank/A3S) which contains these two [user claims](./glossary.md#user-claim).

```json
{
  "nbf": 1572418898,
  "exp": 1572422498,
  "iss": "http://a3s-identity-server",
  "aud": [
    "http://a3s-identity-server/resources",
    "a3s"
  ],
  "client_id": "a3s-default",
  "sub": "8eab8571-9869-4bc4-b4b7-c61a56385541",
  "auth_time": 1572418898,
  "idp": "local",
  "permission": [
    "a3s.users.read",
    "a3s.users.create",
    "a3s.users.update",
    "a3s.users.delete"
  ],
  "dataPolicy": "a3s.viewYourTeamsOnly",
  "email": "a3s-user1@localhost",
  "scope": [
    "a3s"
  ],
  "amr": [
    "pwd"
  ]
}
```
*Figure 2: A decoded JWT for a user, demonstrating the permission and dataPolicy user claims.*

In order for an [application](./glossary.md#application) to enforce [permissions](./glossary.md#permission), it must declare which [permissions](./glossary.md#permission) are required to access various components of the code (the [resource](./glossary.md#resource)). When a request is made for a [resource](./glossary.md#jwt), a [JWT](./glossary.md#jwt) must be supplied with that request. The [application](./glossary.md#application) must ensure that the [permission](./glossary.md#permission) required to access a given [resource](./glossary.md#resource) is present in the `permission` [user claim](./glossary.md#user-claim) list, within the supplied [JWT](./glossary.md#jwt).

[Data Policies](./glossary.md#data-policy) are implemented in a similar fashion. It is the responsibility of the [applicaiton](./glossary.md#application) to know which code is potentially influenced by a [data policy](./glossary.md#data-policy). The [application](./glossary.md#application) must assess the `dataPolicy` [user claim](./glossary.md#user-claim) list within the [JWT](/glossary.md#jwt) and apply data refinement accordingly for any [data policies](./glossary.md#data-policy) that are found to apply to the [resource](./glossary.md#resource) being accessed.


## Protecting Resources in Java Spring/boot

A library, containing all the required security annotations, [JWT](./glossary.md#jwt) manipulation, and [permission](./glossary.md#permission) enforcement code is currently being developed. This will be made available as a Maven package in the near future.

The classes in this library will enable any function within the Springboot application to be annotated with an annotation as follows:

```java
@PreAuthorize("@accessTokenPermissionsServiceImpl.hasPermission('permission_name')")
```
*Figure 3: An example of the @PreAuthorize annotation that can be used to assert authentication and that the user has certain permissions*

This will result in the application:

* Ensuring that the supplied [JWT](./glossary.md#jwt) is valid. Returns a 401 - Not authenticated response if an invalid (or no) [JWT](./glossary.md#jwt) was supplied with the request.
* Ensuring that the `permission_name` [permission](./glossary.md#permission) is within the supplied [JWT](./glossary.md#jwt). Returns a 403 - Forbidden, if the [JWT](./glossary.md#jwt) is valid, but the required [permission](./glossary.md#permission) is not present.

Until the library is complete, you will require the following classes inside your Java Spring/boot application to use the annotations.

* `SecurityContxctUtility.java` - A utility class for operating on [JWTs](./glossary.md#jwt) that are sent to the application.
* `AccessTokenPermissionsService.java` - An interface defining the contract for any potential implementations.
* `AccessTokenPermissionsServiceImpl.java` - An implementation of the `AccessTokenPermissionsService` interface, used for checking [permissions](./glossary.md#permission) within [JWTs](./glossary.md#jwt). This service is called when the ```@PreAuthorize``` annotations are used.

The required classes are supplied below:

```java
import java.io.IOException;
import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.context.SecurityContext;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.security.core.userdetails.UserDetails;
import org.springframework.security.jwt.Jwt;
import org.springframework.security.jwt.JwtHelper;
import org.springframework.stereotype.Component;

import com.fasterxml.jackson.core.JsonParseException;
import com.fasterxml.jackson.databind.JsonMappingException;
import com.fasterxml.jackson.databind.ObjectMapper;

@Component
public class SecurityContextUtility {

	private static final Logger LOGGER = LoggerFactory.getLogger(SecurityContextUtility.class);

	private static final String ANONYMOUS = "anonymous";

	private SecurityContextUtility() {
	}

	public static String getUserName() {
		SecurityContext securityContext = SecurityContextHolder.getContext();
		Authentication authentication = securityContext.getAuthentication();
		String username = ANONYMOUS;

		if (null != authentication) {
			if (authentication.getPrincipal() instanceof UserDetails) {
				UserDetails springSecurityUser = (UserDetails) authentication.getPrincipal();
				username = springSecurityUser.getUsername();

			} else if (authentication.getPrincipal() instanceof String) {
				username = (String) authentication.getPrincipal();

			} else {
				LOGGER.debug("User details not found in Security Context");
			}
		} else {
			LOGGER.debug("Request not authenticated, hence no user name available");
		}

		return username;
	}

	public static Set<String> getUserRoles() {
		SecurityContext securityContext = SecurityContextHolder.getContext();
		Authentication authentication = securityContext.getAuthentication();
		Set<String> roles = new HashSet<>();

		if (null != authentication) {
			authentication.getAuthorities().forEach(e -> roles.add(e.getAuthority()));
		}
		return roles;
	}

	public static Set<String> getUserAuthorities() {
		SecurityContext securityContext = SecurityContextHolder.getContext();
		Authentication authentication = securityContext.getAuthentication();
		Set<String> roles = new HashSet<>();

		if (null != authentication) {
			authentication.getAuthorities().forEach(e -> roles.add(e.getAuthority()));
		}
		return roles;
	}

	@SuppressWarnings("unchecked")
	public static Map<String, Object> getClaimsFromJwt() {
		SecurityContext securityContext = SecurityContextHolder.getContext();
		Authentication authentication = securityContext.getAuthentication();

		ObjectMapper objectMapper = new ObjectMapper();

		Map<String, Object> map = objectMapper.convertValue(authentication.getDetails(), Map.class);

		Jwt jwt = JwtHelper.decode((String) map.get("tokenValue"));

		try {
			Map<String, Object> claims = objectMapper.readValue(jwt.getClaims(), Map.class);
			LOGGER.debug("Claims VAL {}", claims);

			return claims;
		} catch (JsonParseException e) {
			LOGGER.error(e.getMessage());
		} catch (JsonMappingException e) {
			LOGGER.error(e.getMessage());
		} catch (IOException e) {
			LOGGER.error(e.getMessage());
		}

		return null;
	}

	public static String getUserIdFromJwt() {
		Map<String, Object> claims = getClaimsFromJwt();

		if (claims != null) {
			return (String) claims.get("sub");
		}

		return null;
	}
	
	@SuppressWarnings("unchecked")
	public static List<String>  getPermissionsFromJwt() {
		Map<String, Object> claims = getClaimsFromJwt();

		// The permissions can be a list, or possibly a single string if there is only one permission. Check for this!!
		if (claims != null) {
			try {
				return (List<String>) claims.get("permission");
			} catch(ClassCastException e) {
				LOGGER.warn("Could not cast the permission into a list, assuming a single string value. Attempting to create the list from this value.");
				List<String> permissions = new ArrayList<String>();
				
				try {
					String permission = (String) claims.get("permission");
				
					if(permission == null) {
						return null;
					}
				
					permissions.add(permission);
				
					return permissions;
				} catch (Exception a) {
					LOGGER.error("Exception extracting permission string from JWT token. Exception message: {}", e.getMessage());
					return null;
				}
			}
			
		}

		return null;
	}

```
*Figure 4: SecurityContxctUtility.java*

```java
public interface AccessTokenPermissionsService {
	public Boolean hasPermission(String permission);
}
```
*Figure 5: AccessTokenPermissionsService.java*

```java
import java.util.List;
import org.springframework.stereotype.Service;
import SecurityContextUtility;

@Service
public class AccessTokenPermissionsServiceImpl implements AccessTokenPermissionsService {

	@Override
	public Boolean hasPermission(String permission) {
		List<String> permissions = SecurityContextUtility.getPermissionsFromJwt();
		
		if(permissions == null) {
			return false;
		}
		
		return permissions.contains(permission);
	}
}
```
*Figure 6: AccessTokenPermissionsServiceImpl.java*

## Protecting Resources in C#

A library called a3s-security-policy-enforcement-lib, containing all the required security annotations, [JWT](./glossary.md#jwt) manipulation, and [permission](./glossary.md#permission) enforcement code is currently available as a NuGet package.

This library enables any function within a Dotnet C# application to be annotated as follows:

```csharp
[Authorize(Policy = "permission:a3s.applicationFunctions.read")]
```
*Figure 7: Example of an annotation that can be used in Dotnet C# applications to assert that a user has a given permission.*

This will result in the application:

* Ensuring that the supplied [JWT](./glossary.md#jwt) is valid (issuer, expiry, etc). Returns a 401 - Not authenticated response if an invalid (or no) [JWT](./glossary.md#jwt) was supplied with the request.
* Ensuring that the `a3s.applicationFunctions.read` [permission](./glossary.md#permission) is within the supplied [JWT](./glossary.md#jwt). Returns a 403 - Forbidden, if the [JWT](./glossary.md#jwt) is valid, but the required [permission](./glossary.md#permission) is not present.

**Note!** [A3S](https://github.com/GrindrodBank/A3S) uses these annotations to protect it's own APIs.

The following will  need to be added to the `ConfigureServices` function in `Startup.cs` of the [application](./glossary.md#application), as demonstrated in Figure 8:
 
- Registration of the policies that are referenced within the annotations. 
- A singleton lifetime service registration for the `PermissionsAuthorisationHandler`.


```csharp
public void ConfigureServices(IServiceCollection services)
{
	// Earlier code removed for brevity

	services.AddAuthorization(options =>
	{
		options.AddPolicy("permission:a3s.securityContracts.read", policy => policy.Requirements.Add(new PermissionRequirement("a3s.securityContracts.read")));
		options.AddPolicy("permission:a3s.securityContracts.update", policy => policy.Requirements.Add(new PermissionRequirement("a3s.securityContracts.update")));
		options.AddPolicy("permission:a3s.applications.read", policy => policy.Requirements.Add(new PermissionRequirement("a3s.applications.read")));
		options.AddPolicy("permission:a3s.clientRegistration.update", policy => policy.Requirements.Add(new PermissionRequirement("a3s.clientRegistration.update")));
		options.AddPolicy("permission:a3s.functions.read", policy => policy.Requirements.Add(new PermissionRequirement("a3s.functions.read")));
	});

	services.AddSingleton<IAuthorizationHandler, PermissionsAuthorisationHandler>();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
	// Earlier code removed for brevity;
	
	app.UseAuthorization();
}
```
*Figure 8: An excerpt from the A3S Startup.cs file demostrating how policies are registered.*




[![CircleCI](https://circleci.com/gh/GrindrodBank/A3S.svg?style=svg)](https://circleci.com/gh/GrindrodBank/A3S)
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FGrindrodBank%2FA3S.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FGrindrodBank%2FA3S?ref=badge_shield)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=A3S&metric=alert_status)](https://sonarcloud.io/dashboard?id=A3S)

![A3S](doc/img/a3s_logo_2.png)

*An enterprise framework that makes effortless management of centralized authorization, authentication, and accounting. A3S supports OpenID and OAuth 2.0 identity protocols.*

## Introduction

Being able to log in once and securely gain access to a variety of shared or stand-alone resources is one of the most important parts of developing an enterprise system. Additionally, effectively determining permissions to said resources is equally important. Finally, ensuring that both these concepts are implemented with the best security and engineering practices is vital to ensure your centralised users and protected resources are safe against theft and break-in attempts.

A3S makes this process simple for any project by offering all this right out-of-the-box. It can be deployed on virtually any system, and integrate with virtually any technology.

---

## Benefits of A3S

### **AAA done the right way**

A3S takes the guesswork out of authenticating your users centrally and securely, leaving you to focus on the business solution.

### **Easily understandable by business users and developers**

While other AAA systems are very developer-centric, A3S understands that the end-user will define the business capabilities that require protection. Because of this, A3S speaks a language that business users can understand. *[Learn more](./doc/permissions-over-roles.md#Introduction)*

### **Permission over roles**

Classic AAA systems limits access-control to roles and the applications need to manage the finely grained permissions. But in the real world, enterprises understand access by grouped permissions. Shouldn't your application do the same? *[Learn more](./doc/permissions-over-roles.md#The-A3S-Approach)*

---

## Top features

* A3S is on open-standards-based and API-driven, making it integratable with virtually any technology.
* A3S allows delegated access control using OAuth2.
* Because A3S is in a [Docker container](https://hub.docker.com/r/grindrodbank/a3s), it can be deployed on-premise, in-cloud, or hybrid. *[Learn more](./doc/deployment-options.md)*
* The containerization of the components enables faster delivery on development and deployment.
* Enables fine-grained permissions access.
* A3S uses the [OpenID Connect](https://openid.net/connect/) specification on top of the [OAuth 2.0](https://oauth.net/2/) protocol.
* Single-Sign-On allows your user to log in once and be authenticated across multiple applications.
* LDAP and Active Directory connectivity is supported to allow for easy integration into existing user stores. *[Learn more](./doc/ldap-setup.md)*
* A3S has centralized management for users via a friendly API.
* Two-factor authentication with Time-based One-time Password (TOTP) allows even more secure user authentication. *[Learn more](./doc/two-factor-auth.md)*
* Terms of Service functionality allows agreement-based authentication. *[Learn more](./doc/terms-of-service.md)*

---

## Composition

A3S consists of two main components:

* [Identity Server 4](https://identityserver.io), which is the main security protocol and access token engine for A3S. 
* A custom-built business layer for managing how access is given to members of the enterprise. This is currently exposed as a REST API, but there are plans to include a default React UI soon.

Each component has been designed to be independently packaged and deployed, allowing separate configuration of high availability and scaling for each component. Both components running together are collectively referred to as A3S.

---

## Quickstart

Getting started with A3S is incredibly simple. Navigate to the [quickstart](./quickstart) folder within this repository and follow the instructions in the [Readme](./quickstart/README.md) to have A3S running in minutes.

---

## Integration Guide

A3S has a detailed [integration guide](./doc/integration-guide.md) which will assist any developer to understand it's fundamental concepts and get started on implementation in a very short time.

---

## Project Documentation

All project documentation is currently available within the /doc folder.

* [Glossary of Terms](./doc/glossary.md)
* [Integration Guide](./doc/integration-guide.md)
* [Concepts](./doc/concepts.md)
* [Docker-Compose Integration Conventions](./doc/integrate-a3s-docker-compose-convention.md)
* [LDAP Setup](./doc/ldap-setup.md)
* [View OAS3 Specification](./doc/viewing-oas3-spec.md)
* [Deployment Options](./doc/deployment-options.md)
* [Contributing to A3S](./doc/contributing.md)
* [Contribution Workflow](./doc/contribution-workflow.md)
* [Coding Style Guide](./doc/coding-style.md)
* [Roadmap](./doc/roadmap.md)
* [Copyright](./doc/copyright.md)

---
&copy; Copyright 2019, Grindrod Bank Limited, and distributed under the MIT License.

## License

[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FGrindrodBank%2FA3S.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2FGrindrodBank%2FA3S?ref=badge_large)

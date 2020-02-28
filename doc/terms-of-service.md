# A3S Terms of Service Setup

Another top A3S feature is the ability to link user teams to agreements (called terms of service), which would require a user in such a team to agree to the terms before an authorization token can be issued.

Terms of service entries are optional for a team, and a user belonging to multiple teams will be presented with all the appropriate agreements after a successful login.

A user will only be presented with an agreement if they have not previously agreed to it. The moment a new version of the agreement is loaded and linked to the team, the user would need to agree to it on the next login.

## Instructions

 *Prerequisites*
 
 This guide assumes that A3S has already been set up according to the [integration guide](./doc/integration-guide.md), and that are already authenticated with a user that has the following built-in A3S [permission](./glossary.md#permission):

* `a3s.termsOfService.create`

### 1. Create a new terms of service entry

Create a new terms of service entry profile by the `CreateTermsOfService` API method:

```JSON
URL: {{a3s-host}}/termsOfService
Method: POST
Body:
{
 "agreementName": "test-agreement",
 "agreementFileData": "H4sIAB4F5V0AA+1V32/jNgy+5/wVRJ42wEuTrG2wrivgJEojzLED27muTwfFlmttjmVISoNg2P8+Sk4vbdf9eCjuMCBEAJkS+ZEfJTK9M8PVRn+SxSfN1aPIeC/T+sO7Sr/fvzw/B7uOLi/c2h+2upPBxSUMvr8YDYZWOYf+YDQc9T9A/33TeFu22jCFqTRsWz2yOpNVxdUbdmhWFP+A01KBz+v/RNYy38PvHUApZG2+23HxUJorqKXasOrHzh+dTq/kLBf1w1tma1nlrRHfNCXTQj+30mZf8SsQhlUis2Zfm+1JXkvvr/1fmk31rjH+pf+H58Nh2/+D0Wg0GNn+Px8NTv3/JeTa3vaNa9lr2+c312duaXfsdGg/ndpAVjGtf+oeJkL3eGZlIpu9snMBhv3BD3CrRJ0rmcOY1b9BIDbC8PwIdtbcdJ5Bv4Ra4qMUWgtZA46Ukiu+3sODYjVCeFAozkEWkJVMPXAPjARW76HhSqODXBsmajuwGGSYkrU0JcJoWZgdUxyNc0AeMhMM8SCX2XbDa8OMjVeIimv4xpQcusnBo/utC5JzVoGowZ49HcFOmFJuDSiujRKZxfDQKKu2bmg+HVeWfxvBurs6aQu61cjA5unBRuaisCt3tJrtuhK69CAXFnq9Nbip7WbGa+uFPM6kAs2ryiIIzNtxPWbnbGyUxhbUHErk4u5KuXnJBEtUbFWNIbnzySWWzEX8lWfG7ljzAjtE7iy1TNa5sIz01X+81hT92Vo+cke4fSu1NMinzdPeUnO8+sORLhkSXPNDVTE5vAP2jLOyOWKD1kbgBTVSuaRe16L3t0k+Peun/7BX7zqdE0iiWXrnxwRoAss4+kinZApdP0G968EdTefRKgW0iP0wvYdoBn54Dz/TcOoB+WUZkySBKAa6WAaU4B4NJ8FqSsNbGKNfGKUQ0AVNETSNXMADFCWJBVuQeDJH1R/TgKb3HsxoGlrMGYL6sPTjlE5WgR/DchUvo4Rg+CnChjScxRiFLEiY9jAq7gH5iAokcz8IXCh/hdnHLr9JtLyP6e08hXkUTAlujglm5o8D0oZCUpPApwsPpv7CvyXOK0KU2JkdsrubE7eF8Xz8TVIahZbGJArTGFUPWcbpZ9c7mhAP/JgmtiCzOEJ4W070iBwI+oWkRbGlfnkjaGL1VUKOuUyJHyBWYp2fG796A+1HO+Vw8LlR+LUn8klOcpKTfBn5E+xQqtkAEAAA",
 "autoUpdate": false
}
```

#### Body properties

|Property|Description|
|--|--|
|agreementName|A unique friendly name for your terms of service entry.|
|agreementFileData|A base64 string with the terms of service archive file binary contents|
|autoUpdate|Indicates that the newly created entry should replace any previous version links to teams.|

The `a3s.termsOfService.create` permission is required to use this end point.

#### Archive file

The terms of service archive file must conform to the following rules:

* It must be a tarball, meaning a .tar.gz file.
* It must contain a `terms_of_service.html` file, containing the wording of the agreement.
* It must contain a `terms_of_service.css` file, containing the style sheet information for the agreement.

An example file can be downloaded and inspected [here](./resources/terms_of_service.tar.gz).

*Note: For now, no other files inside the archive will be used. Images will need to be specified as base64 strings.*

To create a tarball and retrieve it's base 64 string value in *Nix:

1. In terminal, navigate into the directory containing the `terms_of_service.html` and `terms_of_service.css` files.

2. Enter the following command to create the tarball:
`tar -cvzf terms_of_service.tar.gz ./*`

3. Enter the following command to get the base64 string of the file:
`openssl base64 -in terms_of_service.tar.gz`

### 2. Record the new termsOfServiceId

A successful create in step 1 will return the following response:

```JSON
{
 "uuid": "0e0a6695-81ee-46fb-a0b6-82b8fd2d2e02",
 "agreementName": "test-agreement",
 "version": "2020.1",
 "agreementFileData": "H4sIAB4F5V0AA+1V32/jNgy+5/wVRJ42wEuTrG2wrivgJEojzLED27muTwfFlmttjmVISoNg2P8+Sk4vbdf9eCjuMCBEAJkS+ZEfJTK9M8PVRn+SxSfN1aPIeC/T+sO7Sr/fvzw/B7uOLi/c2h+2upPBxSUMvr8YDYZWOYf+YDQc9T9A/33TeFu22jCFqTRsWz2yOpNVxdUbdmhWFP+A01KBz+v/RNYy38PvHUApZG2+23HxUJorqKXasOrHzh+dTq/kLBf1w1tma1nlrRHfNCXTQj+30mZf8SsQhlUis2Zfm+1JXkvvr/1fmk31rjH+pf+H58Nh2/+D0Wg0GNn+Px8NTv3/JeTa3vaNa9lr2+c312duaXfsdGg/ndpAVjGtf+oeJkL3eGZlIpu9snMBhv3BD3CrRJ0rmcOY1b9BIDbC8PwIdtbcdJ5Bv4Ra4qMUWgtZA46Ukiu+3sODYjVCeFAozkEWkJVMPXAPjARW76HhSqODXBsmajuwGGSYkrU0JcJoWZgdUxyNc0AeMhMM8SCX2XbDa8OMjVeIimv4xpQcusnBo/utC5JzVoGowZ49HcFOmFJuDSiujRKZxfDQKKu2bmg+HVeWfxvBurs6aQu61cjA5unBRuaisCt3tJrtuhK69CAXFnq9Nbip7WbGa+uFPM6kAs2ryiIIzNtxPWbnbGyUxhbUHErk4u5KuXnJBEtUbFWNIbnzySWWzEX8lWfG7ljzAjtE7iy1TNa5sIz01X+81hT92Vo+cke4fSu1NMinzdPeUnO8+sORLhkSXPNDVTE5vAP2jLOyOWKD1kbgBTVSuaRe16L3t0k+Peun/7BX7zqdE0iiWXrnxwRoAss4+kinZApdP0G968EdTefRKgW0iP0wvYdoBn54Dz/TcOoB+WUZkySBKAa6WAaU4B4NJ8FqSsNbGKNfGKUQ0AVNETSNXMADFCWJBVuQeDJH1R/TgKb3HsxoGlrMGYL6sPTjlE5WgR/DchUvo4Rg+CnChjScxRiFLEiY9jAq7gH5iAokcz8IXCh/hdnHLr9JtLyP6e08hXkUTAlujglm5o8D0oZCUpPApwsPpv7CvyXOK0KU2JkdsrubE7eF8Xz8TVIahZbGJArTGFUPWcbpZ9c7mhAP/JgmtiCzOEJ4W070iBwI+oWkRbGlfnkjaGL1VUKOuUyJHyBWYp2fG796A+1HO+Vw8LlR+LUn8klOcpKTfBn5E+xQqtkAEAAA",
 "teamIds": [],
 "acceptedUserIds": []
}
```

Map the returned uuid into the next call's termsOfServiceId property. The `a3s.termsOfService.create` permission is required to use this end point.

### 3. Update a team with the new termsOfServiceId

Update an existing team (or create a new user) by calling the `UpdateTeam` API method:

```JSON
URL: {{a3s-host}}/teams/{{team-guid}}
Method: PUT
Body:
{
 "uuid": "{{team-guid}}",
 "description": "updated team description.",
 "name": "test-team-updated",
 "teamIds": ["{{team-guid}}"],
 "dataPolicies": ["{{application-data-policy-guid}}"],
 "termsOfServiceId": "{{termsOfServiceId}}"
}
```

The `a3s.teams.update` permission is required to use this end point.

#### Body properties

|Property|Description|
|--|--|
|uuid|The Id if the team being updated.|
|description|The description of the team.|
|name|The name of the team.|
|teamIds|An array of child teams in the case of a compound team.|
|dataPolicies|An array of data policies.|
|termsOfServiceId|The Id of the newly created terms of service entry.|

[Back to Readme](../README.md)
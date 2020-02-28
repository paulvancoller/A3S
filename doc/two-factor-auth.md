# Two-factor authentication

The ability for users to register for two-factor authentication is another top A3S feature.

## Instructions

Once a user successfully authenticates with a username and password, the next screen will give them the opportunity to register a Time-based One-time Password (TOTP) app. This can be done with apps like Google Authenticator, Authy or Microsoft Authenticator.

After scanning the displayed bar code, or entering the displayed key, the user will then enter the generated one-time pin, and click the `Validate` button, which will verify the code and confirm the device as registered.

Additionally, the user can also download as list of ten  one-time recovery codes that can be used in case the authenticator is not available.

## Two-factor authentication as a service

Another great feature is an end point made available to allow ad-hoc validation of a one-time pin. This is ideal for client screens that might require a second validation, i.e. high value transactions.

### Using the two-factor authentication validation end point

The end point details for validation one-time pins is:

```JSON
URL: {{a3s-host}}/twoFactorAuth/validate
Method: POST
Body:
{
  userId: "{{user-guid}}",
  OTP: "{{otp}}"
}
```

#### Body properties

|Property|Description|
|--|--|
|userId|The end user id to validate the Otp against.|
|OTP|The one-time pin code supplied by the end user's TOTP-based device.|

The `a3s.ldapAuthenticationModes.update` permission is required to use this end point.

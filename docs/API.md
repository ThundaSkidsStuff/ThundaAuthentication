# api reference

everything is in `Thunda.Auth`.

## ThundaClientAPI

static, works like `PlayFabClientAPI`.

### LoginWithOculus

```csharp
void LoginWithOculus(LoginWithOculusRequest request, Action<LoginResult> resultCallback, Action<ThundaError> errorCallback)
```

runs the whole flow. initialises the platform sdk if you havent, gets the user, gets a nonce, preauths, gets the integrity token, gets a second nonce and logs in.

on success it fills `ThundaSettings` and sets `PlayFabSettings.staticPlayer` so the playfab sdk is logged in too.

### CallApi

```csharp
void CallApi<TResult>(string path, object request, Action<TResult> resultCallback, Action<ThundaError> errorCallback) where TResult : ThundaResponse
```

posts to your backend with the jwt in the `Authorization` header. `request` gets serialised with `JsonUtility` so it has to be `[Serializable]`, pass null for an empty body.

errors with code 7 if youre not logged in yet.

### IsClientLoggedIn

```csharp
bool IsClientLoggedIn()
```

true if theres a jwt.

### ForgetAllCredentials

```csharp
void ForgetAllCredentials()
```

wipes the thunda session and calls `PlayFabClientAPI.ForgetAllCredentials()`. fires `OnLoggedOut`.

## ThundaSettings

| field | type | what |
|-------|------|------|
| Url | string | backend base url, no trailing slash |
| Update | string | update gate, has to match the backend |
| UseOculusUsername | bool | use the oculus name instead of the one you pass |
| Token | string | jwt, set after login |
| PlayFabId | string | set after login |
| SessionTicket | string | set after login |
| OculusId | string | set after login |
| Username | string | set after login |

## LoginWithOculusRequest

| field | type |
|-------|------|
| Username | string |

ignored if `UseOculusUsername` is on.

## LoginResult

| field | type |
|-------|------|
| PlayFabId | string |
| SessionTicket | string |
| Token | string |
| OculusId | string |
| Username | string |

## ThundaError

| field | type |
|-------|------|
| Error | ThundaErrorCode |
| Code | int |
| ErrorMessage | string |

`GenerateErrorReport()` gives you `Banned (6): you have been banned, remaining time: 3600`.

## ThundaErrorCode

`Success` 0, `UpdateRequired` 1, `PlayFabError` 2, `AuthenticationFailed` 3, `InvalidAttestation` 4, `Expired` 5, `Banned` 6, `InvalidJwt` 7, `MetaError` 100, `ConnectionError` 101, `InvalidResponse` 102.

1 to 7 come from the backend, 100+ are client side.

## ThundaResponse

base class for anything coming back from the backend. inherit it for your own routes.

| field | type |
|-------|------|
| message | string |
| code | int |

anything with a non zero code gets turned into a `ThundaError` and goes to the error callback, so if you get into your success callback the code was 0.

## ThundaEvents

```csharp
event Action<LoginResult> OnLoggedIn;
event Action<ThundaError> OnLoginFailed;
event Action OnLoggedOut;
```

fire alongside the callbacks, not instead of them.

## ThundaDebug

`Enabled` (default true), `Log`, `Warn`, `Error`. everything gets a `[Thunda]` prefix. turn it off for release builds.

## endpoints it hits

| method | path | when |
|--------|------|------|
| POST | /v1/account/authenticate/preauth | gets the attest challenge |
| POST | /v1/account/authenticate/custom?username= | the actual login |

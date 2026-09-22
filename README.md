# Thunda Auth

Unity SDK for my backend. handles meta attestation + nonce checks and logs you into playfab, all in one call. works like the playfab sdk so if you already use that it should feel the same.

made for quest games, needs the meta platform sdk and the playfab sdk in your project.

## what it does

- gets the oculus user + user proof nonce for you
- calls preauth on the backend and gets an attest challenge back
- gets the meta integrity token with that challenge
- sends everything to the backend, backend verifies it with meta and logs into playfab
- gives you back a jwt + playfab session ticket, and sets the ticket on the playfab sdk so normal PlayFabClientAPI calls just work

if someone is modding, rooted, on an old update or banned they never get a session ticket, the backend stops it before playfab.

## install

**unity package manager (git url)**

```
https://github.com/YOURNAME/ThundaSDK.git?path=/Unity/Thunda.Auth
```

**or just drag it in**

copy `Unity/Thunda.Auth` into your `Assets` folder.

you need these already in your project:
- meta / oculus platform sdk (with your app id set in the oculus platform settings)
- playfab unity sdk (with your title id set)

device integrity has to be turned on for your app in the meta developer dashboard or `GetIntegrityToken` just errors.

## setup

```csharp
ThundaSettings.Url = "https://yourbackend.com";
ThundaSettings.Update = "YourUpdate";
```

`Update` has to match whats in your backend settings, its the update gate. bump it when you push a new version and old clients get kicked with code 1.

theres also a little editor window under **Thunda > Settings** if you dont wanna set it from code.

## logging in

```csharp
using Thunda.Auth;

ThundaClientAPI.LoginWithOculus(new LoginWithOculusRequest { Username = "bob" }, OnLoggedIn, OnFailed);

void OnLoggedIn(LoginResult result)
{
    Debug.Log("logged in " + result.PlayFabId);
}

void OnFailed(ThundaError error)
{
    Debug.LogError(error.GenerateErrorReport());
}
```

after that playfab is ready to go:

```csharp
PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest(), r => { }, e => { });
```

set `ThundaSettings.UseOculusUsername = true` if you want it to just use their oculus name instead of you passing one.

## your own routes

`CallApi` sends the jwt as a bearer token, so anything on your backend that checks the jwt works with it. your result class needs to inherit `ThundaResponse` so it can read `code` and `message`.

```csharp
[Serializable]
public class GetStatsResponse : ThundaResponse
{
    public int wins;
    public int losses;
}

ThundaClientAPI.CallApi<GetStatsResponse>("/v1/stats/get", null, stats =>
{
    Debug.Log(stats.wins);
}, error => Debug.LogError(error.GenerateErrorReport()));
```

## events

if you dont wanna pass callbacks around everywhere:

```csharp
ThundaEvents.OnLoggedIn += result => Debug.Log("in as " + result.Username);
ThundaEvents.OnLoginFailed += error => Debug.LogError(error.GenerateErrorReport());
ThundaEvents.OnLoggedOut += () => Debug.Log("bye");
```

turn the logs off for release with `ThundaDebug.Enabled = false;`

## error codes

same codes the backend uses so you can just switch on it.

| code | name | what it means |
|------|------|---------------|
| 0 | Success | everything good |
| 1 | UpdateRequired | old client, `Update` didnt match |
| 2 | PlayFabError | playfab setup is wrong, your fault |
| 3 | AuthenticationFailed | nonce check failed, usually modding |
| 4 | InvalidAttestation | root, unknown apk or wrong package |
| 5 | Expired | attestation or jwt expired |
| 6 | Banned | banned, message has the remaining time |
| 7 | InvalidJwt | bad or missing jwt |
| 100 | MetaError | meta platform sdk errored |
| 101 | ConnectionError | couldnt reach the backend |
| 102 | InvalidResponse | backend sent something weird |

## notes

- login is 4 round trips (user, nonce, preauth, attest, login) so it takes a sec on device, do it on a loading screen
- it grabs a second nonce before the login call on purpose, meta nonces are one use only
- the attest data from preauth is the challenge, thats what stops someone replaying an old integrity token
- dont ship your playfab secret key in the game, its backend only. the sdk never touches it

## docs

- [getting started](docs/GettingStarted.md)
- [api reference](docs/API.md)

## license

MIT, do whatever

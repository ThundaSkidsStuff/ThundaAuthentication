# getting started

## 1. get the sdks in

install these first, thunda wont compile without them:

- **meta platform sdk** - oculus integration or the standalone platform package. set your app id in `Oculus > Platform > Edit Settings`
- **playfab unity sdk** - set your title id in `Window > PlayFab > Editor Extensions`

then drop `Unity/Thunda.Auth` into `Assets`, or add it with the git url from the readme.

## 2. turn on device integrity

in the meta developer dashboard, your app > platform services > **device integrity** (attestation). turn it on. if its off, `GetIntegrityToken` returns an error and you get code 100 every time.

also grab these while youre in there, the backend needs them:
- package name
- package cert sha256
- version code

## 3. point it at your backend

```csharp
ThundaSettings.Url = "https://yourbackend.com";
ThundaSettings.Update = "YourUpdate";
```

`Update` has to be the exact same string as `Settings.Update` on the backend. if they dont match everyone gets code 1.

do this before you call login, an awake somewhere is fine.

## 4. log in

```csharp
using Thunda.Auth;
using UnityEngine;

public class Login : MonoBehaviour
{
    void Start()
    {
        ThundaClientAPI.LoginWithOculus(new LoginWithOculusRequest { Username = "bob" }, result =>
        {
            Debug.Log("logged in " + result.PlayFabId);
        }, error =>
        {
            Debug.LogError(error.GenerateErrorReport());
        });
    }
}
```

theres a ready made version of this in the samples, import **Login Example** from the package manager.

## 5. check it worked

`ThundaClientAPI.IsClientLoggedIn()` or just open **Thunda > Settings** in the editor, it shows the playfab id and oculus id after login.

## testing in editor

attestation only works on device. in the editor `GetIntegrityToken` will fail, thats normal. build and deploy to the quest to test the real flow.

## when it breaks

**code 100 straight away** - meta platform sdk isnt set up, app id missing or the build isnt signed with the right key

**code 1** - `ThundaSettings.Update` doesnt match the backend

**code 3** - nonce failed. usually the oculus app token on the backend is wrong or the user id doesnt match the nonce

**code 4** - package name, cert or version in the backend settings dont match what you actually built

**code 101** - backend url is wrong or the server is down, check theres no trailing slash on `ThundaSettings.Url`

**code 102** - backend returned something that isnt json, usually a flask 500, check your server logs

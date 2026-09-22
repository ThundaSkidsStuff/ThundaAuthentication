using Thunda.Auth;
using UnityEngine;

public class ThundaLoginExample : MonoBehaviour
{
    public string Url = "https://yourbackend.com";
    public string Update = "YourUpdate";
    public string Username = "player";

    void Start()
    {
        ThundaSettings.Url = Url;
        ThundaSettings.Update = Update;
        ThundaClientAPI.LoginWithOculus(new LoginWithOculusRequest { Username = Username }, OnLoggedIn, OnFailed);
    }

    void OnLoggedIn(LoginResult result)
    {
        Debug.Log("logged in " + result.PlayFabId + " as " + result.Username);
    }

    void OnFailed(ThundaError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }
}

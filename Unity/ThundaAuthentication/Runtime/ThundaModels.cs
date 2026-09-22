using System;

namespace Thunda.Auth
{
    public enum ThundaErrorCode
    {
        Success = 0,
        UpdateRequired = 1,
        PlayFabError = 2,
        AuthenticationFailed = 3,
        InvalidAttestation = 4,
        Expired = 5,
        Banned = 6,
        InvalidJwt = 7,
        MetaError = 100,
        ConnectionError = 101,
        InvalidResponse = 102
    }

    public class ThundaError
    {
        public ThundaErrorCode Error;
        public int Code;
        public string ErrorMessage;

        public string GenerateErrorReport()
        {
            return Error + " (" + Code + "): " + ErrorMessage;
        }
    }

    [Serializable]
    public class LoginWithOculusRequest
    {
        public string Username;
    }

    public class LoginResult
    {
        public string PlayFabId;
        public string SessionTicket;
        public string Token;
        public string OculusId;
        public string Username;
    }

    [Serializable]
    public class ThundaResponse
    {
        public string message;
        public int code;
    }

    [Serializable]
    internal class PreAuthBody
    {
        public string nonce;
        public string userid;
    }

    [Serializable]
    internal class AppSafety
    {
        public string AttestData;
        public string AttestID;
    }

    [Serializable]
    internal class PreAuthResponse : ThundaResponse
    {
        public AppSafety appsafety;
        public double iat;
        public double exp;
    }

    [Serializable]
    internal class LoginVars
    {
        public string attestId;
        public string attestData;
        public string ClientUserAgent;
        public string nonce;
    }

    [Serializable]
    internal class LoginBody
    {
        public string id;
        public LoginVars vars;
    }

    [Serializable]
    internal class LoginResponse : ThundaResponse
    {
        public string token;
        public string PlayFabId;
        public string SessionTicket;
    }
}

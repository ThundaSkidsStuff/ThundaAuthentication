using System;
using Oculus.Platform;
using Oculus.Platform.Models;
using PlayFab;
using UnityEngine.Networking;

namespace Thunda.Auth
{
    public static class ThundaClientAPI
    {
        public static bool IsClientLoggedIn()
        {
            return !string.IsNullOrEmpty(ThundaSettings.Token);
        }

        public static void ForgetAllCredentials()
        {
            ThundaSettings.Token = null;
            ThundaSettings.PlayFabId = null;
            ThundaSettings.SessionTicket = null;
            ThundaSettings.OculusId = null;
            ThundaSettings.Username = null;
            PlayFabClientAPI.ForgetAllCredentials();
            ThundaEvents.LoggedOut();
        }

        public static void LoginWithOculus(LoginWithOculusRequest request, Action<LoginResult> resultCallback, Action<ThundaError> errorCallback)
        {
            resultCallback = WrapResult(resultCallback);
            errorCallback = WrapError(errorCallback);

            if (!Core.IsInitialized())
            {
                Core.Initialize();
            }

            Users.GetLoggedInUser().OnComplete(usermsg =>
            {
                if (usermsg.IsError)
                {
                    ThundaHttp.Fail(errorCallback, ThundaErrorCode.MetaError, 100, usermsg.GetError().Message);
                    return;
                }

                var id = usermsg.Data.ID.ToString();
                var username = ThundaSettings.UseOculusUsername ? usermsg.Data.OculusID : request?.Username;

                GetNonce(errorCallback, prenonce =>
                {
                    PreAuth(id, prenonce, errorCallback, appsafety =>
                    {
                        GetAttest(appsafety.AttestData, errorCallback, attesttoken =>
                        {
                            GetNonce(errorCallback, loginnonce =>
                            {
                                Login(id, username, appsafety.AttestID, attesttoken, loginnonce, resultCallback, errorCallback);
                            });
                        });
                    });
                });
            });
        }

        public static void CallApi<TResult>(string path, object request, Action<TResult> resultCallback, Action<ThundaError> errorCallback) where TResult : ThundaResponse
        {
            if (!IsClientLoggedIn())
            {
                ThundaHttp.Fail(errorCallback, ThundaErrorCode.InvalidJwt, 7, "not logged in");
                return;
            }
            ThundaHttp.Post(path, request, resultCallback, errorCallback);
        }

        static Action<LoginResult> WrapResult(Action<LoginResult> resultCallback)
        {
            return result =>
            {
                ThundaEvents.LoggedIn(result);
                resultCallback?.Invoke(result);
            };
        }

        static Action<ThundaError> WrapError(Action<ThundaError> errorCallback)
        {
            return error =>
            {
                ThundaEvents.LoginFailed(error);
                errorCallback?.Invoke(error);
            };
        }

        static void GetNonce(Action<ThundaError> errorCallback, Action<string> onnonce)
        {
            Users.GetUserProof().OnComplete(proofmsg =>
            {
                if (proofmsg.IsError)
                {
                    ThundaHttp.Fail(errorCallback, ThundaErrorCode.MetaError, 100, proofmsg.GetError().Message);
                    return;
                }
                onnonce(proofmsg.Data.Value);
            });
        }

        static void GetAttest(string challenge, Action<ThundaError> errorCallback, Action<string> ontoken)
        {
            DeviceApplicationIntegrity.GetIntegrityToken(challenge).OnComplete(attestmsg =>
            {
                if (attestmsg.IsError)
                {
                    ThundaHttp.Fail(errorCallback, ThundaErrorCode.InvalidAttestation, 4, attestmsg.GetError().Message);
                    return;
                }
                ontoken(attestmsg.Data);
            });
        }

        static void PreAuth(string id, string nonce, Action<ThundaError> errorCallback, Action<AppSafety> onpreauth)
        {
            var body = new PreAuthBody { nonce = nonce, userid = id };
            ThundaHttp.Post<PreAuthResponse>("/v1/account/authenticate/preauth", body, preauthjsn =>
            {
                if (preauthjsn.appsafety == null || string.IsNullOrEmpty(preauthjsn.appsafety.AttestData) || string.IsNullOrEmpty(preauthjsn.appsafety.AttestID))
                {
                    ThundaHttp.Fail(errorCallback, ThundaErrorCode.InvalidResponse, 102, "preauth failed");
                    return;
                }
                onpreauth(preauthjsn.appsafety);
            }, errorCallback);
        }

        static void Login(string id, string username, string attestid, string attesttoken, string nonce, Action<LoginResult> resultCallback, Action<ThundaError> errorCallback)
        {
            var path = "/v1/account/authenticate/custom";
            if (!string.IsNullOrEmpty(username))
            {
                path += "?username=" + UnityWebRequest.EscapeURL(username);
            }

            var body = new LoginBody
            {
                id = id,
                vars = new LoginVars
                {
                    attestId = attestid,
                    attestData = attesttoken,
                    ClientUserAgent = ThundaSettings.Update,
                    nonce = nonce
                }
            };

            ThundaHttp.Post<LoginResponse>(path, body, loginjsn =>
            {
                if (string.IsNullOrEmpty(loginjsn.token) || string.IsNullOrEmpty(loginjsn.SessionTicket))
                {
                    ThundaHttp.Fail(errorCallback, ThundaErrorCode.InvalidResponse, 102, "login failed");
                    return;
                }

                ThundaSettings.Token = loginjsn.token;
                ThundaSettings.PlayFabId = loginjsn.PlayFabId;
                ThundaSettings.SessionTicket = loginjsn.SessionTicket;
                ThundaSettings.OculusId = id;
                ThundaSettings.Username = username;

                PlayFabSettings.staticPlayer.PlayFabId = loginjsn.PlayFabId;
                PlayFabSettings.staticPlayer.ClientSessionTicket = loginjsn.SessionTicket;

                resultCallback?.Invoke(new LoginResult
                {
                    PlayFabId = loginjsn.PlayFabId,
                    SessionTicket = loginjsn.SessionTicket,
                    Token = loginjsn.token,
                    OculusId = id,
                    Username = username
                });
            }, errorCallback);
        }
    }
}

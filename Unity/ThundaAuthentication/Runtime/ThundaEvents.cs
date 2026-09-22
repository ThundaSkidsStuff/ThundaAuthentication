using System;

namespace Thunda.Auth
{
    public static class ThundaEvents
    {
        public static event Action<LoginResult> OnLoggedIn;
        public static event Action<ThundaError> OnLoginFailed;
        public static event Action OnLoggedOut;

        internal static void LoggedIn(LoginResult result)
        {
            ThundaDebug.Log("logged in " + result.PlayFabId);
            OnLoggedIn?.Invoke(result);
        }

        internal static void LoginFailed(ThundaError error)
        {
            ThundaDebug.Error(error);
            OnLoginFailed?.Invoke(error);
        }

        internal static void LoggedOut()
        {
            OnLoggedOut?.Invoke();
        }
    }
}

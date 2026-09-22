using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Thunda.Auth
{
    internal class ThundaHttp : MonoBehaviour
    {
        static ThundaHttp instance;

        static ThundaHttp Instance
        {
            get
            {
                if (instance == null)
                {
                    var thego = new GameObject("ThundaHttp");
                    thego.hideFlags = HideFlags.HideInHierarchy;
                    DontDestroyOnLoad(thego);
                    instance = thego.AddComponent<ThundaHttp>();
                }
                return instance;
            }
        }

        public static void Post<TResult>(string path, object body, Action<TResult> onsuccess, Action<ThundaError> onerror) where TResult : ThundaResponse
        {
            Instance.StartCoroutine(Instance.DoPost(path, body, onsuccess, onerror));
        }

        IEnumerator DoPost<TResult>(string path, object body, Action<TResult> onsuccess, Action<ThundaError> onerror) where TResult : ThundaResponse
        {
            var jsn = body == null ? "{}" : JsonUtility.ToJson(body);
            using (var req = new UnityWebRequest(ThundaSettings.Url + path, "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsn));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                if (!string.IsNullOrEmpty(ThundaSettings.Token))
                {
                    req.SetRequestHeader("Authorization", "Bearer " + ThundaSettings.Token);
                }
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.ConnectionError)
                {
                    Fail(onerror, ThundaErrorCode.ConnectionError, 101, req.error);
                    yield break;
                }

                var text = req.downloadHandler.text;
                if (string.IsNullOrEmpty(text))
                {
                    Fail(onerror, ThundaErrorCode.InvalidResponse, (int)req.responseCode, "empty response from server");
                    yield break;
                }

                TResult thersp;
                try
                {
                    thersp = JsonUtility.FromJson<TResult>(text);
                }
                catch (Exception)
                {
                    Fail(onerror, ThundaErrorCode.InvalidResponse, (int)req.responseCode, "invalid response from server");
                    yield break;
                }

                if (thersp == null)
                {
                    Fail(onerror, ThundaErrorCode.InvalidResponse, (int)req.responseCode, "invalid response from server");
                    yield break;
                }

                if (thersp.code != 0)
                {
                    Fail(onerror, (ThundaErrorCode)thersp.code, thersp.code, thersp.message);
                    yield break;
                }

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Fail(onerror, ThundaErrorCode.InvalidResponse, (int)req.responseCode, req.error);
                    yield break;
                }

                onsuccess?.Invoke(thersp);
            }
        }

        internal static void Fail(Action<ThundaError> onerror, ThundaErrorCode error, int code, string message)
        {
            onerror?.Invoke(new ThundaError { Error = error, Code = code, ErrorMessage = message });
        }
    }
}

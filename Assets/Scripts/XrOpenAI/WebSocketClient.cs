using NativeWebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace XrOpenAI
{
    public class WebSocketClient : MonoBehaviour
    {
        private WebSocket _webSocket;

        public event Action onOpen;
        public event Action<byte[]> onMessage;
        public event Action<WebSocketCloseCode> onClose;
        public event Action<string> onError;

        void Update()
        {
            // Process WebSocket messages on main thread
#if !UNITY_WEBGL || UNITY_EDITOR
            _webSocket?.DispatchMessageQueue();
#endif
        }
        
        public static WebSocketClient Create(Transform parent)
        {
            GameObject gameObject = new("WebSocketClient");
            gameObject.transform.SetParent(parent);
            return gameObject.AddComponent<WebSocketClient>();
        }

        public async Task<bool> Connect(string url, Dictionary<string, string> headers)
        {
            try
            {
                _webSocket = new WebSocket(url, headers);

                _webSocket.OnOpen += () => onOpen?.Invoke();
                _webSocket.OnMessage += (bytes) => onMessage?.Invoke(bytes);
                _webSocket.OnError += (errorMsg) => onError?.Invoke(errorMsg);
                _webSocket.OnClose += (closeCode) => onClose?.Invoke(closeCode);

                await _webSocket.Connect();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                onError?.Invoke($"Connection failed: {e.Message}");
                return false;
            }
        }

        public async Task SendText(string message)
        {
            try {
                if (_webSocket == null || _webSocket.State != WebSocketState.Open)
                {
                    onError?.Invoke("WebSocket is not connected.");
                    return;
                }
                await _webSocket.SendText(message);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                onError?.Invoke($"Error checking WebSocket state: {e.Message}");
                return;
            }
        }

        public async Task Close()
        {
            if (_webSocket != null)
            {
                await _webSocket.Close();
                _webSocket = null;
            }
        }

        // private void OnDestroy() => Close().GetAwaiter().GetResult();
        // private void OnApplicationQuit() => Close().GetAwaiter().GetResult();
    }
}

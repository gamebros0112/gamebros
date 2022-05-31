using System;
using System.Collections;
using System.Collections.Generic;
using BestHTTP;
using BestHTTP.JSON.LitJson;
using UnityEngine;
using BestHTTP.WebSocket;
using UnityEngine.UI;


public class WebSocketManager : MonoBehaviour
{
    [SerializeField] private Text resultText;
    [SerializeField] private InputField _inputField;
    [SerializeField] private bool socketPingLoop;
    private const string socketURI = "WSS://playverse.world:11443/";
    private WebSocket _webSocket;
    private Coroutine pingCounter;
    public Action OnSocketOpen;
    public void OnConnectedToServer()
    {
        _webSocket = new WebSocket(new Uri(socketURI));
        
        _webSocket.OnOpen += OnOpen;
        _webSocket.OnMessage += OnMessageReceived;
        _webSocket.OnClosed += OnClosed;
        _webSocket.OnError += OnError;

        // Start connecting to the server
        _webSocket.Open();
        pingCounter = StartCoroutine(PingCustom());
    }

    private IEnumerator PingCustom()
    {
        while (socketPingLoop)
        {
            yield return new WaitForSecondsRealtime(30.0f);
            _webSocket.Send("{\"direction\":\"request\",\"path\": \"/ping\"}");    
        }
        
    }
    
    public void OnCloseButton()
    {
        _webSocket.Close(1000, "Bye!");
    }

    public void OnSend()
    {
        //Debug.Log(_inputField.text);
        _webSocket.Send(_inputField.text);
    }
    public void OnSend(string data)
    {
        Debug.Log("websocket send : "+ data);
       // StopCoroutine(pingCounter);
        _webSocket.Send(data);
      //  pingCounter = StartCoroutine(PingCustom());
    }
    void AddText(string s)
    {
        if (resultText) resultText.text += "\n" + s;
        else Debug.Log("log : " + s);
    }
    
    #region WebSocket Event Handlers

    /// <summary>
    /// Called when the web socket is open, and we are ready to send and receive data
    /// </summary>
    void OnOpen(WebSocket ws)
    {
        OnSocketOpen ?.Invoke();
        AddText("WebSocket Open!");
    }

    /// <summary>
    /// Called when we received a text message from the server
    /// </summary>
    void OnMessageReceived(WebSocket ws, string message)
    {
        //OnSocketMessageReceived?.Invoke(message);
        WebSocketEventSender.GetInstance.JsonParser(message);
        //AddText(string.Format("Message received: <color=yellow>{0}</color>", message));
    }

    /// <summary>
    /// Called when the web socket closed
    /// </summary>
    void OnClosed(WebSocket ws, UInt16 code, string message)
    {
        AddText(string.Format("WebSocket closed! Code: {0} Message: {1}", code, message));
        StopCoroutine(pingCounter);
    }

    /// <summary>
    /// Called when an error occured on client side
    /// </summary>
    void OnError(WebSocket ws, string error)
    {
        AddText(string.Format("An error occured: <color=red>{0}</color>", error));
        Debug.Log("error : " + error);
    }

    #endregion
    
}

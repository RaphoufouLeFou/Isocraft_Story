using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class Chat : NetworkBehaviour
{
    // Start is called before the first frame update
    private List<GameObject> _messages;
    public GameObject ContentParent;
    public GameObject MessagePrefab;
    public GameObject ChatWindow;

    private void Start()
    {
        ChatWindow.SetActive(false);
    }

    [ClientRpc]
    private void RcpClientReceveMessage(string message)
    {
        GameObject msg = Instantiate(MessagePrefab, Vector3.zero, Quaternion.identity, ContentParent.transform);
        msg.GetComponent<TMP_Text>().text = $"Server : {message}";
        _messages.Add(msg);
    }
    
    [Command]
    private void CommandReceveMessage(string message)
    {
        GameObject msg = Instantiate(MessagePrefab, Vector3.zero, Quaternion.identity, ContentParent.transform);
        msg.GetComponent<TMP_Text>().text = $"Client : {message}";
        _messages.Add(msg);
    }
    
    public void SendMessage(string message)
    {
        if (isServer) RcpClientReceveMessage(message);
        else CommandReceveMessage(message);
    }

    public void SendMessageFromChatUI(GameObject self)
    {
        string msg = self.GetComponent<TMP_InputField>().text;
        SendMessage(msg);
    }
}

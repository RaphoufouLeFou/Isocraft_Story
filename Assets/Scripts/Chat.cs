using System.Collections.Generic;
using Mirror;
using UnityEngine;
using TMPro;

public class Chat : NetworkBehaviour
{
    // Start is called before the first frame update
    private Queue<GameObject> _messages = new ();
    
    public GameObject contentParent;
    public GameObject messagePrefab;
    public GameObject chatWindow;

    private void Start()
    {
        if (!isServer)
        {
            SendMessages("A new player joined !");
        }
        chatWindow.SetActive(false);
    }

    [ClientRpc]
    private void RcpClientReceiveMessageFromUser(string prefix ,string message)
    {
        GameObject msg = Instantiate(messagePrefab, Vector3.zero, Quaternion.identity, contentParent.transform);
        msg.GetComponent<TMP_Text>().text = $"{prefix} : {message}";
        _messages.Enqueue(msg);
    }
    
    [Command(requiresAuthority = false)]
    private void CommandReceiveMessageFromUser(string prefix, string message)
    {
        RcpClientReceiveMessageFromUser(prefix, message);
    }

    private void SendMessagesFromUser(string message)
    {
        if (isServer) RcpClientReceiveMessageFromUser("Server", message);
        else CommandReceiveMessageFromUser("Client", message);
    }
    
    [ClientRpc]
    private void RcpClientReceiveMessage(string message)
    {
        GameObject msg = Instantiate(messagePrefab, Vector3.zero, Quaternion.identity, contentParent.transform);
        msg.GetComponent<TMP_Text>().text = $"{message}";
        msg.GetComponent<TMP_Text>().color = Color.yellow;
        _messages.Enqueue(msg);
    }
    
    [Command(requiresAuthority = false)]
    private void CommandReceiveMessage( string message)
    {
        RcpClientReceiveMessage(message);
    }
    private void SendMessages(string message)
    {
        if (isServer) RcpClientReceiveMessage(message);
        else CommandReceiveMessage(message);
    }
    
    public void SendMessageFromChatUI(GameObject self)
    {
        string msg = self.GetComponent<TMP_InputField>().text;
        SendMessagesFromUser(msg);
    }
    
    

    private void Update()
    {
        if (Settings.KeyMap == null) return; // in case this runs before settings are loaded
        if (Input.GetKeyDown(Settings.KeyMap["Chat"]) && Settings.Playing)
        {
            chatWindow.SetActive(true);
            Settings.Playing = false;
        }

        if (Input.GetKeyDown(KeyCode.Return) && chatWindow.activeSelf)
            SendMessageFromChatUI(transform.GetChild(0).gameObject);
        
        while (_messages.Count > 10) Destroy(_messages.Dequeue());
    }
}

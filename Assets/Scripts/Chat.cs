using System.Collections.Generic;
using Mirror;
using UnityEngine;
using TMPro;

public class Chat : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private List<GameObject> messages = new ();
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
        messages.Add(msg);
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
        messages.Add(msg);
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

    public void ClearHistory()
    {
        foreach (GameObject msg in messages)
        {
            Destroy(msg);
        }
        messages.Clear();
    }

    private void Update()
    {
        if(Settings.KeyMap == null) return; // sometimes this execute without the parameter initiated
        if (Input.GetKeyDown(Settings.KeyMap["Chat"]))
        {
            if (!Settings.IsPaused && !chatWindow.activeSelf)
            {
                Settings.IsPaused = true;
                chatWindow.SetActive(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Settings.IsPaused && chatWindow.activeSelf)
            {
                Settings.IsPaused = false;
                chatWindow.SetActive(false);
            }
        }
    }
}

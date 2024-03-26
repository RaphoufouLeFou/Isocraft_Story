using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using TMPro;

public class Msg
{
    public GameObject Obj;
    private float _endTime;

    public Msg(GameObject obj, float lifeTime)
    {
        Obj = obj;
        _endTime = Time.time + lifeTime;
    }
    public bool IsTimesUp() => Time.time >= _endTime;

}
public class Chat : NetworkBehaviour
{
    // Start is called before the first frame update
    private Queue<Msg> _messages = new();
    private List<string> _pastMessages = new();
    private int _pastIndex;

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
        msg.GetComponentInChildren<TMP_Text>().text = $"{prefix} : {message}";
        _messages.Enqueue(new Msg(msg, 5));
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
        msg.GetComponentInChildren<TMP_Text>().text = $"{message}";
        msg.GetComponentInChildren<TMP_Text>().color = Color.yellow;
        _messages.Enqueue(new Msg(msg, 5));
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
        TMP_InputField inputField = self.GetComponent<TMP_InputField>();
        string msg = inputField.text;
        if (msg == "") return;
        if (msg.Length == msg.Count(f => f == ' ')) return;
        inputField.text = "";
        _pastIndex = -1;
        _pastMessages.Add(msg);
        inputField.Select();
        inputField.ActivateInputField();
        if (msg[0] == '/') Commands.ExecuteCommand(msg.Substring(1));  // command
        else SendMessagesFromUser(msg);
        chatWindow.SetActive(false);
    }

    private void Update()
    {
        if (Settings.KeyMap == null) return; // in case this runs before settings are loaded
        if (Input.GetKeyDown(Settings.KeyMap["Chat"]) && Settings.Playing)
        {
            chatWindow.SetActive(true);
            chatWindow.GetComponentInChildren<TMP_InputField>().Select();
            chatWindow.GetComponentInChildren<TMP_InputField>().ActivateInputField();
            Settings.Playing = false;
        }

        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && chatWindow.activeSelf)
            SendMessageFromChatUI(chatWindow.transform.GetChild(0).gameObject);

        foreach (Msg o in _messages)
        {
            if(o.IsTimesUp()) o.Obj.SetActive(chatWindow.activeSelf);
        }
        while (_messages.Count > 10) Destroy(_messages.Dequeue().Obj);

        if (chatWindow.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && _pastIndex < _pastMessages.Count)
            {
                _pastIndex++;
                if (_pastIndex == _pastMessages.Count)_pastIndex --;
                if(_pastMessages.Count != 0)
                    chatWindow.GetComponentInChildren<TMP_InputField>().text =
                        _pastMessages[_pastMessages.Count - 1 - _pastIndex];
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) && _pastIndex > 0)
            {
                chatWindow.GetComponentInChildren<TMP_InputField>().text =
                    _pastMessages[_pastMessages.Count - 1 - _pastIndex];
                if (_pastIndex - 1 >= 0 && _pastIndex - 1 < _pastMessages.Count) _pastIndex--;
            }
        }
    }
}

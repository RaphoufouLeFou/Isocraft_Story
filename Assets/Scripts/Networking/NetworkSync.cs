using Mirror;
using UnityEngine;

public class NetworkSync : NetworkBehaviour
{
    public GameObject game;
    private Game _game;

    [ClientRpc]
    private void GetName(string saveName)
    {
        //SaveInfos.SaveName = saveName;
        Debug.LogError("Name Server 2 = " + _game.SaveManager.SaveName);
    }

    [Command(requiresAuthority = false)]
    private void AskSaveName()
    {
        Debug.LogError("Name Server = " + _game.SaveManager.SaveName);
        GetName(_game.SaveManager.SaveName);
    }

    private void Start()
    {
        // sync everything
        //if (isClientOnly)
        //{
            //SaveInfos.SaveName = "foo";
        //}

        _game = game.GetComponent<Game>();
        _game.SaveManager.SaveName ??= "";

        // start the game after syncing
        _game.StartGame();
    }
}

/*using Mirror;
using UnityEngine;

public class NetworkSync : NetworkBehaviour
{
    public GameObject game;
    private Game _game;

    private void Start()
    {
        Debug.Log("NetworkSync start");
        _game = game.GetComponent<Game>();
        
        if (isClientOnly) Game.SaveManager.SaveName = "foo";
        Game.SaveManager.SaveName ??= "";

        // start the game after syncing
        //_game.StartGame();
    }
}*/
using Mirror;
using UnityEngine;

public class NeworkSync : NetworkBehaviour
{
    
    [ClientRpc]
    private void GetName(string saveName)
    {
        //SaveInfos.SaveName = saveName;
        Debug.LogError("Name Server 2 = " + SaveInfos.SaveName);
    }

    [Command(requiresAuthority = false)]
    private void AskSaveName()
    {
        Debug.LogError("Name Server = " + SaveInfos.SaveName);
        GetName(SaveInfos.SaveName);
    }

    private void Awake()
    {
        // sync everything
        //if (isClientOnly)
        //{
            //SaveInfos.SaveName = "AZERTY";
        //}
        
        //start the game after syncing
        GetComponent<Game>().SartGame();
    }
}

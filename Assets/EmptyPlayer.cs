using Mirror;

public class EmptyPlayer : NetworkBehaviour
{
    [SyncVar] public int serverConnectionId = -1;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

}

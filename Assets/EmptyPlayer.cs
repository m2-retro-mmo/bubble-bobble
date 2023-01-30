using Mirror;

public class EmptyPlayer : NetworkBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

}

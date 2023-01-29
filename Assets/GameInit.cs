using UnityEngine.SceneManagement;
using UnityEngine;

public class GameInit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var isServerBuild = false;
#if UNITY_SERVER
            isServerBuild = true;
#endif
        // if server build, start server
        if (isServerBuild)
        {
            var networkManager = FindObjectOfType<BBNetworkManager>();
            networkManager.StartServer();
            return;
        }
        // if client build, load main menu scene
        SceneManager.LoadScene("MainMenu");
    }
}

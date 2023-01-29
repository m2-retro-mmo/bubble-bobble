using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Mirror;

public class MenuUIManager : MonoBehaviour
{
    BBNetworkManager networkManager;

    UIDocument document;

    TextField address;

    Button joinButton;
    Button quitButton;
    Button serverButton;
    Button hostButton;

    Label statusText;

    void OnEnable()
    {
        networkManager = FindObjectOfType<BBNetworkManager>();
        document = GetComponent<UIDocument>();

        if (document == null)
        {
            Debug.LogError("No document found");
        }

        VisualElement root = document.rootVisualElement;

        address = root.Q("AddressField") as TextField;
        joinButton = root.Q("JoinBtn") as Button;
        quitButton = root.Q("QuitBtn") as Button;
        serverButton = root.Q("ServerBtn") as Button;
        hostButton = root.Q("HostBtn") as Button;
        statusText = root.Q("StatusText") as Label;

        // make host and server btn visible in editor or debug build
#if UNITY_EDITOR || DEBUG
        root.Q("Debug").style.display = DisplayStyle.Flex;
#endif

        // hook up events
        joinButton.clicked += joinButton_Click;
        quitButton.clicked += quitButton_Click;
        hostButton.clicked += () => networkManager.StartHost();
        serverButton.clicked += () => networkManager.StartServer();
    }

    private void joinButton_Click()
    {
        statusText.style.display = DisplayStyle.None;
        networkManager.networkAddress = address.text;
        networkManager.StartClient();
        // check if network client is active
        if (!NetworkClient.active)
        {
            statusText.text = "Failed to connect to server";
            statusText.style.display = DisplayStyle.Flex;
        }
    }

    private void quitButton_Click()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
    }
}

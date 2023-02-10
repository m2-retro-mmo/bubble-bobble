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

    VisualElement bgBubble;

    Label statusText;

    string defaultAddress = null;

    bool flip = false;

    void OnEnable()
    {

        // check if an ip address is given through a command line argument
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--address")
            {
                defaultAddress = args[i + 1];
                break;
            }
        }

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
        bgBubble = root.Q("bgBubble");

        // make host and server btn visible in editor or debug build
#if UNITY_EDITOR || DEBUG
        root.Q("Debug").style.display = DisplayStyle.Flex;
#endif

        if (defaultAddress != null)
        {
            address.style.display = DisplayStyle.None;
            address.value = defaultAddress;
        } else {
            address.value = "localhost";
        }

        // hook up events
        joinButton.clicked += joinButton_Click;
        quitButton.clicked += quitButton_Click;
        hostButton.clicked += () => networkManager.StartHost();
        serverButton.clicked += () => networkManager.StartServer();

        // start coroutine to change rotation of bgBubble every 4 seconds
        StartCoroutine(RotateBgBubble());
    }

    private System.Collections.IEnumerator RotateBgBubble()
    {
        bgBubble.style.rotate = new StyleRotate(new Rotate(new Angle(10, AngleUnit.Degree)));
        yield return new WaitForSeconds(2);
        while (true)
        {
            // get random rotation between -5 and 5
            var rotation = flip ? -10 : 10;
            flip = !flip;
            // set rotation of bgBubble
            bgBubble.style.rotate = new StyleRotate(new Rotate(new Angle(rotation, AngleUnit.Degree)));
            // wait 4 seconds
            yield return new WaitForSeconds(16);
        }
    }

    private void joinButton_Click()
    {
        statusText.style.display = DisplayStyle.None;

        if (defaultAddress != null)
        {
            networkManager.networkAddress = defaultAddress;
        }
        else
        {
            networkManager.networkAddress = address.value;
        }

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

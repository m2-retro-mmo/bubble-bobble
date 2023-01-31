using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.UI;

public class Player : CharacterBase
{
    // Movement
    private Vector2 moveInput;
    private CapsuleCollider2D col;
    private List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
    private ContactFilter2D movementFilter;
    public GameObject directionIndicator;

    public float collisionOffset = 0.1f;

    // direction indicator
    public Vector2 mousePosWorld;
    public Camera cam;
    public Vector3 mousePosScreen = new Vector3();

    private Cinemachine.CinemachineBrain cinemachineBrain;
    public float distanceFactor = 1.5f;

    public float itemDuration = 0;

    [SyncVar(hook = nameof(OnPlayerNameChanged))] private string playerName;
    public TextMeshProUGUI playerNameGUI;

    public Image hortIndicator;
    public Sprite orangeFlag;
    public Vector3 viewSpacePosition;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        if (isLocalPlayer)
        {
            cam.enabled = true;
            cam.GetComponent<Camera>().enabled = true;
            cam.GetComponent<AudioListener>().enabled = true;

            Cinemachine.CinemachineVirtualCamera cm = GameObject.Find("CineMachine").GetComponent<Cinemachine.CinemachineVirtualCamera>();
            cm.Follow = shape.transform;
            cm.m_Lens.OrthographicSize = 10;

            hortIndicator = GameObject.Find("HortIndicatorImage").GetComponent<Image>();
            UIManager uIManager = GameObject.Find("UIDocument").GetComponent<UIManager>();
            if (GetTeamNumber() == 0) 
            {
                uIManager.SetBubbleColorOrange();
                hortIndicator.sprite = orangeFlag;
            }
        }
        col = gameObject.GetComponentInChildren<CapsuleCollider2D>();
        cinemachineBrain = gameObject.GetComponentInChildren<Cinemachine.CinemachineBrain>();

        string[] layerNames = { "Player Move Collider", "Obstacles", "Ground" };
        LayerMask layermask = LayerMask.GetMask(layerNames);
        movementFilter.SetLayerMask(layermask);
        movementFilter.useLayerMask = true;

        if (isServer)
        {
            playerName = NameGenerator.GetRandomName();
            playerNameGUI.text = playerName;
        }
    }

    private void OnPlayerNameChanged(string oldName, string newName)
    {
        playerNameGUI.text = newName;
    }

    private void LookAtMouse()
    {
        Vector2 playerCenter = shape.transform.position;

        // transform mouse screen coordinates into world coordinates
        mousePosScreen.x = Input.mousePosition.x;
        mousePosScreen.y = Input.mousePosition.y;
        mousePosScreen.z = -40;
        mousePosWorld = (Vector2)cam.ScreenToWorldPoint(mousePosScreen);

        // rotate the player 
        Vector2 lookDir = playerCenter - mousePosWorld - (Vector2)cinemachineBrain.CurrentCameraState.PositionCorrection;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

        // set position of direction indicator
        directionIndicator.transform.position = (Vector2)playerCenter - lookDir.normalized * distanceFactor;
        directionIndicator.transform.rotation = Quaternion.Euler(0, 0, angle + 90f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;
        if (Input.GetButton("Fire1"))
        {
            GetComponent<Shooting>().CmdShootBubble();
        }
        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");

        // shape.transform.position = rb.transform.position;
        Rigidbody2D rb2 = GetComponent<Rigidbody2D>();
        rb2.transform.position = rb.transform.position;
        cam.transform.position = rb.transform.position;

        SetHortIndicator();
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        if (!isCaptured)
        {
            bool success = MovePlayer(moveInput);

            // playermovement was not successful check if player can move in only x or only y direction
            if (!success)
            {
                success = MovePlayer(new Vector2(moveInput.x, 0));
                if (!success)
                {
                    success = MovePlayer(new Vector2(0, moveInput.y));
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (!isLocalPlayer) return;
        LookAtMouse();
    }

    /**
    * moves the player to an givn direction
    * @direction direction the player should move to
    */
    public bool MovePlayer(Vector2 direction)
    {
        // Cast returns the number of collisions that would accour when moving n the dsired direction
        int count = col.Cast(
            direction,
            movementFilter,
            castCollisions,
            speed * Time.fixedDeltaTime + collisionOffset
        );

        if (count > 0)
        {
            // Player can't move in the disired direction without collisions
            return false;
        }

        Move(direction);
        return true;
    }

    public void SetHortIndicator()
    {
        // TODO: dont show if hort is in screenview
        // Vector3 screenpos = Camera.main.WorldToScreenPoint(GetHort().position);
        // if (screenpos.z>0 && screenpos.x>0 && screenpos.y>0 && screenpos.x<Screen.width && screenpos.y<Screen.height)
        // {
        //     hortIndicator.enabled = false;
        // }
        // else {
            viewSpacePosition = cam.WorldToViewportPoint(GetHort().position);
            viewSpacePosition.x = Mathf.Clamp01(viewSpacePosition.x);
            viewSpacePosition.y = Mathf.Clamp01(viewSpacePosition.y);
            viewSpacePosition.z = Mathf.Clamp01(viewSpacePosition.z);
            RectTransform parent =  GameObject.Find("Canvas").GetComponent<RectTransform>();
            Vector2 anchoredPosition = new Vector2(
                (viewSpacePosition.x * parent.sizeDelta.x) - (parent.sizeDelta.x * 0.5f),
                (viewSpacePosition.y * parent.sizeDelta.y) - (parent.sizeDelta.y * 0.5f)
            );
            hortIndicator.rectTransform.anchoredPosition = anchoredPosition;
        // }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMove : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float movespeed = 6f;

    [Header("PlayerObject")]

    public Rigidbody rb;
    

    private Vector3 inputMove;
    private Vector3 targetPos;
    private bool isMoving;

    public bool PlayerMoveLock = false;
    public bool PlayerRotateLock = false;

    private Quaternion lockRotation;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if(!PlayerMoveLock)
        {
            GetInput();  
        }
        else
        {
            inputMove = Vector3.zero;
        }

        if(!PlayerRotateLock)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);

            Plane ground = new Plane(Vector3.up, transform.position);

            if (ground.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                Vector3 dir = hitPoint - transform.position;
                dir.y = 0f;

                //플레이어가 보는 방향으로 회전
                if (dir.sqrMagnitude > 0.01f) // 플레이어 오브젝트 중심에 마우스 가져다댔을때 떨림 방지
                {
                    transform.rotation = Quaternion.LookRotation(dir);
                } 
            }
            else
            {
                transform.rotation = lockRotation;
                rb.angularVelocity = Vector3.zero;
            }

        
            //플레이어가 우클릭 눌렀을때 그 곳으로 이동
            /*dd
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {

                    targetPos = ray.GetPoint(distance);
                    targetPos.y = rb.position.y;
                    isMoving = true;
            }
            */
            
        }
        

    }


    void FixedUpdate()
    {
        UpdateMove();
    }


    public void UpdateMove()
    {
        Vector3 pos = rb.position + inputMove * movespeed * Time.fixedDeltaTime;
        rb.MovePosition(pos);
        if (isMoving)
        {
            Vector3 dir = targetPos - rb.position;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.05f)
            {
                isMoving = false;
                return;
            }

            dir = dir.normalized;

            Vector3 nextPos = rb.position + dir * movespeed * Time.fixedDeltaTime;
            rb.MovePosition(nextPos);
        }
    }
    public void SetRotateLock(bool isLock)
    {
        PlayerRotateLock = isLock;

        if (isLock)
        {
            lockRotation = transform.rotation;
            rb.angularVelocity = Vector3.zero;
        }
    }
    private void GetInput()
    {        
        Vector2 move = Vector2.zero;
        if(Keyboard.current.aKey.isPressed) move.x -= 1f;
        if(Keyboard.current.dKey.isPressed) move.x += 1f;
        if(Keyboard.current.sKey.isPressed) move.y -= 1f;
        if(Keyboard.current.wKey.isPressed) move.y += 1f;

        move = move.normalized;
        inputMove = new Vector3(move.x, 0f, move.y);
    }
}

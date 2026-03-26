using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Laser : MonoBehaviour
{
    [SerializeField] private float laserLength = 10f;
    [SerializeField] private float laserSpeed = 5f;
    [SerializeField] public bool Isactive = false;

    [SerializeField] private int maxBounce = 5;
    [SerializeField] private float hitOffset = 0.01f;
    [SerializeField] private bool FixedLaser = false;



    [SerializeField] private LineRenderer line;
    
    private List<Vector3> laserPoints = new List<Vector3>();

    private float laserTime;
    private bool laserEnd = false;

    public float drawTime = 0f;
    
    private ObjectCube hitCube;


    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    void OnEnable()
    {
        hitCube = null;

        StopAllCoroutines();

        Isactive = true;
        laserTime = 0f;
        laserEnd = false;

        MakeLaserPath(); // 활성화했을때 어디로 가야하는지 길(포인트)을 계산해줌

        drawTime = GetLaserFullLength() / laserSpeed;

        line.positionCount = laserPoints.Count;
        for (int i = 0; i < laserPoints.Count; i++)
        {
            line.SetPosition(i, laserPoints[0]);
        }
        //활성화됐을떄, 포지션을 세팅해서 넣어주는거까지.
  
    }

    void OnDisable()
    {
        Isactive = false;
    }

    void Update()
    {
        if (laserEnd) return;

        laserTime += Time.deltaTime * laserSpeed;
        DrawLaser(laserTime);

        if (IsLaserFull())
    {
            laserEnd = true;

            if (hitCube != null)
            {
                hitCube.LaserTrigger();
            }

            if (!FixedLaser)
            {
                StartCoroutine(HideLaser(1f));
            }
    }
    }
    private void MakeLaserPath()
    {
        laserPoints.Clear();

        Vector3 startPos = transform.position;
        Vector3 dir = transform.forward;

        laserPoints.Add(startPos);

        for (int i = 0; i < maxBounce; i++)
        {
            if (Physics.Raycast(startPos, dir, out RaycastHit hit, laserLength))
            {

                if (hit.collider.CompareTag("Mirror"))
                {
                    laserPoints.Add(hit.point);

                    dir = Vector3.Reflect(dir, hit.normal);
                    startPos = hit.point + dir * hitOffset;
                }
                else if (hit.collider.CompareTag("Wall"))
                {
                    laserPoints.Add(hit.point);
                    break;
                }
                else if (hit.collider.CompareTag("Obstacle"))
                {
                    laserPoints.Add(hit.point);
                    break;
                }
                else if (hit.collider.CompareTag("Object"))
                {
                    laserPoints.Add(hit.point);
                    GameObject hitObject = hit.collider.gameObject;
                    hitCube = hitObject.GetComponent<ObjectCube>();
                    break;
                }
                else if (hit.collider.CompareTag("LobbyTrigger"))
                {
                    laserPoints.Add(hit.point);

                    LobbyManager lobby = hit.collider.GetComponent<LobbyManager>();
                    if (lobby != null)
                    {
                        lobby.StartLobbyTrigger();
                    }

                    break;
                }
                
                else
                {
                    startPos = hit.point + dir * hitOffset;
                    i--;
                }

            }
            else
            {
                laserPoints.Add(startPos + dir * laserLength);
                break;
            }
        }
    }
    private void DrawLaser(float nowLength)
    {
        float leftLength = nowLength; // 그릴수있는 남은 길이
        int drawCount = 1; // 시작점보장
        line.positionCount = drawCount; // 라인렌더러 점 개수
        line.SetPosition(0, laserPoints[0]); // 첫점을 시작점으로 지정

        for (int i = 0; i < laserPoints.Count - 1; i++) //각 선분별 길이 검사
        {
            Vector3 a = laserPoints[i];
            Vector3 b = laserPoints[i + 1];
            float partLength = Vector3.Distance(a, b);

            if (leftLength >= partLength)
            {
                drawCount++;
                line.positionCount = drawCount;
                line.SetPosition(drawCount - 1, b);
                leftLength -= partLength;
            }
            else
            {
                Vector3 mid = Vector3.Lerp(a, b, leftLength / partLength);
                drawCount++;
                line.positionCount = drawCount;
                line.SetPosition(drawCount - 1, mid);
                break;
            }
        }

    }
    private float GetLaserFullLength() //레이저 튕기는 횟수 정해주기
    {
        float total = 0f;

        for (int i = 0; i < laserPoints.Count - 1; i++)
        {
            total += Vector3.Distance(laserPoints[i], laserPoints[i + 1]);
        }

        return total;
    }
    private bool IsLaserFull() // 너무 레이저가 오래 지속(살아있으면)
        {
            return laserTime >= GetLaserFullLength();
        }


    IEnumerator HideLaser(float delay) // 래이저 숨기는 작업.
    {
        yield return new WaitForSeconds(delay);

        gameObject.SetActive(false);
    }
    public void RefreshLaser()
    {
        StopAllCoroutines();

        laserTime = 0f;
        laserEnd = false;

        MakeLaserPath();

        line.positionCount = laserPoints.Count;
        for (int i = 0; i < laserPoints.Count; i++)
        {
            line.SetPosition(i, laserPoints[0]);
        }
    }
}

using System.Collections;
using UnityEngine;

public class Tile : MonoBehaviour
{

    [SerializeField] private float durationvalue = 2f;

    private Vector3 startPos;

    public Coroutine moveRoutine;


    void Start()
    {
        startPos = transform.localPosition;
    }
    public void MoveUp(float moveHeight = 4f)
        {
            StartMove(moveHeight);
        }

    public void MoveDown(float moveHeight = 4f)
        {
            StartMove(-moveHeight);
        }
    public void MoveForward(float moveHeight = 1f)
        {
            if (moveRoutine != null)
            {
                return;
            }
            moveRoutine = StartCoroutine(MoveTileforward(moveHeight));

        }    

    private void StartMove(float dir)
    {
        if (moveRoutine != null)
        {
            return;
        }

        moveRoutine = StartCoroutine(MoveTile(dir));
    }
    public void ResetTile()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
        gameObject.SetActive(true);

        transform.localPosition = startPos;
    }
    private IEnumerator MoveTile(float dir)
        { 
            float elapsed = 0f;
            Vector3 startPos = transform.localPosition;
            Vector3 endPos = new Vector3 (startPos.x, startPos.y + dir, startPos.z);
            while (elapsed < durationvalue)
            {
                elapsed += Time.deltaTime;
                float time = Mathf.Clamp01(elapsed / durationvalue);
                transform.localPosition = Vector3.Lerp(startPos, endPos, time);
                yield return null;
            }

            transform.localPosition = endPos;
            moveRoutine = null;
        }
    private IEnumerator MoveTileforward(float dir)
        { 
            float elapsed = 0f;
            Vector3 startPos = transform.localPosition;
            Vector3 endPos = new Vector3 (startPos.x, startPos.y, startPos.z + dir);
            while (elapsed < durationvalue)
            {
                elapsed += Time.deltaTime;
                float time = Mathf.Clamp01(elapsed / durationvalue);
                transform.localPosition = Vector3.Lerp(startPos, endPos, time);
                yield return null;
            }

            transform.localPosition = endPos;
            moveRoutine = null;
        }
    

}
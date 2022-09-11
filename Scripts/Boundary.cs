using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Boundary : MonoBehaviour
{

    public Camera mainCamera;
    BoxCollider boxCollider;

    public UnityEvent<Collider> ExitTriggerFired;

    [SerializeField]
    public float teleportOffset = 0.2f;

    private void Awake()
    {
        this.mainCamera.transform.localScale = Vector3.one;
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        ExitTriggerFired?.Invoke(other);
    }

    public void UpdateBoundSize()
    {
        float y = mainCamera.orthographicSize * 2;
        Vector2 boxColliderSize = new Vector2(y * mainCamera.aspect, y);
        boxCollider.size = boxColliderSize;
    }

    public bool outOfBounds(Vector3 pos)
    {
        return Mathf.Abs(pos.x) > Mathf.Abs(boxCollider.bounds.min.x) || Mathf.Abs(pos.y) > Mathf.Abs(boxCollider.bounds.min.y);
    }

    public Vector3 wrappedPosition(Vector3 pos)
    {
        bool xBound = Mathf.Abs(pos.x) > Mathf.Abs(boxCollider.bounds.min.x);
        bool yBound = Mathf.Abs(pos.y) > Mathf.Abs(boxCollider.bounds.min.y);

        Vector2 signVector = new Vector2(Mathf.Sign(pos.x), Mathf.Sign(pos.y));

        if (xBound && yBound)
        {
            Vector2 newPos = Vector2.Scale(pos, Vector2.one * -1) + Vector2.Scale(new Vector2(teleportOffset, teleportOffset), signVector);
            return new Vector3(newPos.x, newPos.y, 0);
        }
        else if (xBound)
        {
            Vector2 newPos = new Vector2(pos.x * -1, pos.y) + new Vector2(teleportOffset * signVector.x, teleportOffset);
            return new Vector3(newPos.x, newPos.y, 0);
        }
        else if (yBound)
        {
            Vector2 newPos = new Vector2(pos.x, pos.y * -1) + new Vector2(teleportOffset, teleportOffset * signVector.y);
            return new Vector3(newPos.x, newPos.y, 0);
        }
        else
        {
            return pos;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        transform.position = Vector3.zero;
        UpdateBoundSize();
    }
}

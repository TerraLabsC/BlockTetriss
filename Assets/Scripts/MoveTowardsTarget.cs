using UnityEngine;

public class MoveTowardsTarget : MonoBehaviour
{
    public Transform target;
    public float speed = 1.0f;

    private void Start()
    {
        if(target == null)
        {
            target = Camera.main.gameObject.GetComponent<OrientationTraile>().CurrentObjectTrigger;
        }
    }

    void Update()
    {
        if (target != null)
        {
            Vector3 newPosition = Vector3.Lerp(transform.position, target.position, speed * Time.deltaTime);
            transform.position = newPosition;
        }
    }
}

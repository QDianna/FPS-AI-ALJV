using UnityEngine;

public class ProjectileVisual : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    private Vector3 target;

    public void Init(Vector3 endPoint)
    {
        target = endPoint;
        Destroy(gameObject, 2f);
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            Destroy(gameObject);
        }
    }
}
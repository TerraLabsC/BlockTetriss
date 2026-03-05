using UnityEngine;

public class TrainingFinger : MonoBehaviour
{
   [SerializeField] private GameObject OnePolyominus;

    public float positionnx;
    public float positionny;

    void Start()
    {
        gameObject.SetActive(true);
    }

    void Update()
    {
        Vector3 vector = new Vector3(OnePolyominus.transform.position.x + positionnx, OnePolyominus.transform.position.y - positionny, 0) ;

        transform.position = vector;
    }
}

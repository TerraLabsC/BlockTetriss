using System.Collections;
using UnityEngine;

public class TriggerBonus : MonoBehaviour
{
    public GameObject Patricle;

    public GameObject HeadCate;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("TriggerBonus"))
        {
            Destroy(collision.gameObject, 2f);

            //var a = Instantiate(Patricle, transform.position, Quaternion.identity);

            //Destroy(a, 2.5f);

            StartCoroutine(HeadCateActive());
        }
    }

    public IEnumerator HeadCateActive()
    {
        HeadCate.SetActive(true);

        yield return new WaitForSeconds(3);

        HeadCate.SetActive(false);
    }
}

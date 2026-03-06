using System.Collections;
using UnityEngine;
using TMPro;

public class TimerOnGame : MonoBehaviour
{
    public TextMeshProUGUI timer;
    public GameObject Zone;
    public GameObject Blocks;
    public GameObject TainingFinger;

    void Start()
    {
        StartCoroutine(TimerGame());
    }

    public IEnumerator TimerGame()
    {
        yield return new WaitForSeconds(0.3f);
        timer.text = "3";
        yield return new WaitForSeconds(1f);
        timer.text = "2";
        yield return new WaitForSeconds(1f);
        timer.text = "1";
        yield return new WaitForSeconds(1f);
        TainingFinger.SetActive(true);
        Zone.SetActive(false);
        Blocks.SetActive(true);
        gameObject.SetActive(false);
    } 
}

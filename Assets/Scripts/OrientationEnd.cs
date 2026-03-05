using UnityEngine;
using TMPro;
using System.Collections;

public class OrientationEnd : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI TextHorizontal;
    [SerializeField] private TextMeshProUGUI TextVertical;
    
    private void Start()
    {
        Time.timeScale = 1f;
        StartCoroutine(TimerGame());
    }

    private void Update()
    {
        if (Screen.width > Screen.height)
        {
            TextHorizontal.enabled = true;
            TextVertical.enabled = false;
        }
        else
        {
            TextHorizontal.enabled = false;
            TextVertical.enabled = true;
        }
    }

    public IEnumerator TimerGame()
    {
        yield return new WaitForSeconds(0.3f);
        gameObject.GetComponent<Canvas>().enabled = true;
        gameObject.GetComponent<CanvasGroup>().alpha = 1f;
        gameObject.SetActive(false); 
    } 
}

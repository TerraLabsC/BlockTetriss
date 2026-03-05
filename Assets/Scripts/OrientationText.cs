using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class OrientationText : MonoBehaviour
{
    [SerializeField] private GameObject TextHorizontal;
    [SerializeField] private GameObject TextVertical;
    private void Update()
    {
        if (Screen.width > Screen.height)
        {
            TextHorizontal.gameObject.SetActive(true);
            TextVertical.gameObject.SetActive(false);
        }
        else
        {
            TextHorizontal.gameObject.SetActive(false);
            TextVertical.gameObject.SetActive(true);
        }
    }
}

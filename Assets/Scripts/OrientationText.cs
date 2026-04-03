using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class OrientationText : MonoBehaviour
{
    [SerializeField] private GameObject TextHorizontal;
    [SerializeField] private GameObject TextVertical;
    [SerializeField] private GameObject button;
    [SerializeField] private GameObject button2;
    private void Update()
    {
        if (Screen.width > Screen.height)
        {
            TextHorizontal.gameObject.SetActive(true);
            TextVertical.gameObject.SetActive(false);

            if(button != null)
            {
                button.gameObject.SetActive(true);
                button2.gameObject.SetActive(false);
            }

        }
        else
        {
            TextHorizontal.gameObject.SetActive(false);
            TextVertical.gameObject.SetActive(true);

            if (button != null)
            {
                button.gameObject.SetActive(false);
                button2.gameObject.SetActive(true);
            }
        }
    }
}

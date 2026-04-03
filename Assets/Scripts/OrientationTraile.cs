using UnityEngine;

public class OrientationTraile : MonoBehaviour
{
    public Transform CurrentObjectTrigger;

    public Transform TriggerHorizontal;
    public Transform TriggerVertical;

    private void Update()
    {
        if (Screen.width > Screen.height)
        {
            CurrentObjectTrigger = TriggerHorizontal;
        }
        else
        {
            CurrentObjectTrigger = TriggerVertical;
        }
    }
}

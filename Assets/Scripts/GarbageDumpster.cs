using UnityEngine;

public class Dumpster : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pickable") && other.transform.parent == null)
        {
            if (GarbageManager.Instance != null)
            {
                // Only collect if the object is not already collected
                PickableItem pickableItem = other.GetComponent<PickableItem>();
                if (pickableItem != null && !pickableItem.isCollected)
                {
                    pickableItem.isCollected = true;
                    GarbageManager.Instance.CollectGarbage();
                    Destroy(other.gameObject);
                }
            }
        }
    }
}

using UnityEngine;

public abstract class Pickup : MonoBehaviour
{

    public LayerMask applicableLayers;
    protected bool IsPickedUp { get; private set; }

    // OnTriggerEnter is called when the Collider other enters the trigger
    private void OnTriggerEnter(Collider other)
    {
        if (IsPickedUp)
        {
            return;
        }

        if (CanColliderPickUp(other.gameObject.layer))
        {
            OnPickup(other.gameObject);
            IsPickedUp = true;
        }
    }

    protected bool CanColliderPickUp(int layer)
    {
        return applicableLayers == (applicableLayers | (1 << layer));
    }

    protected abstract void OnPickup(GameObject picker);
}

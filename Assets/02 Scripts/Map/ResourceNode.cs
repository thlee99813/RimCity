using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    public ResourceType Type;
    public int Amount = 5;

    public void Consume(int value)
    {
        if (value <= 0) return;

        Amount -= value;
        if (Amount <= 0)
        {
            Amount = 0;

            TileNode tile = GetComponentInParent<TileNode>();
            if (tile != null)
                tile.ClearResource();

            Destroy(gameObject);
        }
    }
}

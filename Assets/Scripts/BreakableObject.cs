using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    private bool isBroken = false;
    public GameObject brokenPrefab;

    private void Start()
    {
        // Register this object with the GameManager at start
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterBreakable(this);
    }

    private void OnDestroy()
    {
        // If the object is destroyed and the game is still active, count it as broken
        if (!isBroken && GameManager.Instance != null && GameManager.Instance.IsGameActive())
        {
            isBroken = true; // prevent double counting if OnDestroy called twice (unlikely)
            GameManager.Instance.ObjectBroken(this);
        }
    }

    // Optional: If you ever want to break it via script (not just from ArmMovement), you can call this
    //public void Break()
    //{
    //    if (!isBroken)
    //    {
    //        isBroken = true;
    //        Destroy(gameObject);
    //    }
    //}
    public void Break()
    {
        if (!isBroken)
        {
            isBroken = true;

            if (brokenPrefab != null)
                Instantiate(brokenPrefab, transform.position, transform.rotation);

            Destroy(gameObject);
        }
    }
}
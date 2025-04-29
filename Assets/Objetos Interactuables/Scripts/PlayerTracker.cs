using UnityEngine;

public class PlayerTracker : MonoBehaviour
{
    public static Transform Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this.transform;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

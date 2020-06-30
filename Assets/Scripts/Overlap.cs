using UnityEngine;

public class Overlap : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // if (collision.gameObject != carPrefab)
        CreateTrack trackScript = FindObjectOfType<CreateTrack>();
        Debug.Assert(trackScript != null, "CreateTrack Script not found");
        trackScript.CollisionDetected();
    }
}

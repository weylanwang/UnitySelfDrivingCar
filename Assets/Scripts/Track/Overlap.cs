using UnityEngine;

public class Overlap : MonoBehaviour {
    // Determines if the track pieces overlap with one another
    private void OnCollisionEnter2D(Collision2D collision) {
        CreateTrack trackScript = FindObjectOfType<CreateTrack>();
        Debug.Assert(trackScript != null, "CreateTrack Script not found");
        trackScript.CollisionDetected();
    }
}

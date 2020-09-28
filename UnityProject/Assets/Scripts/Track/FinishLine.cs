using UnityEngine;

public class FinishLine : MonoBehaviour {
    private int points = 25;

    #region Finish Trigger
    // Rewards the agent for hitting the finish line
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Wall"))
            return;
        CarDriving carDriving = collision.GetComponent<CarDriving>();
        carDriving.AwardPoints(points);
        if (points > 5)
            points -= 5;

        // Trigger a fake collision to terminate this agent
        carDriving.FakeCollision();
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour {
    private int points = 25;

    #region Finish Trigger
    // Rewards the agent for hitting the finish line
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Wall"))
            return;
        collision.GetComponent<CarDriving>().AwardPoints(points);
        if (points > 5)
            points -= 5;
    }
    #endregion
}

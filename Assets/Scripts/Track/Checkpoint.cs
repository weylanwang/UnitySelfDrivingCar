using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class Checkpoint : MonoBehaviour
{
    #region Checkpoint Triggers
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
            return;
        collision.GetComponent<CarDriving>().SetLastCheckpoint(System.Convert.ToInt32(Regex.Match(this.transform.name, @"\(([^)]*)\)").Groups[1].Value));
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Track", menuName = "TrackPiece")]
public class TrackPiece : ScriptableObject
{
    // The prefab
    public GameObject trackPrefab;
    // The distance between the start of this trackPiece and the next
    public Vector2 shift;
    // The orientation shift of the next piece
    public float orientation;
    // The probability of this piece being selected
    [Range(1,5)]
    public float frequency = 1f;
}

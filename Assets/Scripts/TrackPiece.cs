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
    public Vector2 orientation;
}

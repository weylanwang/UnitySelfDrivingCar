using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateTrack : MonoBehaviour
{
    // Start and End Pieces
    public TrackPiece start;
    public TrackPiece end;

    // Number of pieces in-between the start and end piece.
    public uint trackSize;

    // List of usable pieces to create the track
    public TrackPiece[] trackPieces;

    public GameObject[] track;

    private void Start()
    {
        // Check that valid pieces are provided
        CheckPieces();

        // Begin Track Creation
        StartCreation();
    }

    private void CheckPieces()
    {
        // Check the Start and End pieces
        Debug.Assert(start != null, "Start piece is null");
        Debug.Assert(end != null, "End piece is null");

        // Check that TrackPiece has valid pieces and that start and end are valid
        Debug.Assert(trackPieces.Length > 0, "TrackPieces array has size 0");
        List<TrackPiece> allPieces = new List<TrackPiece>();
        allPieces.Add(start);
        allPieces.AddRange(trackPieces);
        allPieces.Add(end);
        int i = 0;
        foreach (TrackPiece piece in allPieces)
        {
            Debug.Assert(piece != null, "TrackPiece with index number " + i + " is null");
            Debug.Assert(piece.trackPrefab != null, piece.name + "'s prefab is null");
            Debug.Assert(piece.shift != Vector2.zero, piece.name + "'s prefab's length is (0, 0)");
            Debug.Assert(piece.orientation != Vector2.zero, piece.name + "'s prefab's direction is (0, 0)");
            i++;
        }
    }

    private void StartCreation()
    {
        System.Random rand = new System.Random();

        // Add 2 more spaces to the track for the start and end pieces
        track = new GameObject[trackSize + 2];

        // Create the Starting Piece
        Vector2 trackPosition = start.shift;
        Vector2 orientation = start.orientation.normalized;
        track[0] = Instantiate(start.trackPrefab, Vector3.zero, Quaternion.Euler(orientation));

        // Create the In-Between Pieces
        for (int i = 1; i <= trackSize; i++)
        {
            TrackPiece random_piece = trackPieces[rand.Next(0, trackPieces.Length)];
            track[i] = Instantiate(random_piece.trackPrefab, trackPosition, Quaternion.Euler(start.orientation));
            trackPosition += random_piece.shift;
            orientation = (orientation + random_piece.orientation).normalized;
        }

        track[trackSize + 1] = Instantiate(end.trackPrefab, trackPosition, Quaternion.Euler(orientation));
    }
}

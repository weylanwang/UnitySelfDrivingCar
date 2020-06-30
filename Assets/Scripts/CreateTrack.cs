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

    // To use probabilities or not
    public bool useProbability;

    // List of usable pieces to create the track
    public TrackPiece[] trackPieces;

    public GameObject[] track;

    private void Start()
    {
        // Check that valid pieces are provided
        CheckPieces();

        // Begin Track Creation
        StartCreation();

        // Check for overlaps        
        Debug.Log(CheckOverlap());

        //for (int i = 1; i != 10; i++)
        //    Invoke("RemakeTrack", 5f * i);
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
            i++;
        }
    }

    private void StartCreation()
    {
        System.Random rand = new System.Random();

        // Pool of Track Piece Index Numbers
        List<int> indexPool = new List<int>();
        for (int i = 0; i != trackPieces.Length; i++)
        {
            if (useProbability)
                for (int k = 0; k != trackPieces[i].frequency; k++)
                    indexPool.Add(i);
            else
                indexPool.Add(i);
        }

        // Add 2 more spaces to the track for the start and end pieces
        track = new GameObject[trackSize + 2];

        // Create the Starting Piece
        track[0] = Instantiate(start.trackPrefab, Vector3.zero, Quaternion.Euler(Vector3.zero));
        Quaternion orientation = Quaternion.Euler(Vector3.zero);
        Vector3 trackPosition = start.shift;

        // Create the In-Between Pieces
        for (int i = 1; i <= trackSize; i++)
        {
            TrackPiece random_piece = trackPieces[indexPool[rand.Next(0, indexPool.Count)]];
            track[i] = Instantiate(random_piece.trackPrefab, trackPosition, orientation);
            trackPosition += orientation * new Vector3(random_piece.shift.x, random_piece.shift.y, 0);
            orientation *= Quaternion.Euler(new Vector3(0, 0, random_piece.orientation)).normalized;
        }

        track[trackSize + 1] = Instantiate(end.trackPrefab, trackPosition, orientation);
    }

    private bool CheckOverlap()
    {
        for (int i = 0; i != track.Length; i++)
            for (int k = i + 1; k != track.Length; k++)
                if ((track[i].transform.position - track[k].transform.position).sqrMagnitude < 8)
                    return true;
        return false;
    }

    private void DestroyTrack()
    {
        foreach (GameObject piece in track)
            Destroy(piece);
    }

    private void RemakeTrack()
    {
        DestroyTrack();
        StartCreation();
        Debug.Log(CheckOverlap());
    }
}

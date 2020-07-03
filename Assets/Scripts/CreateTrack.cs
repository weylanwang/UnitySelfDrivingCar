﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateTrack : MonoBehaviour
{
    // Start and End Pieces
    public TrackPiece start;
    public TrackPiece end;

    // Number of pieces in-between the start and end piece.
    public uint trackSize;

    // Maximum attempts to create a non-intersecting track
    public uint maxAttempts = 8;

    // To use probabilities or not
    public bool useProbability;

    // List of usable pieces to create the track
    public TrackPiece[] trackPieces;

    // All Instantiated GameObjects comprising the track
    public GameObject[] track;

    // Bool tracking track collisions with itself
    private uint collisionCount = 0;

<<<<<<< HEAD
    // Increase collisionCount when a collider collision is detected
=======
>>>>>>> 070f3875845edff7013e32421fdbf88e86e2a94b
    public void CollisionDetected()
    {
        collisionCount++;
    }

    private void Start()
    {
        // Check that valid pieces are provided
        CheckPieces();

        // Begin Track Creation
        StartCoroutine("Create");
    }

<<<<<<< HEAD
    // Check if given pieces are valid
=======
>>>>>>> 070f3875845edff7013e32421fdbf88e86e2a94b
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

<<<<<<< HEAD
    // Called by Create
=======
>>>>>>> 070f3875845edff7013e32421fdbf88e86e2a94b
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
<<<<<<< HEAD
        ChangeVisible(track[0], false);
=======
>>>>>>> 070f3875845edff7013e32421fdbf88e86e2a94b
        Quaternion orientation = Quaternion.Euler(Vector3.zero);
        Vector3 trackPosition = start.shift;

        // Create the In-Between Pieces
        for (int i = 1; i <= trackSize; i++)
        {
            TrackPiece random_piece = trackPieces[indexPool[rand.Next(0, indexPool.Count)]];
            track[i] = Instantiate(random_piece.trackPrefab, trackPosition, orientation);
<<<<<<< HEAD
            ChangeVisible(track[i], false);
=======
>>>>>>> 070f3875845edff7013e32421fdbf88e86e2a94b
            trackPosition += orientation * new Vector3(random_piece.shift.x, random_piece.shift.y, 0);
            orientation *= Quaternion.Euler(new Vector3(0, 0, random_piece.orientation)).normalized;
        }

        track[trackSize + 1] = Instantiate(end.trackPrefab, trackPosition, orientation);
<<<<<<< HEAD
        ChangeVisible(track[trackSize + 1], false);
    }

    // Creates the Track and Checks for intersections. Destroys the track and repeats if intersection found
=======
    }

>>>>>>> 070f3875845edff7013e32421fdbf88e86e2a94b
    private IEnumerator Create()
    {
        uint currentAttempt = 0;
        uint expectedCollisionCount = (trackSize + 1) * 4;
        while (collisionCount != expectedCollisionCount)
        {
            DestroyTrack();
            collisionCount = 0;
            StartCreation();
            yield return new WaitForSeconds(0);
            if (++currentAttempt >= maxAttempts)
                break;
        }

        Debug.Assert(collisionCount == expectedCollisionCount, "Failed to Create a Track that doesn't intersect itself");
        Debug.Log(currentAttempt + " attempts");
<<<<<<< HEAD

        // Set entire track visible
        foreach (GameObject piece in track)
            ChangeVisible(piece, true);
    }

    // Destroys the Entire Track
=======
    }

>>>>>>> 070f3875845edff7013e32421fdbf88e86e2a94b
    private void DestroyTrack()
    {
        foreach (GameObject piece in track)
            if (piece != null)
                Destroy(piece);
    }
<<<<<<< HEAD

    // Set the visibility of a track piece gameObject
    private void ChangeVisible(GameObject piece, bool visible)
    {
        foreach (Renderer render in piece.transform.GetComponentsInChildren<Renderer>())
            render.enabled = visible;
    }
=======
>>>>>>> 070f3875845edff7013e32421fdbf88e86e2a94b
}

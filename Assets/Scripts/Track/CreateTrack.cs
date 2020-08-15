using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateTrack : MonoBehaviour
{
    #region Variables
    // Start and End Pieces
    [SerializeField]
    [Tooltip("Starting piece Prefab")]
    private TrackPiece start;
    [SerializeField]
    [Tooltip("Ending piece Prefab")]
    private TrackPiece end;

    // Number of pieces in-between the start and end piece.
    [SerializeField]
    [Tooltip("Number of pieces in-between the start and end piece")]
    private uint trackSize = 10;

    // Maximum attempts to create a non-intersecting track
    [SerializeField]
    [Tooltip("Maximum attempts to create a non-intersecting track")]
    private uint maxAttempts = 8;

    // To use probabilities or not
    [SerializeField]
    [Tooltip("True factors in piece frequency when selecting track pieces")]
    private bool useProbability = true;

    // List of usable pieces to create the track
    [SerializeField]
    [Tooltip("List of usable pieces to create the track")]
    private TrackPiece[] trackPieces;

    // All Instantiated GameObjects comprising the track
    public GameObject[] track
    {   get; private set;   }

    // Bool tracking track collisions with itself
    private uint collisionCount = 0;
    #endregion

    #region Start
    private void Start()
    {
        // Check that valid pieces are provided
        CheckPieces();

        // Begin Track Creation
        StartCoroutine("Create");
    }
    #endregion

    #region Public Functions
    // Increase collisionCount when a collider collision is detected
    public void CollisionDetected()
    {
        collisionCount++;
    }
    #endregion

    #region Private Functions
    // Check if given pieces are valid
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

    // Called by Create
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
        ChangeVisible(track[0], false);
        Quaternion orientation = Quaternion.Euler(Vector3.zero);
        Vector3 trackPosition = start.shift;

        // Create the In-Between Pieces
        for (int i = 1; i <= trackSize; i++)
        {
            TrackPiece random_piece = trackPieces[indexPool[rand.Next(0, indexPool.Count)]];
            track[i] = Instantiate(random_piece.trackPrefab, trackPosition, orientation);
            ChangeVisible(track[i], false);
            trackPosition += orientation * new Vector3(random_piece.shift.x, random_piece.shift.y, 0);
            orientation *= Quaternion.Euler(new Vector3(0, 0, random_piece.orientation)).normalized;
        }

        track[trackSize + 1] = Instantiate(end.trackPrefab, trackPosition, orientation);
        ChangeVisible(track[trackSize + 1], false);
    }

    // Creates the Track and Checks for intersections. Destroys the track and repeats if intersection found
    private IEnumerator Create()
    {
        if (trackSize == 0)
        {
            StartCreation();
            foreach (GameObject piece in track)
                ChangeVisible(piece, true);
            yield break;
        }

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

        // Set entire track visible
        foreach (GameObject piece in track)
            ChangeVisible(piece, true);
    }

    // Destroys the Entire Track
    private void DestroyTrack()
    {
        if (track == null)
            return;
        foreach (GameObject piece in track)
            if (piece != null)
                Destroy(piece);
    }

    // Set the visibility of a track piece gameObject
    private void ChangeVisible(GameObject piece, bool visible)
    {
        foreach (Renderer render in piece.transform.GetComponentsInChildren<Renderer>())
            render.enabled = visible;
    }
    #endregion
}

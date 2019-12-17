using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private GameObject root;
    private List<GameObject> cells;
    private Dictionary<int, List<GameObject>> pieces;

    /// <summary>
    /// Pools all objects.
    /// </summary>
    /// <param name="numCells">Number of cells to create.</param>
    /// <param name="cellPrefab">Cell prefab to instantiate from.</param>
    /// <param name="numPlayerPieces">Number of pieces per player to create.</param>
    /// <param name="players">Array of all players.</param>
    public void Create(int numCells, GameObject cellPrefab, int numPlayerPieces, Player[] players)
    {
        // Root board object
        root = new GameObject("Game Board");

        // Pool tiles
        cells = new List<GameObject>(numCells);
        for (int i = 0; i < numCells; i++)
        {
            GameObject obj = Instantiate(cellPrefab, Vector3.zero, Quaternion.identity);
            obj.transform.parent = root.transform;
            obj.name = $"{cellPrefab.name} (P{i + 1})";
            obj.SetActive(false);

            cells.Add(obj);
        }

        // Pool pieces for each player
        pieces = new Dictionary<int, List<GameObject>>(numPlayerPieces * players.Length);
        for (int i = 0; i < players.Length; i++)
        {
            pieces[i] = new List<GameObject>();
            
            for (int j = 0; j < numPlayerPieces; j++)
            {
                Player player = players[i];
                GameObject obj = Instantiate(player.PiecePrefab, Vector3.zero, Quaternion.identity);
                obj.transform.parent = root.transform;
                obj.name = $"{player.PiecePrefab.name} (P{j + 1})";
                obj.SetActive(false);

                MeshRenderer renderer = obj.GetComponentInChildren<MeshRenderer>();
                renderer?.material.SetColor("_EmissionColor", player.Color);

                pieces[i].Add(obj);
            }
        }
    }

    /// <summary>
    /// Resets the pool, disabling all active objects.
    /// </summary>
    public void Reset()
    {
        // If the pool hasn't been created yet
        if (pieces == null)
            return;

        // Disable cells
        foreach (GameObject cell in cells)
            cell.SetActive(false);

        // Disable pieces
        foreach (var pool in pieces.Values)
            foreach (var piece in pool)
                piece.SetActive(false);
    }

    /// <summary>
    /// Gets an inactive pooled piece for player index and enables it.
    /// </summary>
    public GameObject GetPiece(int playerIndex)
    {
        if (!pieces.ContainsKey(playerIndex))
            return null;

        for (int i = 0; i < pieces[playerIndex].Count; i++)
        {
            GameObject piece = pieces[playerIndex][i];

            if (!piece.activeSelf)
            {
                // Enable the piece and return
                piece.SetActive(true);
                return piece;
            }
        }

        Debug.Assert(false, "We've run out of pieces, this shouldn't happen!");

        return null;
    }

    /// <summary>
    /// Gets an inactive cell and enables it.
    /// </summary>
    public GameObject GetCell()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            if (!cells[i].activeSelf)
            {
                cells[i].SetActive(true);
                return cells[i];
            }
        }

        Debug.Assert(false, "We've run out of cells, this shouldn't happen!");

        return null;
    }
}
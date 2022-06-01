using UnityEngine;
using UnityEngine.Tilemaps;

public class Holder : MonoBehaviour
{
    public TetrominoData? heldPiece { get; private set; }
    private Tilemap tilemap;

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
    }

    public void SetHeldPiece(TetrominoData data)
    {
        if (heldPiece.HasValue)
        {
            Utilities.SetCells(tilemap, heldPiece.Value.cells, null);
        }

        heldPiece = data;

        if (heldPiece.HasValue)
        {
            Utilities.SetCells(tilemap, heldPiece.Value.cells, heldPiece.Value.tile);
        }
    }
}

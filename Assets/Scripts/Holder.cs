using UnityEngine;
using UnityEngine.Tilemaps;

public class Holder : MonoBehaviour
{
    public TetrominoData heldPiece { get; private set; }
    private Tilemap tilemap;

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
    }

    public void SetHeldPiece(TetrominoData data)
    {
        tilemap.ClearAllTiles();
        heldPiece = data;
        Utilities.SetCells(tilemap, heldPiece.cells, heldPiece.tile);
    }
}

using UnityEngine;
using UnityEngine.Tilemaps;

public class Holder : MonoBehaviour
{
    private Tilemap tilemap;
    public TetrominoData HeldPiece { get; private set; }

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
    }

    public void SetHeldPiece(TetrominoData data)
    {
        tilemap.ClearAllTiles();
        HeldPiece = data;
        Utilities.SetCells(tilemap, HeldPiece.Cells, HeldPiece.tile);
    }
}

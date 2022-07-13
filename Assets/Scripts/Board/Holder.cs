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
}

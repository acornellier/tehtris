using UnityEngine;

public abstract class Controller : MonoBehaviour
{
    public virtual bool Muted => false;

    public virtual void NotifyNewPiece()
    {
    }

    public abstract Move GetMove(BoardState state);
}

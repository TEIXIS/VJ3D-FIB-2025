// GameState.cs
using UnityEngine;

public static class GameState
{
    /// <summary>
    /// Mientras sea false, los scripts de control (paleta, bola) deben ignorar el input.
    /// Cuando sea true, el juego ya se puede jugar.
    /// </summary>
    public static bool allowInput = false;
}

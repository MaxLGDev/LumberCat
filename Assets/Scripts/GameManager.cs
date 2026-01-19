using UnityEngine;

enum GameState
{
    WaitingForStart,
    Cooldown,
    InGame,
    GameWon,
    GameOver
}

public class GameManager : MonoBehaviour
{
    [SerializeField] InputReader input;
    [SerializeField] RoundManager rounds;

}

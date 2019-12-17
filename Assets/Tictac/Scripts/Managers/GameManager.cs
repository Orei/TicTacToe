using System.Collections;
using UnityEngine;

[RequireComponent(typeof(GameBoard))]
public class GameManager : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private Player[] players = null;
    
    [Header("Audio")]
    [SerializeField] private AudioClip placeSound = null;
    [SerializeField] private AudioClip endSound = null;
    [SerializeField] private Music backgroundMusic = default;
    
    private new AudioManager audio = null;
    private GameBoard board = null;
    private UIManager ui = null;
    private VaporGrid grid = null;
    private int turn = 0;
    private bool hasEnded = false;

    public GameBoard Board => board;
    public int Turn => turn;
    public int NextTurn => (turn + 1) % players.Length;

    private void Awake()
    {
        board = GetComponent<GameBoard>();
        ui = GetComponent<UIManager>();
        audio = GetComponent<AudioManager>();
        grid = FindObjectOfType<VaporGrid>();

        Debug.Assert(board != null, $"Couldn't find GameBoard component on {transform.name}.");
        Debug.Assert(ui != null, $"Couldn't find UIManager component on {transform.name}.");
        Debug.Assert(audio != null, $"Couldn't find AudioManager component on {transform.name}.");
        Debug.Assert(grid != null, $"Couldn't find VaporGrid.");

        board?.Create(players);
        audio?.Play(backgroundMusic);
        grid?.SetColor(players[turn].Color, 0.2f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            Restart();

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit(0);

        if (players == null || players.Length <= 0 || board == null || hasEnded)
            return;

        players[turn].Controller?.Process(this);
    }

    public void Place(int cell)
    {
        if (hasEnded || !board.Set(cell, turn))
            return;

        if (board.IsTerminalState(out int winner))
        {
            StartCoroutine(EndGame(winner, 1.5f));
            return;
        }

        EndTurn();
    }

    public void Restart()
    {
        hasEnded = false;
        turn = 0;
        grid.SetColor(players[turn].Color, 0.2f);
        board.Reset();
    }

    private IEnumerator EndGame(int winner, float restartAfter)
    {
        hasEnded = true;

        string result;
        if (winner != -1)
        {
            result = $"{players[winner].Name} wins!";
        }
        else
        {
            grid.SetColor(new Color(0.6f, 0.6f, 0.6f, 1f), 0.2f);
            result = "It's a tie!";
        }

        audio.Play(endSound);
        ui.ShowText($"{result}", 2f);

        yield return new WaitForSeconds(restartAfter);
        Restart();
        yield return null;
    }

    private void EndTurn()
    {
        turn = NextTurn;
        audio.Play(placeSound);
        grid.SetColor(players[turn].Color, 0.2f);
    }
}
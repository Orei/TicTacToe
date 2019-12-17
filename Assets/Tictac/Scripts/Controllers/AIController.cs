using UnityEngine;

[CreateAssetMenu(menuName = "Controllers/AI")]
public class AIController : Controller
{
	[Range(0f, 3f)]
	[Tooltip("Duration before the controller makes it's move.\nNote: Randomized within the range of duration / 2f and duration.")]
	[SerializeField] private float thinkDuration = 0f;
	private float timer = 0f;

	public override void Process(GameManager manager)
	{
		if (timer > 0f)
		{
			timer -= Time.deltaTime;
			return;
		}

		GameBoard board = manager.Board;

		// Random first cell, minimax rest, otherwise first will always be same, not fun!
        int move = board.IsAllEmpty() ? Random.Range(0, board.Cells.Length)
            : board.Minimax(manager.Turn, manager.NextTurn, true).Move;

		manager.Place(move);

		timer = Random.Range(thinkDuration / 2f, thinkDuration);
	}
}
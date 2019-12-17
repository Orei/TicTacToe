using UnityEngine;

[CreateAssetMenu(menuName = "Controllers/Human")]
public class HumanController : Controller
{
    public override void Process(GameManager manager)
    {
		GameBoard board = manager.Board;

        if (Input.GetKeyDown(KeyCode.F))
        {
            int move = board.Minimax(manager.Turn, manager.NextTurn, true).Move;
            manager.Place(move);
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                CellData data = hit.transform.GetComponent<CellData>();

                if (data != null)
                {
                    manager.Place(data.Index);
                }
            }
        }
    }
}
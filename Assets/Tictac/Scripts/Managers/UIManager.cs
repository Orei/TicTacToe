using System.Collections;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh = null;
    private TextDrop backdrop = null;
    private Scaler scaler = null;
    private Coroutine routine = null;

    private void Awake()
    {
        if (textMesh != null)
        {
            scaler = textMesh.GetComponent<Scaler>();
            backdrop = textMesh.GetComponent<TextDrop>();
        }

        Debug.Assert(textMesh != null, "Unable to find TextMeshPro component.");
        Debug.Assert(scaler != null, "Unable to find Scaler component in textMesh.");
        Debug.Assert(backdrop != null, "Unable to find TextDrop component in textMesh.");
    }

    // Order is important here, backdrops are created on Start, not Awake
    private void Start()
    {
        if (backdrop != null)
            backdrop.SetText(string.Empty);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                UIButton button = hit.transform.GetComponent<UIButton>();

                if (button != null)
                    button.Toggle();
            }
        }
    }

    public void ShowText(string text, float duration)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(Display(text, duration));
    }

    private IEnumerator Display(string text, float duration)
    {
        scaler.ScaleIn(0.50f);
        backdrop.SetText(text);
        yield return new WaitForSeconds(duration);
        scaler.ScaleOut(0.25f);
        yield return null;
    }
}
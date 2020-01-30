using TMPro;
using UnityEngine;

public class TextDrop : MonoBehaviour
{
    [Tooltip("How many clones we'll use to simulate the backdrop.")]
    [SerializeField] private int layers = 4;
    [Tooltip("Target color, each layer will be interpolated between the original text color and this.")]
    [SerializeField] private Color color = Color.black;
    [Tooltip("Offset between each layer.")]
    [SerializeField] private Vector3 offset = Vector3.zero;
    [Tooltip("Distance of the rotating text per layer.")]
    [SerializeField] private float distance = 0.06f;
    [Tooltip("Enhances text readability by offsetting the gradient between the first two layers.")]
    [SerializeField, Range(0f, 1f)] private float readability = 0.25f;
    
    private TextMeshPro textMesh = null;
    private TextMeshPro[] backdrops = null;

    private void Start()
    {
        textMesh = GetComponent<TextMeshPro>();
        
        Debug.Assert(textMesh != null, $"Unable to find TextMeshPro component in {transform.name}.");

        // Create all gameobjects
        GameObject[] clones = CreateClones(layers);

        backdrops = new TextMeshPro[layers];
        for (int i = 0; i < backdrops.Length; i++)
        {
            float alpha = Mathf.Clamp01((float)(i + 1) / layers);
            alpha = alpha / 1f * (1f - readability) + readability;

            backdrops[i] = clones[i].GetComponent<TextMeshPro>();
            backdrops[i].color = Color.Lerp(textMesh.color, color, alpha);
            backdrops[i].sortingOrder = -(i + 1);
        }
    }

    private void Update()
    {
        if (backdrops == null || textMesh == null)
            return;

        for (int i = 0; i < backdrops.Length; i++)
        {
            Vector3 animation = new Vector3(Mathf.Sin(Time.time), 
                Mathf.Cos(Time.time), 0f) * distance * (i + 1);

            backdrops[i].transform.localPosition = offset + animation;
        }
    }

    /// <summary>
    /// Sets the text of all layers, including the original text mesh.
    /// </summary>
    public void SetText(string text)
    {
        if (textMesh != null)
            textMesh.SetText(text);

        for (int i = 0; i < backdrops.Length; i++)
            if (backdrops[i] != null)
                backdrops[i].SetText(text);
    }

    /// <summary>
    /// Creates a specified amount of clones of this game object and attaches them to it.
    /// </summary>
    private GameObject[] CreateClones(int num)
    {
        GameObject[] clones = new GameObject[num];

        for (int i = 0; i < clones.Length; i++)
        {
            // Clone this object
            clones[i] = Instantiate(gameObject, offset * (i + 1), Quaternion.identity);
            clones[i].name = $"{transform.name} (Backdrop {i})";
            
            // Destroy text drop in child, as it will loop endlessly otherwise
            Destroy(clones[i].GetComponent<TextDrop>());
        }

        // Attach children after creating all clones, otherwise we will clone children recursively
        for (int i = 0; i < clones.Length; i++)
        {
            clones[i].transform.SetParent(transform, false);
        }

        return clones;
    }
}
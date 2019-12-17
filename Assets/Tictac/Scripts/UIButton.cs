using UnityEngine;

public class UIButton : MonoBehaviour
{
    [SerializeField] private bool isEnabled = true;
    [Tooltip("Inverts the value sent to OnClick event, in case we're actually disabling when enabled.")]
    [SerializeField] private bool invert = false;
    [SerializeField] private Color enabledColor = Color.white;
    [SerializeField] private Color disabledColor = Color.black;
    [SerializeField] private UEBool onClick = null;

    private SpriteRenderer sprite = null;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        SetColor();
    }

    public void Toggle()
    {
        isEnabled = !isEnabled;
        onClick?.Invoke(invert ? !isEnabled : isEnabled);
        SetColor();
    }

    private void SetColor()
    {
        if (sprite == null)
            return;
        
        sprite.color = isEnabled ? enabledColor : disabledColor;
    }
}
using System.Collections;
using UnityEngine;

public enum ScalerDirection
{
    In,
    Out
}

public class Scaler : MonoBehaviour
{
    [Tooltip("Default duration of scaling if none is specified during method call.\nNote: Also applies to default in/out scaling.")]
    [SerializeField] private float defaultDuration = 0.3f;
    [Tooltip("Default duration of scaling if none is specified during method call.")]
    [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("Automatically scales on Awake.")]
    [SerializeField] private bool autoStart = true;
    [Tooltip("Direction in which the scale will be applied when scaling from Auto Start.")]
    [SerializeField] private ScalerDirection direction = ScalerDirection.In;

    // Store the coroutine so we can cancel it
    private Coroutine routine;

    private void OnEnable()
    {
        if (!autoStart)
            return;
            
        if (direction == ScalerDirection.In)
            ScaleIn();
        else if (direction == ScalerDirection.Out)
            ScaleOut();
    }

    /// <summary>
    /// Scales the object in, from 0 to 1.
    /// </summary>
    public void ScaleIn() => Scale(Vector3.zero, Vector3.one);
    public void ScaleIn(float duration) => Scale(Vector3.zero, Vector3.one, duration);

    /// <summary>
    /// Scales the object out, from 1 to 0 according to .
    /// </summary>
    public void ScaleOut() => Scale(Vector3.one, Vector3.zero);
    public void ScaleOut(float duration) => Scale(Vector3.one, Vector3.zero, duration);

    /// <summary>
    /// Scales the object over time according to it's default duration.
    /// </summary>
    public void Scale(Vector3 from, Vector3 to) => Scale(from, to, defaultDuration);

    /// <summary>
    /// Scales the object over time.
    /// </summary>
    public void Scale(Vector3 from, Vector3 to, float duration)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(PerformScale(from, to, duration));
    }

    private IEnumerator PerformScale(Vector3 from, Vector3 to, float duration)
    {
        transform.localScale = from;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;

            float alpha = Mathf.Clamp01(timer / duration);
            float ease = curve.Evaluate(alpha);
            transform.localScale = Vector3.Lerp(from, to, ease);

            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }
}
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class EffectManager : MonoBehaviour
{
    [Tooltip("Profile which we will modify.")]
    [SerializeField] private PostProcessProfile profile = null;
    [Tooltip("Intensity decay.")]
    [SerializeField] private float decay = 2f;

    private float intensity = 0f;
    private bool isEnabled = true;

    public void Pulse()
    {
        if (!isEnabled)
            return;
            
        intensity = 1f;
    }

    public void Toggle(bool enabled)
    {
        isEnabled = enabled;
    }

    private void Update()
    {
        var bloom = profile.GetSetting<Bloom>();
        bloom.intensity.value = 0.5f + intensity * 0.75f;

        var distortion = profile.GetSetting<LensDistortion>();
        distortion.intensity.value = -intensity * 9f;

        intensity = Mathf.Max(0f, intensity - decay * Time.deltaTime);
    }
}
using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

[Serializable]
public struct Music
{
    public AudioClip Song;
    public AudioClip BeatStem;

    public Music(AudioClip song, AudioClip beatStem)
    {
        Song = song;
        BeatStem = beatStem;
    }
}

public class AudioManager : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] private AudioMixerGroup soundMixerGroup = null;
    [SerializeField] private AudioMixerGroup musicMixerGroup = null;

    [Header("Beat Detection")]
    [Tooltip("Size of the buffer used to calculate the current intensity.")]
    [SerializeField] private int bufferSampleSize = 32;
    [Tooltip("How fast the beat intensity decays.")]
    [SerializeField] private float decaySpeed = 2f;
    [Tooltip("How much of a difference in volume is required to hit a beat.")]
    [SerializeField] private float hysteresis = 0.1f;
    [SerializeField] private UnityEvent OnBeatHit = null;
    private float[] samples = null;
    private float intensity = 0f;

    private Music song = default;
    private AudioSource effects = null;
    private AudioSource music = null;

    private void Awake()
    {
        effects = gameObject.AddComponent<AudioSource>();
        music = gameObject.AddComponent<AudioSource>();

        effects.loop = false;
        effects.playOnAwake = false;
        effects.volume = 0.5f;
        effects.outputAudioMixerGroup = soundMixerGroup;

        music.loop = true;
        music.playOnAwake = false;      
        music.volume = 0.5f;
        music.outputAudioMixerGroup = musicMixerGroup;

        samples = new float[bufferSampleSize];
    }

    private void Start()
    {
        Play(song);
    }

    private void Update()
    {
        if (!music.isPlaying || song.BeatStem == null)
            return;

        // Get pcm stream from stem
        if (!song.BeatStem.GetData(samples, music.timeSamples))
            return;

        // Normalize dB
        float min = -60f;
        float max = 0f;
        float db = ComputeDB(samples, 0, samples.Length);
        db = Mathf.Clamp(db, min, max);
        db = (db - min) / (max - min);

        // Intensity hits instantly, but decays over time
        if (db > intensity)
        {
            // Volume difference is sharp enough to count as a beat
            if (db - intensity > hysteresis)
                OnBeatHit?.Invoke();
            
            intensity = db;
        }
        else if (intensity > 0f)
        {
            intensity = Math.Max(0f, intensity - decaySpeed * Time.deltaTime);
        }
    }

    public void Mute(bool enabled)
    {
        effects.mute = music.mute = enabled;
    }

    public void Play(AudioClip sfx)
    {
        if (sfx == null)
            return;

        effects.PlayOneShot(sfx);
    }

    public void Play(Music bgm)
    {
        if (bgm.Song == null)
            return;

        song = bgm;
        music.clip = bgm.Song;
        music.Play();
    }

    private float ComputeRMS(float[] buffer, int offset, int length)
    {
        float sum = 0f;
        float val = 0f;
 
        if (offset + length > buffer.Length)
            length = buffer.Length - offset;
 
        for (int i = 0; i < length; i++)
        {
            val = buffer[offset];
            sum += val * val;
            offset++;
        }
 
        return Mathf.Sqrt(sum / length);
    }
 
    private float ComputeDB(float[] buffer, int offset, int length)
    {
        float rms = ComputeRMS(buffer, offset, length);

        return 10 * Mathf.Log10(rms);
    }
}
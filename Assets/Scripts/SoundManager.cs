using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private AudioSource audioSource;
    private AudioClip puttClip;
    private AudioClip bounceClip;
    private AudioClip holeClip;
    private AudioClip cheerClip;
    private AudioClip boosterClip;
    private AudioClip bumperClip;
    private AudioClip sandClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        GenerateClips();
    }

    private void GenerateClips()
    {
        puttClip = CreatePuttClip();
        bounceClip = CreateBounceClip();
        holeClip = CreateHoleClip();
        cheerClip = CreateCheerClip();
        boosterClip = CreateBoosterClip();
        bumperClip = CreateBumperClip();
        sandClip = CreateSandClip();
    }

    public void PlayPutt(float powerRatio)
    {
        if (audioSource != null && puttClip != null)
        {
            audioSource.pitch = 0.9f + powerRatio * 0.35f;
            audioSource.PlayOneShot(puttClip, Mathf.Clamp01(0.6f + powerRatio * 0.4f));
        }
    }

    public void PlayBounce(float intensity)
    {
        if (audioSource != null && bounceClip != null)
        {
            audioSource.pitch = 0.95f + Random.Range(-0.1f, 0.1f);
            audioSource.PlayOneShot(bounceClip, Mathf.Clamp01(intensity * 0.6f));
        }
    }

    public void PlayHoleCup()
    {
        if (audioSource != null && holeClip != null)
        {
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(holeClip, 0.9f);
        }
    }

    public void PlayCheer()
    {
        if (audioSource != null && cheerClip != null)
        {
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(cheerClip, 0.85f);
        }
    }

    public void PlayBooster()
    {
        if (audioSource != null && boosterClip != null)
        {
            audioSource.pitch = 1.15f;
            audioSource.PlayOneShot(boosterClip, 0.85f);
        }
    }

    public void PlayBumper()
    {
        if (audioSource != null && bumperClip != null)
        {
            audioSource.pitch = 1.0f + Random.Range(-0.05f, 0.1f);
            audioSource.PlayOneShot(bumperClip, 0.9f);
        }
    }

    public void PlaySand()
    {
        if (audioSource != null && sandClip != null)
        {
            audioSource.pitch = 0.85f;
            audioSource.PlayOneShot(sandClip, 0.5f);
        }
    }

    private AudioClip CreatePuttClip()
    {
        int sampleRate = 44100;
        int length = (int)(sampleRate * 0.12f);
        float[] samples = new float[length];

        for (int i = 0; i < length; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 40f);
            float freq = 180f * Mathf.Exp(-t * 20f);
            float noise = (Random.value * 2f - 1f) * Mathf.Exp(-t * 80f) * 0.3f;
            samples[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.7f + noise) * env;
        }

        AudioClip clip = AudioClip.Create("Putt", length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateBounceClip()
    {
        int sampleRate = 44100;
        int length = (int)(sampleRate * 0.08f);
        float[] samples = new float[length];

        for (int i = 0; i < length; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 60f);
            float tone = Mathf.Sin(2f * Mathf.PI * 300f * t);
            float click = (Random.value * 2f - 1f) * Mathf.Exp(-t * 120f) * 0.4f;
            samples[i] = (tone * 0.6f + click) * env;
        }

        AudioClip clip = AudioClip.Create("Bounce", length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateHoleClip()
    {
        int sampleRate = 44100;
        int length = (int)(sampleRate * 0.35f);
        float[] samples = new float[length];

        for (int i = 0; i < length; i++)
        {
            float t = (float)i / sampleRate;
            float env1 = Mathf.Exp(-t * 15f);
            float tone1 = Mathf.Sin(2f * Mathf.PI * 440f * t) * env1 * 0.5f;

            float t2 = Mathf.Max(0f, t - 0.06f);
            float env2 = Mathf.Exp(-t2 * 25f);
            float tone2 = Mathf.Sin(2f * Mathf.PI * 220f * t2) * env2 * 0.7f;

            samples[i] = Mathf.Clamp(tone1 + tone2, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("HoleInCup", length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateCheerClip()
    {
        int sampleRate = 44100;
        float duration = 1.2f;
        int length = (int)(sampleRate * duration);
        float[] samples = new float[length];

        float[] notes = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5, E5, G5, C6

        for (int i = 0; i < length; i++)
        {
            float t = (float)i / sampleRate;
            int noteIdx = Mathf.Clamp((int)(t * 4f), 0, notes.Length - 1);
            float noteTime = t - (noteIdx * 0.25f);

            float freq = notes[noteIdx];
            float env = Mathf.Exp(-noteTime * 4f);
            float wave = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.5f;
            wave += Mathf.Sin(4f * Mathf.PI * freq * t) * 0.25f;

            samples[i] = wave * env;
        }

        AudioClip clip = AudioClip.Create("Cheer", length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateBoosterClip()
    {
        int sampleRate = 44100;
        int length = (int)(sampleRate * 0.35f);
        float[] samples = new float[length];

        for (int i = 0; i < length; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 8f);
            float freq = 300f + t * 900f; // Rising frequency
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.8f;
        }

        AudioClip clip = AudioClip.Create("Booster", length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateBumperClip()
    {
        int sampleRate = 44100;
        int length = (int)(sampleRate * 0.25f);
        float[] samples = new float[length];

        for (int i = 0; i < length; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 12f);
            float tone1 = Mathf.Sin(2f * Mathf.PI * 659.25f * t) * 0.6f; // E5
            float tone2 = Mathf.Sin(2f * Mathf.PI * 1318.5f * t) * 0.3f; // E6
            samples[i] = (tone1 + tone2) * env;
        }

        AudioClip clip = AudioClip.Create("Bumper", length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateSandClip()
    {
        int sampleRate = 44100;
        int length = (int)(sampleRate * 0.2f);
        float[] samples = new float[length];

        for (int i = 0; i < length; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 15f);
            float noise = (Random.value * 2f - 1f) * env * 0.5f;
            samples[i] = noise;
        }

        AudioClip clip = AudioClip.Create("Sand", length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}

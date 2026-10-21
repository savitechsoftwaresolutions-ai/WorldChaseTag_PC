using UnityEngine;
using WorldChaseTag.Core;
using WorldChaseTag.Combat;
using WorldChaseTag.Movement;

namespace WorldChaseTag.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class WCTAudioSynthesizer : MonoBehaviour
    {
        public static WCTAudioSynthesizer Instance { get; private set; }

        private AudioSource audioSource;

        private AudioClip buzzerClip;
        private AudioClip beepLowClip;
        private AudioClip beepHighClip;
        private AudioClip tagImpactClip;
        private AudioClip evasionWinClip;
        private AudioClip boundaryFailClip;
        private AudioClip vaultWhooshClip;
        private AudioClip footstepClip;
        private AudioClip landingClip;
        private AudioClip slideClip;
        private AudioClip uiClickClip;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 1.0f;

            GenerateClips();
        }

        private void Start()
        {
            // Subscribe to match events
            if (WCTMatchController.Instance != null)
            {
                WCTMatchController.Instance.OnStateChanged += HandleStateChanged;
                WCTMatchController.Instance.OnChaseResultCommitted += HandleChaseResult;
            }

            if (WCTTagSystem.Instance != null)
            {
                WCTTagSystem.Instance.OnTagVisualEffect += _ => PlayTagSound();
            }
        }

        private void OnDestroy()
        {
            if (WCTMatchController.Instance != null)
            {
                WCTMatchController.Instance.OnStateChanged -= HandleStateChanged;
                WCTMatchController.Instance.OnChaseResultCommitted -= HandleChaseResult;
            }
        }

        private void GenerateClips()
        {
            int sampleRate = 44100;

            // 1. Official Referee Buzzer (850Hz + 425Hz square wave burst)
            buzzerClip = CreateTone(sampleRate, 0.6f, (t) =>
            {
                float s1 = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 850f * t));
                float s2 = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 425f * t));
                return (s1 * 0.6f + s2 * 0.4f) * (1f - t / 0.6f);
            });

            // 2. Beep Low (3, 2, 1 countdown: 440Hz sine)
            beepLowClip = CreateTone(sampleRate, 0.15f, (t) =>
            {
                return Mathf.Sin(2f * Mathf.PI * 440f * t) * (1f - t / 0.15f);
            });

            // 3. Beep High (GO! buzzer: 880Hz sine)
            beepHighClip = CreateTone(sampleRate, 0.35f, (t) =>
            {
                return Mathf.Sin(2f * Mathf.PI * 880f * t) * (1f - t / 0.35f);
            });

            // 4. Tag Impact Punch (White noise burst + 110Hz body thump)
            tagImpactClip = CreateTone(sampleRate, 0.25f, (t) =>
            {
                float noise = (Random.value * 2f - 1f) * (1f - t / 0.25f);
                float thump = Mathf.Sin(2f * Mathf.PI * 110f * (1f - t)) * (1f - t / 0.25f);
                return (noise * 0.45f + thump * 0.55f);
            });

            // 5. Evasion Victory Fanfare (Ascending major chord: 523Hz -> 659Hz -> 784Hz)
            evasionWinClip = CreateTone(sampleRate, 0.85f, (t) =>
            {
                float freq = t < 0.25f ? 523.25f : (t < 0.5f ? 659.25f : 783.99f);
                return Mathf.Sin(2f * Mathf.PI * freq * t) * 0.7f * (1f - t / 0.85f);
            });

            // 6. Boundary Violation Error Buzzer (Descending saw wave)
            boundaryFailClip = CreateTone(sampleRate, 0.45f, (t) =>
            {
                float freq = Mathf.Lerp(320f, 120f, t / 0.45f);
                return Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t)) * 0.55f * (1f - t / 0.45f);
            });

            // 7. Vault Wind Whoosh
            vaultWhooshClip = CreateTone(sampleRate, 0.32f, (t) =>
            {
                float env = Mathf.Sin(Mathf.PI * (t / 0.32f));
                return (Random.value * 2f - 1f) * env * 0.4f;
            });

            // 8. Athletic Footstep (Court rubber squeak & impact)
            footstepClip = CreateTone(sampleRate, 0.08f, (t) =>
            {
                float env = 1f - t / 0.08f;
                float squeak = Mathf.Sin(2f * Mathf.PI * 820f * t) * 0.3f;
                float thud = Mathf.Sin(2f * Mathf.PI * 90f * t) * 0.7f;
                return (squeak + thud) * env;
            });

            // 9. Heavy Landing Impact Thud
            landingClip = CreateTone(sampleRate, 0.28f, (t) =>
            {
                float env = (1f - t / 0.28f) * (1f - t / 0.28f);
                float sub = Mathf.Sin(2f * Mathf.PI * 75f * t) * 0.8f;
                float noise = (Random.value * 2f - 1f) * 0.2f;
                return (sub + noise) * env;
            });

            // 10. Sprint Slide Ground Scrape
            slideClip = CreateTone(sampleRate, 0.45f, (t) =>
            {
                float env = Mathf.Sin(Mathf.PI * (t / 0.45f));
                return (Random.value * 2f - 1f) * env * 0.35f;
            });

            // 11. UI Menu Click / Select
            uiClickClip = CreateTone(sampleRate, 0.06f, (t) =>
            {
                return Mathf.Sin(2f * Mathf.PI * 1200f * t) * (1f - t / 0.06f) * 0.5f;
            });
        }

        private AudioClip CreateTone(int sampleRate, float duration, System.Func<float, float> waveFunc)
        {
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] data = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                data[i] = Mathf.Clamp(waveFunc(t), -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("SynthesizedClip", totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public void PlayBuzzer() => audioSource.PlayOneShot(buzzerClip, 0.8f);
        public void PlayCountdownBeep(bool isGo) => audioSource.PlayOneShot(isGo ? beepHighClip : beepLowClip, 0.7f);
        public void PlayTagSound() => audioSource.PlayOneShot(tagImpactClip, 0.9f);
        public void PlayEvasionWin() => audioSource.PlayOneShot(evasionWinClip, 0.8f);
        public void PlayBoundaryViolation() => audioSource.PlayOneShot(boundaryFailClip, 0.8f);
        public void PlayVaultWhoosh() => audioSource.PlayOneShot(vaultWhooshClip, 0.5f);
        public void PlayFootstep() => audioSource.PlayOneShot(footstepClip, 0.35f);
        public void PlayLanding() => audioSource.PlayOneShot(landingClip, 0.65f);
        public void PlaySlide() => audioSource.PlayOneShot(slideClip, 0.5f);
        public void PlayUIClick() => audioSource.PlayOneShot(uiClickClip, 0.6f);

        private void HandleStateChanged(WCTMatchState state)
        {
            if (state == WCTMatchState.CHASE_ACTIVE)
            {
                PlayCountdownBeep(true);
            }
            else if (state == WCTMatchState.PRE_CHASE)
            {
                PlayCountdownBeep(false);
            }
        }

        private void HandleChaseResult(ChaseRecord record)
        {
            if (record.outcome == ChaseOutcome.EvaderSurvived)
            {
                PlayEvasionWin();
            }
            else if (record.outcome == ChaseOutcome.Tagged)
            {
                PlayTagSound();
            }
            else if (record.outcome == ChaseOutcome.OutOfBounds)
            {
                PlayBoundaryViolation();
            }
        }
    }
}

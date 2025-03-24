using System;
using UnityEngine;

namespace LCOffice.Components
{
    public class ElevatorMusic : MonoBehaviour
    {
        public AudioSource audioSource;
        private int currentMusic;
        private float musicPlayTimer = 0;
        private bool pitchDown;

        private bool active = false;

        public ElevatorTrack[] elevatorTracks = new ElevatorTrack[0];

        [Serializable]
        public struct ElevatorTrack
        {
            public AudioClip music;
            public AudioClip downPitch;

            public readonly AudioClip Get(bool pitchDown) => pitchDown ? downPitch : music;
        }

        void Start()
        {
            audioSource.volume = 0.4f * (Plugin.musicVolume.Value / 100f);
            pitchDown = Plugin.elevatorMusicPitchdown.Value;
            active = true;
        }

        public void ToggleMusic(bool on)
        {
            active = on;
            audioSource.Stop();
        }

        void Update()
        {
            if (active && !audioSource.isPlaying)
            {
                musicPlayTimer += Time.deltaTime;
                if (musicPlayTimer > 0.5f)
                {
                    musicPlayTimer = 0;
                    currentMusic++;
                    if (currentMusic >= elevatorTracks.Length) currentMusic = 0;

                    audioSource.PlayOneShot(elevatorTracks[currentMusic].Get(pitchDown));
                }
            }
        }
    }
}

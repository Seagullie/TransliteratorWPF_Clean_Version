using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Media;

namespace TransliteratorBackend
{
    public class Sound
    {
        private MediaPlayer m_mediaPlayer;

        public void Play(string filename, int volume = 100)
        {
            m_mediaPlayer = new MediaPlayer();

            SetVolume(volume);

            m_mediaPlayer.Open(new Uri(filename));
            m_mediaPlayer.Play();
        }

        // `volume` is assumed to be between 0 and 100.
        public void SetVolume(int volume)
        {
            // MediaPlayer volume is a float value between 0 and 1.
            m_mediaPlayer.Volume = volume / 100.0f;
        }
    }
}
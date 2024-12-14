using UnityEngine;

namespace CavrnusDemo
{
    public class CavrnusMicrophoneTest : MonoBehaviour
    {
        private AudioSource audioSource;
        private string selectedMicrophone;
        private bool isRecording;

        void Start()
        {
            // Check for available microphones
            if (Microphone.devices.Length > 0)
            {
                selectedMicrophone = Microphone.devices[0];
                Debug.Log($"Selected Microphone: {selectedMicrophone}");

                // Initialize AudioSource component
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.loop = false; // Allow playback after recording
                audioSource.mute = true; // Mute during recording to avoid feedback

                isRecording = false;
            }
            else
            {
                Debug.LogError("No microphone detected!");
            }
        }

        public void SetMicrophoneStatus(bool state)
        {
            if (state)
            {
                StartMicrophone();
            }
            else
            {
                StopMicrophone();
            }
        }

        private void StartMicrophone()
        {
            if (selectedMicrophone != null)
            {
                Debug.Log("Starting microphone...");
                audioSource.clip = Microphone.Start(selectedMicrophone, true, 10, 44100); // 10-second buffer
                while (Microphone.GetPosition(selectedMicrophone) <= 0) { } // Wait for the microphone to start
                isRecording = true;
                Debug.Log("Microphone started.");
            }
        }

        private void StopMicrophone()
        {
            if (selectedMicrophone != null)
            {
                Debug.Log("Stopping microphone...");
                Microphone.End(selectedMicrophone);
                isRecording = false;

                // Playback the recorded audio
                if (audioSource.clip != null)
                {
                    audioSource.mute = false; // Unmute for playback
                    audioSource.Play();
                    Debug.Log("Playing back recorded audio...");
                }
                else
                {
                    Debug.LogWarning("No audio clip available for playback.");
                }
            }
        }

        void OnApplicationQuit()
        {
            // Ensure microphone is stopped when the application quits
            StopMicrophone();
        }
    }
}
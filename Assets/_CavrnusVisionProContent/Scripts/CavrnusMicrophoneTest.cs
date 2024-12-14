using UnityEngine;

namespace CavrnusDemo
{
    public class CavrnusMicrophoneTest : MonoBehaviour
    {
        private AudioSource audioSource;
        private string selectedMicrophone;

        void Start()
        {
            // Check for available microphones
            if (Microphone.devices.Length > 0)
            {
                selectedMicrophone = Microphone.devices[0];
                Debug.Log($"Selected Microphone: {selectedMicrophone}");

                // Initialize AudioSource component
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.loop = true; // Loop to keep capturing audio
                audioSource.mute = true; // Mute playback to avoid feedback
            }
            else
            {
                Debug.LogError("No microphone detected!");
            }
        }

        public void SetMicrophoneStatus(bool state)
        {
            if (state)
                StartMicrophone();
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
                while (Microphone.GetPosition(selectedMicrophone) <= 0)
                {
                } // Wait for the microphone to start

                audioSource.Play();
                Debug.Log("Microphone started.");
            }
        }

        private void StopMicrophone()
        {
            if (selectedMicrophone != null)
            {
                Debug.Log("Stopping microphone...");
                Microphone.End(selectedMicrophone);
                audioSource.Stop();
                Debug.Log("Microphone stopped.");
            }
        }

        void OnApplicationQuit()
        {
            // Ensure microphone is stopped when the application quits
            StopMicrophone();
        }
    }
}
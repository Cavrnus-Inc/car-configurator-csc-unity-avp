using System.Collections;
using System.Collections.Generic;
using CavrnusSdk.API;
using UnityEngine;

namespace CavrnusDemo
{
    public class CavrnusMicrophoneTest : MonoBehaviour
    {
        private CavrnusSpaceConnection spaceConnection;

        void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc =>
            {
                spaceConnection = sc;

                StartCoroutine(InitLSAudio());
                
                // Check for available microphones
                if (Microphone.devices.Length > 0)
                {
                }
                else
                {
                    Debug.LogError("No microphone detected!");
                }
            });
        }

        private IEnumerator InitLSAudio()
        {
            yield return new WaitForSeconds(5f);

            SetMicrophoneStatus(true);
        }

        public void SetMicrophoneStatus(bool state)
        {
            if (spaceConnection == null)
                return;
            
            if (state)
                StartMicrophone();
            else
            {
                if (routine != null)
                    StopCoroutine(routine);
            }
        }

        private Coroutine routine;
        private void StartMicrophone()
        {
            spaceConnection.FetchAudioInputs(inputDevices =>
            {
                if (routine != null)
                    StopCoroutine(routine);
                
                routine = StartCoroutine(SwitchAudioInputs(inputDevices));
            });
        }
        
        private IEnumerator SwitchAudioInputs(List<CavrnusInputDevice> inputDevices)
        {
            foreach (var input in inputDevices)
            {
                print($"TEST Audio Device: {input.Name}");
                spaceConnection.UpdateAudioInput(input);

                yield return new WaitForSeconds(10f);
            }
            
            print($"FINISH Testing Audio Devices");
        }
    }
}
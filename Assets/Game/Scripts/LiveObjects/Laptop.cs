using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.InputSystem;

namespace Game.Scripts.LiveObjects
{
    public class Laptop : MonoBehaviour
    {
        [SerializeField]
        private Slider _progressBar;
        [SerializeField]
        private int _hackTime = 5;
        private bool _hacked = false;
        [SerializeField]
        private CinemachineVirtualCamera[] _cameras;
        private int _activeCamera = 0;
        [SerializeField]
        private InteractableZone _interactableZone;

        public static event Action onHackComplete;
        public static event Action onHackEnded;

        // New Input System
        private PlayerInputActions _inputActions;
        private bool _isStartKeyDown = false;
        private bool _isExitKeyDown = false;
        private bool _switchCamera = false;

        private void Start()
        {
            _inputActions = new PlayerInputActions();
            if(_inputActions == null)
            {
                Debug.LogWarning("Input Actions is Null!");
            }
            else
            {
                _inputActions.Player.Enable();
            }

            _inputActions.Player.Interaction.started += Interaction_started;
            _inputActions.Player.Interaction.performed += Interaction_performed;
            _inputActions.Player.Interaction.canceled += Interaction_canceled;
            _inputActions.Player.EndInteraction.performed += EndInteraction_performed;
        }

        private void Interaction_started(InputAction.CallbackContext obj) // Interaction Key down
        {
            _isStartKeyDown = true;
        }

        private void Interaction_performed(InputAction.CallbackContext obj) // Cycle through cameras
        {
            _switchCamera = true;
        }

        private void Interaction_canceled(InputAction.CallbackContext obj) // Interaction Key Up
        {
            _isStartKeyDown = false;
        }

        private void EndInteraction_performed(InputAction.CallbackContext obj)// End Interaction Key Pressed
        {
            _isExitKeyDown = true;
        }

        private void OnEnable()
        {
            InteractableZone.onHoldStarted += InteractableZone_onHoldStarted;
            InteractableZone.onHoldEnded += InteractableZone_onHoldEnded;
        }

        private void Update()
        {
            if (_hacked == true)
            {
                if (_switchCamera)
                {
                    var previous = _activeCamera;
                    _activeCamera++;


                    if (_activeCamera >= _cameras.Length)
                        _activeCamera = 0;


                    _cameras[_activeCamera].Priority = 11;
                    _cameras[previous].Priority = 9;
                    _switchCamera = false;
                }

                if (_isExitKeyDown)
                {
                    _hacked = false;
                    onHackEnded?.Invoke();
                    ResetCameras();
                    _isExitKeyDown = false;
                }
            }
        }

        void ResetCameras()
        {
            foreach (var cam in _cameras)
            {
                cam.Priority = 9;
            }
        }

        private void InteractableZone_onHoldStarted(int zoneID)
        {
            if (zoneID == 3 && _hacked == false) //Hacking terminal
            {
                _progressBar.gameObject.SetActive(true);
                StartCoroutine(HackingRoutine());
                onHackComplete?.Invoke();
            }
        }

        private void InteractableZone_onHoldEnded(int zoneID)
        {
            if (zoneID == 3) //Hacking terminal
            {
                if (_hacked == true)
                    return;

                StopAllCoroutines();
                _progressBar.gameObject.SetActive(false);
                _progressBar.value = 0;
                onHackEnded?.Invoke();
            }
        }

        
        IEnumerator HackingRoutine()
        {
            while (_progressBar.value < 1)
            {
                _progressBar.value += Time.deltaTime / _hackTime;
                yield return new WaitForEndOfFrame();
            }

            //successfully hacked
            _hacked = true;
            _interactableZone.CompleteTask(3);

            //hide progress bar
            _progressBar.gameObject.SetActive(false);

            //enable Vcam1
            _cameras[0].Priority = 11;
        }
        
        private void OnDisable()
        {
            InteractableZone.onHoldStarted -= InteractableZone_onHoldStarted;
            InteractableZone.onHoldEnded -= InteractableZone_onHoldEnded;

            // New Input System
            _inputActions.Player.Interaction.started -= Interaction_started;
            _inputActions.Player.Interaction.performed -= Interaction_performed;
            _inputActions.Player.Interaction.canceled += Interaction_canceled;
            _inputActions.Player.EndInteraction.performed -= EndInteraction_performed;
        }
    }
}


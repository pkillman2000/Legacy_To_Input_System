using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Scripts.UI;
using UnityEngine.InputSystem;

namespace Game.Scripts.LiveObjects
{
    public class InteractableZone : MonoBehaviour
    {
        private enum ZoneType
        {
            Collectable,
            Action,
            HoldAction
        }

        private enum KeyState
        {
            Press,
            PressHold
        }

        [SerializeField]
        private ZoneType _zoneType; // Collectable, Action, HoldAction
        [SerializeField]
        private int _zoneID;//0-Pickup C4, 1- ,2-Detonate C4, 3-Hack Cameras, 4-Fly Drone, 5-Drive Forklift, 6-Destructable Crate, 7-End Zone
        [SerializeField]
        private int _requiredID;//-1-C4 Pickup, 1-Detonate C4, 2-Hack Cameras, 3-Fly Drone, 4-Drive Forklift, 5-Destructable Crate, 6- End Zone
        [SerializeField]
        [Tooltip("Press the (---) Key to .....")]
        private string _displayMessage;
        [SerializeField]
        private GameObject[] _zoneItems;
        private bool _inZone = false;
        private bool _itemsCollected = false;
        private bool _actionPerformed = false;
        [SerializeField]
        private Sprite _inventoryIcon;
        /*Legacy Input System
        [SerializeField]
        private KeyCode _zoneKeyInput;
        [SerializeField]
        private KeyState _keyState;
        */
        [SerializeField]
        private GameObject _marker;

        //private bool _inHoldState = false;

        private static int _currentZoneID = 0;
        public static int CurrentZoneID
        { 
            get 
            { 
               return _currentZoneID; 
            }
            set
            {
                _currentZoneID = value; 
                         
            }
        }


        public static event Action<InteractableZone> onZoneInteractionComplete;
        public static event Action<int> onHoldStarted;
        public static event Action<int> onHoldEnded;

        // New Input System
        private PlayerInputActions _inputActions;
        private string _interactKeyName;
        private bool _isKeyDown = false;

        private void Start()
        {
            // New Input System
            _inputActions = new PlayerInputActions();
            if(_inputActions == null ) 
            {
                Debug.LogWarning("Input Actions is Null!");
            }
            else
            {
                _inputActions.Player.Enable();
            }

            _interactKeyName = _inputActions.Player.Interaction.GetBindingDisplayString(0); // 0 = keyboard, 1 = gamepad
            _inputActions.Player.Interaction.started += Interaction_started;
            _inputActions.Player.Interaction.performed += Interaction_performed;
            _inputActions.Player.Interaction.canceled += Interaction_canceled;

        }


        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += SetMarker;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && _currentZoneID > _requiredID)
            {
                switch (_zoneType)
                {
                    case ZoneType.Collectable:
                        if (_itemsCollected == false)
                        {
                            _inZone = true;
                            if (_displayMessage != null)
                            {
                                //string message = $"Press the {_zoneKeyInput.ToString()} key to {_displayMessage}.";
                                string message = $"{_interactKeyName} key to {_displayMessage}.";

                                UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                            }
                            else
                                //UIManager.Instance.DisplayInteractableZoneMessage(true, $"Press the {_zoneKeyInput.ToString()} key to collect");
                                UIManager.Instance.DisplayInteractableZoneMessage(true, $"{_interactKeyName} key to collect");
                        }
                        break;

                    case ZoneType.Action:
                        if (_actionPerformed == false)
                        {
                            _inZone = true;
                            if (_displayMessage != null)
                            {
                                //string message = $"Press the {_zoneKeyInput.ToString()} key to {_displayMessage}.";
                                string message = $"{_interactKeyName} key to {_displayMessage}.";
                                UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                            }
                            else
                                //UIManager.Instance.DisplayInteractableZoneMessage(true, $"Press the {_zoneKeyInput.ToString()} key to perform action"); 
                                UIManager.Instance.DisplayInteractableZoneMessage(true, $"{_interactKeyName} key to perform action");

                        }
                        break;

                    case ZoneType.HoldAction:
                        _inZone = true;
                        if (_displayMessage != null)
                        {
                            //string message = $"Press the {_zoneKeyInput.ToString()} key to {_displayMessage}.";
                            string message = $"{_interactKeyName} key to {_displayMessage}.";
                            UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                            Debug.Log("IZ-149");
                        }
                        else
                            //UIManager.Instance.DisplayInteractableZoneMessage(true, $"Hold the {_zoneKeyInput.ToString()} key to perform action");
                            UIManager.Instance.DisplayInteractableZoneMessage(true, $"Hold the {_interactKeyName} key to perform action");
                        break;
                }
            }
        }

        private void Update()
        {
            if (_inZone == true)
            {                
                //if (Input.GetKeyDown(_zoneKeyInput) && _keyState != KeyState.PressHold)
                if (_isKeyDown)
                    {
                        switch (_zoneType)
                        {
                            case ZoneType.Collectable:
                                if (_itemsCollected == false)
                                {
                                    CollectItems();
                                    _itemsCollected = true;
                                    UIManager.Instance.DisplayInteractableZoneMessage(false);
                                }
                                break;

                            case ZoneType.Action:
                                if (_actionPerformed == false)
                                {
                                    PerformAction();
                                    _actionPerformed = true;
                                    UIManager.Instance.DisplayInteractableZoneMessage(false);
                                }
                                break;

                            case ZoneType.HoldAction:
                                //_inHoldState = true;
                                Debug.Log("IZ-188");
                                PerformHoldAction();
                                break;

                    }
                }
                /*
                //else if (_isKeyDown && _keyState == KeyState.PressHold && _inHoldState == false)
                else if (_isKeyDown && _inHoldState == false)
                {
                    _inHoldState = true;
                    Debug.Log("IZ-191");
                    switch (_zoneType)
                    {                      
                        case ZoneType.HoldAction:
                            PerformHoldAction();
                            break;           
                    }
                }
                */
                //if (_isKeyDown && _keyState == KeyState.PressHold)
                if (!_isKeyDown)
                {
                    //_inHoldState = false;
                    onHoldEnded?.Invoke(_zoneID);
                }               
            }
        }

        // New Input System
        private void Interaction_started(InputAction.CallbackContext obj)// Key Pressed
        {
            _isKeyDown = true;
        }

        private void Interaction_performed(InputAction.CallbackContext obj) // Completed
        {
            _isKeyDown = true;
        }

        private void Interaction_canceled(InputAction.CallbackContext obj) // Key Up
        {
            _isKeyDown = false;
        }
        // End New Input System

        private void CollectItems()
        {
            foreach (var item in _zoneItems)
            {
                item.SetActive(false);
            }

            UIManager.Instance.UpdateInventoryDisplay(_inventoryIcon);

            CompleteTask(_zoneID);

            onZoneInteractionComplete?.Invoke(this);

        }

        private void PerformAction()
        {
            foreach (var item in _zoneItems)
            {
                item.SetActive(true);
            }

            if (_inventoryIcon != null)
                UIManager.Instance.UpdateInventoryDisplay(_inventoryIcon);

            onZoneInteractionComplete?.Invoke(this);
        }

        private void PerformHoldAction()
        {
            UIManager.Instance.DisplayInteractableZoneMessage(false);
            onHoldStarted?.Invoke(_zoneID);
            Debug.Log("IZ-266");
        }

        public GameObject[] GetItems()
        {
            return _zoneItems;
        }

        public int GetZoneID()
        {
            return _zoneID;
        }

        public void CompleteTask(int zoneID)
        {
            if (zoneID == _zoneID)
            {
                _currentZoneID++;
                onZoneInteractionComplete?.Invoke(this);
            }
        }

        public void ResetAction(int zoneID)
        {
            if (zoneID == _zoneID)
                _actionPerformed = false;
        }

        public void SetMarker(InteractableZone zone)
        {
            if (_zoneID == _currentZoneID)
                _marker.SetActive(true);
            else
                _marker.SetActive(false);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _inZone = false;
                UIManager.Instance.DisplayInteractableZoneMessage(false);
            }
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= SetMarker;

            // New Input System
            _inputActions.Player.Interaction.started -= Interaction_started;
            _inputActions.Player.Interaction.performed -= Interaction_performed;
            _inputActions.Player.Interaction.canceled -= Interaction_canceled;

        }
    }
}



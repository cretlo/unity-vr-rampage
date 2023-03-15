using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction;

public class CharacterStateManager : MonoBehaviour
{
  private LocomotionSystem locomotionSystem;
  private ContinuousMoveProviderBase continuousMoveProvider;
  private ContinuousTurnProviderBase continuousTurnProvider;
  private CharacterController characterController;
  private Rigidbody _xrRb;


  public static bool isClimbing;
  public static bool isLeaping;
  void Awake()
  {
    _xrRb = GetComponent<Rigidbody>();

    locomotionSystem = GetComponent<LocomotionSystem>();
    continuousMoveProvider = GetComponent<ContinuousMoveProviderBase>();
    continuousTurnProvider = GetComponent<ContinuousTurnProviderBase>();
    characterController = GetComponent<CharacterController>();

  }
  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {

  }

  public void DeactivateOpenXRComponents()
  {
    // GetComponent<LocomotionSystem>().enabled = false;
    // GetComponent<ContinuousMoveProviderBase>().enabled = false;
    // GetComponent<ContinuousTurnProviderBase>().enabled = false;
    // GetComponent<CharacterController>().enabled = false;

    locomotionSystem.enabled = false;
    continuousMoveProvider.enabled = false;
    continuousTurnProvider.enabled = false;
    characterController.enabled = false;
  }

  public void ActivateOpenXRComponents()
  {
    // GetComponent<LocomotionSystem>().enabled = true;
    // GetComponent<ContinuousMoveProviderBase>().enabled = true;
    // GetComponent<ContinuousTurnProviderBase>().enabled = true;
    // GetComponent<CharacterController>().enabled = true;

    locomotionSystem.enabled = true;
    continuousMoveProvider.enabled = true;
    continuousTurnProvider.enabled = true;
    characterController.enabled = true;
  }

  public void ActivatePhysics()
  {
    _xrRb.isKinematic = false;
    _xrRb.useGravity = true;
    // Possibly need to turn on a collider for collision
  }

  public void DeactivatePhysics()
  {
    _xrRb.isKinematic = true;
    _xrRb.useGravity = false;
    // Possibly disabel rb collider
  }

}
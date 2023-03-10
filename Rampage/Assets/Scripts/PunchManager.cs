using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PunchManager : MonoBehaviour
{
  Rigidbody rb;
  public Transform hand;
  public InputActionProperty trigger;
  public Collider punchCollider;
  private bool canPunch;
  private Vector3 handsDir;
  // Start is called before the first frame update
  void Start()
  {
    // Start at rig position
    transform.position = hand.position;

    canPunch = false;
    rb = GetComponent<Rigidbody>();
  }

  void Update()
  {
    handsDir = hand.position - rb.position;
    if (trigger.action.ReadValue<float>() == 1)
    {
      canPunch = true;
    }
    else
    {
      canPunch = false;
    }
  }

  // Update is called once per frame
  void FixedUpdate()
  {
    //Moves physics hands to controller
    rb.MovePosition(hand.transform.position);
    // rb.velocity = handsDir / Time.fixedDeltaTime;
    rb.MoveRotation(hand.rotation);
  }

  void OnTriggerExit(Collider other)
  {
    if (other.transform.tag == "WallChunk")
    {
      print(this.name + " HAS EXITED WALLCHUNK");
      punchCollider.isTrigger = false;

    }

  }

  void OnCollisionEnter(Collision other)
  {

    if (canPunch)
    {
      // if (collision.transform.tag != "WallChunk") { return; }
      Vector3 forceOfHit = other.impulse / Time.fixedDeltaTime;
      Vector3 clampedForce = Vector3.ClampMagnitude(forceOfHit, 100);
      // print("Unclamped Force: " + forceOfHit + ", Clamped Force: " + clampedForce);

      other.gameObject.GetComponent<Rigidbody>().AddRelativeForce(clampedForce * 25, ForceMode.Force);
      // collision.gameObject.GetComponent<Rigidbody>().AddRelativeForce(final * 50, ForceMode.Force);

      ConfigurableJoint[] joints = other.gameObject.GetComponents<ConfigurableJoint>();

      if (joints.Length == 0) { return; }

      for (int i = 0; i < joints.Length; i++)
      {
        joints[i].breakForce -= 50;
      }

    }
    else
    {
      if (other.transform.tag == "WallChunk")
      {
        punchCollider.isTrigger = true;
        // WallChunk wallChunk = other.gameObject.GetComponent<WallChunk>();
        // if (!wallChunk.isBroken)
        // {
        //   punchCollider.enabled = false;
        // }
      }

    }

  }
}

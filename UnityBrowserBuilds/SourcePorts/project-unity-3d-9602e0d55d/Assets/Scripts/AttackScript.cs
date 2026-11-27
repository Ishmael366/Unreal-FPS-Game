using UnityEngine;
using UnityEngine.UI;
public class AttackScript : MonoBehaviour {
public int hitpoints = 10; //amount of damage
public float totarget; //how far it is to the target
public float range = 3.5f; // the maximum distance allowed

void Update () {
	if (Input.GetButtonDown("Attack")) {
		RaycastHit hit;
		
		if (Physics.Raycast (transform.position, transform.TransformDirection(Vector3.forward), out hit)) {
			totarget = hit.distance;
			
			if (totarget < range) {
				hit.transform.SendMessage("DeductPoints", hitpoints, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}

}

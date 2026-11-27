using UnityEngine;
using UnityEngine.UI;
public class AnimateWeapon : MonoBehaviour {
void Update() {
    if(Input.GetButtonDown("Attack")) {
	    GetComponent<Animation>().Play("AttackAnimation");
	}
}
}

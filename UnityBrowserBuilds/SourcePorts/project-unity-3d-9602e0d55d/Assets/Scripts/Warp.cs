using UnityEngine;
using UnityEngine.UI;
public class Warp : MonoBehaviour {
public Transform warptarget;

void Update() {

}

void OnTriggerEnter(Collider col) {
	if(col.gameObject.tag == "warp001") {
		this.transform.position = warptarget.position;
	}
}
}

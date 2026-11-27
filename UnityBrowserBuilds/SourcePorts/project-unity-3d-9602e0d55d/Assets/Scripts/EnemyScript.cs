using UnityEngine;
using UnityEngine.UI;
public class EnemyScript : MonoBehaviour {
public int enemyhealth = 50;

void DeductPoints(int hitpoints) {
	enemyhealth -= hitpoints;
}

void Update() {
	if(enemyhealth <= 0) {
		Destroy(gameObject);
	}
}
}

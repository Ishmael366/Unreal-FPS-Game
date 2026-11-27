using UnityEngine;
using UnityEngine.UI;
public class CoinCollect : MonoBehaviour {
System.Collections.IEnumerator OnTriggerEnter(Collider info) {
	AudioSource coinaudio = GetComponent<AudioSource>();
	coinaudio.Play();
    CoinSystem.coinscollect += 1;
	transform.position = new Vector3(0, -1000, 0);
	yield return new WaitForSeconds(2);
	Destroy(gameObject);
}
}

using UnityEngine;
using UnityEngine.UI;
public class CoinSystem : MonoBehaviour {
public static int coinscollect = 0;

void OnGUI() {
    GUI.Label (new Rect(10, 10, 90, 40), ("Coins: " + coinscollect));
}

}

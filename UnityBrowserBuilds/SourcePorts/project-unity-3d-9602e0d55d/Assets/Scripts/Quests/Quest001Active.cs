using UnityEngine;
using UnityEngine.UI;
public class Quest001Active : MonoBehaviour {
public int TextBoxOnCheck = 0;
public GameObject MessageBox;
public GameObject TextBox;

public GameObject QuestItemToClose;
public GameObject QusetItemToShow;

System.Collections.IEnumerator OnMouseDown() {
	if(TextBoxOnCheck == 0) {
		TextBoxOnCheck = 1;
		MessageBox.SetActive(true);
		TextBox.GetComponent<Text>().text = "You found the loot. Thanks friend";
		yield return new WaitForSeconds(3);
		gameObject.SetActive(false);
		MessageBox.SetActive(false);
		QuestItemToClose.SetActive(false);
		QusetItemToShow.SetActive(true);
	} 
}
}

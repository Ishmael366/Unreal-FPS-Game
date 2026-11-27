using UnityEngine;
using UnityEngine.UI;
public class Quest001 : MonoBehaviour {
public int TextBoxOnCheck = 0;
public GameObject MessageBox;
public GameObject TextBox;
public string TextMessage;
public GameObject QuestBox;
public GameObject QuestText;
public string QuestName;

void OnMouseDown() {
	if(TextBoxOnCheck == 0) {
		TextBoxOnCheck = 1;
		MessageBox.SetActive(true);
		TextBox.GetComponent<Text>().text = TextMessage;
		QuestName = "Active Quest: Recover the Loot";
		QuestText.GetComponent<Text>().text = QuestName;
	} else {
		TextBoxOnCheck = 0;
		MessageBox.SetActive(false);
		TextMessage = "Villager: Please Help Traveller";
	}
}

void Update() {
	if(Input.GetButtonDown("Submit")) {
		if(TextBoxOnCheck == 1) {
			MessageBox.SetActive(false);
			TextBoxOnCheck = 0;
			TextMessage = "Villager: Please Help Traveller";
		}
	}

	if(Input.GetButtonDown("Cancel")) {
		if(TextBoxOnCheck == 1) {
			MessageBox.SetActive(false);
			TextBoxOnCheck = 0;
			TextMessage = "Villager: Please Help Traveller";
		}
	}
}
}

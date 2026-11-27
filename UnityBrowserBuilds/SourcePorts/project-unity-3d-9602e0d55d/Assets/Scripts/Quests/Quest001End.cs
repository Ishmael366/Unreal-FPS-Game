using UnityEngine;
using UnityEngine.UI;
public class Quest001End : MonoBehaviour {
public int TextBoxOnCheck = 0;
public GameObject MessageBox;
public GameObject TextBox;
public GameObject QuestBox;
public GameObject QuestText;


void OnMouseDown() {
	if(TextBoxOnCheck == 0) {
		TextBoxOnCheck = 1;
		MessageBox.SetActive(true);
		TextBox.GetComponent<Text>().text = "Villager: Thank you very much";
		QuestText.GetComponent<Text>().text = "Active Quest: (none)";
	} else {
		TextBoxOnCheck = 0;
		MessageBox.SetActive(false);
		//TextMessage = "Villager: Please Help Traveller";
	}
}

void Update() {
	if(Input.GetButtonDown("Submit")) {
		if(TextBoxOnCheck == 1) {
			MessageBox.SetActive(false);
			TextBoxOnCheck = 0;
		}
	}

	if(Input.GetButtonDown("Cancel")) {
		if(TextBoxOnCheck == 1) {
			MessageBox.SetActive(false);
			TextBoxOnCheck = 0;
		}
	}
}
}

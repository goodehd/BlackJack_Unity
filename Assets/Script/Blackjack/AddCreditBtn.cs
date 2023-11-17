using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlackJack;

namespace BlackJack {
    public class AddCreditBtn : MonoBehaviour {
        public int AddCredit;

        public void AddCreditClick() {
            string inputTxt = UiManager.IUiManager.startInput.text;

            if (inputTxt == "") {
                UiManager.IUiManager.startInput.text = AddCredit.ToString();
            } else {
                int curInputCredit = int.Parse(inputTxt);
                curInputCredit += AddCredit;
                UiManager.IUiManager.startInput.text = curInputCredit.ToString();
            }
        }
    }
}

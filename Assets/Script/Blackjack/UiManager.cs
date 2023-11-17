using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BlackJack;

namespace BlackJack {
    public class UiManager : MonoBehaviour {
        //BettingWindow
        public GameObject bettingWindow;
        public TMP_InputField startInput;

        //InsuranceWindow
        public GameObject insuranceWindow;

        //EvenMonyWindow
        public GameObject evenMoneyWindow;

        //ResultWindow
        public GameObject resultWindow;
        public TextMeshProUGUI resultTxt;
        public TextMeshProUGUI playerScoreTxt;
        public TextMeshProUGUI dealerScoreTxt;
        public TextMeshProUGUI bettingTxt;
        public TextMeshProUGUI insuranceTxt;
        public TextMeshProUGUI totalTxt;

        //PlayerBtn
        public GameObject playerBtn;
        public GameObject surrenderBtn;
        public GameObject doubleDownBtn;
        public GameObject splitBtn;

        //scoreBox
        public TextMeshProUGUI PlayerscoreBoxTxt;
        public TextMeshProUGUI DealerscoreBoxTxt;

        public TextMeshProUGUI upCreditTxtTxt;
        public TextMeshProUGUI upBettingTxt;

        private static UiManager instance = null;
        public static UiManager IUiManager {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<UiManager>();
                    if (instance != null) {
                        GameObject obj = new GameObject(typeof(UiManager).Name);
                        instance = obj.AddComponent<UiManager>();
                    }
                }
                return instance;
            }
        }

        private void Awake() {
            if (instance != null)
                Destroy(this.gameObject);
            else
                instance = this;
        }

        public void BettingWindowActive() {
            bettingWindow.SetActive(true);
        }

        public void ResetUI() {
            PlayerscoreBoxTxt.text = "0";
            DealerscoreBoxTxt.text = "0";
        }
    }
}

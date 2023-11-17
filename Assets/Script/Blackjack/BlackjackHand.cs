using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using BlackJack;

namespace BlackJack {
    public class BlackjackHand : MonoBehaviour {
        public List<List<GameObject>> ListPlayerHand { get; set; }
        public List<GameObject> ListDealerHand { get; set; }
        public int CurPlayerHand { get; set; } = 0;
        public GameObject deck;

        void Start() {
            ListPlayerHand = new List<List<GameObject>>();
            ListPlayerHand.Add(new List<GameObject>());
            ListDealerHand = new List<GameObject>();
        }

        public int GetPlayerHandCount() {
            return ListPlayerHand[CurPlayerHand].Count;
        }

        public int GetDealerHandCount() {
            return ListDealerHand.Count;
        }

        public int GetTotalCardCount() {
            return ListPlayerHand[CurPlayerHand].Count + ListDealerHand.Count;
        }

        public int GetPlayerNumSum() {
            int sum = 0;
            bool isAce = false;

            for (int i = 0; i < ListPlayerHand[CurPlayerHand].Count; ++i) {
                int cardValue = ListPlayerHand[CurPlayerHand][i].GetComponent<Card>().CardNumber;

                if (cardValue > 10)
                    cardValue = 10;

                if (cardValue == 1) {
                    cardValue = 11;
                    isAce = true;
                }

                sum += cardValue;
            }

            if (sum > 21 && isAce)
                sum -= 10;

            return sum;
        }

        public int GetDealerNumSum() {
            int sum = 0;
            bool isAce = false;

            for (int i = 0; i < ListDealerHand.Count; ++i) {
                int cardValue = ListDealerHand[i].GetComponent<Card>().CardNumber;

                if (cardValue > 10)
                    cardValue = 10;

                if (cardValue == 1) {
                    cardValue = 11;
                    isAce = true;
                }

                sum += cardValue;
            }

            if (sum > 21 && isAce)
                sum -= 10;

            return sum;
        }

        public void Split() {
            ListPlayerHand.Add(new List<GameObject>());
            ListPlayerHand[1].Add(ListPlayerHand[0][1]);
            ListPlayerHand[0][1].transform.position = ListPlayerHand[0][0].transform.position;
            ListPlayerHand[0][1].GetComponent<SpriteRenderer>().sortingOrder = 0;
            ListPlayerHand[0][1].SetActive(false);
            ListPlayerHand[0].RemoveAt(1);
            deck.GetComponent<Deck>().Draw(Deck.DrawActor.Player);
            GameManager.IGameManager.MoveToAction(GameManager.IGameManager.CheckStartPhase);
        }

        public void HandReset() {
            Deck deckScrip = deck.GetComponent<Deck>();

            for (int i = 0; i < ListDealerHand.Count; ++i) {
                ListDealerHand[i].GetComponent<Card>().MoveCard(new Vector3(0.0f, 6.5f, 0.0f), true, true);
                deckScrip.ListCard.Add(ListDealerHand[i]);
            }

            for (int i = 0; i < ListPlayerHand.Count; ++i) {
                for (int j = 0; j < ListPlayerHand[i].Count; ++j) {
                    ListPlayerHand[i][j].SetActive(true);
                    ListPlayerHand[i][j].GetComponent<Card>().MoveCard(new Vector3(0.0f, 6.5f, 0.0f), true, true);
                    deckScrip.ListCard.Add(ListPlayerHand[i][j]);
                }
            }

            ListDealerHand.Clear();
            ListPlayerHand.Clear();
            ListPlayerHand.Add(new List<GameObject>());
            deckScrip.ShuffleList();
            CurPlayerHand = 0;
        }
    }
}

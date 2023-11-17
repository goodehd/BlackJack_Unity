using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using BlackJack;

namespace BlackJack {
    public class Deck : MonoBehaviour {
        public enum DrawActor {
            Player,
            Dealer,
            Max
        }

        public GameObject card;
        public GameObject cards;
        public List<GameObject> ListCard;

        private readonly string[] typeName = { "Diamond", "Club", "Spade", "Heart" };
        private readonly float startPosX = 2.0f;
        private readonly float startPosY = 2.7f;
        private readonly float offsetX = 0.7f;

        // Start is called before the first frame update
        void Start() {
            ListCard = new List<GameObject>();

            for (int i = 0; i < typeName.Length; ++i) {
                for (int j = 1; j <= 13; ++j) {
                    string fileName = typeName[i] + j.ToString("D2");
                    GameObject newCard = Instantiate(card);
                    Card script = newCard.GetComponent<Card>();
                    newCard.SetActive(false);
                    newCard.transform.SetParent(cards.transform);
                    newCard.name = fileName;
                    newCard.transform.localPosition = new Vector3(0.0f, 0.3f, 0.0f);
                    newCard.GetComponent<SpriteRenderer>().sortingLayerName = "Card";

                    fileName = "Cards/" + fileName;
                    script.FrontSprite = Resources.Load<Sprite>(fileName);
                    script.Suit = (Card.CardSuit)i;
                    script.CardNumber = j;

                    ListCard.Add(newCard);
                }
            }

            ShuffleList();
        }

        private void TestCardSwap(int index1, int index2) {
            GameObject tem = ListCard[index1];
            ListCard[index1] = ListCard[index2];
            ListCard[index2] = tem;
        }

        public void ShuffleList() {
            int n = ListCard.Count;
            while (n > 1) {
                n--;
                int k = Random.Range(0, n + 1);
                GameObject value = ListCard[k];
                ListCard[k] = ListCard[n];
                ListCard[n] = value;
            }
        }

        public void StartDraw() {
            StartCoroutine(CoStartDraw());
        }

        private IEnumerator CoStartDraw() {
            GameManager gameManager = GameManager.IGameManager;

            if (gameManager.BettingCredit <= 0)
                yield break;

            if (ListCard != null) {
                for (int i = 0; i < 2; ++i) {
                    Draw(DrawActor.Player);
                    yield return StartCoroutine(gameManager.CoMoveToAction());
                }

                for (int i = 0; i < 2; ++i) {
                    Draw(DrawActor.Dealer, i == 0);
                    yield return StartCoroutine(gameManager.CoMoveToAction());
                }
            }

            GameManager.IGameManager.CheckStartPhase();
        }

        public void Draw(DrawActor actor, bool rotate = true) {
            BlackjackHand hand = GameManager.IGameManager.Hand;
            int sortingOrder = 0;
            Vector3 movePos = new Vector3();

            int playerHandCount = hand.GetPlayerHandCount();
            int dealerHandCount = hand.GetDealerHandCount();

            if (actor == DrawActor.Player) {
                movePos = new Vector3(-startPosX + offsetX * playerHandCount, -startPosY);
                sortingOrder = playerHandCount;
            } else if (actor == DrawActor.Dealer) {
                movePos = new Vector3(-startPosX + offsetX * dealerHandCount, startPosY);
                sortingOrder = dealerHandCount;
            }

            GameObject Card = ListCard[ListCard.Count - 1];
            ListCard.RemoveAt(ListCard.Count - 1);
            Card.gameObject.SetActive(true);
            Card.GetComponent<Card>().MoveCard(movePos, rotate);
            Card.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
        }
    }
}

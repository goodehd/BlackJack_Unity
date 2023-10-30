using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using BlackJack;
using static UnityEngine.GraphicsBuffer;

namespace BlackJack {
    public class Card : MonoBehaviour {
        public enum CardSuit {
            Diamond,
            Club,
            Spade,
            Heart
        }

        public CardSuit Suit { get; set; }
        public int CardNumber { get; set; }
        public Sprite FrontSprite { get; set; }
        public Sprite backSprite;
        private SpriteRenderer spritRend;

        public bool facedUp { get; set; }
        private bool coroutineAllowed;

        public float moveSpeed;
        private bool isMove = false;

        public GameObject cards;
        // Start is called before the first frame update
        void Start() {
            moveSpeed = 10.0f;
            spritRend = GetComponent<SpriteRenderer>();
            spritRend.sprite = backSprite;
            coroutineAllowed = true;
            facedUp = false;
        }

        public void RoteateCard(bool gameEnd = false) {
            if (coroutineAllowed) {
                StartCoroutine(CoRoteateCard(gameEnd));
            }
        }

        private IEnumerator CoRoteateCard(bool gameEnd) {
            coroutineAllowed = false;

            if (facedUp) {
                for (float i = 0.0f; i <= 180.0f; i += 10.0f) {
                    transform.rotation = Quaternion.Euler(0.0f, i, 0.0f);
                    if (i == 90.0f) {
                        spritRend.sprite = backSprite;
                    }
                    yield return new WaitForSeconds(0.01f);
                }
            } else if (!facedUp) {
                for (float i = 180.0f; i >= 0.0f; i -= 10.0f) {
                    transform.rotation = Quaternion.Euler(0.0f, i, 0.0f);
                    if (i == 90.0f) {
                        spritRend.sprite = FrontSprite;
                    }
                    yield return new WaitForSeconds(0.01f);
                }
            }

            if (transform.position.y > 0.0f) {
                int score = GameManager.IGameManager.Hand.GetDealerNumSum();
                UiManager.IUiManager.DealerscoreBoxTxt.text = score.ToString();
            } else {
                int score = GameManager.IGameManager.Hand.GetPlayerNumSum();
                UiManager.IUiManager.PlayerscoreBoxTxt.text = score.ToString();
            }

            facedUp = !facedUp;
            coroutineAllowed = true;

            if (gameEnd) {
                gameObject.SetActive(false);
                transform.localPosition = new Vector3(0.0f, 0.3f, 0.0f);
            }
        }

        public void MoveCard(Vector3 targetPos, bool rotate, bool gameEnd = false) {
            if (!isMove) {
                GameManager.IGameManager.MovingCardCount++;
                StartCoroutine(CoMoveCard(targetPos, rotate, gameEnd));
            }
        }

        private IEnumerator CoMoveCard(Vector3 targetPos, bool rotate, bool gameEnd) {
            isMove = true;

            while (transform.position != targetPos) {
                float step = moveSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
                yield return null;
            }

            if (rotate)
                RoteateCard(gameEnd);


            if (!gameEnd) {
                if (transform.position.y > 0.0f) {
                    GameManager.IGameManager.Hand.ListDealerHand.Add(this.gameObject);
                } else {
                    GameManager.IGameManager.Hand.ListPlayerHand[GameManager.IGameManager.Hand.CurPlayerHand].Add(this.gameObject);
                }
            }

            GameManager.IGameManager.MovingCardCount--;
            isMove = false;
        }
    }
}

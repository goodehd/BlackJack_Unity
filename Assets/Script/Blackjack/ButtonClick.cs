using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using BlackJack;

namespace BlackJack {
    public class ButtonClick : MonoBehaviour {
        public void DealClick() {
            int credit = GameManager.IGameManager.PlayerCredit;
            string inputTxt = UiManager.IUiManager.startInput.text;

            if (inputTxt == "")
                return;

            int inputCredit = int.Parse(UiManager.IUiManager.startInput.text);
            if (credit > 0 && credit >= inputCredit) {
                GameManager gameManager = GameManager.IGameManager;
                gameManager.UpdateBettingCredit(inputCredit);
                gameManager.NextPhase();

                UiManager uiManager = UiManager.IUiManager;
                uiManager.startInput.text = "";
                uiManager.bettingWindow.SetActive(false);
            }
        }

        public void InsuranceYesClick() {
            GameManager gameManager = GameManager.IGameManager;
            int bettingCredit = gameManager.BettingCredit;
            int playerCredit = gameManager.PlayerCredit;
            int insuranceCredit = Convert.ToInt32(bettingCredit * 0.5f);

            if (insuranceCredit > playerCredit)
                return;

            gameManager.InsuranceCredit = insuranceCredit;

            if (gameManager.Hand.GetDealerNumSum() == 21) {
                gameManager.Hand.ListDealerHand[1].GetComponent<Card>().RoteateCard();
                gameManager.ResultCalculation(insuranceCredit * 2 - bettingCredit, bettingCredit, GameManager.Result.Lose);
            } else {
                gameManager.PlayerTurn();
            }

            UiManager.IUiManager.insuranceWindow.SetActive(false);
        }

        public void InsuranceNoClick() {
            GameManager gameManager = GameManager.IGameManager;
            int bettingCredit = gameManager.BettingCredit;

            if (gameManager.Hand.GetDealerNumSum() == 21) {
                gameManager.Hand.ListDealerHand[1].GetComponent<Card>().RoteateCard();
                gameManager.ResultCalculation(-bettingCredit, bettingCredit, GameManager.Result.Lose);
            } else {
                gameManager.PlayerTurn();
            }

            UiManager.IUiManager.insuranceWindow.SetActive(false);
        }

        public void EvenMoneyYesClick() {
            GameManager gameManager = GameManager.IGameManager;
            int bettingCredit = gameManager.BettingCredit;
            gameManager.Hand.ListDealerHand[1].GetComponent<Card>().RoteateCard();
            gameManager.ResultCalculation(bettingCredit, bettingCredit, GameManager.Result.Win);
            UiManager.IUiManager.evenMoneyWindow.SetActive(false);
        }

        public void EvenMoneyNoClick() {
            GameManager gameManager = GameManager.IGameManager;
            int bettingCredit = gameManager.BettingCredit;

            if (gameManager.Hand.GetDealerNumSum() == 21) {
                gameManager.ResultCalculation(0, bettingCredit, GameManager.Result.Draw);
            } else {
                gameManager.ResultCalculation((int)(bettingCredit * 2.5), bettingCredit, GameManager.Result.Win);
            }

            gameManager.Hand.ListDealerHand[1].GetComponent<Card>().RoteateCard();
            UiManager.IUiManager.evenMoneyWindow.SetActive(false);
        }

        public void ResultWindowClose() {
            UiManager.IUiManager.resultWindow.SetActive(false);
            GameManager.IGameManager.GameReset();
        }

        public void HitClick() {
            GameManager gameManager = GameManager.IGameManager;
            if (gameManager.MovingCardCount != 0 ||
                gameManager.CurPhase != GameManager.Phase.PlayerPhase ||
                gameManager.IsWait)
                return;

            UiManager.IUiManager.splitBtn.SetActive(false);

            gameManager.Hand.deck.GetComponent<Deck>().Draw(Deck.DrawActor.Player);
            gameManager.MoveToAction(gameManager.CheckBust);
        }

        public void StandClick() {
            GameManager gameManager = GameManager.IGameManager;
            if (gameManager.CurPhase != GameManager.Phase.PlayerPhase || gameManager.IsWait)
                return;

            UiManager.IUiManager.splitBtn.SetActive(false);

            gameManager.RecordPlayerHand(gameManager.Hand.GetPlayerNumSum(), gameManager.BettingCredit, false, false);
            if (gameManager.CheckPlayerTurnEnd())
                gameManager.DealerTurn();
            else
                gameManager.NextPlayerHand();
        }

        public void DoubleDownClick() {
            GameManager gameManager = GameManager.IGameManager;
            if (gameManager.CurPhase != GameManager.Phase.PlayerPhase ||
                gameManager.Hand.GetPlayerHandCount() != 2 ||
                gameManager.IsWait)
                return;

            UiManager.IUiManager.splitBtn.SetActive(false);

            StartCoroutine(CoDoubleDown());
        }

        public void SurrenderClick() {
            GameManager gameManager = GameManager.IGameManager;
            if (gameManager.CurPhase != GameManager.Phase.PlayerPhase ||
                gameManager.Hand.GetPlayerHandCount() != 2 ||
                gameManager.IsWait)
                return;

            UiManager.IUiManager.splitBtn.SetActive(false);

            int bettingCredit = gameManager.BettingCredit;
            gameManager.ResultCalculation((int)(-bettingCredit * 0.5f), bettingCredit, GameManager.Result.Lose);
        }

        public void SplitClick() {
            GameManager gameManager = GameManager.IGameManager;
            if (gameManager.CurPhase != GameManager.Phase.PlayerPhase)
                return;

            UiManager.IUiManager.splitBtn.SetActive(false);

            gameManager.Split();
        }

        private IEnumerator CoDoubleDown() {
            GameManager gameManager = GameManager.IGameManager;
            gameManager.IsDoubleDown = true;
            gameManager.Hand.deck.GetComponent<Deck>().Draw(Deck.DrawActor.Player);
            yield return StartCoroutine(gameManager.CoMoveToAction(gameManager.CheckBust));

            if (gameManager.CurPhase == GameManager.Phase.PlayerPhase && gameManager.Hand.GetPlayerNumSum() <= 21) {
                gameManager.RecordPlayerHand(gameManager.Hand.GetPlayerNumSum(), gameManager.BettingCredit * 2, false, false);
                gameManager.DealerTurn();
            }
        }
    }
}

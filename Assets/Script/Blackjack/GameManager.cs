using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;

namespace BlackJack {
    public class GameManager : MonoBehaviour {
        public enum Phase {
            BettingPhase,
            StartPhase,
            PlayerPhase,
            DealerPhase,
            ResultPhase,
            Max
        }

        public enum Result {
            Win,
            Draw,
            Lose,
            Split,
            Max
        }

        public struct HandRecord {
            public int score;
            public int bettingCredit;
            public bool isBlackJack;
            public bool isBust;
        }

        public int PlayerCredit { get; set; } = 1000;
        public int BettingCredit { get; set; } = 0;
        public int InsuranceCredit { get; set; } = 0;
        public Phase CurPhase { get; set; } = Phase.BettingPhase;
        public BlackjackHand Hand { get; set; }
        public int MovingCardCount { get; set; } = 0;
        public bool IsWait { get; set; }
        public bool IsDoubleDown { get; set; }

        private List<HandRecord> listplayerHandRecord;
        private bool dealerBust;

        private readonly string[] resultStr = { "YOU WIN", "PUSH", "YOU lOSE", "SPLIT" };
        private readonly Color[] resultStrColor = { Color.blue, Color.gray, Color.red, Color.gray };

        private static GameManager instance = null;

        public static GameManager IGameManager {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<GameManager>();
                    if (instance != null) {
                        GameObject obj = new GameObject(typeof(GameManager).Name);
                        instance = obj.AddComponent<GameManager>();
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

        void Start() {
            IsDoubleDown = false;
            IsWait = false;
            dealerBust = false;
            MovingCardCount = 0;
            Hand = GetComponent<BlackjackHand>();
            listplayerHandRecord = new List<HandRecord>();
            UiManager.IUiManager.upCreditTxtTxt.text = PlayerCredit.ToString();
        }

        public void NextPhase() {
            CurPhase = CurPhase + 1 == Phase.Max ? Phase.BettingPhase : CurPhase + 1;
        }

        public void CheckStartPhase() {
            // 딜러 공개 카드가 Ace인 경우
            if (Hand.ListDealerHand[0].GetComponent<Card>().CardNumber == 1 &&
                Hand.ListPlayerHand.Count == 1) {
                //플레이어 블랙잭 체크
                if (Hand.GetPlayerNumSum() == 21) {
                    // 이븐머니
                    UiManager.IUiManager.evenMoneyWindow.SetActive(true);
                } else {
                    // 인슈어런스
                    UiManager.IUiManager.insuranceWindow.SetActive(true);
                }
            } else {
                int playerSum = Hand.GetPlayerNumSum();
                int dealerSum = Hand.GetDealerNumSum();
                //플레이어 블랙잭 체크
                if (playerSum == 21 && dealerSum == 21) {
                    ResultCalculation(0, BettingCredit, GameManager.Result.Draw);
                } else if (dealerSum == 21) {
                    ResultCalculation(-BettingCredit, BettingCredit, GameManager.Result.Lose);
                } else if (playerSum == 21) {
                    //ResultCalculation((int)(BettingCredit * 1.5), BettingCredit, GameManager.Result.Win);
                    RecordPlayerHand(0, (int)(BettingCredit * 1.5), true, false);

                    if (CheckPlayerTurnEnd())
                        DealerTurn();
                    else
                        NextPlayerHand();
                } else {
                    PlayerTurn();
                }
            }
        }

        public void PlayerTurn() {
            CurPhase = Phase.PlayerPhase;
            UiManager.IUiManager.playerBtn.SetActive(true);

            int firstNum = Hand.ListPlayerHand[Hand.CurPlayerHand][0].GetComponent<Card>().CardNumber;
            int secondNum = Hand.ListPlayerHand[Hand.CurPlayerHand][1].GetComponent<Card>().CardNumber;
            if (firstNum == secondNum)
                UiManager.IUiManager.splitBtn.SetActive(true);
        }

        public void DealerTurn() {
            //CurPhase = Phase.DealerPhase;
            //Hand.ListDealerHand[1].GetComponent<Card>().RoteateCard();
            //if(Hand.GetDealerNumSum() >= 17) {
            //    ResultTurn();
            //} else {
            //    StartCoroutine(CoDealerPlay());
            //}

            CurPhase = Phase.DealerPhase;
            Hand.ListDealerHand[1].GetComponent<Card>().RoteateCard();
            bool dealerTurnTry = false;

            for (int i = 0; i < listplayerHandRecord.Count; ++i) {
                if (!listplayerHandRecord[i].isBlackJack && !listplayerHandRecord[i].isBust) {
                    dealerTurnTry = true;
                    break;
                }
            }

            if (!dealerTurnTry || Hand.GetDealerNumSum() >= 17)
                ResultTurn();
            else
                StartCoroutine(CoDealerPlay());
        }

        public void ResultTurn() {
            //int playerScore = Hand.GetPlayerNumSum();
            //int DealerScore = Hand.GetDealerNumSum();

            //if(playerScore > DealerScore) {
            //    ResultCalculation(BettingCredit, BettingCredit, GameManager.Result.Win);
            //} else if (playerScore < DealerScore) {
            //    ResultCalculation(-BettingCredit, BettingCredit, GameManager.Result.Lose);
            //} else {
            //    ResultCalculation(0, BettingCredit, GameManager.Result.Draw);
            //}



            CurPhase = Phase.ResultPhase;
            int totalCredit = -InsuranceCredit;
            int bettingCredit = 0;

            for (int i = 0; i < listplayerHandRecord.Count; ++i) {
                HandRecord record = listplayerHandRecord[i];
                bettingCredit += record.bettingCredit;
                if (dealerBust) {
                    if (!record.isBust) {
                        totalCredit += record.bettingCredit;
                    } else {
                        totalCredit -= record.bettingCredit;
                    }
                } else {
                    if (record.isBlackJack) {
                        totalCredit += record.bettingCredit;
                    } else if (record.isBust) {
                        totalCredit -= record.bettingCredit;
                    } else {
                        int DealerScore = Hand.GetDealerNumSum();
                        int playerScore = record.score;

                        if (playerScore > DealerScore) {
                            totalCredit += record.bettingCredit;
                        } else if (playerScore < DealerScore) {
                            totalCredit -= record.bettingCredit;
                        }
                    }
                }
            }

            Result result;
            if (totalCredit == 0)
                result = Result.Draw;
            else if (totalCredit > 0)
                result = Result.Win;
            else
                result = Result.Lose;

            ResultCalculation(totalCredit, bettingCredit, result);
        }

        public void ResultCalculation(int totalCredit, int bettingCredit, Result result) {
            CurPhase = Phase.ResultPhase;

            if (!Hand.ListDealerHand[1].GetComponent<Card>().facedUp)
                Hand.ListDealerHand[1].GetComponent<Card>().RoteateCard();

            UpdatePlayerCredit(totalCredit);

            UiManager uiManager = UiManager.IUiManager;
            uiManager.resultTxt.text = resultStr[(int)result];
            uiManager.resultTxt.color = resultStrColor[(int)result];
            uiManager.playerScoreTxt.text = Hand.GetPlayerNumSum().ToString();
            uiManager.dealerScoreTxt.text = Hand.GetDealerNumSum().ToString();
            uiManager.bettingTxt.text = bettingCredit.ToString();
            uiManager.insuranceTxt.text = InsuranceCredit.ToString();
            uiManager.totalTxt.text = totalCredit.ToString();
            uiManager.resultWindow.SetActive(true);
        }

        public void GameReset() {
            CurPhase = Phase.BettingPhase;
            InsuranceCredit = 0;
            listplayerHandRecord.Clear();
            dealerBust = false;
            UpdateBettingCredit(0);
            Hand.HandReset();
            UiManager.IUiManager.ResetUI();
            MoveToAction(UiManager.IUiManager.BettingWindowActive);
        }

        public void UpdatePlayerCredit(int credit) {
            PlayerCredit += credit;
            UiManager.IUiManager.upCreditTxtTxt.text = PlayerCredit.ToString();
        }

        public void UpdateBettingCredit(int credit) {
            BettingCredit = credit;
            UiManager.IUiManager.upBettingTxt.text = BettingCredit.ToString();
        }

        public void CheckBust() {
            //if (CurPhase == Phase.PlayerPhase && Hand.GetPlayerNumSum() > 21) {
            //    ResultCalculation(-BettingCredit, BettingCredit, GameManager.Result.Lose);
            //} else if (CurPhase == Phase.DealerPhase && Hand.GetDealerNumSum() > 21) {
            //    ResultCalculation(BettingCredit, BettingCredit, GameManager.Result.Win);
            //}

            if (CurPhase == Phase.PlayerPhase && Hand.GetPlayerNumSum() > 21) {
                StartCoroutine(CoWaitAction(PlayerBust, 0.5f));
            } else if (CurPhase == Phase.DealerPhase && Hand.GetDealerNumSum() > 21) {
                dealerBust = true;
                ResultTurn();
            }
        }

        public void PlayerBust() {
            int bettingCredit = BettingCredit;

            if (IsDoubleDown) {
                bettingCredit *= 2;
                IsDoubleDown = false;
            }

            RecordPlayerHand(0, bettingCredit, false, true);

            if (CheckPlayerTurnEnd())
                DealerTurn();
            else
                NextPlayerHand();
        }

        public void RecordPlayerHand(int score, int bettingCredit, bool isBlackJack, bool isBust) {
            HandRecord record = new HandRecord();

            record.score = score;
            record.bettingCredit = bettingCredit;
            record.isBust = isBust;
            record.isBlackJack = isBlackJack;

            listplayerHandRecord.Add(record);
        }

        public void NextPlayerHand() {
            for (int i = 0; i < Hand.ListPlayerHand[Hand.CurPlayerHand].Count; ++i) {
                Hand.ListPlayerHand[Hand.CurPlayerHand][i].SetActive(false);
            }

            Hand.CurPlayerHand++;
            Hand.ListPlayerHand[Hand.CurPlayerHand][0].SetActive(true);
            Hand.deck.GetComponent<Deck>().Draw(Deck.DrawActor.Player);
            GameManager.IGameManager.MoveToAction(GameManager.IGameManager.CheckStartPhase);
        }

        public void Split() {
            Hand.Split();
        }

        public bool CheckPlayerTurnEnd() {
            return Hand.ListPlayerHand.Count - 1 == Hand.CurPlayerHand;
        }

        public int GetPlayerHandCount() {
            return Hand.GetPlayerHandCount();
        }

        public int GetDealerHandCount() {
            return Hand.GetDealerHandCount();
        }

        public int GetTotalCardCount() {
            return Hand.GetTotalCardCount();
        }

        public void MoveToAction(Action Func) {
            StartCoroutine(CoMoveToAction(Func));
        }

        public IEnumerator CoMoveToAction(Action Func) {
            while (true) {
                if (MovingCardCount == 0)
                    break;
                yield return null;
            }
            Func();
        }

        public IEnumerator CoWaitAction(Action Func, float seconds) {
            IsWait = true;
            yield return new WaitForSeconds(seconds);
            Func();
            IsWait = false;
        }

        public IEnumerator CoMoveToAction() {
            while (true) {
                if (MovingCardCount == 0)
                    break;
                yield return null;
            }
        }

        private IEnumerator CoDealerPlay() {
            while (Hand.GetDealerNumSum() < 17) {
                Hand.deck.GetComponent<Deck>().Draw(Deck.DrawActor.Dealer);
                yield return StartCoroutine(CoMoveToAction(CheckBust));
            }

            if (CurPhase == Phase.DealerPhase)
                ResultTurn();
        }
    }
}
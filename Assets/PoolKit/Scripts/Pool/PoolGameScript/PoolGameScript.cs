using UnityEngine;
using System.Collections;
using TNet;

namespace PoolKit
{
	//we are either going to use a 8 ball or 9 ball gamescript depending on which game we are playing. 
	public class PoolGameScript : TNBehaviour 
	{
		//the ref of pool balls
		protected PoolBall[] m_balls;

		//have we fired the ball
		protected bool m_firedBall=false;

		public enum State
		{
            IDLE,
			ROLLING,
			DONE_ROLLING
		};

		//a ref to the white ball
		protected WhiteBall m_whiteBall;

		//what state aer we in
		protected State m_state;

		//the currnet players turn.
		protected int m_playerTurn = 0;

		//did the white ball enter the pocket.
		protected bool m_whiteEnteredPocket;

		//have we "broken the balls"
		protected bool m_break=false;

		//the foul string
		protected string m_foulSTR;
		//the number of balls pocketed
		protected int m_ballsPocketed;

		//whos turn is it
		protected int m_turnCounter = 0;

		//a ref to the current play
		protected PoolKit.BasePlayer m_currentPlayer;
        public PoolKit.BasePlayer CurrentPlayer
        {
            get { return m_currentPlayer; }
            set { m_currentPlayer = value; }
        }

        //the minimum ball speed before the ball is considred stopepd
        public float minBallSpeed = 0.2f;

		//are in we in gameover state yet
		protected bool m_gameover=false;
        protected bool m_gamestarted = false;

        //a ref to the players
        protected PoolKit.BasePlayer[] m_players;

		//do we have a foul
		protected bool m_foul=false;

        static public PoolGameScript Instance;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        void Start () {

			m_balls = (PoolBall[])GameObject.FindObjectsOfType(typeof(PoolBall));
			m_whiteBall = (WhiteBall)GameObject.FindObjectOfType(typeof(WhiteBall));

			for(int i=0; i<m_balls.Length; i++)
			{
				m_balls[i].minSpeed = minBallSpeed;
			}
		}

		void onGameStart()
		{
            BasePlayer[] players = (BasePlayer[])FindObjectsOfType(typeof(BasePlayer));
			m_players = new PoolKit.BasePlayer[players.Length];
			if(m_players.Length>1)
			{
				for(int i=0; i<players.Length; i++)
				{
                    // player index can only be 0 - 1
					int playerIndex = players[i].playerIndex;

                    // makes sure the index mapping
					m_players[playerIndex] = players[i];
				}
			}else{
				m_players  = players;
			}
            m_state = State.IDLE;
            // player 0 plays first
            if (TNManager.isConnected && TNManager.isHosting)
            {
                BaseGameManager.playersTurn(0);
            }
        }
		void OnEnable()
		{
			BaseGameManager.onFireBall += onFireBall;
			BaseGameManager.onBallEnterPocket	+= onEnterPocket;
			BaseGameManager.onWhiteBallHitBall += onWhiteBallHitBall;
			BaseGameManager.onGameStart += onGameStart;
			BaseGameManager.onIsMyTurn += onIsMyTurn;
            BaseGameManager.onPlayerTurn += onPlayerTurn;

		}
		void OnDisable()
		{
			BaseGameManager.onFireBall -= onFireBall;
			BaseGameManager.onBallEnterPocket	-= onEnterPocket;
			BaseGameManager.onWhiteBallHitBall -= onWhiteBallHitBall;
			BaseGameManager.onGameStart-= onGameStart;
			BaseGameManager.onIsMyTurn -= onIsMyTurn;
            BaseGameManager.onPlayerTurn -= onPlayerTurn;
        }
        public bool onIsMyTurn(int playerID)
		{
			return m_currentPlayer.playerIndex == playerID;
		}

        public void onPlayerTurn(int pi)
        {
            if (tno != null)
            {
                tno.Send("playerTurnRFC", Target.All, pi);
            }
            else
            {
                playerTurnRFC(pi);
            }
        }

        [RFC]
        public void playerTurnRFC(int pi)
        {
            //DebugLabel.Instance.ShowMsg("Player " + pi + "'s turn");
            CurrentPlayer = m_players[pi];
            m_players[pi].onMyTurn();
            m_players[1 - pi].onNotMyTurn();
        }

        void onWhiteBallHitBall(bool hitBall,PoolBall ball)
		{
			//its our first hit
			if(hitBall==false)
			{
				handleFirstBallHitByWhiteBall(ball);
			}
		}
		public virtual void handleFirstBallHitByWhiteBall(PoolBall ball)
		{}

		void onEnterPocket(string pocketIndex, PoolBall ball)
		{
            if(TNManager.isHosting)
			    enterPocket(ball);
		}
		public virtual void enterPocket(PoolBall ball)
		{
		}
		public int getNomBallsPocketed()
		{
			return m_ballsPocketed;
		}

		void onFireBall()
		{
			m_state = State.ROLLING;
		}
		void Update () {

            if (m_state == State.DONE_ROLLING)
            {
                if (m_gameover == false)
                    handleWhiteInPocket();
            }
            if (m_state == State.ROLLING)
            {
                if (m_gameover == false)
                    checkDoneRolling();
            }

        }

        void handleWhiteInPocket()
		{
			if(m_whiteEnteredPocket)
			{
				m_whiteEnteredPocket=false;
				if(m_whiteBall)
				{
					m_whiteBall.reset();
				}

			}
		}

        [RFC]
		public virtual void changeTurnRPC(bool foul,int turn)
		{
			m_foul = foul;
			m_playerTurn = turn;
			m_currentPlayer  = m_players[m_playerTurn];

			m_currentPlayer.foul = m_foul;

			Vector3 oldBallPos = Vector3.zero;
			bool hasOldBallpos = false;
			if(m_whiteBall!=null)
			{
				hasOldBallpos=true;
				oldBallPos= m_whiteBall.transform.position;
				//Debug.Log ("disable whiteBall: " + m_whiteBall.name);
				m_whiteBall.gameObject.SetActive(false);
			}

			m_whiteBall = m_currentPlayer.getWhiteBall();
			if(m_whiteBall)
			{
				m_whiteBall.gameObject.SetActive(true);
				if(hasOldBallpos)
				{
					m_whiteBall.transform.position = oldBallPos;
				}
				//Debug.Log ("enable whiteBall: " + m_whiteBall.name);

				m_whiteBall.clear();
				m_whiteBall.setPoolCue(m_currentPlayer.getCue());

				m_whiteBall.foul = m_foul;
			}

			PoolKit.BaseGameManager.showTitleCard( "Player " + m_players[m_playerTurn].playerName + "' turn!");
			
			PoolKit.BaseGameManager.playersTurn(m_playerTurn);
		}

	
		IEnumerator changeTurn(float waitTime)
		{
			yield return new WaitForSeconds(waitTime);
			if(m_gameover==false)
			{
                if (tno != null)
                {
                    tno.Send("changeTurnRPC", Target.All, m_foul, m_playerTurn);
                }
                else
                    changeTurnRPC(m_foul, m_playerTurn);
            }

		}
		void checkDoneRolling()
		{
			bool doneRolling = true;
			for(int i=0; i<m_balls.Length; i++)
			{
                if (m_balls[i] == m_whiteBall)
                    Debug.Log("white ball state: " + m_whiteBall.CurrentState);

				if(m_balls[i] && (m_balls[i].isDoneRolling()==false &&
				   m_balls[i].gameObject.activeInHierarchy))
				{
					doneRolling = false;
				}
			}
			if(doneRolling)
			{
				handleFouls();
                //play the foul sound
                if (m_foul)
                {
                    if (tno != null)
                        tno.Send("ShowFoulMessageRFC", Target.All, m_foulSTR);
                    else
                        ShowFoulMessageRFC(m_foulSTR);
                }


                BaseGameManager.ballStopped();
				m_state = State.DONE_ROLLING;


				//change turn!
				if(m_ballsPocketed==0 || m_whiteEnteredPocket || m_foul)
				{
                    //DebugLabel.Instance.ShowMsg("Change turn");
					if(m_players.Length>1)
					{
						m_playerTurn^=1;

						m_currentPlayer = m_players[m_playerTurn];
					}

					StartCoroutine(changeTurn(2f));
				}else
				{
                    //DebugLabel.Instance.ShowMsg("Dont Change turn");
                    StartCoroutine(changeTurn(2f));
                }
				m_ballsPocketed=0;
				m_turnCounter++;
			}
		}
		public virtual void handleFouls()
		{
		}

		public void clearWallHit()
		{
			for(int i=0; i<m_balls.Length; i++)
			{
				if(m_balls[i])
					m_balls[i].hitWall=false;
			}
		}

        [RFC]
        public void ShowFoulMessageRFC(string msg)
        {
            BaseGameManager.foul("");
            BaseGameManager.showTitleCard(msg);
        }
    }
}

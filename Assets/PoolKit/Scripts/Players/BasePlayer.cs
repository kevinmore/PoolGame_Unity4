﻿using UnityEngine;
using System.Collections;
using TNet;
namespace PoolKit
{
	//the base play for our bowling characters -- wether it be human or AI
	public class BasePlayer : TNBehaviour 
	{
		//a scalar that effects how much our x-component will effect when firing the ball. The smaller it is the easier it should be to get a strike.
		public float xScalar = .25f;
		
		//the power we use to fire the ball
		public float power = 100f;

		//have we already fired the ball.
		protected bool m_fired = false;

		//do we have a gameover yet
		protected bool m_gameOver=false;

		//the player index for the player
		public int playerIndex = 0;

		//is it my turn.
		protected bool m_myTurn = false;

		//the white ball
		protected WhiteBall m_ball;

		//the pool cue
		protected PoolCue m_cue;

		//greater then 8?
		protected int m_greaterThen8 = 0;

		//the balls
		protected PoolBall[] m_balls;

		//the name of the player
		public string playerName = "Player 1";

		//do we have a foul
		public bool foul=true;


		public virtual void Awake()
		{
			m_cue = (PoolCue)GameObject.FindObjectOfType(typeof(PoolCue));
			m_balls= (PoolBall[])GameObject.FindObjectsOfType(typeof(PoolBall));
		}
		public virtual void Start()
		{
			m_ball = (WhiteBall)GameObject.FindObjectOfType(typeof(WhiteBall));
		}

		public virtual void OnEnable()
		{
			BaseGameManager.onGameOver 		+= onGameOver;
			//BaseGameManager.onPlayerTurn 	+= onPlayerTurn;
			BaseGameManager.onResetPlayer 	+= onResetPlayer;
			BaseGameManager.onSetStripesOrSolids += onSetStripesOrSolids;
		}
		public virtual void OnDisable()
		{
			BaseGameManager.onSetStripesOrSolids -= onSetStripesOrSolids;
			BaseGameManager.onGameOver 		-= onGameOver;
			//BaseGameManager.onPlayerTurn 	-= onPlayerTurn;
			BaseGameManager.onResetPlayer 	-= onResetPlayer;
		}
		public void onSetStripesOrSolids(int pi, bool greater8)
		{
			if(playerIndex==pi)
			{
				if(greater8)
				{
					m_greaterThen8=1;
				}else{
					m_greaterThen8=-1;
				}
			}
		}
		public bool areAllBallsDown()
		{

			bool allBallsDown = true;
			if(m_greaterThen8==0)
			{
				allBallsDown=false;
			}
			if(m_greaterThen8!=0)
			{
				PoolBall[] balls =m_balls;
				for(int i=0; i<balls.Length; i++)
				{
					if(balls[i] && balls[i].pocketed==false)
					{
						bool isMyBall = false;
						if(m_greaterThen8==1)
						{
							isMyBall = balls[i].ballType == PoolBall.BallType.STRIPE;
						}
						if(m_greaterThen8==-1)
						{
							isMyBall = balls[i].ballType == PoolBall.BallType.SOLID;
						}
						if(isMyBall)
						{
							allBallsDown=false;
						}
					}
				}
			}
			return allBallsDown;
		}

		
		public virtual void onMyTurn()
		{
            m_fired = false;
            m_myTurn = true;
            //DebugLabel.Instance.ShowMsg("my turn");

            if (m_cue)
			{
				m_cue.greaterThen8 = m_greaterThen8;
				m_cue.areAllBallsDown = areAllBallsDown();
				m_cue.gameObject.SetActive(true);
                m_cue.setPower(0.5f);
                m_cue.CurrentState = PoolCue.State.ROTATE;
			}
		}

        public virtual void onNotMyTurn()
        {
            m_myTurn = false;
            //DebugLabel.Instance.ShowMsg("not my turn, my index " + playerIndex);
        }

//         public void onPlayerTurn(int pi)
// 		{
//             if (tno != null)
//             {
//                 tno.Send("myTurnRPC", Target.All, pi);
//             }
//             else
//                 myTurnRPC(pi);
//         }
// 
//         [RFC]
//         public void myTurnRPC(int pi)
// 		{
//             if (pi == playerIndex)
//             {
//                 
//                 onMyTurn();
//             }
//             else
//             {
//                 onNotMyTurn();
//             }
//         }
        void onGameOver(string vic)
		{
			m_gameOver=true;
		}
		public void onResetPlayer(int pi)
		{
			if(playerIndex==pi)
				onMyTurn();
			reset ();
		}

		public virtual void reset()
		{
			m_fired=false;
		}
// 		void onShotExpires()
// 		{
// 			if(m_myTurn)
// 			{
// 				fireBall();
// 			}
// 		}
// 		public virtual void fireBall()
// 		{
// 		}
		public PoolCue getCue()
		{
			return m_cue;
		}
		
		
		public WhiteBall getWhiteBall()
		{
			
			return m_ball;
		}
	}		

}

using UnityEngine;
using System.Collections;
using TNet;

namespace PoolKit
{

	public class PoolGameScript8Ball : PoolGameScript 
	{

		public override void handleFirstBallHitByWhiteBall(PoolBall ball)
		{
			m_foul = false;
		}
		public override  void enterPocket(PoolBall ball)
		{
//             if (tno != null)
//             {
//                 tno.Send("enterPocketRPC", Target.All, ball.name, m_playerTurn);
//             }
//             else
                enterPocketRPC(ball.name, m_playerTurn);
        }

        [RFC]
		void enterPocketRPC(string name,int playerTurn)
		{
			m_playerTurn = playerTurn;
			GameObject go = GameObject.Find(name);
			PoolBall ball = null;
			if(go)
			{
				ball = go.GetComponent<PoolBall>();
			}


			//we sunk the 8 ball
			if(ball && 
			   ball.ballType == PoolBall.BallType.BLACK)
			{
				m_gameover=true;

				//we got all the balls down.
				if(m_players[m_playerTurn].areAllBallsDown())
				{	
					BaseGameManager.gameover( m_players[m_playerTurn].playerName + " Wins!");
                }
                else
                {
                    BaseGameManager.gameover(m_players[m_playerTurn].playerName + " Loses!");
                }
            }

			if(ball && ball == m_whiteBall)
			{
				m_whiteEnteredPocket = true;
			}
            else if(ball && ball.pocketed==false)
			{
                Debug.Log("Ball pocked: " + ball.ballIndex);
				m_ballsPocketed++;
			}
		}

        [RFC]
		public override void changeTurnRPC(bool foul,
		                                   int turn)
		{
			base.changeTurnRPC(foul,turn);
		}


		//handle the fouls for 8-ball.
		public override void handleFouls()
		{
            m_foul = false;

            if (m_whiteEnteredPocket)
            {
                m_foulSTR = "FOUL - White ball pocketed!";
                m_foul = true;
                clearWallHit();
                return;
            }

            if (m_ballsPocketed > 0)
            {
                m_foulSTR = "";
                m_foul = false;
                m_break = true;
                clearWallHit();
                return;
            }

            int wallHit = 0;
            for (int i = 0; i < m_balls.Length; i++)
            {
                if (m_balls[i] && m_balls[i].pocketed == false && m_balls[i].hitWall)
                {
                    wallHit++;
                }
            }

            if (!m_break)
            {
                if (wallHit < 4)
                {
                    //it was a foul ball.
                    m_break = true;
                    m_foulSTR = "FOUL - At least 4 balls must hit the wall after a break!";
                    m_foul = true;
                    clearWallHit();
                    return;
                }
                else
                {
                    m_break = true;
                }
            }

            if (wallHit == 0 && m_ballsPocketed == 0)
            {
                BaseGameManager.showTitleCard("FOUL - No balls hit wall, or were pocketed!");
                m_foulSTR = "FOUL - No balls hit wall, or were pocketed!";
                m_foul = true;
                clearWallHit();
                return;
            }

            m_foulSTR = "";
            clearWallHit();
        }
	}
}

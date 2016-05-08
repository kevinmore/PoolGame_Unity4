﻿using UnityEngine;
using System.Collections;
namespace PoolKit
{
	//our human player script
	public class HumanPlayer : BasePlayer
	{
		//the rotation speed 
		public float rotationSpeed = 5;

		//the minium angle that we will accept for a flick.
		public float minAngle = 80f;

		//the maximum angle that we will accept for a flick
		public float maxAngle = 100f;

		//we will decrease the power by this ammount for each rank
		public float powerScalarPerLevel = 0.25f;

		//our currnet power 
		private float m_power;

		public Texture fireTexture;

		//the current x-scalar.
		private float m_xScalar = 0f;

		//the layer mask
		public LayerMask layermask;

		//the style to use
		public GUIStyle style0;

        public int m_netWorkPlayerID;

        public override void Awake()
        {
            base.Awake();
            if (TNManager.isConnected)
            {
                m_netWorkPlayerID = tno.ownerID;
                playerName = tno.isMine ? TNManager.player.name : TNManager.players[0].name;

                // check if 2 players are all created
                int playersCount = FindObjectsOfType(typeof(HumanPlayer)).Length;
                if (playersCount == 2)
                {
                    BaseGameManager.startGame();
                }
            }
        }

        public override void Start()
		{
			base.Start();

			m_power = power;
		}
		public override void OnEnable()
		{
			base.OnEnable();
			BaseGameManager.onButtonPress += onButtonPress;
		}

		public override void OnDisable()
		{
			base.OnDisable();
			BaseGameManager.onButtonPress-= onButtonPress;
		}
		public override void onMyTurn()
		{
			base.onMyTurn();
			if(m_cue)
			{
				m_cue.requestRotate();
			}
		}
		void OnGUI()
		{
			if(m_myTurn && tno.isMine)
			{
				Rect r = new Rect(Screen.width-200,Screen.height-70,64,64);
				if(GUI.Button(r,fireTexture,style0) && m_ball.isRolling()==false)
				{
					m_cue.requestFire();
				}
			}
		}
		void onButtonPress(string buttonID)
		{
			if(buttonID.Equals("Fire"))
			{
				//m_cue.requestFire();
			}
		}

        bool IsMyTurn()
        {
            HumanPlayer player = PoolGameScript.Instance.CurrentPlayer as HumanPlayer;
            return player.m_netWorkPlayerID == tno.ownerID && tno.isMine;
        }

        void Update()
		{
			if(!m_myTurn || m_gameOver)
			{
				return;
			}

            if (tno.isMine)
            {
                rotateBall();
            }
        }
		void rotateBall()
		{
			float mx = Input.GetAxis("Mouse X");

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			bool mouseDown = true;
			if(Application.platform==RuntimePlatform.Android || 
			   Application.platform==RuntimePlatform.IPhonePlayer)
			{
				mx = 0;
				if(Input.touchCount>0)
				{
					Touch t0 = Input.touches[0];
					if(t0.phase==TouchPhase.Moved)
					{
						mx = t0.deltaPosition.x;
					}else{
						mouseDown=false;
					}
				}
			}else{
				mouseDown = Input.GetMouseButton(0);
			}

			Vector3 pos = Input.mousePosition;
			bool hitWhiteBall = Physics.Raycast(ray,1000f,layermask.value);


			if(pos.y >60 && hitWhiteBall==false)
			{
				if(mouseDown)
				{
					if(m_ball)
					{
						m_ball.transform.Rotate(new Vector3(0,mx * rotationSpeed * Time.deltaTime,0));
					}
					m_cue.requestRotate();
				}
			}
		}	
	}
}

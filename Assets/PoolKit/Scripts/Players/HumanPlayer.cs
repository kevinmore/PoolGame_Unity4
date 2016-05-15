using UnityEngine;
using System.Collections;
using TNet;

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


        public override void Awake()
        {
            base.Awake();
            if (TNManager.isConnected)
            {
                if(tno.isMine)
                {
                    playerIndex = TNManager.playerID < TNManager.players[0].id ? 0 : 1;
                    playerName = TNManager.player.name;
                }
                else
                {
                    playerIndex = TNManager.playerID < TNManager.players[0].id ? 1 : 0;
                    playerName = TNManager.players[0].name;
                }


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

        void OnGUI()
		{
			if(m_myTurn && tno.isMine && !m_fired)
			{
				Rect r = new Rect(Screen.width-200,Screen.height-70,64,64);
				if(GUI.Button(r,fireTexture,style0) && m_ball.isRolling()==false)
				{
					m_cue.requestFire();
                    m_fired = true;
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
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                if (hit.collider.gameObject != m_ball)
                {
                    //Vector3 targetPos = hit.point + 10f * (hit.point - m_ball.transform.position);
                    m_ball.transform.LookAt(hit.point);

                    // the the mouse button is down, enter fire mode

                }
            }

		}	

    }
}

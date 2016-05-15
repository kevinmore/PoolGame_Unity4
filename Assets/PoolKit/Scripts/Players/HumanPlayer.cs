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

        private bool allowRotate = true;


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

        void onFireBall()
        {
            allowRotate = true;
        }

		public override void OnEnable()
		{
			base.OnEnable();
			BaseGameManager.onButtonPress += onButtonPress;
            BaseGameManager.onFireBall += onFireBall;
            EasyTouch.On_SwipeStart += On_SwipeStart;
            EasyTouch.On_Swipe += On_Swipe;
            EasyTouch.On_SwipeEnd += On_SwipeEnd;
        }

		public override void OnDisable()
		{
			base.OnDisable();
			BaseGameManager.onButtonPress-= onButtonPress;
            BaseGameManager.onFireBall -= onFireBall;
            EasyTouch.On_SwipeStart -= On_SwipeStart;
            EasyTouch.On_Swipe -= On_Swipe;
            EasyTouch.On_SwipeEnd -= On_SwipeEnd;
        }

//         void OnGUI()
// 		{
// 			if(m_myTurn && tno.isMine && !m_fired)
// 			{
// 				Rect r = new Rect(Screen.width-200,Screen.height-70,64,64);
// 				if(GUI.Button(r,fireTexture,style0) && m_ball.isRolling()==false || Input.GetKeyDown(KeyCode.Space))
// 				{
// 					m_cue.requestFire();
//                     m_fired = true;
//                 }
// 			}
// 		}
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

            if (tno.isMine && allowRotate)
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
                    Vector3 targetPos = new Vector3(hit.point.x, m_ball.transform.position.y, hit.point.z);
                    m_ball.transform.LookAt(targetPos);
                    tno.Send("SyncBallRotation", Target.Others, targetPos);
                }
            }
		}


        [RFC]
        public void SyncBallRotation(Vector3 targetPos)
        {
            m_ball.transform.LookAt(targetPos);
        }

        // At the swipe beginning 
        private void On_SwipeStart(Gesture gesture)
        {
            allowRotate = false;
        }

        // During the swipe
        private void On_Swipe(Gesture gesture)
        {

            // the world coordinate from touch for z=5
            Vector3 position = gesture.GetTouchToWorldPoint(5);

            Vector3 shootDirection = m_ball.transform.forward;
            Vector2 swipeVec = gesture.swipeVector.normalized;
            Vector3 swipeDirection = new Vector3(swipeVec.x, 0, swipeVec.y);
            float amount = Vector3.Dot(shootDirection, swipeDirection);
            float newPower = m_cue.getPower() - 0.02f * amount;
            m_cue.setPower(newPower);
        }

        // At the swipe end 
        private void On_SwipeEnd(Gesture gesture)
        {
            // Get the swipe angle
            float angles = gesture.GetSwipeOrDragAngle();
            //Debug.Log("Last swipe : " + gesture.swipe.ToString() + " /  vector : " + gesture.swipeVector.normalized + " / angle : " + angles.ToString("f2"));

            if(!m_ball.isRolling() && !m_fired)
            {
                m_cue.requestFire();
                m_fired = true;
            }
        }
    }
}

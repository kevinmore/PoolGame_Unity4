﻿using UnityEngine;
using System.Collections;
using TNet;

namespace PoolKit
{

	public class PoolBall :TNBehaviour 
	{
		public enum BallType
		{
			STRIPE,
			SOLID,
			BLACK,
			WHITE
		};
		//the type of ball
		public BallType ballType;

		//the ball texture to use.
		public Texture ballTex;
		public Vector3 ballTorque;
//		protected PhotonView m_view;
		protected Rigidbody m_rigidbody;

		//the inital position
		protected Vector3 m_initalPos;

		//the inital rotation 
		protected Quaternion m_initalRot;
		public enum State
		{
			IDLE,
			ROLL,
			DONE
		};

		//what state is the ball in
		protected State m_state = State.IDLE;
        public State CurrentState
        {
            get { return m_state; }
            set { m_state = value; }
        }
        //the minimum speed
        public float minSpeed = 0.5f;

		//the current time the ball has to be slowed down
		protected float m_slowTime;

		//the time the ball has to be slowed down before its considered stopped
		public float slowTime = 1;

		//the balls last position
		protected Vector3 lastPosition = Vector3.zero;

		//the balls currnet speed.
		protected float Speed = 0;
		
		//did we hit the wall.
		public bool hitWall=false;

		//has the ball been pocketed
		public bool pocketed=false;

		//the balls index
		public int ballIndex=-1;
		public virtual void Awake()
		{
			m_rigidbody =gameObject.GetComponent<Rigidbody>();
		}
		public virtual void Start()
		{
			m_initalPos = transform.position;
			m_initalRot = transform.rotation;
			m_rigidbody.useConeFriction=true;
			//m_view = gameObject.GetComponent<PhotonView>();
			
			if(name.Length>4 && ballType!=BallType.WHITE)
			{
				string str = name.Substring(4,name.Length-4);;
				ballIndex = int.Parse(str);
			}
        }
        //point the ball at the target
        public void pointAtTarget(Vector3 target)
		{
			Vector3 vel = m_rigidbody.velocity;
			float mag = vel.magnitude;
			Vector3 newDir = target - transform.position;
			m_rigidbody.velocity = newDir.normalized * mag;
		}


		public virtual void OnCollisionEnter (Collision col){
			if(col.gameObject.name.Contains("Wall"))
			{
				//we hit the wall.
				PoolKit.BaseGameManager.ballHitWall(GetComponent<Rigidbody>().velocity);
				hitWall=true;
			}
			if (col.gameObject.name.Contains("Ball"))
			{
				PoolKit.BaseGameManager.ballHitBall(GetComponent<Rigidbody>().velocity);
			}
		}


		public void OnEnable()
		{
			PoolKit.BaseGameManager.onBallStop += onBallStop;
			PoolKit.BaseGameManager.onFireBall	+= onFireBall;
		}
		public void OnDisable()
		{
			PoolKit.BaseGameManager.onBallStop -= onBallStop;
			PoolKit.BaseGameManager.onFireBall	-= onFireBall;
		}
		public void onFireBall()
		{
            // only the server gets dynamic state
            if (TNManager.isHosting)
            {
                m_rigidbody.isKinematic = false;
                m_state = State.ROLL;
            }
		}
		public void Update()
		{
			if(m_state==State.ROLL)
			{
				if(Speed<minSpeed)
				{
					m_slowTime+=Time.deltaTime;
					if(m_slowTime>slowTime)
					{
						m_state = State.DONE;
					}
				}
            }

            if (TNManager.isHosting && TNManager.players.Count > 0 && tno != null)
                tno.Send("SyncBallTransformRFC", Target.Others, transform.position, transform.rotation);
        }

        void FixedUpdate()
		{
			Speed = (transform.position - lastPosition).magnitude / Time.deltaTime * 3.6f;
			lastPosition = transform.position;
        }


		public void enterPocket()
		{
			if(m_rigidbody)
			{
				pocketed=true;
				//we entered a pocket lets freeze the x and z constraints so it doesnt bounce around. 
                if(!m_rigidbody.isKinematic)
				    m_rigidbody.velocity = Vector3.zero;		

				m_state = State.DONE;
				if(ballType!=BallType.WHITE)
				{
					Destroy(gameObject);
				}else{
					transform.position = m_initalPos;
                    onBallStop();
                }
			}
		}
		public virtual void onBallStop()
		{
			m_rigidbody.isKinematic=false;
            m_rigidbody.angularVelocity = Vector3.zero;
            m_rigidbody.velocity = Vector3.zero;
			m_rigidbody.isKinematic=true;

            // sync one more time
            //tno.Send("SyncBallTransformRFC", Target.Others, transform.position, transform.rotation);
        }

        public bool isDoneRolling()
		{
			return m_state == State.DONE;
		}

        [RFC]
        void SyncBallTransformRFC(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
        }
	}
}

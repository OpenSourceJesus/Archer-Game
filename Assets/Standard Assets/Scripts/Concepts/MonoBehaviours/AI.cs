using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Extensions;

namespace ArcherGame
{
	public class AI : Agent
	{
		public Player myPlayer;
		public Player enemyPlayer;
		ArrowEntry[] shotArrowEntries = new ArrowEntry[0];
		public BehaviorParameters behaviorParameters;
		public float reward;
		public float wrongShootDirectionReward;
		public float arrowDistanceToEnemyRewardMultiplier;
		public float myDamageToEnemyRewardMultiplier;
		public float myDamageFromEnemyRewardMultiplier;
		public float winReward;
		public float loseReward;
		Vector2 aimInput;
		bool shootInput;
		bool previousShootInput;
		Vector2 previousVelocity;
		bool isHittingWall;
		uint maxHp;
		float myPreviousHp;
		float enemyPreviousHp;
		
		public override void Initialize ()
		{
			myPlayer.velocityEffectors_floatDict.Clear();
			foreach (PlatformerEntity.VelocityEffector_float velocityEffector_float in myPlayer.velocityEffectors_float)
				myPlayer.velocityEffectors_floatDict.Add(velocityEffector_float.name, velocityEffector_float);
			myPlayer.velocityEffectors_Vector2Dict.Clear();
			foreach (PlatformerEntity.VelocityEffector_Vector2 velocityEffector_Vector2 in myPlayer.velocityEffectors_Vector2)
				myPlayer.velocityEffectors_Vector2Dict.Add(velocityEffector_Vector2.name, velocityEffector_Vector2);
			myPlayer.whatICollideWith = Physics2D.GetLayerCollisionMask(gameObject.layer);
		}

		void FixedUpdate ()
		{
			RequestDecision ();
		}

		public override void OnActionReceived (float[] vectorAction)
		{
			HandleJumping (vectorAction);
			HandleVelocity (vectorAction);
			HandleAiming (vectorAction);
			HandleShooting (vectorAction);
			myPlayer.speedSqr = myPlayer.rigid.velocity.sqrMagnitude;
			if (myPlayer.Hp < myPlayer.MaxHp - maxHp)
			{
				_AddReward (loseReward);
				EndEpisode();
			}
			if (myPreviousHp > myPlayer.Hp)
				_AddReward ((myPlayer.Hp - myPreviousHp) * myDamageFromEnemyRewardMultiplier);
			myPreviousHp = myPlayer.Hp;
			if (enemyPlayer.Hp < enemyPlayer.MaxHp - maxHp)
			{
				_AddReward (winReward);
				EndEpisode();
			}
			if (enemyPreviousHp > enemyPlayer.Hp)
				_AddReward ((enemyPreviousHp - enemyPlayer.Hp) * myDamageToEnemyRewardMultiplier);
			enemyPreviousHp = enemyPlayer.Hp;
			for (int i = 0; i < shotArrowEntries.Length; i ++)
			{
				ArrowEntry arrowEntry = shotArrowEntries[i];
				if (arrowEntry.arrow == null)
				{
					_AddReward (1f / Mathf.Sqrt(arrowEntry.closestDistanceToEnemySqr) * arrowDistanceToEnemyRewardMultiplier);
					shotArrowEntries = shotArrowEntries.RemoveAt(i);
					i --;
				}
				else
				{
					float distanceToEnemySqr = (arrowEntry.arrow.trs.position - enemyPlayer.trs.position).sqrMagnitude;
					if (distanceToEnemySqr < arrowEntry.closestDistanceToEnemySqr)
						arrowEntry.closestDistanceToEnemySqr = distanceToEnemySqr;
				}
			}
		}

		void HandleJumping (float[] vectorAction)
		{
			bool jumpInput = vectorAction[1] == 1;
			if (jumpInput && myPlayer.isGrounded && !myPlayer.isJumping)
				myPlayer.StartJump ();
			else if (myPlayer.isJumping)
			{
				if (!jumpInput)
					myPlayer.StopJump ();
				if (myPlayer.rigid.velocity.y <= 0)
					myPlayer.isJumping = false;
			}
		}

		void HandleVelocity (float[] vectorAction)
		{
			myPlayer.Move (Vector2.right * (vectorAction[0] - 1));
			myPlayer.SetIsGrounded ();
			SetIsHittingWall ();
			myPlayer.SetVelocityEffector (myPlayer.velocityEffectors_Vector2Dict["Falling"]);
			myPlayer.SetVelocityEffector (myPlayer.velocityEffectors_Vector2Dict["Jump"]);
			myPlayer.rigid.velocity = myPlayer.velocityEffectors_Vector2Dict["Falling"].effect + myPlayer.velocityEffectors_Vector2Dict["Jump"].effect + myPlayer.velocityEffectors_Vector2Dict["Movement"].effect;
			previousVelocity = myPlayer.rigid.velocity;
			if (Mathf.Abs(previousVelocity.x) > myPlayer.maxXVelocityToBeStuck && Mathf.Abs(myPlayer.rigid.position.x - myPlayer.previousXPosition) <= myPlayer.maxXVelocityToBeStuck * Time.deltaTime && !isHittingWall)
				myPlayer.rigid.position += Vector2.up * myPlayer.raiseAmountToGetUnstuck;
			myPlayer.previousXPosition = myPlayer.trs.position.x;
		}

		void SetIsHittingWall ()
		{
			isHittingWall = false;
			if (previousVelocity.x > 0)
			{
				Vector2 offsetPhysicsQuery = Vector2.right * myPlayer.wallLinecastOffset.positionChangeAlongPerpendicular;
				Vector2 bottomRightColliderCorner = new Vector2(myPlayer.ColliderRect.xMax, myPlayer.ColliderRect.yMin);
				Vector2 topRightColliderCorner = myPlayer.ColliderRect.max;
				ContactFilter2D contactFilter = new ContactFilter2D();
				contactFilter.layerMask = myPlayer.whatICollideWith;
				contactFilter.useLayerMask = true;
				if (Physics2D.Linecast(bottomRightColliderCorner + offsetPhysicsQuery + (Vector2.down * myPlayer.wallLinecastOffset.lengthChange1), topRightColliderCorner + offsetPhysicsQuery + (Vector2.up * myPlayer.wallLinecastOffset.lengthChange2), contactFilter, new RaycastHit2D[1]) > 0)
				{
					isHittingWall = true;
					foreach (KeyValuePair<string, PlatformerEntity.VelocityEffector_Vector2> keyValuePair in myPlayer.velocityEffectors_Vector2Dict)
						myPlayer.velocityEffectors_Vector2Dict[keyValuePair.Key].effect.x = Mathf.Min(0, myPlayer.velocityEffectors_Vector2Dict[keyValuePair.Key].effect.x);
				}
			}
			else if (previousVelocity.x < 0)
			{
				Vector2 offsetPhysicsQuery = Vector2.left * myPlayer.wallLinecastOffset.positionChangeAlongPerpendicular;
				Vector2 bottomLeftColliderCorner = myPlayer.ColliderRect.min;
				Vector2 topLeftColliderCorner = new Vector2(myPlayer.ColliderRect.xMin, myPlayer.ColliderRect.yMax);
				ContactFilter2D contactFilter = new ContactFilter2D();
				contactFilter.layerMask = myPlayer.whatICollideWith;
				contactFilter.useLayerMask = true;
				if (Physics2D.Linecast(bottomLeftColliderCorner + offsetPhysicsQuery + (Vector2.down * myPlayer.wallLinecastOffset.lengthChange1), topLeftColliderCorner + offsetPhysicsQuery + (Vector2.up * myPlayer.wallLinecastOffset.lengthChange2), contactFilter, new RaycastHit2D[1]) > 0)
				{
					isHittingWall = true;
					foreach (KeyValuePair<string, PlatformerEntity.VelocityEffector_Vector2> keyValuePair in myPlayer.velocityEffectors_Vector2Dict)
						myPlayer.velocityEffectors_Vector2Dict[keyValuePair.Key].effect.x = Mathf.Max(0, myPlayer.velocityEffectors_Vector2Dict[keyValuePair.Key].effect.x);
				}
			}
		}

		void HandleAiming (float[] vectorAction)
		{
			float normalizedAimInputX = Mathf.InverseLerp(0, behaviorParameters.BrainParameters.VectorActionSize[3] - 1, vectorAction[3]);
			float normalizedAimInputY = Mathf.InverseLerp(0, behaviorParameters.BrainParameters.VectorActionSize[4] - 1, vectorAction[4]);
			aimInput = new Vector2(Mathf.Lerp(-1, 1, normalizedAimInputX), Mathf.Lerp(-1, 1, normalizedAimInputY));
		}

		void HandleShooting (float[] vectorAction)
		{
			shootInput = vectorAction[2] == 1;
			if (shootInput && myPlayer.reloadTimer.timeRemaining <= 0 && myPlayer.canShoot && myPlayer.CurrentArrowEntry.holdingCount > 0 && myPlayer.CurrentArrowEntry.isOwned && myPlayer.CurrentArrowEntry.canShoot)
			{
				Arrow arrow = myPlayer.Shoot(aimInput);
				if (Mathf.Sign(aimInput.x) != Mathf.Sign(enemyPlayer.trs.position.x - myPlayer.shootSpawnPoint.position.x))
					_AddReward (wrongShootDirectionReward);
				ArrowEntry arrowEntry = new ArrowEntry();
				arrowEntry.arrow = arrow;
				arrowEntry.closestDistanceToEnemySqr = (enemyPlayer.trs.position - arrow.trs.position).sqrMagnitude;
				shotArrowEntries = shotArrowEntries.Add(arrowEntry);
			}
			else if (!shootInput && previousShootInput)
				myPlayer.CurrentArrowEntry.canShoot = true;
			previousShootInput = shootInput;
		}

		public override void OnEpisodeBegin ()
		{
			maxHp = myPlayer.maxHp;
			myPlayer.maxHp = int.MaxValue;
			myPlayer.Hp = myPlayer.maxHp;
			if (myPlayer.playerIndex == 1)
			{
				enemyPlayer.maxHp = int.MaxValue;
				enemyPlayer.Hp = enemyPlayer.maxHp;
			}
			myPlayer.previousXPosition = myPlayer.trs.position.x;
		}

		public override void Heuristic (float[] actionsOut)
		{
			actionsOut[0] = Mathf.RoundToInt(InputManager.GetMoveInput(myPlayer.playerIndex));
			actionsOut[1] = InputManager.GetJumpInput(myPlayer.playerIndex).GetHashCode();
			actionsOut[2] = InputManager.GetShootInput(myPlayer.playerIndex).GetHashCode();
			Vector2 aimInput = InputManager.GetAimInput(myPlayer.playerIndex);
			float normalizedAimInputX = Mathf.InverseLerp(-1, 1, aimInput.x);
			actionsOut[3] = Mathf.RoundToInt(Mathf.Lerp(0, behaviorParameters.BrainParameters.VectorActionSize[3] - 1, normalizedAimInputX));
			float normalizedAimInputY = Mathf.InverseLerp(-1, 1, aimInput.y);
			actionsOut[4] = Mathf.RoundToInt(Mathf.Lerp(0, behaviorParameters.BrainParameters.VectorActionSize[4] - 1, normalizedAimInputY));
		}

		void _AddReward (float reward)
		{
			this.reward += reward;
			AddReward (reward);
			// print(this.reward);
		}

		public class ArrowEntry
		{
			public Arrow arrow;
			public float closestDistanceToEnemySqr;
		}
	}
}
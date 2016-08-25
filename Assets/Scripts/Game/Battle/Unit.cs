using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Strategy
{
    public class Unit : MonoBehaviour
    {
        public delegate void UnitDestroyedHandler(Unit unit);
        public static event UnitDestroyedHandler onUnitDestroyedE;

        public delegate void UnitSelectedHandler(Unit unit);
        public static event UnitSelectedHandler onUnitSelectedE;


        public int instanceID;
        public int factionID;
        [HideInInspector]
        public bool isAIUnit = false;

        public Tile tile;	//occupied tile

        public float moveSpeed = 10;

        [HideInInspector]
        public int moveRemain = 1;
        [HideInInspector]
        public int attackRemain = 1;
        [HideInInspector]
        public bool finished = false;

        void Start()
        {

        }


        public bool CanAttack()
        {
            return attackRemain > 0;
        }
        public bool CanMove()
        {
            return moveRemain > 0;
        }

        #region unit operater ----------------------------------------------------------------
        public void Select()
        {
            if (unitAudio != null)
                unitAudio.Select();
            if (onUnitSelectedE != null)
                onUnitSelectedE(this);
        }
        public static void Deselect()
        {
            if (onUnitSelectedE != null)
                onUnitSelectedE(null);
        }

        public void Move(Tile targetTile)
        {
            if (moveRemain <= 0)
                return;

            moveRemain -= 1;
           
            BattleControl.LockUnitSelect();

            StartCoroutine(MoveRoutine(targetTile));
        }
        public IEnumerator MoveRoutine(Tile targetTile)
        {
            tile.unit = null;

            byte[,] grid = TileManager.GetTiledMap().GetCostGrid(TileManager.TerrainToCost);
            List<Tile> path = AStar.SearchWalkableTile(grid,tile,targetTile);

            while (!TurnControl.ClearToProceed())
                yield return null;
            
            TurnControl.ActionCommenced();

            if (unitAnim != null)
                unitAnim.Move();
            if (unitAudio != null)
                unitAudio.Move();


            while (path.Count > 0)
            {
                while (true)
                {
                    float dist = Vector3.Distance(transform.position, path[0].GetPos());
                    if (dist < 0.05f)
                        break;

                    //Quaternion wantedRot = Quaternion.LookRotation(path[0].GetPos() - transform.position);
                    //transform.rotation = Quaternion.Slerp(transform.rotation, wantedRot, Time.deltaTime * moveSpeed * 3);

                    unitAnim.Direction(path[0].GetPos().x - transform.position.x,
                                        path[0].GetPos().y - transform.position.y);
                    Vector3 dir = (path[0].GetPos() - transform.position).normalized;
                    transform.Translate(dir * Mathf.Min(moveSpeed * Time.deltaTime, dist), Space.World);
                    yield return null;
                }

                tile = path[0];

                //FactionManager.UpdateHostileUnitTriggerStatus(this);

                path.RemoveAt(0);
            }

            if (unitAnim != null)
                unitAnim.StopMove();

            tile.unit = this;
            transform.position = tile.GetPos();

            TurnControl.ActionCompleted(0.15f);

            BattleControl.UnlockUnitSelect();
            
            FinishAction();
        }

        public void Attack(Unit targetUnit)
        {
            if (attackRemain == 0)
                return;
            attackRemain -= 1;

            BattleControl.LockUnitSelect();

            //AttackInstance attInstance = new AttackInstance(this, targetUnit);
            //attInstance.Process();


            StartCoroutine(AttackRoutine(targetUnit.tile, targetUnit/*, attInstance*/));

        }

        public IEnumerator AttackRoutine(Tile targetTile, Unit targetUnit/*, AttackInstance attInstance*/)
        {
            while (!TurnControl.ClearToProceed()) 
                yield return null;
            TurnControl.ActionCommenced();

            //play animation
            if (unitAnim != null)
                unitAnim.Attack();
            if (unitAudio != null)
                unitAudio.Attack();

            //TODO attack
            //target fight back?
            
            TurnControl.ActionCompleted(0.15f);
            while (!TurnControl.ClearToProceed()) 
                yield return null;

            FinishAction();
        }
		
        #endregion unit operater ----------------------------------------------------------------

        //called when a unit just reach it's turn
        public void ResetUnitTurnData()
        {
            finished = false;
            moveRemain = 1;
            attackRemain = 1;
            //Restore AP,HP?
        }

        public void FinishAllAction()
        {
            finished = true;
        }
        void FinishAction()
        {
            //if (isAIUnit)
            //    return;
            if (!IsAllActionCompleted())
                BattleControl.SelectUnit(this);
            else
            {
                TurnControl.NextUnit();
            }
        }
        public bool IsAllActionCompleted()
        {
            if (finished)
                return true;
            if (attackRemain > 0)
                return false;
            if (moveRemain > 0)
                return false;
            return true;
        }

        [HideInInspector]
        private UnitAudio unitAudio;
        public void SetAudio(UnitAudio unitAudioInstance)
        {
            unitAudio = unitAudioInstance;
        }

        [HideInInspector]
        private UnitAnimation unitAnim;
        public void SetAnimation(UnitAnimation unitAnimInstance)
        {
            unitAnim = unitAnimInstance;
        }
        public void DisableAnimation()
        {
            unitAnim = null;
        }
    }
}
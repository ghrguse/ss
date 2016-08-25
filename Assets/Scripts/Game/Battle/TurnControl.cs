using UnityEngine;
using System.Collections;

namespace Strategy
{

    public enum _TurnMode
    {
        FactionPerTurn, 			//each faction take turn to move all units in each round
        UnitPerTurn,				//all units (regardless of faction) take turn to move according to the stats, when all unit is moves, the round is completed
    }

    public enum _MoveOrder
    {
        Free, 			//unit switching is enabled
        StatsBased	    //arrange the order based on unit's stats
    }

    public class TurnControl : MonoBehaviour
    {
        [HideInInspector]
        public _TurnMode turnMode;
        public static _TurnMode GetTurnMode() { return instance.turnMode; }

        [HideInInspector]
        public _MoveOrder moveOrder;
        public static _MoveOrder GetMoveOrder() { return instance.moveOrder; }

        public static TurnControl instance;

        private int currentTurnID = -1;	//indicate how many turn has passed

        //this is the flag/counter indicate how many action are on-going, no new action should be able to start as long as this is not clear(>0)
        private static int actionInProgress = 0;

        void Awake()
        {
            if (instance == null)
                instance = this;
        }

        //called in GameControl when endTurn button is pressed, move the turn forward
        //also used when the game first started
        public static void EndTurn() { instance._EndTurn(); }
        public void _EndTurn()
        {
            //if (GameControl.GetGamePhase() == _GamePhase.Over)
            //    return;
            BattleControl.UnlockUnitSelect();
            currentTurnID += 1;

            if (turnMode == _TurnMode.FactionPerTurn)
            {
                if (moveOrder == _MoveOrder.Free)
                    ;// FactionManager.SelectNextFaction();
                //else if (moveOrder == _MoveOrder.StatsBased)
                //    FactionManager.SelectNextUnitInFaction();
            }
            //else if (turnMode == _TurnMode.FactionUnitPerTurn)
            //{
            //    FactionManager.SelectNextUnitInNextFaction();
            //}
            //else if (turnMode == _TurnMode.UnitPerTurn)
            //{
            //    FactionManager.SelectNextUnit();
            //}
        }

        //call by unit when all action is depleted
        public static void NextUnit()
        {
            //if (GameControl.GetGamePhase() == _GamePhase.Over)
            //    return;
            BattleControl.UnlockUnitSelect();

            if (instance.turnMode == _TurnMode.FactionPerTurn)
            {
                FactionManager.SelectNextUnitInFaction();
            }
            //else if (instance.turnMode == _TurnMode.FactionUnitPerTurn)
            //{
            //    EndTurn();
            //}
            //else if (instance.turnMode == _TurnMode.UnitPerTurn)
            //{
            //    EndTurn();
            //}
        }

        //called by all to check if a new action can take place (shoot, move, ability, etc)
        public static bool ClearToProceed()
        {
            return (actionInProgress == 0) ? true : false;
        }

        //called to indicate that an action has been started, prevent any other action from starting
        public static void ActionCommenced()
        {
            actionInProgress += 1;
        }

        //called to indicate that an action has been completed
        public static void ActionCompleted(float delay = 0)
        {
            instance.StartCoroutine(instance._ActionCompleted(delay));
        }
        IEnumerator _ActionCompleted(float delay = 0)
        {
            if (delay > 0)
                yield return new WaitForSeconds(delay);
            actionInProgress = Mathf.Max(0, actionInProgress -= 1);
            yield return null;
        }
    }
}

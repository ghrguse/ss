using UnityEngine;
using System.Collections;

namespace Strategy
{
    [RequireComponent(typeof(TurnControl))]
    public class BattleControl : MonoBehaviour
    {

        public static Unit selectedUnit;	//the current selected unit, only for player's unit

        private static BattleControl instance;
		
		void Awake()
        {
            if (instance == null)
                instance = this;
		}

        private bool allowUnitSelect = true;	//lock unit select after a unit has been moved
        public static bool AllowUnitSelect()
        {
            return instance.allowUnitSelect;
        }
        public static void LockUnitSelect()
        {
            instance.allowUnitSelect = false;
        }
        public static void UnlockUnitSelect()
        {
            instance.allowUnitSelect = true;
        }
        //function to select unit, unit selection start here
        public static void SelectUnit(Unit unit)
        {

            //if (!FactionManager.IsPlayerFaction(unit.factionID))
            //{	//used in FactionUnitPerTurn & UnitPerTurn mode
            //    ClearSelectedUnit();
            //    AIManager.MoveUnit(unit);
            //}
            //else
            {
                ClearSelectedUnit();
                selectedUnit = unit;
                TileManager.Select(unit);
                unit.Select();
            }
        }
        public static void ClearSelectedUnit()
        {
            //if (selectedUnit != null)
            //    selectedUnit.ClearSelectedAbility();
            TileManager.ClearAllTile();
            selectedUnit = null;
            Unit.Deselect();
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
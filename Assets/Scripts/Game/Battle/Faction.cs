using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Strategy
{
    [System.Serializable]
    public class Faction
    {
        public int ID = 0;
        public string name = "Faction";
        public Color color = Color.white;

        public List<Unit> startingUnitList = new List<Unit>();	//unit to be deployed at the start
        public List<Tile> deployableTileList = new List<Tile>();	//tiles available for unit deployment

        public List<Unit> allUnitList = new List<Unit>();

        //public bool isPlayerFaction = false;
        //public bool loadFromData = false;
        //public int dataID = 0;

        //information of unit to replace default startingUnitList, for loadFromData, 
        //refer to Data.cs for class declaration of DataUnit
        //[HideInInspector]
        //public List<DataUnit> dataList = new List<DataUnit>();


        public int selectedUnitID = -1;	//used when move-order are not free, cycle through allUnitList

        //[HideInInspector]
        //public bool foldSpawnInfo = false;	//for editor use only, show or hide spawnInfoList
        //public List<FactionSpawnInfo> spawnInfoList = new List<FactionSpawnInfo>();

        //used in FactionPerTurn, FreeMove order only
        public bool SelectFirstAvailableUnit()
        {
            int index = -1;
            for (int i = 0; i < allUnitList.Count; i++)
            {
                if (!allUnitList[i].IsAllActionCompleted())
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                selectedUnitID = index;
                BattleControl.SelectUnit(allUnitList[selectedUnitID]);
                return true;
            }

            return false;
        }

        //for FactionPerTurn mode, breakWhenExceedLimit is set to true, where selectUnitID resets when it reachs end
         public bool SelectNextUnitInQueue(bool breakWhenExceedLimit = true)
        {
            selectedUnitID += 1;
            if (selectedUnitID >= allUnitList.Count)
            {
                if (breakWhenExceedLimit)
                {	//for FactionPerTurn mode, reset the ID
                    selectedUnitID = -1;
                    return true;
                }
                selectedUnitID = 0;
            }
            allUnitList[selectedUnitID].ResetUnitTurnData();
            BattleControl.SelectUnit(allUnitList[selectedUnitID]);
            return false;
        }


        public void RemoveUnit(Unit unit)
        {
            int index = allUnitList.IndexOf(unit);
            if (index !=-1 && index <= selectedUnitID)
            {
                selectedUnitID -= 1;
            }
            allUnitList.RemoveAt(index);
            //allUnitList.Remove(unit);
        }


        //called by FactionManager.SelectNextFaction in FactionPerTurn mode (resetSelectedID=true)
        public void ResetFactionTurnData()
        {
            selectedUnitID = -1;

            for (int i = 0; i < allUnitList.Count; i++)
                allUnitList[i].ResetUnitTurnData();
        }

        //clear all unit in allUnitList
        public void ClearUnit()
        {
            for (int i = 0; i < allUnitList.Count; i++)
            {
                if (allUnitList[i] != null)
                    MonoBehaviour.DestroyImmediate(allUnitList[i].gameObject);
            }
            allUnitList = new List<Unit>();
        }

    }


    //spawnInfo for a certain area on the grid for a faction
    [System.Serializable]
    public class FactionSpawnInfo
    {
        public enum _LimitType { UnitCount, UnitValue }

        public _LimitType limitType;
        public int limit = 2;

        public List<Unit> unitPrefabList = new List<Unit>();	//the prefab to be spawned
    }

}
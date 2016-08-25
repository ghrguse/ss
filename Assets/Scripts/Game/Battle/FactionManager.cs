using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Strategy
{
    public class FactionManager : MonoBehaviour
    {
        public List<Unit> allUnitList = new List<Unit>();

        private static FactionManager instance;
        public static FactionManager GetInstance()
        {
            return instance;
        }

        public int selectedFactionID = 1;
        public List<Faction> factionList = new List<Faction>();
        public static List<Faction> GetFactionList()
        {
            return instance.factionList;
        }
        public static int GetSelectedFactionID()
        {
            Debug.Log("selectedFactionID:" + instance.selectedFactionID);
            return instance.selectedFactionID;
        }

        void Awake()
        {
            if (instance == null)
                instance = this;
        }

        //called by GameControl to initiate the factions, 
        //load from data when needed, spawn the startingUnit,
        //initiate the unit, check if unit deployment is required....
        public void Init()
        {
            if (instance == null)
                instance = this;

            //setup all the unit in the game
            for (int i = 0; i < factionList.Count; i++)
            {
                for (int n = 0; n < factionList[i].allUnitList.Count; n++)
                {
                    factionList[i].allUnitList[n].isAIUnit = false;
                }
            }

            Vector3 pos = new Vector3(0, 99999, 0);
            Quaternion rot = Quaternion.identity;
            for (int i = 0; i < factionList.Count; i++)
            {
                Faction fac = factionList[i];

                //if load from data, then load the list from data and then put it to startingUnitList
                //if (fac.loadFromData)
                //{
                //    fac.startingUnitList = new List<Unit>();
                //    fac.dataList = Data.GetLoadData(fac.dataID);
                //    if (fac.dataList == null)
                //    {
                //        Debug.LogWarning("TBTK faction's data not setup properly", this);
                //        continue;
                //    }
                //    Debug.Log("unit from data: " + fac.dataList.Count);
                //    for (int n = 0; n < fac.dataList.Count; n++) fac.startingUnitList.Add(fac.dataList[n].unit);

                //    //put the data list back into the end data first, to save the current starting lineup for next menu loading
                //    //in case the player didnt finish the level and GameOver is not called
                //    Data.SetEndData(fac.dataID, fac.dataList);
                //}
                //else
                {
                    //if using default startingUnitList, make sure none of the element in startingUnitList is empty
                    for (int n = 0; n < fac.startingUnitList.Count; n++)
                    {
                        if (fac.startingUnitList[n] == null)
                        {
                            fac.startingUnitList.RemoveAt(n);
                            n -= 1;
                        }
                    }
                }

                for (int n = 0; n < fac.startingUnitList.Count; n++)
                {
                    GameObject unitObj = (GameObject)Instantiate(fac.startingUnitList[n].gameObject, pos, rot);
                    fac.startingUnitList[n] = unitObj.GetComponent<Unit>();
                    unitObj.transform.parent = transform;
                    unitObj.SetActive(false);
                }

            }



            //if (!BattleControl.EnableManualUnitDeployment())
            {
                for (int i = 0; i < factionList.Count; i++)
                {
                    if (factionList[i].startingUnitList.Count <= 0)
                        continue;

                    AutoDeployFaction(i);
                }
            }
        }

        void OnEnable()
        {
            Unit.onUnitDestroyedE += OnUnitDestroyed;
        }
        void OnDisable()
        {
            Unit.onUnitDestroyedE -= OnUnitDestroyed;
        }

        void OnUnitDestroyed(Unit unit)
        {
            //remove the unit from the faction, and if the faction has no active unit remain, the faction itself is remove (out of the game)
            for (int i = 0; i < factionList.Count; i++)
            {
                if (factionList[i].ID == unit.factionID)
                {
                    factionList[i].RemoveUnit(unit);
                    if (factionList[i].allUnitList.Count == 0)
                        factionList.RemoveAt(i);
                    break;
                }
            }

            //if there's only 1 faction remain (since faction with no active unit will be removed), then faction has won the game
            if (factionList.Count == 1)
                ;// BattleControl.GameOver(factionList[0].ID);
        }

        //used in FactionPerTurn mode only
        //switch to next faction in turn and select a unit based on the moveOrder
        public static void SelectNextFaction()
        {
            instance._SelectNextFaction();
        }
        public void _SelectNextFaction()
        {
            BattleControl.ClearSelectedUnit();

            selectedFactionID += 1;
            if (selectedFactionID >= factionList.Count)
                selectedFactionID = 0;
            factionList[selectedFactionID].ResetFactionTurnData();

            if (true) //if it's a player's faction
            {	
                if (TurnControl.GetMoveOrder() == _MoveOrder.Free)
                {
                    factionList[selectedFactionID].SelectFirstAvailableUnit();
                }
                else
                    factionList[selectedFactionID].SelectNextUnitInQueue(true);
            }
            //else
            //{//if it's a AI's faction, execute AI move
            //    Debug.Log("AIManager.MoveFaction ------------------------");
            //    BattleControl.DisplayMessage("AI's Turn");
            //    if (TurnControl.GetMoveOrder() == _MoveOrder.Free)
            //    {
            //        AIManager.MoveFaction(factionList[selectedFactionID]);
            //    }
            //    else
            //    {
            //        SelectNextUnitInFaction();	//when an AI unit pass to BattleControl.Select(), AI routine will be called
            //    }
            //}
        }

        //used in FactionPerTurn mode only, select the next unit in faction
        public static void SelectNextUnitInFaction()
        {
            instance._SelectNextUnitInFaction();
        }
        public void _SelectNextUnitInFaction()
        {
            if (selectedFactionID < 0)
                selectedFactionID = 0;

            if (TurnControl.GetMoveOrder() == _MoveOrder.Free)
            {
                bool unitAvailable = factionList[selectedFactionID].SelectFirstAvailableUnit();
                if (!unitAvailable)
                    _SelectNextFaction();
            }
            else
            {
                //pass true to SelectNextUnitInQueue() so when the all unit in the faction has been selected before, the function will return true
                bool allUnitCycled = factionList[selectedFactionID].SelectNextUnitInQueue(true);
                //original line
                //bool allUnitCycled=factionList[selectedFactionID].SelectNextUnitInQueue(true);	
                if (allUnitCycled)
                    _SelectNextFaction();
            }
        }

        //get the number of active unit on the grid
        public static List<Unit> GetAllUnit()
        {
            return instance._GetAllUnit();
        }
        public List<Unit> _GetAllUnit()
        {
            if (TurnControl.GetTurnMode() == _TurnMode.UnitPerTurn)
                return allUnitList;
            else
            {
                List<Unit> list = new List<Unit>();
                for (int i = 0; i < factionList.Count; i++)
                {
                    for (int n = 0; n < factionList[i].allUnitList.Count; n++)
                        list.Add(factionList[i].allUnitList[n]);
                }
                return list;
            }
        }
		

        public void AutoDeployFaction(int factionID = -1)
        {
            List<Unit> unitList = factionList[factionID].startingUnitList;
            List<Tile> tileList = TileManager.GetDeployableTileList(factionList[factionID].ID);

            for (int i = 0; i < tileList.Count; i++)
            {
                if (!tileList[i].walkable || tileList[i].unit != null)
                {
                    tileList.RemoveAt(i);
                    i -= 1;
                }
            }

            int count = 0;
            for (int i = 0; i < unitList.Count; i++)
            {
                if (tileList.Count == 0)
                    break;

                Unit unit = unitList[i];
                int rand = Random.Range(0, tileList.Count);
                Tile tile = tileList[rand];
                tileList.RemoveAt(rand);
                tile.unit = unit;
                unit.tile = tile;
                unit.transform.position = tile.GetPos();
                unit.gameObject.SetActive(true);
                unit.factionID = factionList[factionID].ID;

                factionList[i].allUnitList.Add(unit);
                count += 1;
            }

            for (int i = 0; i < count; i++)
                unitList.RemoveAt(0);

        }
    }
}
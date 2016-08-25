using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Tiled2Unity;

namespace Strategy
{
    public class TileManager : MonoBehaviour
    {
        public delegate void HoverAttackableHandler(Tile tile);
        public static event HoverAttackableHandler onHoverAttackableTileE;		//listen by UI only

        public delegate void ExitAttackableHandler();
        public static event ExitAttackableHandler onExitAttackableTileE;	    //listen by UI only

        public delegate void HoverWalkableHandler(Tile tile);
        public static event HoverWalkableHandler onHoverWalkableTileE;		    //listen by UI only

        public delegate void ExitWalkableHandler();
        public static event ExitWalkableHandler onExitWalkableTileE;			//listen by UI only

        private static TileManager instance;
        public TiledMap _tileMap;
        public static TiledMap GetTiledMap() { return instance._tileMap; }

        public float tileSize = 1;
        public static float GetTileSize() { return instance.tileSize; }

        public float gridToTileRatio = 1;
        public static float GetGridToTileSizeRatio() { return instance.gridToTileRatio; }

        public static readonly IDictionary<string, byte> TerrainToCost = new Dictionary<string, byte>()
        {
	        { "Forest", 2 },
	        { "Grass", 1 },
	        { "Water", 3 },
	        { "Fence", 255 },
	        { "Tower", 1 },
	        { "Camp", 1 },
        };

        //active cursor and indicator in used during runtime
        public Transform indicatorCursor;
        public Transform indicatorSelected;
        
        public GameObject indicatorWalk;
        private List<GameObject> MoveRangeList = new List<GameObject>();

        public Material sqMatNormal;
        public Material sqMatSelected;
        public Material sqMatWalkable;
        public Material sqMatHostile;

        public static Material GetTileMaterial(_TileState state)
        {
            if(state==_TileState.Default)
                return instance.sqMatNormal;
            else if (state == _TileState.Selected)
                return instance.sqMatSelected;
            else if (state == _TileState.Walkable)
                return instance.sqMatWalkable;
            else if (state == _TileState.Hostile)
                return instance.sqMatHostile;

            return instance.sqMatNormal;
        }

        //temporarily tile list for selected unit storing attackable and walkable tiles, reset when a new unit is selected
        private List<Tile> walkableTileList = new List<Tile>();
        private List<Tile> attackableTileList = new List<Tile>();

        void Awake()
        {
            if (instance == null)
                instance = this;
        }	

        // Use this for initialization
        void Start()
        {
			indicatorCursor = (Transform)Instantiate(indicatorCursor);
			indicatorSelected = (Transform)Instantiate(indicatorSelected);
            indicatorCursor.parent = transform;
            indicatorSelected.parent = transform;

            HideIndicator();


            //TODO 暂时先放这里，应该放在加载地图后
            if (_tileMap == null)
            {
                _tileMap = this.GetComponentInChildren<TiledMap>();
                if (_tileMap)
                    _tileMap.Init();
            }
        }

        private bool targetMode = false;

        // Update is called once per frame
        private Tile hoveredTile;
        private Vector3 cursorPosition;
        // Update is called once per frame
        void Update()
        {
            #if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
			if(Input.touchCount==1){
				Touch touch=Input.touches[0];
					
				cursorPosition=touch.position;
					
				//if(touch.phase==TouchPhase.Ended && targetMode) ClearTargetMode();
				if(touch.phase!=TouchPhase.Began)
                    return;
					
			}
			else return;
            #else
            cursorPosition = Input.mousePosition;
            //if (Input.GetMouseButtonDown(1) && targetMode) _ClearTargetMode();
            //uses individual collider on individual tile, then this section is not required
            return;
            #endif
        }

        public static void OnTileCursorDown(Tile tile)
        {
            instance._OnTileCursorDown(tile);
        }
        public void _OnTileCursorDown(Tile tile)
        {
            //if (targetMode)
            //    targetModeTargetSelected(tile);
            //else
                tile.OnTouchMouseDown();
        }

        //call when cursor just hover over a new tile
        public static void NewHoveredTile(Tile tile)
        {
            instance._NewHoveredTile(tile);
        }
        void _NewHoveredTile(Tile tile)
        {
            _ClearHoveredTile();
            ShowIndicator(tile.GetPos());

        }

        //cleared the tile which has just been hovered over by the cursor
        public static void ClearHoveredTile() 
        {
            instance._ClearHoveredTile();
        }
        void _ClearHoveredTile()
        {
            hoveredTile = null;
            HideIndicator();

        }

        //click on tile
        public static void OnTile(Tile tile)
        {
            instance._OnTile(tile);
        }
        public void _OnTile(Tile tile)
        {
            //if (!FactionManager.IsPlayerTurn()) return;

            if (tile.unit != null)
            {
                //select the unit if the unit belong's to current player in turn
                if (FactionManager.GetSelectedFactionID() == tile.unit.factionID)
                {
                    if (TurnControl.GetMoveOrder() != _MoveOrder.Free)
                        return;
                //    if (TurnControl.GetTurnMode() == _TurnMode.UnitPerTurn) return;

                    if (!BattleControl.AllowUnitSelect())
                        return;
                    if (BattleControl.selectedUnit != null && BattleControl.selectedUnit.tile == tile)
                        return;
                    BattleControl.SelectUnit(tile.unit);

                }
                //if the unit in the tile can be attack by current selected unit, attack it
                else if (attackableTileList.Contains(tile))
                {
                    BattleControl.selectedUnit.Attack(tile.unit);
                }
            }
            //if the tile is within the move range of current selected unit, move to it
            else
            {
                if (walkableTileList.Contains(tile))
                {
                    if (onExitWalkableTileE != null)
                        onExitWalkableTileE();

                    BattleControl.selectedUnit.Move(tile);
                    ClearAllTile();
                }
                else
                    BattleControl.ClearSelectedUnit();
            }
            

            ClearHoveredTile();	//clear the hovered tile so all the UI overlay will be cleared
        }

        //select a unit, setup the walkable, attackable tiles and what not
        public static void Select(Unit unit)
        {
            unit.tile.SetState(_TileState.Selected);
            if (unit.CanMove())
                instance.SetupWalkableTileList(unit);
            if (unit.CanAttack())
                instance.SetupAttackableTileList(unit);
            //if (!unit.CanMove() && instance.attackableTileList.Count == 0)
            //    TurnControl.NextUnit();
            instance.indicatorSelected.position = unit.tile.GetPos();
        }

        //function to setup and clear walkable tiles in range for current selected unit
        private void ClearWalkableTileList()

        {
            for (int i = 0; i < walkableTileList.Count; i++)
            {
                walkableTileList[i].SetState(_TileState.Default);
                //walkableTileList[i].hostileInRangeList = new List<Tile>();
            }
            walkableTileList = new List<Tile>();
        }
        private void SetupWalkableTileList(Unit unit)
        {
            ClearWalkableTileList();
            byte[,] grid = _tileMap.GetCostGrid(TerrainToCost);
            List<Tile> newList = AStar.GetTilesInRange(grid,unit.tile, 5);

            for (int i = 0; i < newList.Count; i++)
            {
                if (newList[i].unit == null)
                {
                    walkableTileList.Add(newList[i]);
                    newList[i].SetState(_TileState.Walkable);
                }
            }
            //SetupHostileInRangeforTile(unit, walkableTileList);
        }

        //function to setup and clear attackable tiles in range for current selected unit
        private void ClearAttackableTileList()
        {
            for (int i = 0; i < attackableTileList.Count; i++)
            {
                attackableTileList[i].SetState(_TileState.Default);
            }
            attackableTileList = new List<Tile>();
        }
        private void SetupAttackableTileList(Unit unit)
        {
            ClearAttackableTileList();
            attackableTileList = SetupHostileInRangeforTile(unit, unit.tile);
            for (int i = 0; i < attackableTileList.Count; i++)
                attackableTileList[i].SetState(_TileState.Hostile);
            
            //ShowHostileIndicator(attackableTileList);
        }

        //given a unit and a list of tiles, 
        //setup the attackable tiles with that unit in each of those given tiles.
        //the attackable tile list are stored in each corresponding tile
        public static List<Tile> SetupHostileInRangeforTile(Unit unit, Tile tile)
        {
            List<Unit> allUnitList = FactionManager.GetAllUnit();
            List<Unit> allEnemyUnitList = new List<Unit>();
            for (int i = 0; i < allUnitList.Count; i++)
            {
                if (allUnitList[i].factionID != unit.factionID)
                    allEnemyUnitList.Add(allUnitList[i]);
            }

            List<Tile> enemyInRangeList = new List<Tile>();

            int range = 1;  //TODO 
            for (int j = 0; j < allEnemyUnitList.Count; j++)
            {
                Tile targetTile = allEnemyUnitList[j].tile;

                if (TileManager.GetDistance(tile, targetTile) > range)
                    continue;

                bool inSight = true;
                if (inSight)
                    enemyInRangeList.Add(targetTile);
            }
            return enemyInRangeList;
        }

        //reset all selection, walkablelist and what not
        public static void ClearAllTile()
        {
            if (BattleControl.selectedUnit != null)
                BattleControl.selectedUnit.tile.SetState(_TileState.Default);
            instance.ClearWalkableTileList();
            instance.ClearAttackableTileList();
            instance.indicatorSelected.position = new Vector3(0, 99999, 0);
            //instance.ClearHostileIndicator();
            //instance.ClearWalkableHostileList();
        }

        //get delployable tile list for certain faction
        public static List<Tile> GetDeployableTileList(int factionID)
        {
            return instance._GetDeployableTileList(factionID);
        }

        public List<Tile> _GetDeployableTileList(int factionID)
        {
            List<Tile> deployableTileList = new List<Tile>();
            for (int i = 0; i < _tileMap.tileList.Count; i++)
            {
                if (_tileMap.tileList[i].deployAreaID == factionID)
                    deployableTileList.Add(_tileMap.tileList[i]);
            }
            return deployableTileList;
        }

        public static void ShowIndicator(Vector3 pos) 
        {
            instance.indicatorCursor.position = pos+new Vector3(0,0,-0.01f);
        }

        public static void HideIndicator()
        {
            instance.indicatorCursor.position = new Vector3(0,99999,0);
			instance.indicatorSelected.position = new Vector3(0,9999,0);
        }

        //get the distance (in term of tile) between 2 tiles, 
        public static int GetDistance(Tile tile1, Tile tile2, bool walkable = false)
        {
            //if (!walkable)
                return instance._tileMap.GetDistance(tile1, tile2);
            //else
            //    return instance._tileMap.GetWalkableDistance(tile1, tile2);
        }
    }
}

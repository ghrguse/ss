using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;  

using Strategy;

namespace Strategy
{
    public enum _TileState { Default, Selected, Walkable, Hostile, Range }


    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class Tile : MonoBehaviour
    {
        private static readonly IDictionary<string, Color> TypeToColor = new Dictionary<string, Color>()
        {
            { "Grass", new Color(0.67f, 1, 0.5f) },
		    { "Forest", new Color(0, 0.5f, 0) },
            { "Water", Color.blue },
            { "Mountain", Color.red },
            { "Fence", Color.red },
        };

        public _TileState state = _TileState.Default;
		public string Terrain = "<no type>";
		public string Note = "<no note>";

        public Unit unit = null;
        public TileAStar aStar;

        public bool walkable = true;
        public int id = 0;
        //coordinate
        public int x = 0;
        public int y = 0;
        public int z = 0;
        public int deployAreaID = -1;	//factionID of units that can be deploy on the tile, -1 means the tile is close

        private void Awake()
        {
            gameObject.AddComponent<MeshRenderer>();
            MeshFilter mf = (MeshFilter)gameObject.AddComponent<MeshFilter>();
		    Mesh mesh = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/Editors/Common/Meshs/" + "Quad.asset", typeof(Mesh));
            if (mesh == null)
            {
                mesh = MakeQuad("Quad");
                //Save the mesh for future use
                AssetDatabase.CreateAsset(mesh, "Assets/Editors/Common/Meshs/" + "Quad.asset");
                AssetDatabase.SaveAssets();
            }
            mf.sharedMesh = mesh;
        }

        void Start()
        {
            SetMaterial(TileManager.GetTileMaterial(_TileState.Default));
        }

        //disable in mobile so it wont interfere with touch input
        #if !UNITY_IPHONE && !UNITY_ANDROID
        //function called when mouse cursor enter the area of the tile, default MonoBehaviour method
        void OnMouseEnter()
        {
            OnTouchMouseEnter();
        }
        //function called when mouse cursor leave the area of the tile, default MonoBehaviour method
        void OnMouseExit()
        {
            OnTouchMouseExit();
        }
        //function called when mouse cursor enter the area of the tile, default MonoBehaviour method
        public void OnMouseDown()
        {
#if UNITY_ANDROID || UNITY_IPHONE  
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))  
#else
            if (EventSystem.current.IsPointerOverGameObject())
#endif
                return;
            TileManager.OnTileCursorDown(this);
        }

        //onMouseDown for right click
        //function called when mouse cursor enter the area of the tile, default MonoBehaviour method
        //used to detech right mouse click on the tile
        /*
        void OnMouseOver(){
            if(Input.GetMouseButtonDown(1)) OnRightClick();
        }
        public void OnRightClick(){
				
        }
        */

        #endif

        //for when using inividual tile collider
        public void OnTouchMouseEnter()
        {
            TileManager.NewHoveredTile(this);
        }
        public void OnTouchMouseExit()
        {
            TileManager.ClearHoveredTile();
        }


        //code execution for when a left mouse click happen on a tile
        public void OnTouchMouseDown()
        {
            TileManager.OnTile(this);
        }



        public Vector3 GetPos() 
        {
            return transform.position;
        }

        public void SetState(_TileState tileState)
        {
            state = tileState;
            SetMaterial(TileManager.GetTileMaterial(state));

            //if(Application.isPlaying){
            //if(state==_TileState.Default) renderer.enabled=false;
            //else renderer.enabled=true;
            //renderer.enabled=true;
            //}
        }

        //used in ability target mode to assign a material directly without changing state
        public void SetMaterial(Material mat)
        {
            GetComponent<Renderer>().material = mat;
            GetComponent<Renderer>().enabled = true;
        }

        public static Mesh MakeQuad(string name)
        {
            //Generate a new Mesh and name it
            Mesh mesh;
            mesh = new Mesh();
            mesh.name = name;

            //Create four vertices for our quad and apply them to the mesh
            Vector3[] vertices = new Vector3[]{
			new Vector3(0.5f,0.5f,0),	
			new Vector3(0.5f,-0.5f,0),
			new Vector3(-0.5f,0.5f,0),
			new Vector3(-0.5f,-0.5f,0),
		    };

                mesh.vertices = vertices;

                //Generate uvs and apply it
                Vector2[] uvs = new Vector2[]{
			    new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(0, 0),	
		    };

                mesh.uv = uvs;

                //Generate triangles
                int[] triangles = new int[]
            {
                0, 1, 2,
                2, 1, 3,
            };

            mesh.triangles = triangles;

            //Since new generated mesh have no normals, recalculate it
            mesh.RecalculateNormals();

            return mesh;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 position = this.transform.position;
            //position.x += 0.5f;
            //position.y -= 0.5f;

            Color drawColor;
            if (!TypeToColor.TryGetValue(this.Terrain, out drawColor))
            {
                drawColor = Color.black;
            }


            Color fillColor = drawColor;
            fillColor.a = 0.25f;

            Gizmos.color = fillColor;
            Gizmos.DrawCube(position, new Vector3(1, 1, 0));

            Gizmos.color = drawColor;
            Gizmos.DrawWireCube(position, new Vector3(1, 1, 0));

        }
    }

}
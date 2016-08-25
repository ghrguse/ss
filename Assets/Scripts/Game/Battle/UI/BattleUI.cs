using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using Strategy;

namespace Strategy
{

	public class BattleUI : MonoBehaviour {

		public float scaleFactor=1;
		public static float GetScaleFactor(){ return instance.scaleFactor; }

        public GameObject endTurnButtonObj;
		public GameObject endUnitButtonObj;

        public Image testImage;
		
		public bool disablePerkMenu=false;

        private static BattleUI instance;
		
		void Awake(){
			instance=this;
			transform.position=Vector3.zero;
		}
		
		// Use this for initialization
		void Start () 
        {
            endUnitButtonObj.SetActive(true);
            testImage.fillAmount = 0.5f;
			
		}

        void OnEnable()
        {
            //FactionManager.onUnitDeploymentPhaseE += OnUnitDeploymentPhase;
            //Unit.onUnitSelectedE += OnUnitSelected;
            //GameControl.onGameOverE += OnGameOver;
		}
		void OnDisable()
        {
            //FactionManager.onUnitDeploymentPhaseE -= OnUnitDeploymentPhase;
            //Unit.onUnitSelectedE -= OnUnitSelected;
			
            //GameControl.onGameOverE -= OnGameOver;
		}
		
		
		void OnUnitDeploymentPhase(bool flag)
        {
            //if(flag)
            //    UIUnitDeployment.Show();
            //else
            //    UIUnitDeployment.Hide();
			//StartCoroutine(_OnUnitDeploymentPhase());
		}
		IEnumerator _OnUnitDeploymentPhase()
        {
			yield return null;	
		}
		
		
		void OnUnitSelected(Unit unit)
        {
			if(unit!=null)
                endUnitButtonObj.SetActive(true);
			else
                endUnitButtonObj.SetActive(false);
		}
		
		
		public void OnEndTurnButton()
        {
            //BattleControl.EndTurn();
            
		}

        public void OnEndUnitButton()
        {
            BattleControl.selectedUnit.FinishAllAction();
            TurnControl.NextUnit();

        }
		
		
		public void OnGameOver(int factionID){ StartCoroutine(ShowGameOverScreen(factionID)); }
		IEnumerator ShowGameOverScreen(int factionID){
			yield return new WaitForSeconds(2);
            //UIGameOver.Show(factionID);
		}
		
		
		// Update is called once per frame
		void Update () {
		
		}
		
	}

}
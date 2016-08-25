using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Strategy;

namespace Strategy
{
	
	[RequireComponent (typeof (Unit))]
	public class UnitAudio : MonoBehaviour {
		
		
		public AudioClip selectSound;
		public AudioClip moveSound;
		public AudioClip attackSound;
		public AudioClip hitSound;
		public AudioClip destroySound;
		
		//~ private AudioSource audioSrc;
		
		// Use this for initialization
		void Awake () {
			//~ audioSrc=gameObject.GetComponent<AudioSource>();
			//~ if(audioSrc==null){
				//~ audioSrc=gameObject.AddComponent<AudioSource>();
				//~ audioSrc.playOnAwake=false;
				//~ audioSrc.loop=false;
			//~ }
			
			Unit unit=gameObject.GetComponent<Unit>();
			if(unit!=null) unit.SetAudio(this);
			//else DestroyImmediate(this);
		}
		
		
		public void Select(){ if(selectSound!=null) AudioManager.PlaySound(selectSound);	}
		
		public void Move(){ if(moveSound!=null)AudioManager.PlaySound(moveSound);	}
		//public void StopMove(){ AudioManager.PlaySound(moveSound);	}
		
		public void Attack(){ if(attackSound!=null)AudioManager.PlaySound(attackSound);	}
		
		public void Hit(){ if(hitSound!=null)AudioManager.PlaySound(hitSound);	}
		
		public float Destroy(){ 
			if(destroySound!=null){
				AudioManager.PlaySound(destroySound);	
				return destroySound.length;
			}
			return 0;
		}
		
	}

}

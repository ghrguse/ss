using UnityEngine;
using System.Collections;

using Strategy;

namespace Strategy
{

	public class UnitAnimation : MonoBehaviour {

		private Unit unit;
        private Animator _animator;
		
        //public AnimationClip clipIdle;
        //public AnimationClip clipMove;
        //public AnimationClip clipAttack;
		
		void Awake () 
        {
			unit=gameObject.GetComponent<Unit>();
			
			if(unit!=null)
            {
                _animator = gameObject.GetComponent<Animator>();
				if(_animator!=null)
                    unit.SetAnimation(this);
			}
			else
                return;
		}

        void Start()
        {

        }
		
		void Update ()
        {
		}

        public void Direction(float x, float y)
        {
            _animator.SetFloat("move_x", x);
            _animator.SetFloat("move_y", y);
        }
		
		public void Move()
        {
            _animator.SetBool("IsMoving", true);
		}
		
        public void StopMove()
        {
            _animator.SetBool("IsMoving", false);
		}
		
		public void Attack(){
            _animator.SetBool("IsAttacking",true);
		}

        public void FinishAttack()
        {
            _animator.SetBool("IsAttacking", false);
        }
	}

}
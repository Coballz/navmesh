using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FSM : MonoBehaviour 
{
    //List of points for patrolling

    //Bullet shooting rate
    protected float shootRate;
    protected float elapsedTime;

    //Tank Turret
    public Transform turret { get; set; }
    public Transform bulletSpawnPoint { get; set; }

    protected virtual void Initialize() { }
    protected virtual void FSMUpdate() { }
    protected virtual void FSMFixedUpdate() { }

	// Use this for initialization
	void Start () 
    {
        Initialize();
	}
	
	// Update is called once per frame
	void Update () 
    {
        FSMUpdate();
	}

    void FixedUpdate()
    {
        FSMFixedUpdate();
    }    
}


using System;
using System.Collections.Generic;
using UnityEngine;

public class AStarTest : MonoBehaviour
{
    //private AStar aStar;

    //[SerializeField] private NPCPath npcPath = null;
    [SerializeField] private bool moveNPC = true;
    [SerializeField] private Vector2Int finishPosition;
    [SerializeField] private AnimationClip idleDownAnimationClip = null;
    [SerializeField] private AnimationClip eventAnimationClip = null;
    //private NPCMovement npcMovement;
    private Vector2Int lastPosition;
    private float pauseTimeSofar;
    private List<GameObject> npcs;
    System.Random rand = new System.Random();

    private void Start()
    {
        npcs = new List<GameObject>();
        npcs.Add(GameObject.Find("NPC_Butch"));
        npcs.Add(GameObject.Find("NPC_Butch2"));

        lastPosition = new Vector2Int(-100, -100);
        pauseTimeSofar = 0;
    }

    private void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 player_position = player.transform.position;
        //Debug.Log("World" + player_position.ToString());

        Grid grid = GameObject.FindObjectOfType<Grid>();
        // Get grid position for crop
        Vector3Int player_grid_position = grid.WorldToCell(player_position);

        pauseTimeSofar += Time.deltaTime;

        if (pauseTimeSofar > 4.0 && (Math.Abs(player_grid_position.x - lastPosition.x) > 1 || Math.Abs(player_grid_position.y - lastPosition.y) > 1))
        {
            pauseTimeSofar = 0;
            lastPosition.x = player_grid_position.x;
            lastPosition.y = player_grid_position.y;

            int bias_x = -2; //for npc position bias
            for (int i = 0; i < npcs.Count; i++)
            //foreach (GameObject npc in npcs)
            {
                NPCPath npcPath = npcs[i].GetComponent<NPCPath>();
                NPCMovement npcMovement = npcPath.GetComponent<NPCMovement>();
                npcMovement.npcFacingDirectionAtDestination = Direction.down;
                npcMovement.npcTargetAnimationClip = idleDownAnimationClip;

                //Debug.Log("Player Grid Position " + player_grid_position.ToString());

                Stack<NPCMovementStep> npcMovementStepStack = npcPath.npcMovementStepStack;
                //Debug.Log("Stack Count " + npcMovementStepStack.Count.ToString());
                if (moveNPC && npcMovementStepStack.Count == 0)
                {
                    Vector2Int newPosition = new Vector2Int(player_grid_position.x + bias_x + rand.Next(-1, 1), player_grid_position.y + rand.Next(0, 4) - 2);
                    bias_x += 10;
                    Debug.Log("AStarTest Adding newPosition " + newPosition.ToString());
                    NPCScheduleEvent npcScheduleEvent = new NPCScheduleEvent(0, 0, 0, 0, Weather.none, Season.none, SceneName.Scene1_Farm, new GridCoordinate(newPosition.x, newPosition.y), eventAnimationClip);
                    npcPath.BuildPath(npcScheduleEvent);
                }
            }
        }

    }
}

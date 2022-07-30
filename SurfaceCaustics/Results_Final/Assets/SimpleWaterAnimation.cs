using System.Collections.Generic;
using UnityEngine;

public class SimpleWaterAnimation : MonoBehaviour
{
    private List<Vector3> movementDirections;

    private bool movingToDirection;

    private bool movingBackHome;

    private Vector3? currentDirection;

    private Vector3 targetPosition;

    private Vector3 homePosition;

    public float MaxDisplacement = 3;

    public float Speed = 1;


    // Start is called before the first frame update
    void Start()
    {
        this.movementDirections = new List<Vector3>()
        {
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 1)
        };

        this.homePosition = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.currentDirection == null)
        {
            this.currentDirection = this.PickRandomDirection();
            this.targetPosition = this.transform.position + ((Vector3)this.currentDirection * this.MaxDisplacement);
            this.movingToDirection = true;
            this.movingBackHome = false;
            return;
        }

        if (this.movingToDirection)
        {
            if (Vector3.Distance(this.transform.position, this.targetPosition) <= 0.5f)
            {
                this.currentDirection = -this.currentDirection;
                this.movingToDirection = false;
                this.movingBackHome = true;
            }
            else
            {
                this.transform.Translate((Vector3)this.currentDirection * this.Speed * Time.smoothDeltaTime);
            }
        }
        else if (this.movingBackHome)
        {
            if (Vector3.Distance(this.transform.position, this.homePosition) > 0.5f)
            {
                this.transform.Translate((Vector3)this.currentDirection * this.Speed * Time.smoothDeltaTime);
            }
            else
            {
                this.currentDirection = null;
                this.movingBackHome = false;
            }
        }
    }

    private Vector3 PickRandomDirection()
    {
        int movementIndex = Random.Range(0, this.movementDirections.Count - 1);
        int oppositeDirection = Random.RandomRange(0, 1) == 0 ? -1 : 1;
        return this.movementDirections[movementIndex] * oppositeDirection;
    }
}

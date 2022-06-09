using UnityEngine;

public class AnimatedFish : MonoBehaviour
{
    public float Speed = 0.00003f;

    public GameObject PrimaryObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Transform primaryObjectTransform = this.PrimaryObject.transform;
        //Vector3 currentPosition = primaryObjectTransform.position;
        //Vector3 forwardDirection = primaryObjectTransform.forward * 2;
        //Vector3 rightDirection = primaryObjectTransform.right * 0.5f;
        //Vector3 desiredPosition = currentPosition + forwardDirection + rightDirection;
        //Vector3 currentToDesired = (desiredPosition - currentPosition).normalized;

        //this.transform.LookAt(desiredPosition * 0.03f);
        //this.transform.Translate(currentToDesired * this.Speed * Time.smoothDeltaTime);
        //this.PrimaryObject.transform.LookAt(currentToDesired);
        this.transform.Rotate(new Vector3(0, 1, 0), this.Speed * Time.smoothDeltaTime);
    }
}

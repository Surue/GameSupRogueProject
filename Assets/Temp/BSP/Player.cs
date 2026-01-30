using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _delayBeforeTeleport;
    
    private float _timer = 0;

    private void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveX, 0, moveY);
        movement = movement.normalized; //movement.Normalize();
        
        transform.position += movement * (Time.deltaTime * _moveSpeed);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (LayerMask.LayerToName(other.gameObject.layer) == "Teleport")
        {
            _timer = 0;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (LayerMask.LayerToName(other.gameObject.layer) == "Teleport")
        {
            _timer += Time.deltaTime;

            if (_timer >= _delayBeforeTeleport)
            {
                Teleporter teleporter = other.gameObject.GetComponent<Teleporter>();
                Teleporter teleporterToTeleportTo = teleporter.GetDestinationTeleporter();
                transform.position = teleporterToTeleportTo.transform.position;
            }
        }
    }
}

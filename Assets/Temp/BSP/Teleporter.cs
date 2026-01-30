using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private Teleporter _destinationTeleporter;

    public Teleporter GetDestinationTeleporter()
    {
        return _destinationTeleporter;
    }

    public void SetDestinationTeleporter(Teleporter destinationTeleporter)
    {
        _destinationTeleporter = destinationTeleporter;
    }
    
    private void OnDrawGizmos()
    {
        if (_destinationTeleporter != null)
        {
            Gizmos.DrawLine(transform.position, _destinationTeleporter.transform.position);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 1.0f);
        }
    }
}

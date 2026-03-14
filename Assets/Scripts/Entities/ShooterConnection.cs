using UnityEngine;

public class ShooterConnection : MonoBehaviour
{
    public Shooter Head { get; private set;}
    public Shooter Tail { get; private set;}

    [SerializeField]
    private LineRenderer _lineRenderer;

    private void FixedUpdate() 
    {
        if (Head == null || Tail == null)
            return;

        _lineRenderer.SetPosition(0, Head.transform.position);
        _lineRenderer.SetPosition(1, Tail.transform.position);
    }

    public void Setup(Shooter headShooter, Shooter tailShooter)
    {
        Head = headShooter;
        Tail = tailShooter;

        headShooter.Connection = this;
        tailShooter.Connection = this;
    }
}

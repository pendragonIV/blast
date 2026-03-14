using UnityEngine;
using UnityEngine.Pool;

public class ParticleSystemVFX : MonoBehaviour
{
    public IObjectPool<ParticleSystemVFX> ObjectPool;

    private void OnEnable() 
    {
        Invoke("ReleaseVFX", 0.5f);
    }

    void ReleaseVFX()
    {
        if (ObjectPool == null)
            return;

        ObjectPool.Release(this);
    }
}

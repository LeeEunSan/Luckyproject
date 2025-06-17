using UnityEngine;
using SWS;

public class EnemyFindWayPoint : MonoBehaviour
{
    [SerializeField] public splineMove splineMove;
    private PathManager pathManager;

    void Awake()
    {
        pathManager = FindObjectOfType<PathManager>();
        if (pathManager == null)
        {
            Debug.LogError("씬에서 PathManager를 찾을 수 없습니다! Hierarchy에 PathManager가 있는지 확인하세요.");
            return;
        }

        splineMove.pathContainer = pathManager;
        //Debug.Log($"Assigned PathManager to {gameObject.name}: {pathManager.gameObject.name}");

        splineMove.onStart = (true);
    }
}

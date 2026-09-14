using UnityEngine;

public class BackgroundDecorationsController : MonoBehaviour
{
    [SerializeField] private Transform planetTop;
    [SerializeField] private Transform planetBottom;
    [SerializeField, Min(0f)] private float topSpeed = 0.7f;
    [SerializeField, Min(0f)] private float bottomSpeed = 0.7f;
    [SerializeField, Min(0f)] private float startMargin = 3f;
    [SerializeField, Min(0f)] private float bottomDelay = 4f;
    [SerializeField, Min(0f)] private float recycleMargin = 4f;

    private Camera mainCamera;
    private SpriteRenderer planetTopRenderer;
    private SpriteRenderer planetBottomRenderer;
    private float planetTopY;
    private float planetTopZ;
    private float planetBottomY;
    private float planetBottomZ;

    private void Awake()
    {
        mainCamera = Camera.main;
        planetTopRenderer = GetRenderer(planetTop);
        planetBottomRenderer = GetRenderer(planetBottom);

        if (planetTop != null)
        {
            planetTopY = planetTop.position.y;
            planetTopZ = planetTop.position.z;
        }

        if (planetBottom != null)
        {
            planetBottomY = planetBottom.position.y;
            planetBottomZ = planetBottom.position.z;
        }

        PlaceInitialPlanets();
    }

    private void Update()
    {
        if (mainCamera == null ||
            GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        MovePlanet(planetTop, planetTopY, planetTopZ, topSpeed);
        MovePlanet(planetBottom, planetBottomY, planetBottomZ, bottomSpeed);
        RecycleExitedPlanet(
            planetTop,
            planetTopRenderer,
            planetTopY,
            planetTopZ);
        RecycleExitedPlanet(
            planetBottom,
            planetBottomRenderer,
            planetBottomY,
            planetBottomZ);
    }

    private void PlaceInitialPlanets()
    {
        if (mainCamera == null)
        {
            return;
        }

        float cameraRight = GetCameraRight();

        PlacePlanet(
            planetTop,
            planetTopY,
            planetTopZ,
            cameraRight + startMargin);
        PlacePlanet(
            planetBottom,
            planetBottomY,
            planetBottomZ,
            cameraRight + startMargin + bottomDelay);
    }

    private void RecycleExitedPlanet(
        Transform planet,
        SpriteRenderer renderer,
        float originalY,
        float originalZ)
    {
        if (planet == null || renderer == null)
        {
            return;
        }

        float cameraLeft = GetCameraLeft();
        if (renderer.bounds.max.x < cameraLeft - recycleMargin)
        {
            PlacePlanet(
                planet,
                originalY,
                originalZ,
                GetCameraRight() + recycleMargin);
        }
    }

    private static void MovePlanet(
        Transform planet,
        float originalY,
        float originalZ,
        float speed)
    {
        if (planet == null)
        {
            return;
        }

        Vector3 position = planet.position;
        position.x -= speed * Time.deltaTime;
        position.y = originalY;
        position.z = originalZ;
        planet.position = position;
    }

    private static void PlacePlanet(
        Transform planet,
        float originalY,
        float originalZ,
        float x)
    {
        if (planet == null)
        {
            return;
        }

        planet.position = new Vector3(x, originalY, originalZ);
    }

    private float GetCameraRight()
    {
        return mainCamera.transform.position.x +
            mainCamera.orthographicSize * mainCamera.aspect;
    }

    private float GetCameraLeft()
    {
        return mainCamera.transform.position.x -
            mainCamera.orthographicSize * mainCamera.aspect;
    }

    private static SpriteRenderer GetRenderer(Transform target)
    {
        return target != null
            ? target.GetComponent<SpriteRenderer>()
            : null;
    }
}

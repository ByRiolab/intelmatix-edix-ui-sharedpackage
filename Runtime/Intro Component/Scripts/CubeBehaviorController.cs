using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Intelmatix
{
    /// <summary>
    /// Controla el comportamiento de un conjunto de cubos en una escena.
    /// </summary>
    public class CubeBehaviorController : MonoBehaviour
    {
        [SerializeField] private Transform cubesParent;
        private readonly List<(Vector3 position, Quaternion rotation, Vector3 scale)> initialTransforms = new();
        [SerializeField] private Camera cubesCamera;

        [SerializeField] private float wordTransitionSpeed = 3;
        [SerializeField] private float cubeTransitionSpeed = 2;
        [SerializeField] private float rotationSpeed = 3;
        [SerializeField] private LeanTweenType menuEaseType = LeanTweenType.easeInOutBack;
        [SerializeField] private LeanTweenType wordEaseType = LeanTweenType.easeInOutBack;
        [SerializeField] private LeanTweenType cubeEaseType = LeanTweenType.easeInOutBack;

        [SerializeField] private bool cubeWordLoop = false;
        [SerializeField] private float loopTimeout = 5;

        private int alphaID;
        private List<Transform> cubes = new();

        private void Awake()
        {
            cubes = cubesParent.GetComponentsInChildren<Transform>().ToList();
            cubes.Remove(cubesParent);

            alphaID = Shader.PropertyToID("_CubeAlpha");
            foreach (Transform cube in cubes)
            {
                initialTransforms.Add((cube.localPosition, cube.localRotation, cube.localScale));
            }
            SetupCube();
        }

        /// <summary>
        /// Configura la visualización de palabras.
        /// </summary>
        private void SetupWordDisplay()
        {
            ShuffleCubesList();

            LeanTween.rotate(cubesParent.gameObject, Vector3.zero, rotationSpeed * 0.5f);

            for (int i = 0; i < Mathf.Min(initialTransforms.Count, cubes.Count); i++)
            {
                var (pos, rot, scale) = initialTransforms[i];
                var cube = cubes[i];

                var cubeGameObject = cube.gameObject;

                LeanTween.cancel(cubeGameObject);

                var cubeRenderer = cube.GetComponent<MeshRenderer>();
                var startMaterial = cubeRenderer.material.GetFloat(alphaID);

                LeanTween.value(cubeGameObject, startMaterial, 1, wordTransitionSpeed)
                    .setEase(menuEaseType)
                    .setOnUpdate(value => cubeRenderer.material.SetFloat(alphaID, value));

                LeanTween.moveLocal(cubeGameObject, pos, wordTransitionSpeed).setEase(wordEaseType);
                LeanTween.rotateLocal(cubeGameObject, rot.eulerAngles, UnityEngine.Random.Range(0.5f, 0.75f) * wordTransitionSpeed).setEase(wordEaseType);
                LeanTween.scale(cubeGameObject, scale, wordTransitionSpeed).setEase(wordEaseType);
            }

            LeanTween.cancel(gameObject);
            if (cubeWordLoop)
                Invoke(nameof(SetupCube), loopTimeout);

        }

        /// <summary>
        /// Configura los cubos para que se posicionen como un cubo compuesto.
        /// </summary>
        private void SetupCube()
        {
            if (cubes == null) return;

            ShuffleCubesList();
            ResetVisibility();

            LeanTween.cancel(gameObject);

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        int index = x * 9 + y * 3 + z;

                        if (index >= cubes.Count) continue;

                        var cube = cubes[index].gameObject;
                        LeanTween.cancel(cube);

                        if (!cube.activeSelf)
                            cube.SetActive(true);

                        var cubeRenderer = cube.GetComponent<MeshRenderer>();
                        var startMaterial = cubeRenderer.material.GetFloat(alphaID);

                        LeanTween.value(cube, startMaterial, .25f, cubeTransitionSpeed)
                            .setEase(menuEaseType)
                            .setOnUpdate(value => cubeRenderer.material.SetFloat(alphaID, value))
                            .setDelay(cubeTransitionSpeed)
                            .setOnComplete(() =>
                            {
                                LeanTween.value(cube, .25f, 1, cubeTransitionSpeed)
                                    .setEase(menuEaseType)
                                    .setOnUpdate(value => cubeRenderer.material.SetFloat(alphaID, value));
                            });

                        LeanTween.moveLocal(cube, new Vector3(x, y, z) - Vector3.one, cubeTransitionSpeed).setEase(cubeEaseType);
                        LeanTween.rotateLocal(cube, Vector3.zero, cubeTransitionSpeed).setEase(cubeEaseType);
                        LeanTween.scale(cube, Vector3.one, cubeTransitionSpeed).setEase(cubeEaseType);
                    }
                }
            }

            for (int i = 27; i < cubes.Count; i++)
            {
                var cube = cubes[i].gameObject;
                LeanTween.cancel(cube);
                LeanTween.scale(cube, Vector3.zero, cubeTransitionSpeed).setEase(cubeEaseType);
            }

            LeanTween.cancel(cubesParent.gameObject);
            LeanTween.rotate(cubesParent.gameObject, new Vector3(45, 45, 0), rotationSpeed);

            Invoke(nameof(SetupWordDisplay), loopTimeout);
        }

        /// <summary>
        /// Colapsa los cubos.
        /// </summary>
        public void Collapse() => SetupCube();

        /// <summary>
        /// Expande la visualización de palabras.
        /// </summary>
        public void Expand() => SetupWordDisplay();

        /// <summary>
        /// Ordena aleatoriamente la lista de cubos,
        /// </summary>
        private void ShuffleCubesList()
        {
            try
            {
                // Get the total number of cubes
                int cubeCount = cubes.Count;
                // Create a new random number generator
                System.Random rng = new();

                // Loop through the list of cubes
                for (int index = cubeCount - 1; index > 0; index--)
                {
                    // Generate a random index within the range of remaining cubes
                    int randomIndex = rng.Next(index + 1);
                    // Swap the current cube with a randomly selected cube
                    (cubes[index], cubes[randomIndex]) = (cubes[randomIndex], cubes[index]);
                }
            }
            catch (System.Exception ex) // *Catch any exceptions that might occur
            {
                // If an exception occurs, print an error message
                System.Console.WriteLine($"Error al barajar los cubos: {ex.Message}");
                // Handle the exception according to your application's requirements
            }
        }

        /// <summary>
        /// Restablece los CanvasGroup a su estado inicial.
        /// </summary>
        private void ResetVisibility()
        {
            LeanTween.cancel(gameObject);
            cubesCamera.enabled = true;
            foreach (var cube in cubes)
            {
                cube.gameObject.SetActive(true);
            }
        }

        public void Stop()
        {
            LeanTween.cancel(gameObject);
            LeanTween.cancel(cubesParent.gameObject);
            CancelInvoke();
            foreach (var cube in cubes)
            {
                LeanTween.cancel(cube.gameObject);
            }
        }

        public static Vector2 GetRandomNormalizedDirection()
        {
            // Generate random angle in radians
            float randomAngle = Random.Range(0f, Mathf.PI * 2f);

            // Convert angle to a direction vector
            Vector2 randomDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));

            // Normalize the vector to get a unit vector
            randomDirection.Normalize();

            return randomDirection;
        }

        public void MoveCubesToPositions(RectTransform[] positions, float scaleFactor)
        {
            for (int i = 0; i < cubes.Count; i++)
            {
                if (i < positions.Length)
                {
                    var rectTransform = positions[i];
                    // Get the center position of the RectTransform's rectangle
                    Vector3 centerPosition = rectTransform.TransformPoint(rectTransform.rect.center);
                    // Obtener la posición en la pantalla del objeto UI
                    Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(null, centerPosition);

                    // Convertir la posición de la pantalla a una posición en el mundo
                    Vector3 worldPosition = cubesCamera.ScreenToWorldPoint(new Vector3(screenPosition.x / scaleFactor, screenPosition.y / scaleFactor, 15));

                    // Asignar la posición en el mundo al objeto que deseas mover
                    cubes[i].LeanMove(worldPosition, 1.85f).setEaseInOutCubic();
                    cubes[i].LeanRotate(new(-25, 50, -25 + 360 * 3), 2f).setEaseOutCubic();
                    cubes[i].LeanScale(Vector3.one * 0.5f, 1f);
                }
                else
                {
                    cubes[i].LeanScale(Vector3.zero, 1f).setEaseInCirc();

                    var direction = GetRandomNormalizedDirection() * 20;

                    cubes[i].LeanMove(cubes[i].position + (Vector3)direction, 1f).setEaseInQuad();
                }
                var cubeRenderer = cubes[i].GetComponent<MeshRenderer>();
                var startMaterial = cubeRenderer.material.GetFloat(alphaID);
                LeanTween.value(cubes[i].gameObject, startMaterial, 1, 1f)
    .setEase(menuEaseType)
    .setOnUpdate(value => cubeRenderer.material.SetFloat(alphaID, value));
            }
        }

        public void Deactivate()
        {
            Stop();
            cubesCamera.enabled = false;
            foreach (var cube in cubes)
            {
                cube.gameObject.SetActive(false);
            }
        }
    }
}

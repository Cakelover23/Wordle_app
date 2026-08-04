using System.Collections;
using UnityEngine;

/// <summary>
/// Drives the title screen's open/close animation (see OpenAnimation.controller's "CloseMain"/
/// "OpenMain" states) and, once it finishes opening, reveals the Menu scene's main content via
/// <see cref="MainMenuRevealGate"/>. Lives on the same "OpenAnimation" GameObject as the Animator
/// and <see cref="TitleBar"/> components.
///
/// Flow: "CloseMain" plays immediately when the Menu scene loads (title screen starts closed) ->
/// player taps/clicks anywhere on screen -> after a short pause, "OpenMain" plays -> once that
/// finishes, the Category Select screen and first-launch username prompt are revealed.
/// </summary>
[RequireComponent(typeof(Animator))]
public class TitleScreenController : MonoBehaviour
{
    private const string CloseStateName = "CloseMain";
    private const string OpenStateName = "OpenMain";

    [Tooltip("How long to wait after the player touches the screen before the OpenMain animation starts playing.")]
    [SerializeField] private float delayBeforeOpening = 1f;

    private Animator _animator;
    private TitleBar _titleBar;
    private bool _hasStartedOpening;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _titleBar = GetComponent<TitleBar>();
    }

    private void OnEnable()
    {
        _hasStartedOpening = false;
        _titleBar?.ApplyTheme();
        _animator.Play(CloseStateName, 0, 0f);
    }

    private void Update()
    {
        if (_hasStartedOpening)
        {
            return;
        }

        bool tapped = Input.GetMouseButtonDown(0)
            || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);

        if (tapped)
        {
            _hasStartedOpening = true;
            StartCoroutine(PlayOpenAfterDelay());
        }
    }

    private IEnumerator PlayOpenAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeOpening);

        _animator.Play(OpenStateName, 0, 0f);
        // Let the Animator commit the new state before reading its length below.
        yield return null;

        float openDuration = _animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(openDuration);

        MainMenuRevealGate.Reveal();
    }
}

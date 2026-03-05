using UnityEngine;
using System.Collections;

public class EyesCate : MonoBehaviour
{
    [SerializeField] private GameObject[] _eyes;
    [SerializeField] private float _switchDelay = 0.1f;
    [SerializeField] private float _randomDelayMin = 3f;
    [SerializeField] private float _randomDelayMax = 8f;

    private bool _isRunning;
    private bool _goingForward = true;
    private int _currentIndex;
    private Coroutine _randomRoutine;
    private bool _wasLandscape;
    private bool _wasPortrait;

    private void Start()
    {        
        StartRandomRoutine();
        UpdateOrientationFlags();
    }

    private void OnEnable()
    {
        StartRandomRoutine();
        UpdateOrientationFlags();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _isRunning = false;
    }

    private void Update()
    {
        bool isLandscape = Screen.width > Screen.height;
        bool isPortrait = Screen.height > Screen.width;

        if (isLandscape && !_wasLandscape)
        {
            RestartRandomRoutine();
            _wasLandscape = true;
            _wasPortrait = false;

            Debug.Log("Land");
        }
        else if (isPortrait && !_wasPortrait)
        {
            RestartRandomRoutine();
            _wasLandscape = false;
            _wasPortrait = true;

            Debug.Log("Port");
        }
    }

    private void UpdateOrientationFlags()
    {
        _wasLandscape = Screen.width > Screen.height;
        _wasPortrait = Screen.height > Screen.width;
    }

    private void StartRandomRoutine()
    {
        if (_randomRoutine != null)
            StopCoroutine(_randomRoutine);
        
        _randomRoutine = StartCoroutine(RandomStartRoutine());
    }

    private void RestartRandomRoutine()
    {
        if (_isRunning)
        {
            StopCoroutine(EyesAnimationRoutine());
            _isRunning = false;
        }
        
        DisableAllEyes();
        
        StartRandomRoutine();
    }

    private IEnumerator RandomStartRoutine()
    {
        while (true)
        {
            float randomDelay = Random.Range(_randomDelayMin, _randomDelayMax);
            yield return new WaitForSeconds(randomDelay);

            if (!_isRunning)
            {
                _isRunning = true;
                _currentIndex = 0;
                _goingForward = true;
                StartCoroutine(EyesAnimationRoutine());
            }
        }
    }

    private IEnumerator EyesAnimationRoutine()
    {
        while (_isRunning)
        {
            UpdateEyes();
            yield return new WaitForSeconds(_switchDelay);

            if (_goingForward)
            {
                _currentIndex++;
                if (_currentIndex >= _eyes.Length)
                {
                    _currentIndex = _eyes.Length - 2;
                    _goingForward = false;
                }
            }
            else
            {
                _currentIndex--;
                if (_currentIndex < 0)
                {
                    _isRunning = false;
                    DisableAllEyes();
                    yield break;
                }
            }
        }
    }

    private void UpdateEyes()
    {
        for (int i = 0; i < _eyes.Length; i++)
        {
            _eyes[i].SetActive(i == _currentIndex);
        }
    }

    private void DisableAllEyes()
    {
        foreach (var eye in _eyes)
        {
            eye.SetActive(false);
        }
        if (_eyes.Length > 0)
            _eyes[0].SetActive(true);
    }
}
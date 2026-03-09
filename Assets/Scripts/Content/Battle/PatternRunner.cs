using System;
using System.Collections;
using UnityEngine;

public class PatternRunner : MonoBehaviour
{
    private Coroutine _runningCoroutine;
    private bool _isRunning;

    public bool IsRunning => _isRunning;

    public void Run(AttackPatternSO pattern, Action onComplete = null)
    {
        if (pattern == null)
        {
            Debug.LogWarning("[PatternRunner] pattern이 null입니다.");
            onComplete?.Invoke();
            return;
        }

        StopCurrent();

        _runningCoroutine = StartCoroutine(CoRun(pattern, onComplete));
    }

    public void StopCurrent()
    {
        if (_runningCoroutine != null)
        {
            StopCoroutine(_runningCoroutine);
            _runningCoroutine = null;
        }

        _isRunning = false;
    }

    private IEnumerator CoRun(AttackPatternSO pattern, Action onComplete)
    {
        _isRunning = true;

        float elapsed = 0f;

        while (elapsed < pattern.duration)
        {
            SpawnWave(pattern);

            yield return new WaitForSeconds(pattern.spawnInterval);
            elapsed += pattern.spawnInterval;
        }

        _isRunning = false;
        _runningCoroutine = null;
        onComplete?.Invoke();
    }

    private void SpawnWave(AttackPatternSO pattern)
    {
        if (pattern.bulletPrefab == null)
        {
            Debug.LogWarning("[PatternRunner] bulletPrefab이 비어 있습니다.");
            return;
        }

        for (int i = 0; i < pattern.spawnCountPerWave; i++)
        {
            float x = UnityEngine.Random.Range(pattern.minX, pattern.maxX);
            Vector3 spawnPos = new Vector3(x, pattern.spawnY, 0f);

            GameObject bulletObj = Instantiate(pattern.bulletPrefab, spawnPos, Quaternion.identity);

            BulletBase bullet = bulletObj.GetComponent<BulletBase>();
            if (bullet != null)
            {
                bullet.Init(pattern.direction, pattern.bulletSpeed);
            }
            else
            {
                Debug.LogWarning("[PatternRunner] 생성된 bulletPrefab에 BulletBase가 없습니다.");
            }
        }
    }
}
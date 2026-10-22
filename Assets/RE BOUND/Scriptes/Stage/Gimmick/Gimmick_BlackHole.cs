using UnityEngine;

/// <summary>
/// ブラックホール。近づいた Player を引き寄せる（Space で斥力に切り替え可能）。
/// 引力は Player の「位置・大きさ・スピード」から計算する。
///   加速度 = 強さ * 距離の減衰 * 大きさ係数 / スピード係数
/// 見た目（吸い込まれるリング・粒子・回転・脈動）はすべてコードで再生する（Animator不要）。
/// </summary>
[RequireComponent(typeof(GimmickController))]
public class Gimmick_BlackHole : MonoBehaviour, IGimmick
{
    [Header("Gravity")]
    [Tooltip("引力が届く半径（ワールド単位）")]
    [SerializeField] private float _influenceRadius = 4f;
    [Tooltip("中心に最も近いときの加速度（速度/秒^2）")]
    [SerializeField] private float _gravityStrength = 30f;
    [Tooltip("距離による減衰の強さ。大きいほど中心の近くだけ強く引く")]
    [SerializeField] private float _falloffPower = 2f;

    [Header("Player の大きさ")]
    [Tooltip("この半径の Player を「標準」とする")]
    [SerializeField] private float _referencePlayerRadius = 0.5f;
    [Tooltip("大きさの影響度。0 = 影響なし / 1 = 大きいほど強く引かれる")]
    [SerializeField, Range(0f, 1f)] private float _sizeInfluence = 0.5f;

    [Header("Player のスピード")]
    [Tooltip("速いほど引かれにくくなる度合い。0 = 影響なし")]
    [SerializeField] private float _speedResistance = 0.05f;

    [Header("Core (中心)")]
    [SerializeField] private float _coreRadius = 0.5f;
    [Tooltip("中心に触れたらリトライにする")]
    [SerializeField] private bool _retryOnCore = true;
    [Tooltip("Space で 引力 / 斥力 を切り替える")]
    [SerializeField] private bool _toggleOnActive = true;

    [Header("Visual")]
    [Tooltip("回転・脈動させる見た目（子の円盤など）。空ならこのオブジェクト")]
    [SerializeField] private Transform _visual;
    [SerializeField] private float _spinSpeed = 90f;
    [SerializeField] private float _pulseAmount = 0.08f;
    [SerializeField] private float _pulseSpeed = 3f;
    [SerializeField] private int _sortingOrder = 5;

    [Header("Effect (リング / 粒子)")]
    [Tooltip("リング・粒子のマテリアル。空なら Sprites/Default。丸い粒子にしたいときは丸いテクスチャのマテリアルを入れる")]
    [SerializeField] private Material _effectMaterial;
    [SerializeField] private Color _attractColor = new(0.65f, 0.35f, 1f, 1f);
    [SerializeField] private Color _repelColor = new(0.3f, 0.85f, 1f, 1f);
    [SerializeField] private int _ringCount = 3;
    [SerializeField] private float _ringSpeed = 0.5f;
    [SerializeField] private float _ringWidth = 0.05f;
    [SerializeField] private int _ringSegments = 48;
    [SerializeField] private float _particleRate = 24f;
    [SerializeField] private float _particleLifetime = 1.3f;
    [SerializeField] private float _orbitSpeed = 1.5f;

    private float _sign = 1f;
    private float _time;
    private float _pull;             

    private bool _isInitialized;
    private bool _isBuilt;
    private Vector3 _visualBaseScale;

    private LineRenderer[] _rings;
    private ParticleSystem _particles;

    private Rigidbody2D _playerRb;
    private Collider2D _playerCollider;

    #region INITIALIZE

    public void Initialize()
    {
        if (_visual == null) _visual = transform;

        if (!_isBuilt)
        {
            _visualBaseScale = _visual.localScale;
            BuildRings();
            BuildParticles();
            _isBuilt = true;
        }

        BuildCore();

        _sign = 1f;
        _time = 0f;
        _pull = 0f;
        _playerRb = null;
        _playerCollider = null;

        _visual.localScale = _visualBaseScale;

        ApplyMode();

        _isInitialized = true;
    }

    private void BuildCore()
    {
        if (!TryGetComponent(out CircleCollider2D core)) core = gameObject.AddComponent<CircleCollider2D>();

        core.isTrigger = true;
        core.radius = _coreRadius / Mathf.Max(0.0001f, Mathf.Abs(transform.lossyScale.x));
    }

    private Material GetMaterial()
    {
        if (_effectMaterial != null) return _effectMaterial;

        Shader shader = Shader.Find("Sprites/Default");

        if (shader == null)
        {
            Debug.LogWarning($"{name} : Sprites/Default が見つかりません。Effect Material を設定してください");
            return null;
        }

        _effectMaterial = new Material(shader);
        return _effectMaterial;
    }

    private void BuildRings()
    {
        _rings = new LineRenderer[Mathf.Max(0, _ringCount)];

        for (int i = 0; i < _rings.Length; i++)
        {
            GameObject go = new($"Ring{i}");
            go.transform.SetParent(transform, false);

            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.loop = true;
            lr.positionCount = _ringSegments;
            lr.widthMultiplier = _ringWidth;
            lr.sharedMaterial = GetMaterial();
            lr.sortingOrder = _sortingOrder;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;

            _rings[i] = lr;
        }
    }

    private void BuildParticles()
    {
        GameObject go = new("Particles");
        go.transform.SetParent(transform, false);

        _particles = go.AddComponent<ParticleSystem>();
        _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = _particles.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
        main.maxParticles = 120;

        ParticleSystem.EmissionModule emission = _particles.emission;
        emission.rateOverTime = _particleRate;

        ParticleSystem.ShapeModule shape = _particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radiusThickness = 0f;   // 円の縁から出す

        // 最初は透明 → 見える → 透明
        ParticleSystem.ColorOverLifetimeModule colorModule = _particles.colorOverLifetime;
        colorModule.enabled = true;

        Gradient fade = new();
        fade.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.25f), new GradientAlphaKey(0f, 1f) });
        colorModule.color = fade;

        // 近づくほど小さくなる
        ParticleSystem.SizeOverLifetimeModule sizeModule = _particles.sizeOverLifetime;
        sizeModule.enabled = true;
        sizeModule.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

        ParticleSystemRenderer pr = go.GetComponent<ParticleSystemRenderer>();
        pr.sharedMaterial = GetMaterial();
        pr.sortingOrder = _sortingOrder + 1;
    }

    // 引力 / 斥力 に合わせて、色と粒子の向きを切り替える
    private void ApplyMode()
    {
        bool attract = _sign > 0f;
        Color color = attract ? _attractColor : _repelColor;

        if (_particles == null) return;

        float travel = Mathf.Max(0.01f, _influenceRadius - _coreRadius);

        ParticleSystem.MainModule main = _particles.main;
        main.startColor = color;
        main.startLifetime = _particleLifetime;

        // 引力: 外周から中心へ / 斥力: 中心から外周へ
        ParticleSystem.ShapeModule shape = _particles.shape;
        shape.radius = attract ? _influenceRadius : _coreRadius;

        ParticleSystem.VelocityOverLifetimeModule vel = _particles.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.x = 0f; vel.y = 0f; vel.z = 0f;
        vel.orbitalX = 0f; vel.orbitalY = 0f;
        vel.orbitalZ = _orbitSpeed * _sign;
        vel.radial = (attract ? -1f : 1f) * travel / _particleLifetime;

        _particles.Clear();
        _particles.Play();
    }

    #endregion

    #region GRAVITY

    private void FixedUpdate()
    {
        if (!_isInitialized) return;

        _pull = 0f;

        if (GameController.Instance == null || !GameController.Instance.IsPlaying) return;
        if (PlayerManager.Instance == null) return;

        PlayerMovement player = PlayerManager.Instance.Player;

        if (player == null || !player.gameObject.activeInHierarchy) return;
        if (!player.IsLaunched || player.IsPerformance) return;

        if (_playerRb == null || _playerRb.gameObject != player.gameObject)
        {
            _playerRb = player.GetComponent<Rigidbody2D>();
            _playerCollider = player.GetComponent<Collider2D>();
        }

        if (_playerRb == null) return;

        // --- 位置 ---
        Vector2 toCenter = (Vector2)transform.position - _playerRb.position;
        float distance = toCenter.magnitude;

        if (distance >= _influenceRadius || distance < 0.0001f) return;

        // 中心に近いほど 1 に近づく
        float closeness = 1f - distance / _influenceRadius;
        float falloff = Mathf.Pow(closeness, _falloffPower);

        // --- 大きさ（大きい Player ほど引かれやすい）---
        float playerRadius = GetPlayerRadius();
        float sizeRate = Mathf.Lerp(1f, playerRadius / Mathf.Max(0.0001f, _referencePlayerRadius), _sizeInfluence);

        // --- スピード（速い Player ほど引かれにくい）---
        float speed = _playerRb.linearVelocity.magnitude;
        float speedRate = 1f + speed * _speedResistance;

        float acceleration = _gravityStrength * falloff * sizeRate / speedRate;

        // 速度の向きを曲げる（スピードは PlayerMovement 側が保つので、軌道が曲がる）
        _playerRb.linearVelocity += toCenter.normalized * (acceleration * _sign * Time.fixedDeltaTime);

        _pull = closeness;
    }

    private float GetPlayerRadius()
    {
        if (_playerCollider == null) return _referencePlayerRadius;

        Vector3 extents = _playerCollider.bounds.extents;
        return Mathf.Max(extents.x, extents.y);
    }

    #endregion

    #region VISUAL

    private void Update()
    {
        if (!_isInitialized) return;
        if (GameController.Instance != null && !GameController.Instance.IsPlaying) return;

        _time += Time.deltaTime;

        // 回転と脈動
        _visual.Rotate(0f, 0f, _spinSpeed * _sign * Time.deltaTime);
        _visual.localScale = _visualBaseScale * (1f + Mathf.Sin(_time * _pulseSpeed) * _pulseAmount);

        UpdateRings();
    }

    // リングが外から中心へ縮みながら消える（斥力のときは中心から広がる）
    private void UpdateRings()
    {
        Color baseColor = _sign > 0f ? _attractColor : _repelColor;
        Vector2 center = transform.position;

        for (int i = 0; i < _rings.Length; i++)
        {
            float phase = (_time * _ringSpeed + i / (float)_rings.Length) % 1f;
            float t = _sign > 0f ? phase : 1f - phase;

            float radius = Mathf.Lerp(_influenceRadius, _coreRadius, t * t);

            // 出始めと消え際は薄く。Player を引いている間は濃く
            float alpha = Mathf.Sin(phase * Mathf.PI) * (0.35f + _pull * 0.65f);

            Color c = baseColor;
            c.a = alpha;

            _rings[i].startColor = c;
            _rings[i].endColor = c;

            for (int p = 0; p < _ringSegments; p++)
            {
                float angle = p / (float)_ringSegments * Mathf.PI * 2f;
                _rings[i].SetPosition(p, center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
        }
    }

    #endregion

    #region GIMMICK

    public void OnPlayerHit(PlayerMovement player)
    {
        if (!_retryOnCore) return;

        AudioManager.Instance.Play(Enum_SEType.Retry);
        RetryManager.Instance.Retry();
    }

    public void OnPlayerMiss(PlayerMovement player) { }

    public void OnActiveEvent()
    {
        if (!_toggleOnActive) return;

        _sign *= -1f;
        ApplyMode();
    }

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.65f, 0.35f, 1f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, _influenceRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _coreRadius);
    }
#endif
}
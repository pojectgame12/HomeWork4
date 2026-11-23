using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Controls beam and publishes bridge state; listens to crystal pickup events.</summary>
public sealed class LampController : MonoBehaviour
{
    private const float AimRotationSpeedDegreesPerSecond = 90f;
    private const float MinimumAimDegrees = -75f;
    private const float MaximumAimDegrees = 75f;
    private const float MouseAimMinimumDistanceSquared = 0.1f;
    private const float BeamLengthWorldUnits = 5f;
    private const float BeamOffsetX = 0.15f;
    private const float BeamOffsetY = 0.18f;
    private const float BeamStartWidth = 0.12f;
    private const float BeamEndWidth = 0.27f;
    private const float BeamTailAlpha = 0.05f;
    private const int BeamSortingOrder = 7;
    private const int BeamVertexCount = 2;

    public static LampController Instance { get; private set; }
    public Transform designatedObelisk;
    public float energy = 60f;
    public float consumptionPerSecond = 12f;
    public float aimDegrees = -30f;
    public float allowedAngleError = 10f;
    public float interactionRadius = 3.3f;
    public bool isLit;

    public event Action<bool> BridgeConditionsChanged;
    public float Energy => energy;
    public bool BridgeConditionsMet
    {
        get
        {
            if (!isLit || energy <= 0f || designatedObelisk == null) return false;
            Vector2 direction = designatedObelisk.position - transform.position;
            Vector2 aim = new Vector2(Mathf.Cos(aimDegrees * Mathf.Deg2Rad), Mathf.Sin(aimDegrees * Mathf.Deg2Rad));
            return Vector2.Angle(aim, direction) <= allowedAngleError;
        }
    }

    private LineRenderer beam;
    private Transform player;
    private bool lastBridgeState;

    private void Awake()
    {
        Instance = this;
        beam = GetComponent<LineRenderer>();
        if (beam == null) beam = gameObject.AddComponent<LineRenderer>();
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null) beam.material = new Material(shader);
        beam.startWidth = BeamStartWidth;
        beam.endWidth = BeamEndWidth;
        beam.positionCount = BeamVertexCount;
        beam.useWorldSpace = true;
        beam.sortingOrder = BeamSortingOrder;
    }

    private void OnEnable() { GameEvents.CrystalCollected += Recharge; }
    private void OnDisable() { GameEvents.CrystalCollected -= Recharge; }

    private void Start()
    {
        PlayerController controller = FindFirstObjectByType<PlayerController>();
        if (controller != null) player = controller.transform;
        LevelController level = FindFirstObjectByType<LevelController>();
        if (level != null && level.Settings != null)
        {
            energy = level.Settings.startingEnergy;
            consumptionPerSecond = level.Settings.drainPerSecond;
        }
        lastBridgeState = BridgeConditionsMet;
        GameEvents.RaiseLampEnergyChanged(energy);
        BridgeConditionsChanged?.Invoke(lastBridgeState);
    }

    private void Update()
    {
        bool near = player != null && Vector2.Distance(player.position, transform.position) <= interactionRadius;
        bool mobileClick = MobileInputState.ConsumeLamp();
        if (near && (Input.GetKeyDown(KeyCode.E) || mobileClick) && energy > 0f)
            isLit = !isLit;

        if (near)
        {
            float turn = 0f;
            if (Input.GetKey(KeyCode.J) || MobileInputState.rotateLeft) turn -= 1f;
            if (Input.GetKey(KeyCode.L) || MobileInputState.rotateRight) turn += 1f;
            aimDegrees = Mathf.Clamp(aimDegrees + turn * AimRotationSpeedDegreesPerSecond * Time.deltaTime, MinimumAimDegrees, MaximumAimDegrees);

            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (Input.GetMouseButtonDown(0) && Input.touchCount == 0 && !overUI && Camera.main != null)
            {
                Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 delta = mouse - transform.position;
                if (delta.sqrMagnitude > MouseAimMinimumDistanceSquared)
                    aimDegrees = Mathf.Clamp(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, MinimumAimDegrees, MaximumAimDegrees);
            }
        }

        if (isLit)
        {
            energy = Mathf.Max(0f, energy - consumptionPerSecond * Time.deltaTime);
            if (energy <= 0f) isLit = false;
            GameEvents.RaiseLampEnergyChanged(energy);
        }

        Vector2 direction = new Vector2(Mathf.Cos(aimDegrees * Mathf.Deg2Rad), Mathf.Sin(aimDegrees * Mathf.Deg2Rad));
        beam.enabled = isLit;
        beam.SetPosition(0, transform.position + new Vector3(BeamOffsetX, BeamOffsetY, 0f));
        beam.SetPosition(1, transform.position + (Vector3)(direction * BeamLengthWorldUnits));
        Color glow = BridgeConditionsMet ? new Color(.2f, 1f, .8f, .85f) : new Color(1f, .8f, .3f, .8f);
        beam.startColor = glow;
        beam.endColor = new Color(glow.r, glow.g, glow.b, BeamTailAlpha);

        bool current = BridgeConditionsMet;
        if (current == lastBridgeState) return;
        lastBridgeState = current;
        BridgeConditionsChanged?.Invoke(current);
    }

    public void Recharge(float amount)
    {
        if (amount <= 0f) return;
        energy = Mathf.Clamp(energy + amount, 0f, ShadowGameTuning.MaximumEnergy);
        GameEvents.RaiseLampEnergyChanged(energy);
    }

    private void OnDestroy()
    {
        if (beam != null && beam.material != null) Destroy(beam.material);
        if (Instance == this) Instance = null;
    }
}

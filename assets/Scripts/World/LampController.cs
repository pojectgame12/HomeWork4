using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Controls beam and publishes bridge state; listens to crystal pickup events.</summary>
public sealed class LampController : MonoBehaviour
{
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
        beam.startWidth = 0.12f;
        beam.endWidth = 0.27f;
        beam.positionCount = 2;
        beam.useWorldSpace = true;
        beam.sortingOrder = 7;
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
            aimDegrees = Mathf.Clamp(aimDegrees + turn * 90f * Time.deltaTime, -75f, 75f);

            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (Input.GetMouseButtonDown(0) && Input.touchCount == 0 && !overUI && Camera.main != null)
            {
                Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 delta = mouse - transform.position;
                if (delta.sqrMagnitude > 0.1f)
                    aimDegrees = Mathf.Clamp(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, -75f, 75f);
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
        beam.SetPosition(0, transform.position + new Vector3(0.15f, 0.18f, 0f));
        beam.SetPosition(1, transform.position + (Vector3)(direction * 5f));
        Color glow = BridgeConditionsMet ? new Color(.2f, 1f, .8f, .85f) : new Color(1f, .8f, .3f, .8f);
        beam.startColor = glow;
        beam.endColor = new Color(glow.r, glow.g, glow.b, .05f);

        bool current = BridgeConditionsMet;
        if (current == lastBridgeState) return;
        lastBridgeState = current;
        BridgeConditionsChanged?.Invoke(current);
    }

    public void Recharge(float amount)
    {
        if (amount <= 0f) return;
        energy = Mathf.Clamp(energy + amount, 0f, 100f);
        GameEvents.RaiseLampEnergyChanged(energy);
    }

    private void OnDestroy()
    {
        if (beam != null && beam.material != null) Destroy(beam.material);
        if (Instance == this) Instance = null;
    }
}

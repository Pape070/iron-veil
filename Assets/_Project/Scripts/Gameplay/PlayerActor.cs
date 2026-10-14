using System;
using System.Collections.Generic;
using IronVeil.Rules;
using UnityEngine;

namespace IronVeil
{
    /// <summary>Per-weapon runtime state: magazine, reserve, cadence and reload.</summary>
    public sealed class WeaponRuntime
    {
        public WeaponDefinition Definition;
        public int UpgradeLevel = 1;
        public int Magazine;
        public int Reserve;
        public float ShotTimer;
        public float ReloadTimer;
        public int BurstRemaining;
        public float BurstTimer;
        public float ChargeTimer;
        public bool Reloading => ReloadTimer > 0f;

        public void Reset(WeaponDefinition definition, int upgradeLevel)
        {
            Definition = definition;
            UpgradeLevel = Mathf.Clamp(upgradeLevel, 1, definition?.MaxUpgradeLevel ?? 5);
            Magazine = definition?.MagazineSize ?? 0;
            Reserve = definition != null ? definition.MaxReserveAmmo / 2 : 0;
            ShotTimer = 0f;
            ReloadTimer = 0f;
            BurstRemaining = 0;
            BurstTimer = 0f;
            ChargeTimer = 0f;
        }

        public int MagazineSize => Definition?.MagazineSize ?? 0;
        public int MaxReserve => Definition?.MaxReserveAmmo ?? 0;

        public void AddAmmo(int amount)
        {
            if (Definition == null) return;
            Reserve = Mathf.Clamp(Reserve + amount, 0, MaxReserve);
        }

        public float DamageMultiplier => WeaponMath.UpgradeDamageMultiplier(UpgradeLevel);

        /// <summary>Reloading refills the magazine from the reserve.</summary>
        public bool CompleteReload()
        {
            if (Definition == null) return false;
            int needed = MagazineSize - Magazine;
            if (needed <= 0) return false;
            int taken = Mathf.Min(needed, Reserve);
            Reserve -= taken;
            Magazine += taken;
            return taken > 0;
        }

        public bool BeginReload()
        {
            if (Definition == null || Reloading) return false;
            if (Magazine >= MagazineSize) return false;
            if (Reserve <= 0) return false;
            return true;
        }
    }

    /// <summary>
    /// The player character: movement, aiming, shooting, abilities, passives, flasks
    /// and death. Both keyboard/mouse and gamepad are handled identically because
    /// InputService already collapsed them into one aim direction.
    /// </summary>
    public sealed class PlayerActor : IDamageable
    {
        private const float CoyoteTime = 0.12f;
        private const float JumpBufferTime = 0.12f;
        private const float BarrierAbsorbRatio = 0.85f;

        private readonly ChapterRuntime _ctx;
        private readonly CharacterDefinition _character;
        private readonly ProgressionService _progression;

        private readonly List<WeaponRuntime> _weapons = new List<WeaponRuntime>();
        private int _weaponIndex;

        private CharacterBody _body;
        private LevelGeometry _level;
        private GameObject _root;
        private SpriteRenderer _renderer;
        private SpriteRenderer _aimGuide;
        private SpriteRenderer _reticle;
        private SpriteRenderer _barrierSprite;
        private SpriteRenderer _muzzle;
        private GameObject _barrierRoot;
        private GameObject _decoyRoot;

        private float _health;
        private float _shield;
        private float _maxHealth;
        private float _maxShield;
        private StatBlock _stats;

        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private float _invulnerableTimer;
        private float _hitFlashTimer;
        private float _secondsSinceDamage = 99f;
        private float _stationaryTimer;
        private float _fireCooldown;
        private float _primaryCooldown;
        private float _secondaryCooldown;
        private float _overclockTimer;
        private float _barrierTimer;
        private float _barrierHealth;
        private float _dashTimer;
        private Vector2 _dashDirection;
        private int _momentumStacks;
        private float _momentumDecayTimer;
        private int _flasks;
        private float _flaskCooldown;
        private float _deathTimer;
        private bool _firing;
        private bool _braced;
        private bool _dead;
        private float _stepTimer;

        public event Action<PlayerActor> Died;
        public event Action WeaponChanged;
        public event Action<string> Callout;

        public PlayerActor(ChapterRuntime context, CharacterDefinition character, ProgressionService progression)
        {
            _ctx = context;
            _character = character;
            _progression = progression;
        }

        // ------------------------------------------------------------- IDamageable

        public bool IsAlive => !_dead && _health > 0f;
        public Team Team => Team.Player;
        public Vector2 Center => _body.Position;
        public float HealthRatio => _maxHealth <= 0f ? 0f : Mathf.Clamp01(_health / _maxHealth);
        public Aabb2 HitBox => _body.Bounds;

        // ------------------------------------------------------------- state

        public CharacterBody Body => _body;
        public float Health => _health;
        public float MaxHealth => _maxHealth;
        public float Shield => _shield;
        public float MaxShield => _maxShield;
        public int Flasks => _flasks;
        public float PrimaryCooldownRatio => _character == null ? 0f : Ratio(_primaryCooldown, PrimaryCooldownSeconds);
        public float SecondaryCooldownRatio => _character == null ? 0f : Ratio(_secondaryCooldown, SecondaryCooldownSeconds);
        public bool IsDead => _dead;
        public bool IsOverclocked => _overclockTimer > 0f;
        public bool HasBarrier => _barrierTimer > 0f && _barrierHealth > 0f;
        public int MomentumStacks => _momentumStacks;
        public WeaponRuntime Weapon => _weapons.Count == 0 ? null : _weapons[_weaponIndex];
        public List<WeaponRuntime> Weapons => _weapons;
        public float DeathTime => _deathTimer;

        private static float Ratio(float remaining, float total) => total <= 0f ? 0f : Mathf.Clamp01(remaining / total);

        public float PrimaryCooldownSeconds => _character?.PrimaryAbility == null
            ? 0f
            : AbilityMath.Cooldown(_character.PrimaryAbility, _stats ?? new StatBlock());

        public float SecondaryCooldownSeconds => _character?.SecondaryAbility == null
            ? 0f
            : AbilityMath.Cooldown(_character.SecondaryAbility, _stats ?? new StatBlock());

        public string PrimaryAbilityName => _character?.PrimaryAbility?.DisplayName ?? "Ability";
        public string SecondaryAbilityName => _character?.SecondaryAbility?.DisplayName ?? "Ability";

        // ------------------------------------------------------------- lifecycle

        public void Spawn(LevelGeometry level, Vector2 position)
        {
            _level = level;
            _ctx.RegisterDamageable(this);

            if (_root == null)
            {
                BuildVisuals();
            }

            _root.SetActive(true);
            _body = new CharacterBody(position, new Vector2(0.85f, 1.7f));
            _stats = _progression != null ? _progression.Stats : new StatBlock();
            _maxHealth = _stats.MaxHealth;
            _health = _maxHealth;
            _maxShield = _stats.Shield;
            _shield = _maxShield;
            _flasks = 3;
            _dead = false;
            _deathTimer = 0f;
            _invulnerableTimer = 0.75f;
            _secondsSinceDamage = 99f;
            _momentumStacks = 0;
            _overclockTimer = 0f;
            _barrierTimer = 0f;
            _barrierHealth = 0f;
            _primaryCooldown = 0f;
            _secondaryCooldown = 0f;
            _dashTimer = 0f;

            RebuildWeapons();
            _root.transform.position = new Vector3(position.x, position.y, 0f);
        }

        private void RebuildWeapons()
        {
            _weapons.Clear();
            _weaponIndex = 0;

            List<WeaponDefinition> owned = _progression != null ? _progression.OwnedWeapons() : new List<WeaponDefinition>();
            WeaponDefinition equipped = _progression?.EquippedWeapon();
            if (equipped != null && !owned.Contains(equipped)) owned.Insert(0, equipped);

            for (int i = 0; i < owned.Count; i++)
            {
                WeaponDefinition weapon = owned[i];
                if (weapon == null) continue;
                WeaponRuntime runtime = new WeaponRuntime();
                runtime.Reset(weapon, _progression?.WeaponUpgradeLevel(weapon.Id) ?? 1);
                if (weapon == equipped) _weaponIndex = _weapons.Count;
                _weapons.Add(runtime);
            }

            if (_weapons.Count == 0 && _character != null)
            {
                // Absolute fallback so the player is never unarmed.
                WeaponDefinition fallback = _ctx.Content?.Weapon(_character.SignatureWeaponId);
                if (fallback != null)
                {
                    WeaponRuntime runtime = new WeaponRuntime();
                    runtime.Reset(fallback, 1);
                    _weapons.Add(runtime);
                }
            }

            _weaponIndex = Mathf.Clamp(_weaponIndex, 0, Mathf.Max(0, _weapons.Count - 1));
            WeaponChanged?.Invoke();
        }

        public void Despawn()
        {
            _ctx.UnregisterDamageable(this);
            if (_root != null) _root.SetActive(false);
            if (_barrierRoot != null) _barrierRoot.SetActive(false);
            if (_decoyRoot != null) _decoyRoot.SetActive(false);
        }

        public void RefreshStats()
        {
            if (_progression == null) return;
            float healthRatio = _maxHealth <= 0f ? 1f : _health / _maxHealth;
            _stats = _progression.Stats;
            _maxHealth = _stats.MaxHealth;
            _health = Mathf.Clamp(_maxHealth * healthRatio, 1f, _maxHealth);
            _maxShield = _stats.Shield;
            _shield = Mathf.Min(_shield, _maxShield);
        }

        // ------------------------------------------------------------- visuals

        private void BuildVisuals()
        {
            _root = new GameObject("Player");
            _root.transform.SetParent(_ctx.ActorRoot, false);

            _renderer = _root.AddComponent<SpriteRenderer>();
            Color primary = _character != null && _character.Id == CharacterId.Vera ? Palette.VeraPrimary : Palette.KadePrimary;
            Color accent = _character != null && _character.Id == CharacterId.Vera ? Palette.VeraAccent : Palette.KadeAccent;
            _renderer.sprite = PlaceholderArt.Actor(_character?.Id.ToString() ?? "player", primary, accent, 26, 40);
            _renderer.sortingOrder = 15;
            _root.transform.localScale = Vector3.one * 0.95f;

            GameObject muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(_root.transform, false);
            _muzzle = muzzle.AddComponent<SpriteRenderer>();
            _muzzle.sprite = PlaceholderArt.Circle("fx_muzzle", new Color(1f, 0.95f, 0.75f), 12, 16f);
            _muzzle.sortingOrder = 33;
            _muzzle.enabled = false;

            GameObject aim = new GameObject("AimGuide");
            aim.transform.SetParent(_ctx.ActorRoot, false);
            _aimGuide = aim.AddComponent<SpriteRenderer>();
            _aimGuide.sprite = PlaceholderArt.Rect("fx_aimline", new Color(1f, 0.9f, 0.6f, 0.5f), 8, 2, 16f);
            _aimGuide.sortingOrder = 6;
            _aimGuide.transform.localScale = new Vector3(1f, 0.12f, 1f);

            GameObject reticle = new GameObject("Reticle");
            reticle.transform.SetParent(_ctx.ActorRoot, false);
            _reticle = reticle.AddComponent<SpriteRenderer>();
            _reticle.sprite = PlaceholderArt.Circle("fx_reticle", new Color(1f, 0.85f, 0.45f, 0.9f), 20, 16f);
            _reticle.sortingOrder = 34;
            _reticle.enabled = false;

            _barrierRoot = new GameObject("Barrier");
            _barrierRoot.transform.SetParent(_ctx.ActorRoot, false);
            _barrierSprite = _barrierRoot.AddComponent<SpriteRenderer>();
            _barrierSprite.sprite = PlaceholderArt.Rect("fx_barrier", new Color(0.55f, 0.78f, 1f, 0.55f), 16, 48, 16f);
            _barrierSprite.sortingOrder = 14;
            _barrierRoot.SetActive(false);

            _decoyRoot = new GameObject("Decoy");
            _decoyRoot.transform.SetParent(_ctx.ActorRoot, false);
            SpriteRenderer decoy = _decoyRoot.AddComponent<SpriteRenderer>();
            decoy.sprite = PlaceholderArt.Actor("decoy", new Color(0.5f, 0.75f, 1f, 0.6f), new Color(0.8f, 0.92f, 1f, 0.7f), 26, 40);
            decoy.sortingOrder = 14;
            _decoyRoot.SetActive(false);
        }

        // ------------------------------------------------------------- tick

        public void Tick(float deltaTime)
        {
            if (_root == null) return;

            if (_dead)
            {
                _deathTimer += deltaTime;
                _body.Velocity.x *= 0.9f;
                _body.Step(deltaTime, _level);
                SyncVisuals(deltaTime, Vector2.right);
                return;
            }

            InputService input = _ctx.Input;
            Vector2 moveInput = input == null ? Vector2.zero : input.Move;
            bool hasAim;
            Vector2 aimDirection = input == null
                ? Vector2.right
                : input.ResolveAimDirection(_body.Position + new Vector2(0f, 0.35f), _ctx.Camera, out hasAim);

            float speed = _stats.MoveSpeed;
            if (_braced) speed *= 0.45f;
            if (_overclockTimer > 0f) speed *= 1.05f;

            _body.Velocity.x = moveInput.x * speed;

            // --- jump handling ---
            if (_body.Grounded) _coyoteTimer = CoyoteTime;
            else _coyoteTimer = Mathf.Max(0f, _coyoteTimer - deltaTime);

            bool jumpHeld = input != null && input.Move.y > 0.5f;
            if (jumpHeld && _jumpBufferTimer <= 0f) _jumpBufferTimer = JumpBufferTime;
            else _jumpBufferTimer = Mathf.Max(0f, _jumpBufferTimer - deltaTime);

            if (_dashTimer > 0f)
            {
                _body.Velocity.x = _dashDirection.x * 19f;
                if (_dashDirection.y < -0.2f) _body.Velocity.y = _dashDirection.y * 19f;
            }
            else if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
            {
                _body.Velocity.y = 16.4f;
                _jumpBufferTimer = 0f;
                _coyoteTimer = 0f;
                _body.Grounded = false;
                _ctx.Effects?.Dust(_body.Position - new Vector2(0f, _body.HalfHeight));
            }

            // Variable jump height: release early, rise less.
            if (_body.Velocity.y > 0f && !jumpHeld) _body.Velocity.y -= 34f * deltaTime;

            _body.Step(deltaTime, _level);
            if (_body.Grounded && Mathf.Abs(moveInput.x) > 0.1f)
            {
                _stepTimer -= deltaTime;
                if (_stepTimer <= 0f)
                {
                    _stepTimer = 0.3f;
                    _ctx.Effects?.Dust(_body.Position - new Vector2(0f, _body.HalfHeight), 2);
                }
            }

            // --- timers ---
            _invulnerableTimer = Mathf.Max(0f, _invulnerableTimer - deltaTime);
            _hitFlashTimer = Mathf.Max(0f, _hitFlashTimer - deltaTime);
            _fireCooldown = Mathf.Max(0f, _fireCooldown - deltaTime);
            _primaryCooldown = Mathf.Max(0f, _primaryCooldown - deltaTime);
            _secondaryCooldown = Mathf.Max(0f, _secondaryCooldown - deltaTime);
            _overclockTimer = Mathf.Max(0f, _overclockTimer - deltaTime);
            _flaskCooldown = Mathf.Max(0f, _flaskCooldown - deltaTime);
            _dashTimer = Mathf.Max(0f, _dashTimer - deltaTime);
            if (_barrierTimer > 0f)
            {
                _barrierTimer -= deltaTime;
                _barrierHealth = Mathf.Max(0f, _barrierHealth - deltaTime * 4f);
                if (_barrierTimer <= 0f || _barrierHealth <= 0f) DeactivateBarrier();
            }

            _secondsSinceDamage += deltaTime;
            _shield = DamageRules.RechargeShield(_shield, _maxShield, _stats.ShieldRegen, _secondsSinceDamage, 2.2f, deltaTime);

            // --- passives ---
            UpdatePassives(deltaTime, moveInput, speed);

            // --- actions ---
            if (input != null)
            {
                _braced = input.BraceHeld && _body.Grounded;
                HandleWeaponInput(input, aimDirection, deltaTime);
                HandleAbilityInput(input, aimDirection);
                if (input.UseFlaskPressed) UseFlask();
                if (input.InteractPressed) _ctx.TryInteract(this);
            }

            if (Mathf.Abs(_body.Velocity.x) > 0.05f) { /* facing is driven by aim, not movement */ }
            SyncVisuals(deltaTime, aimDirection);
        }

        private void UpdatePassives(float deltaTime, Vector2 moveInput, float speed)
        {
            PassiveDefinition passive = _character?.Passive;
            if (passive == null) return;

            if (moveInput.sqrMagnitude < 0.01f && _body.Grounded) _stationaryTimer += deltaTime;
            else _stationaryTimer = 0f;

            if (passive.Kind == PassiveKind.Reinforced)
            {
                if (_stationaryTimer >= passive.StationaryThreshold && !_braced) _braced = true;
            }
            else if (passive.Kind == PassiveKind.Momentum)
            {
                if (_momentumStacks > 0)
                {
                    _momentumDecayTimer += deltaTime;
                    if (_momentumDecayTimer >= passive.StackDecaySeconds)
                    {
                        _momentumDecayTimer = 0f;
                        _momentumStacks = PassiveMath.DecayStacks(passive, _momentumStacks, passive.StackDecaySeconds);
                    }
                }
            }
        }

        // ------------------------------------------------------------- weapons

        private void HandleWeaponInput(InputService input, Vector2 aimDirection, float deltaTime)
        {
            WeaponRuntime weapon = Weapon;
            if (weapon == null) return;
            WeaponDefinition definition = weapon.Definition;

            if (input.NextWeaponPressed) CycleWeapon(1);
            if (input.PreviousWeaponPressed) CycleWeapon(-1);
            if (input.SwapWeaponPressed) _ctx.ToggleWeaponWheel();
            if (input.ReloadPressed) StartReload(weapon);

            if (weapon.Reloading)
            {
                weapon.ReloadTimer -= deltaTime;
                if (weapon.ReloadTimer <= 0f)
                {
                    weapon.CompleteReload();
                    _ctx.Audio?.PlaySfx("sfx_reload_done");
                }
                return;
            }

            bool trigger = definition.Mode == FireMode.Automatic || definition.Mode == FireMode.Beam
                ? input.FireHeld
                : input.FirePressed;

            if (definition.Mode != FireMode.Charge) _firing = trigger;

            switch (definition.Mode)
            {
                case FireMode.Automatic:
                case FireMode.SemiAutomatic:
                case FireMode.Beam:
                    if (trigger && _fireCooldown <= 0f) FireShot(weapon, aimDirection, 1f);
                    break;
                case FireMode.Burst:
                    if (trigger && weapon.BurstRemaining == 0 && _fireCooldown <= 0f)
                    {
                        weapon.BurstRemaining = Mathf.Max(1, definition.BurstCount);
                        weapon.BurstTimer = 0f;
                    }
                    break;
                case FireMode.Charge:
                    if (input.FireHeld)
                    {
                        weapon.ChargeTimer += deltaTime;
                        _firing = true;
                    }
                    else if (weapon.ChargeTimer > 0f)
                    {
                        float chargeRatio = definition.ChargeTime <= 0f ? 1f : Mathf.Clamp01(weapon.ChargeTimer / definition.ChargeTime);
                        if (chargeRatio >= 0.35f && _fireCooldown <= 0f) FireShot(weapon, aimDirection, chargeRatio);
                        weapon.ChargeTimer = 0f;
                        _firing = false;
                    }
                    break;
            }

            // Burst continuation runs even when the trigger was released.
            if (weapon.BurstRemaining > 0)
            {
                weapon.BurstTimer -= deltaTime;
                if (weapon.BurstTimer <= 0f)
                {
                    FireShot(weapon, aimDirection, 1f);
                    weapon.BurstRemaining--;
                    weapon.BurstTimer = Mathf.Max(0.02f, definition.BurstInterval);
                }
            }
        }

        private void FireShot(WeaponRuntime weapon, Vector2 aimDirection, float chargeRatio)
        {
            WeaponDefinition definition = weapon.Definition;
            if (definition == null) return;

            if (weapon.Magazine <= 0)
            {
                StartReload(weapon);
                return;
            }

            float shotsPerSecond = WeaponMath.ShotsPerSecond(definition, _stats);
            if (_overclockTimer > 0f)
            {
                float bonus = _character?.SecondaryAbility != null
                    ? AbilityMath.FireRateBonus(_character.SecondaryAbility, _stats)
                    : 0.6f;
                shotsPerSecond *= 1f + bonus;
            }

            _fireCooldown = 1f / Mathf.Max(0.1f, shotsPerSecond);
            weapon.Magazine--;
            _ctx.Saves?.RegisterShot();

            float damageMultiplier = weapon.DamageMultiplier * chargeRatio * MomentumMultiplier();
            DamageRequest request = DamageRequest.FromWeapon(definition, _stats, true);
            request.BaseDamage *= damageMultiplier;

            bool moving = Mathf.Abs(_body.Velocity.x) > 1f;
            float spread = WeaponMath.SpreadDegrees(definition, moving && !_braced, !_body.Grounded);
            if (_braced) spread *= 0.35f;

            Vector2 origin = _body.Position + aimDirection * 0.6f + new Vector2(0f, 0.35f);

            int pellets = Mathf.Max(1, definition.PelletsPerShot);
            for (int i = 0; i < pellets; i++)
            {
                Vector2 direction = CombatMath.WithSpread(aimDirection, spread, _ctx.Rng);
                _ctx.Projectiles.Spawn(
                    origin,
                    direction,
                    request,
                    Team.Player,
                    definition.ProjectileSpeed,
                    Mathf.Max(0.4f, definition.Range / Mathf.Max(1f, definition.ProjectileSpeed)),
                    definition.Kind == WeaponKind.RocketLauncher ? 0.28f : 0.16f,
                    definition.Kind == WeaponKind.GrenadeLauncher ? -22f : 0f,
                    false,
                    definition.Pierces,
                    definition.Kind == WeaponKind.RocketLauncher ? 2.6f : 0f,
                    definition.Kind.ToString(),
                    VisualFor(definition.Kind));
            }

            _ctx.Effects?.MuzzleFlash(origin, aimDirection, definition.Kind == WeaponKind.Shotgun ? 1.4f : 1f);
            _ctx.Audio?.PlaySfx("sfx_shoot_" + definition.Kind, UnityEngine.Random.Range(0.94f, 1.06f));
            ApplyRecoil(aimDirection, definition.Recoil);
            _ctx.Feedback?.Shake(0.05f * definition.Recoil);
            if (_muzzle != null) _muzzle.enabled = true;
        }

        private void StartReload(WeaponRuntime weapon)
        {
            if (weapon == null || !weapon.BeginReload()) return;
            weapon.ReloadTimer = WeaponMath.ReloadTime(weapon.Definition, _stats);
            _ctx.Audio?.PlaySfx("sfx_reload_start");
            Callout?.Invoke("Reloading");
        }

        public void CycleWeapon(int direction)
        {
            if (_weapons.Count <= 1) return;
            _weaponIndex = (_weaponIndex + direction + _weapons.Count) % _weapons.Count;
            _ctx.Audio?.PlaySfx("sfx_weapon_switch");
            WeaponChanged?.Invoke();
        }

        public void SelectWeapon(int index)
        {
            if (index < 0 || index >= _weapons.Count || index == _weaponIndex) return;
            _weaponIndex = index;
            _ctx.Audio?.PlaySfx("sfx_weapon_switch");
            WeaponChanged?.Invoke();
        }

        public void GiveAmmo(int amount)
        {
            for (int i = 0; i < _weapons.Count; i++) _weapons[i].AddAmmo(amount);
        }

        private void ApplyRecoil(Vector2 aimDirection, float recoil)
        {
            if (recoil <= 0f) return;
            _body.Velocity -= aimDirection * recoil * 0.35f;
        }

        private float MomentumMultiplier()
        {
            PassiveDefinition passive = _character?.Passive;
            if (passive == null || passive.Kind != PassiveKind.Momentum) return 1f;
            return PassiveMath.DamageMultiplier(passive, _momentumStacks);
        }

        private void RegisterMomentumHit()
        {
            PassiveDefinition passive = _character?.Passive;
            if (passive == null || passive.Kind != PassiveKind.Momentum) return;
            _momentumDecayTimer = 0f;
            PassiveMath.AddStackAndGetMultiplier(passive, ref _momentumStacks);
        }

        private static int VisualFor(WeaponKind kind)
        {
            switch (kind)
            {
                case WeaponKind.RocketLauncher: return 3;
                case WeaponKind.RailPistol: return 2;
                case WeaponKind.ArcEmitter: return 2;
                case WeaponKind.GrenadeLauncher: return 3;
                case WeaponKind.Lmg:
                case WeaponKind.RotaryCannon: return 5;
                default: return 0;
            }
        }

        // ------------------------------------------------------------- abilities

        private void HandleAbilityInput(InputService input, Vector2 aimDirection)
        {
            if (input.AbilityPrimaryPressed && _primaryCooldown <= 0f) UsePrimary(aimDirection);
            if (input.AbilitySecondaryPressed && _secondaryCooldown <= 0f) UseSecondary(aimDirection);
        }

        private void UsePrimary(Vector2 aimDirection)
        {
            AbilityDefinition ability = _character?.PrimaryAbility;
            if (ability == null || ability.Kind == AbilityKind.None) return;

            _primaryCooldown = AbilityMath.Cooldown(ability, _stats);
            _ctx.Audio?.PlaySfx("sfx_ability_primary");
            _ctx.Feedback?.Shake(0.25f);

            switch (ability.Kind)
            {
                case AbilityKind.Bulwark:
                    DeployBarrier(ability, aimDirection);
                    Callout?.Invoke("Bulwark deployed");
                    break;
                case AbilityKind.PhaseBlink:
                    PhaseBlink(ability, aimDirection);
                    Callout?.Invoke("Phase Blink");
                    break;
                case AbilityKind.GroundSlam:
                case AbilityKind.Overclock:
                    // Defensive: a character could be authored with these as primary.
                    UseSecondary(aimDirection);
                    break;
            }
        }

        private void UseSecondary(Vector2 aimDirection)
        {
            AbilityDefinition ability = _character?.SecondaryAbility;
            if (ability == null || ability.Kind == AbilityKind.None) return;

            _secondaryCooldown = AbilityMath.Cooldown(ability, _stats);
            _ctx.Audio?.PlaySfx("sfx_ability_secondary");
            _ctx.Feedback?.Shake(0.3f);

            switch (ability.Kind)
            {
                case AbilityKind.Overclock:
                    _overclockTimer = Mathf.Max(1f, AbilityMath.Duration(ability, _stats));
                    Callout?.Invoke("Overclock: +" +
                                    Mathf.RoundToInt((_character != null ? ability.FireRateBonus * 100f : 60f)) + "% fire rate");
                    break;
                case AbilityKind.GroundSlam:
                    GroundSlam(ability);
                    Callout?.Invoke("Ground Slam");
                    break;
                case AbilityKind.Bulwark:
                    DeployBarrier(ability, aimDirection);
                    break;
                case AbilityKind.PhaseBlink:
                    PhaseBlink(ability, aimDirection);
                    break;
            }
        }

        private void DeployBarrier(AbilityDefinition ability, Vector2 aimDirection)
        {
            _barrierTimer = Mathf.Max(1.5f, AbilityMath.Duration(ability, _stats));
            _barrierHealth = Mathf.Max(40f, AbilityMath.ShieldAmount(ability, _stats));
            if (_barrierRoot != null)
            {
                _barrierRoot.SetActive(true);
                Vector2 offset = aimDirection.sqrMagnitude < 0.01f ? Vector2.right : aimDirection.normalized;
                _barrierRoot.transform.localScale = new Vector3(0.6f, 2.4f, 1f);
            }
        }

        private void DeactivateBarrier()
        {
            _barrierTimer = 0f;
            _barrierHealth = 0f;
            if (_barrierRoot != null) _barrierRoot.SetActive(false);
        }

        private void PhaseBlink(AbilityDefinition ability, Vector2 aimDirection)
        {
            float distance = Mathf.Max(3f, ability.Distance);
            Vector2 direction = aimDirection.sqrMagnitude < 0.01f ? Vector2.right : aimDirection.normalized;
            _dashDirection = direction;
            _dashTimer = 0.22f;
            _invulnerableTimer = Mathf.Max(_invulnerableTimer, 0.35f + ability.InvulnerabilitySeconds);
            _ctx.Effects?.Dust(_body.Position, 10);

            if (_decoyRoot != null)
            {
                _decoyRoot.SetActive(true);
                _decoyRoot.transform.position = new Vector3(_body.Position.x, _body.Position.y, 0f);
            }

            // Teleport a short distance, stopping at walls.
            Vector2 target = _body.Position + direction * distance;
            target.x = Mathf.Clamp(target.x, _level.Bounds.xMin + _body.HalfWidth, _level.Bounds.xMax - _body.HalfWidth);
            float ground = _level.GroundHeightAt(target.x);
            if (target.y < ground + _body.HalfHeight) target.y = ground + _body.HalfHeight;

            Vector2 step = direction * Mathf.Min(1.2f, distance);
            Vector2 probe = _body.Position;
            while ((probe - _body.Position).magnitude < distance)
            {
                probe += step;
                if (BlockedAt(probe)) break;
            }

            _body.Teleport(probe);
            _ctx.Effects?.Shockwave(_body.Position, 2.2f, 0.25f, new Color(0.5f, 0.8f, 1f, 0.7f));
        }

        private bool BlockedAt(Vector2 position)
        {
            Aabb2 box = Aabb2.FromCenter(position, _body.Size);
            for (int i = 0; i < _level.Platforms.Count; i++)
            {
                Platform platform = _level.Platforms[i];
                if (platform.OneWay) continue;
                Rect rect = platform.Rect;
                if (box.Overlaps(new Aabb2(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMax)))) return true;
            }
            return false;
        }

        private void GroundSlam(AbilityDefinition ability)
        {
            float radius = Mathf.Max(2.5f, AbilityMath.Radius(ability, _stats));
            float damage = Mathf.Max(20f, AbilityMath.Damage(ability, _stats));
            float stun = Mathf.Max(0.6f, ability.StunSeconds);

            DamageRequest request = new DamageRequest
            {
                BaseDamage = damage,
                Power = _stats.Power,
                PowerScaling = ability.PowerScaling,
                CritChance = _stats.CritChance,
                CritDamageMultiplier = _stats.CritDamageMultiplier,
                CanCrit = true
            };

            Vector2 center = _body.Position - new Vector2(0f, _body.HalfHeight);
            List<IDamageable> hits = CombatMath.InRadius(center, radius, _ctx.Damageables, Team.Player);
            for (int i = 0; i < hits.Count; i++)
            {
                DamageOutcome outcome = hits[i].TakePlayerDamage(request, hits[i].Center, false, "ground_slam");
                _ctx.Text?.Damage(hits[i].Center, outcome, true);
                _ctx.NotifyStun(hits[i], stun);
            }

            _ctx.Effects?.Shockwave(center, radius, 0.4f, new Color(1f, 0.75f, 0.35f, 0.85f));
            _ctx.Feedback?.Shake(0.6f);
            _invulnerableTimer = Mathf.Max(_invulnerableTimer, 0.3f);
        }

        private void UseFlask()
        {
            if (_flasks <= 0 || _flaskCooldown > 0f) return;
            if (_health >= _maxHealth) return;

            _flasks--;
            _flaskCooldown = 0.8f;
            float heal = _progression != null ? _progression.FlaskHealAmount() : 40f;
            _health = DamageRules.ApplyHealing(_health, heal, _maxHealth);
            _ctx.Effects?.Shockwave(_body.Position, 1.6f, 0.35f, new Color(0.5f, 1f, 0.6f, 0.8f));
            _ctx.Text?.Callout(_body.Position, "+" + Mathf.RoundToInt(heal), Palette.Success, 1.1f, 0.9f);
            _ctx.Audio?.PlaySfx("sfx_flask");
        }

        /// <summary>Heals the player. A negative amount means "to full", used at rest points.</summary>
        public void Heal(float amount = -1f)
        {
            if (_dead || _maxHealth <= 0f) return;
            float heal = amount < 0f ? _maxHealth : amount;
            _health = DamageRules.ApplyHealing(_health, heal, _maxHealth);
            _shield = _maxShield;
            _ctx.Text?.Callout(_body.Position, "+" + Mathf.RoundToInt(heal), Palette.Success, 1.1f, 1f);
        }

        public void RefillFlasks(int amount)
        {
            _flasks = Mathf.Min(9, _flasks + amount);
        }

        public void GrantInvulnerability(float seconds)
        {
            _invulnerableTimer = Mathf.Max(_invulnerableTimer, seconds);
        }

        // ------------------------------------------------------------- damage

        public DamageOutcome TakePlayerDamage(DamageRequest request, Vector2 hitPoint, bool isWeakPoint, string sourceTag)
        {
            // The player never damages themselves; friendly fire from own explosives is off.
            return new DamageOutcome();
        }

        public DamageOutcome TakeEnemyDamage(float attackDamage, Vector2 hitPoint, string sourceTag)
        {
            DamageOutcome outcome = new DamageOutcome();
            if (_dead || attackDamage <= 0f) return outcome;

            if (_invulnerableTimer > 0f)
            {
                _ctx.Text?.Callout(_body.Position, "dodged", Palette.AccentCool, 0.9f, 0.7f);
                return outcome;
            }

            // The bulwark barrier eats most of a frontal hit while it lasts.
            if (HasBarrier)
            {
                float absorbed = attackDamage * BarrierAbsorbRatio;
                _barrierHealth = Mathf.Max(0f, _barrierHealth - absorbed);
                attackDamage = Mathf.Max(0f, attackDamage - absorbed);
                _ctx.Text?.Callout(_body.Position + new Vector2(0.6f, 0f), "blocked", Palette.AccentCool, 0.9f, 0.7f);
                if (_barrierHealth <= 0f) DeactivateBarrier();
            }

            float defense = _stats.Defense;
            PassiveDefinition passive = _character?.Passive;
            if (passive != null && passive.Kind == PassiveKind.Reinforced)
            {
                bool stationary = _stationaryTimer >= passive.StationaryThreshold;
                defense *= 1f / Mathf.Max(0.05f, PassiveMath.DamageTakenMultiplier(passive, stationary ? passive.StationaryThreshold : 0f));
            }
            if (_braced) defense *= 1.6f;

            float difficultyMultiplier = CombatMath.EnemyDamageMultiplier(_ctx.Difficulty);
            if (_ctx.Settings != null && _ctx.Settings.Settings.HonoursDamageReduction) difficultyMultiplier *= 0.6f;

            outcome = DamageRules.ComputeIncoming(attackDamage, defense, difficultyMultiplier, _shield, _health);
            if (!outcome.AnyDamage) return outcome;

            _shield = Mathf.Max(0f, _shield - outcome.ShieldAbsorbed);
            _health = DamageRules.ApplyToHealth(_health, outcome.HealthDamage);
            _secondsSinceDamage = 0f;
            _hitFlashTimer = 0.18f;
            _invulnerableTimer = Mathf.Max(_invulnerableTimer, 0.55f * _ctx.Difficulty.PlayerInvulnerabilitySeconds);
            _momentumStacks = 0;

            _ctx.Feedback?.Shake(0.45f);
            _ctx.Feedback?.HitStop(0.05f);
            _ctx.Audio?.PlaySfx("sfx_player_hurt");
            _ctx.NotifyPlayerDamaged();

            if (_health <= 0f) Die();
            return outcome;
        }

        private void Die()
        {
            if (_dead) return;
            _dead = true;
            _health = 0f;
            _deathTimer = 0f;
            _body.Velocity = new Vector2(0f, 6f);
            DeactivateBarrier();
            _ctx.Effects?.Explosion(_body.Position, 3.5f);
            _ctx.Audio?.PlaySfx("sfx_player_death");
            Died?.Invoke(this);
        }

        // ------------------------------------------------------------- visuals

        private void SyncVisuals(float deltaTime, Vector2 aimDirection)
        {
            _root.transform.position = new Vector3(_body.Position.x, _body.Position.y, 0f);

            bool flip = aimDirection.x < 0f;
            _renderer.flipX = flip;

            Color tint = Color.white;
            if (_hitFlashTimer > 0f) tint = new Color(1f, 0.55f, 0.55f);
            else if (_invulnerableTimer > 0f) tint = new Color(1f, 0.85f, 0.85f, 0.75f);
            if (_overclockTimer > 0f) tint = Color.Lerp(tint, new Color(1f, 0.8f, 0.35f), 0.5f);
            if (_dead) tint.a = Mathf.Clamp01(1f - _deathTimer * 0.8f);
            _renderer.color = tint;

            if (_muzzle != null)
            {
                _muzzle.transform.localPosition = new Vector3(Mathf.Abs(0.55f) * (flip ? -1f : 1f), 0.35f, 0f);
                if (_muzzle.enabled)
                {
                    _muzzle.enabled = false;
                }
            }

            bool showAim = _ctx.Settings == null || _ctx.Settings.Settings.AimGuide;
            if (_aimGuide != null)
            {
                _aimGuide.enabled = showAim && !_dead;
                if (_aimGuide.enabled)
                {
                    Vector2 origin = _body.Position + new Vector2(0f, 0.35f);
                    float length = 9f;
                    Vector2 end = origin + aimDirection * length;
                    Vector2 mid = (origin + end) * 0.5f;
                    _aimGuide.transform.position = new Vector3(mid.x, mid.y, 0f);
                    _aimGuide.transform.localScale = new Vector3(length, 0.06f, 1f);
                    _aimGuide.transform.localRotation = Quaternion.Euler(0f, 0f, CombatMath.AngleOf(aimDirection));
                    Color color = new Color(1f, 0.9f, 0.6f, 0.22f);
                    _aimGuide.color = color;
                }
            }

            if (_reticle != null)
            {
                bool cursorAim = _ctx.Input != null && _ctx.Input.UsesCursorAim;
                _reticle.enabled = showAim && cursorAim && !_dead;
                if (_reticle.enabled)
                {
                    Vector2 origin = _body.Position + new Vector2(0f, 0.35f);
                    Vector2 end = origin + aimDirection * 9f;
                    _reticle.transform.position = new Vector3(end.x, end.y, 0f);
                }
            }

            if (_barrierRoot != null && _barrierRoot.activeSelf)
            {
                Vector2 offset = aimDirection.sqrMagnitude < 0.01f ? Vector2.right : aimDirection.normalized;
                _barrierRoot.transform.position = new Vector3(
                    _body.Position.x + offset.x * 1.1f,
                    _body.Position.y + offset.y * 0.4f,
                    0f);
                Color color = new Color(0.55f, 0.78f, 1f, 0.25f + 0.4f * Mathf.Clamp01(_barrierHealth / 120f));
                _barrierSprite.color = color;
            }

            if (_decoyRoot != null && _decoyRoot.activeSelf)
            {
                if (_dashTimer <= 0f && _invulnerableTimer <= 0.2f) _decoyRoot.SetActive(false);
            }
        }

        /// <summary>Called by the runtime when one of the player's shots connects.</summary>
        public void NoteHit()
        {
            RegisterMomentumHit();
        }
    }
}

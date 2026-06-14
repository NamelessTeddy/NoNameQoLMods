using GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Tools;

namespace NoNameQoLMods;

/// <summary>The mod entry point.</summary>
internal sealed class ModEntry : Mod
{
    private ModConfig config;
    private bool _cancelTimerSet = false;
    private uint _cancelAfterTick = 0;

    /// <summary>The mod entry point, called after the mod is first loaded.</summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        I18n.Init(Helper.Translation);
        config = helper.ReadConfig<ModConfig>();
        helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
    }

    private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
    {
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (configMenu is null)
            return;
        
        // register mod
        configMenu.Register(
            mod: ModManifest,
            reset: () => config = new ModConfig(),
            save: () => Helper.WriteConfig(config)
        );
        
        configMenu.AddKeybindList(
            ModManifest,
            name: () => I18n.Gmcm_Keybind_Name(),
            tooltip: () => I18n.Gmcm_Keybind_Tooltip(),
            getValue: () => config.CancelKey,
            setValue: value => config.CancelKey = value);
        configMenu.AddTextOption(
            ModManifest,
            name: () => I18n.Gmcm_Suppression_Name(),
            tooltip: () => I18n.Gmcm_Suppression_Tooltip(I18n.Gmcm_Keybind_Name()),
            getValue: () => config.Suppression.ToString(),
            setValue: value => config.Suppression = Enum.Parse<KeySuppression>(value),
            allowedValues: Enum.GetValues<KeySuppression>().Select(v => v.ToString()).ToArray(),
            formatAllowedValue: value => Helper.Translation.Get($"Gmcm.Suppression.Option.{value}"));
        configMenu.AddNumberOption(
            ModManifest,
            name: () => I18n.Gmcm_CancelDelay_Name(),
            tooltip: () => I18n.Gmcm_CancelDelay_Tooltip(),
            getValue: () => config.CancelDelayMs,
            setValue: value => config.CancelDelayMs = value,
            min: 50,
            max: 1000,
            interval: 10);
        configMenu.AddBoolOption(
            ModManifest,
            name: () => I18n.Gmcm_ClubSpecial_Name(),
            tooltip: () => I18n.Gmcm_ClubSpecial_Tooltip(),
            getValue: () => config.EnableClubSpecial,
            setValue: value => config.EnableClubSpecial = value);
    }

    private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;
        if (Game1.freezeControls)
            return;

        var keybind = config.CancelKey.GetKeybindCurrentlyDown();
        bool keyHeld = keybind is not null;

        // If no cancel is pending and key isn't held, nothing to do
        if (!_cancelTimerSet && !keyHeld)
            return;

        var player = Game1.player;
        if (player.CurrentTool is FishingRod)
        {
            _cancelTimerSet = false;
            return;
        }

        // Club special attack: trigger the spinning smash and spam attacks during its animation
        if (config.EnableClubSpecial
            && player.CurrentTool is MeleeWeapon clubWeapon
            && clubWeapon.type.Value == MeleeWeapon.club)
        {
            _cancelTimerSet = false;
            if (!keyHeld) return;
            HandleClubSpecial(clubWeapon, player, keybind);
            return;
        }

        uint delayTicks = (uint)Math.Max(1, Math.Round(config.CancelDelayMs / (1000.0 / 60.0)));

        if (keyHeld && config.Suppression == KeySuppression.Always)
            SuppressKeybind(keybind);

        if (player.UsingTool)
        {
            if (!_cancelTimerSet && keyHeld)
            {
                // Key pressed during tool use — commit to cancelling this animation
                _cancelTimerSet = true;
                _cancelAfterTick = e.Ticks + delayTicks;
            }
            else if (_cancelTimerSet && e.Ticks >= _cancelAfterTick)
            {
                // Timer elapsed — cancel regardless of whether key is still held
                player.forceCanMove();
                _cancelTimerSet = false;
                // Key still held + Always mode: manually re-trigger below
                // Key still held + OnCancel mode: game sees key held and re-triggers naturally
                // Key released: no re-trigger — single use + cancel is complete
            }
        }
        else
        {
            // Tool not in use (ended naturally or was just cancelled)
            _cancelTimerSet = false;
            if (keyHeld && player.canMove && config.Suppression == KeySuppression.Always)
                Game1.pressUseToolButton();
            // OnCancel mode + key held: game handles held-key re-trigger on its own
        }
    }

    private void HandleClubSpecial(MeleeWeapon weapon, Farmer player, Keybind keybind)
    {
        if (config.Suppression == KeySuppression.Always)
            SuppressKeybind(keybind);

        if (weapon.isOnSpecial)
        {
            // Special animation is active — spam attacks so the next one starts immediately after
            Game1.pressUseToolButton();
        }
        else if (!player.UsingTool && player.canMove && MeleeWeapon.clubCooldown <= 0)
        {
            // Player is free and off cooldown — trigger the club's special smash attack
            weapon.triggerClubFunction(player);
        }
        // If mid-swing or on cooldown: wait for it to clear
    }

    private void SuppressKeybind(Keybind keybind)
    {
        foreach (var button in keybind.Buttons)
        {
            Helper.Input.Suppress(button);
        }
    }
}

enum KeySuppression { OnCancel, Always }

class ModConfig
{
    public KeybindList CancelKey { get; set; } = KeybindList.Parse("Space");
    public KeySuppression Suppression { get; set; } = KeySuppression.OnCancel;
    public int CancelDelayMs { get; set; } = 220;
    public bool EnableClubSpecial { get; set; } = true;
}

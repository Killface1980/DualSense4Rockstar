using GTA;
using LemonUI;
using LemonUI.Menus;
using System;
using System.Windows.Forms;
using GTA.Native;

namespace DualSense4GTAV;

public class ControllerConfig
{
  public readonly ObjectPool pool;
  public int startofResistanceVehicle = 0;
  public int endofResistanceVehicle = 6;
  public int minResistanceVehicle = 0;
  public int maxResistanceVehicle = 10;
  public bool showbatstat = true;
  public bool showconmes = true;
  public bool showHealth;
  public bool showPlayerColor;
  public bool showRPM;
  public bool showWanted;

  public static bool isDisabled;

  private NativeSliderItem item_endofResistanceVehicle;
  private NativeSliderItem item_maxResistanceVehicle;
  private NativeSliderItem item_minResistanceVehicle;
  private NativeCheckboxItem item_showHealth;
  private NativeCheckboxItem item_showPlayerColor;
  private NativeCheckboxItem item_showRPM;
  private NativeCheckboxItem item_showWanted;
  private NativeSliderItem item_startofResistanceVehicle;
  private NativeMenu menu;

  private NativeMenu menuVehicle;
  private ScriptSettings settings;

  public ControllerConfig()
  {
    // File load / save ...

    settings = ScriptSettings.Load("Scripts\\DualSense4GTAV.ini");

    showWanted = settings.GetValue("Controls", nameof(showWanted), true);
    showHealth = settings.GetValue("Controls", nameof(showHealth), true);
    showRPM = settings.GetValue("Controls", nameof(showRPM), true);
    showPlayerColor = settings.GetValue("Controls", nameof(showPlayerColor), true);

    startofResistanceVehicle = settings.GetValue("Controls", nameof(startofResistanceVehicle), 1) * 25;
    endofResistanceVehicle   = settings.GetValue("Controls", nameof(endofResistanceVehicle), 7) * 25;
    minResistanceVehicle     = settings.GetValue("Controls", nameof(minResistanceVehicle), 2) * 25;
    maxResistanceVehicle     = settings.GetValue("Controls", nameof(maxResistanceVehicle), 7) * 25;

    pool = new ObjectPool();

    menu = new NativeMenu("DualSense for GTA V", "Options");
    menuVehicle = new NativeMenu("Vehicle");

    item_showWanted = new NativeCheckboxItem("LED: Show wanted", "Shows red and blue lights when wanted.", showWanted);
    item_showWanted.CheckboxChanged += DS4GTAV_Controls_SettingsChanged;
    menu.Add(item_showWanted);
    item_showRPM = new NativeCheckboxItem("LED: Show rpm", "Shows the current rpm when in vehicle.", showRPM);
    item_showRPM.CheckboxChanged += DS4GTAV_Controls_SettingsChanged;
    menu.Add(item_showRPM);
    item_showHealth = new NativeCheckboxItem("LED: Show health", "Shows the current health green to red.", showHealth);
    item_showHealth.CheckboxChanged += DS4GTAV_Controls_SettingsChanged;
    menu.Add(item_showHealth);
    item_showPlayerColor = new NativeCheckboxItem("LED: Show player color", "Shows the player color.", showPlayerColor);
    item_showPlayerColor.CheckboxChanged += DS4GTAV_Controls_SettingsChanged;
    menu.Add(item_showPlayerColor);

    menu.Add(new NativeItem("Vehicle Resistance"));

    item_startofResistanceVehicle = new NativeSliderItem("Start ", 10, startofResistanceVehicle / 25);
    item_startofResistanceVehicle.ValueChanged += DS4GTAV_Controls_SettingsChanged;
    menu.Add(item_startofResistanceVehicle);

    item_endofResistanceVehicle = new NativeSliderItem("End ", 10, endofResistanceVehicle / 25);
    item_endofResistanceVehicle.ValueChanged += DS4GTAV_Controls_SettingsChanged;
    menu.Add(item_endofResistanceVehicle);

    item_minResistanceVehicle = new NativeSliderItem("Minimum ", 10, minResistanceVehicle / 25);
    item_minResistanceVehicle.ValueChanged += DS4GTAV_Controls_SettingsChanged;
    menu.Add(item_minResistanceVehicle);

    item_maxResistanceVehicle = new NativeSliderItem("Maximum ", 10, maxResistanceVehicle / 25);
    item_maxResistanceVehicle.ValueChanged += DS4GTAV_Controls_SettingsChanged;
    menu.Add(item_maxResistanceVehicle);

    NativeCheckboxItem checkbox = new NativeCheckboxItem("Test");
    // menu.AddSubMenu(menuVehicle);

    pool.Add(menu);

    UpdateMenuDescriptions();
  }

  public void OnKeyDown(object sender, KeyEventArgs e)
  {
    if (e.KeyCode == KeyConf.showMenu)
    {
      ControllerConfig.isDisabled = false;

      menu.Visible = !menu.Visible;
    }
    else if (e.KeyCode == KeyConf.showBatStat)
    {
      iO obj = new();

      obj.GetStat(out int bat, out bool isconnected);
      GTA.UI.Notification.Show("Controller connection status: " + isconnected + " controller battery status: " + bat +
                               ".");// "% \n to hide this Press F8");
    }
    else if (e.KeyCode == KeyConf.toggleBatStat)
    {
      showbatstat = !showbatstat;
    }
    else if (e.KeyCode ==  KeyConf.showCommStat)
    {
      showconmes = !showconmes;
    }

  }

  private void DS4GTAV_Controls_SettingsChanged(object sender, EventArgs e)
  {
    if (item_startofResistanceVehicle.Value > item_endofResistanceVehicle.Value)
    {
      item_startofResistanceVehicle.Value = item_endofResistanceVehicle.Value;
    }
    if (item_minResistanceVehicle.Value > item_maxResistanceVehicle.Value)
    {
      item_minResistanceVehicle.Value = item_maxResistanceVehicle.Value;
    }

    UpdateMenuDescriptions();

    showWanted = item_showWanted.Checked;
    settings.SetValue("LED", nameof(showWanted), item_showWanted.Checked);

    showHealth = item_showHealth.Checked;
    settings.SetValue("LED", nameof(showHealth), item_showHealth.Checked);

    showRPM = item_showRPM.Checked;
    settings.SetValue("LED", nameof(showRPM), item_showRPM.Checked);

    showPlayerColor = item_showPlayerColor.Checked;
    settings.SetValue("LED", nameof(showPlayerColor), item_showPlayerColor.Checked);

    startofResistanceVehicle = (int)255f / 10 * item_startofResistanceVehicle.Value;
    settings.SetValue("Controls", nameof(startofResistanceVehicle), item_startofResistanceVehicle.Value);

    endofResistanceVehicle = (int)255f / 10 * item_endofResistanceVehicle.Value;
    settings.SetValue("Controls", nameof(endofResistanceVehicle), item_endofResistanceVehicle.Value);

    minResistanceVehicle = (int)255f / 10 * item_minResistanceVehicle.Value;
    settings.SetValue("Controls", nameof(minResistanceVehicle), item_minResistanceVehicle.Value);

    maxResistanceVehicle = (int)255f / 10 * item_maxResistanceVehicle.Value;
    settings.SetValue("Controls", nameof(maxResistanceVehicle), item_maxResistanceVehicle.Value);

    // minResistanceVehicle = Math.Min(item_minResistanceVehicle.Value, item_maxResistanceVehicle.Value);
    // maxResistanceVehicle = Math.Max(item_minResistanceVehicle.Value, item_maxResistanceVehicle.Value);
    GTA.UI.Screen.ShowSubtitle("\nRange is " + startofResistanceVehicle + " - " + endofResistanceVehicle + "\nResistance is " + minResistanceVehicle + " / " + maxResistanceVehicle, 4000);

    settings.Save();
  }

  private void UpdateMenuDescriptions()
  {
    item_startofResistanceVehicle.Description = item_startofResistanceVehicle.Value.ToString();
    item_endofResistanceVehicle.Description = item_endofResistanceVehicle.Value.ToString();
    item_minResistanceVehicle.Description = item_minResistanceVehicle.Value.ToString();
    item_maxResistanceVehicle.Description = item_maxResistanceVehicle.Value.ToString();
  }
}
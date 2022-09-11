using System;
using System.Windows.Forms;
using GTA;
using LemonUI;
using LemonUI.Menus;

namespace DualSense4GTAV;

public class ControllerConfig
{
    public int startofResistanceVehicle = 0;
    public int endofResistanceVehicle = 6;
    public int minResistanceVehicle = 0;
    public int maxResistanceVehicle = 10;
    

    public readonly ObjectPool pool;
    NativeMenu menu;

    NativeMenu menuVehicle;
    NativeSliderItem item_minResistanceVehicle;
    NativeSliderItem item_maxResistanceVehicle;
    NativeSliderItem item_startofResistanceVehicle;
    NativeSliderItem item_endofResistanceVehicle;
    private ScriptSettings settings;

    public ControllerConfig()
    {
        // File load / save ...

        settings = ScriptSettings.Load("Scripts\\DualSense4GTAV.ini");

        startofResistanceVehicle = settings.GetValue("Controls", nameof(startofResistanceVehicle), 1) * 25;
        endofResistanceVehicle = settings.GetValue("Controls", nameof(endofResistanceVehicle), 7) * 25;
        minResistanceVehicle = settings.GetValue("Controls", nameof(minResistanceVehicle), 2) * 25;
        maxResistanceVehicle = settings.GetValue("Controls", nameof(maxResistanceVehicle), 7) * 25;


        pool = new ObjectPool();

        menu = new NativeMenu("DualSense for GTA V", "Options");
        menuVehicle = new NativeMenu("Vehicle");

        menu.Add(new NativeItem("Vehicle Resistance"));

        item_startofResistanceVehicle = new NativeSliderItem("Start ", 10, startofResistanceVehicle / 25 );
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
        if (e.KeyCode == Keys.F10)
        {
            menu.Visible = !menu.Visible;
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


        startofResistanceVehicle = (int)255f / 10* item_startofResistanceVehicle.Value;
        settings.SetValue("Controls", nameof(startofResistanceVehicle), item_startofResistanceVehicle.Value);
     
        endofResistanceVehicle = (int)255f /10* item_endofResistanceVehicle.Value;
        settings.SetValue("Controls", nameof(endofResistanceVehicle), item_endofResistanceVehicle.Value);

        minResistanceVehicle = (int)255f / 10* item_minResistanceVehicle.Value;
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
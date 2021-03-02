using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Controller : MonoBehaviour
{
    public World world;

    public Dropdown ShadowOptionsDropdown;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(world != null);
        Debug.Assert(ShadowOptionsDropdown != null);

        InitializeShadowDropdown();

        ShadowOptionsDropdown.onValueChanged.AddListener(delegate {
            ShadowDropdownChanged(ShadowOptionsDropdown);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitializeShadowDropdown()
    {
        ShadowOptions currentOption = world.GetCurrentShadowOption();

        switch (currentOption)
        {
            case ShadowOptions.UNITY:
                ShadowOptionsDropdown.value = 0;
                break;
            case ShadowOptions.UNITY_DEPTH:
                ShadowOptionsDropdown.value = 1;
                break;
            case ShadowOptions.FULL_CUSTOM:
                ShadowOptionsDropdown.value = 2;
                break;
        }
    }

    private void ShadowDropdownChanged(Dropdown change)
    {
        try
        {
            ShadowOptions option = (ShadowOptions)change.value;
            world.SetShadowOption(option);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error occurrent determing desired shadow option: " + ex.Message);
        }
        
    }
}

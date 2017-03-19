﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;

public class RestingPanel : PanelBase
{
	public UISlider HoursSlider;
	public UILabel HoursLabel;
	public UILabel CaloriesLabel;
	public UIButton Rest;
	public UIButton Cancel;

	private int _hours;
	private float _timer;
	private bool _isFading;

	public override void Initialize ()
	{

		Hide();
	}

	public override void PerFrameUpdate ()
	{
		if(!_isFading)
		{
			return;
		}

		if(_timer < 1.5f)
		{
			_timer += Time.unscaledDeltaTime;
		}
		else
		{
			_isFading = false;
			//advance time

			//increase stats
			GameManager.Inst.PlayerControl.Survival.CompleteResting();

			//close window
			Hide();
		}

	}

	public override void Show ()
	{
		Camera.main.GetComponent<BlurOptimized>().enabled = true;

		Time.timeScale = 0;

		NGUITools.SetActive(this.gameObject, true);
		this.IsActive = true;

		HoursSlider.numberOfSteps = 6;
		_hours = 3;
		_isFading = false;

		CaloriesLabel.text = "Calories Consumed: " + GameManager.Inst.PlayerControl.Survival.GetEatenCalories();

		InputEventHandler.Instance.State = UserInputState.Resting;

		InputEventHandler.OnPopupMouseWheel -= OnMouseWheelInput;
		InputEventHandler.OnPopupMouseWheel += OnMouseWheelInput;

		GameManager.Inst.SoundManager.UI.PlayOneShot(GameManager.Inst.SoundManager.GetClip("OpenSplitMenu"), 0.5f);
	}

	public override void Hide ()
	{
		_isFading = false;
		UIEventHandler.Instance.TriggerCloseWindow();
		Camera.main.GetComponent<BlurOptimized>().enabled = false;
		Time.timeScale = 1;

		NGUITools.SetActive(this.gameObject, false);
		this.IsActive = false;

		InputEventHandler.OnPopupMouseWheel -= OnMouseWheelInput;

		InputEventHandler.Instance.State = UserInputState.Normal;


	}

	public override bool HasBodySlots (out List<BodySlot> bodySlots)
	{
		bodySlots = null;
		return false;
	}

	public override bool HasTempSlots (out List<TempSlot> tempSlots)
	{
		tempSlots = null;

		return false;
	}

	public void OnSliderValueChange()
	{
		_hours = 3 + Mathf.RoundToInt(HoursSlider.value * 5);
		HoursLabel.text = "Hours To Rest: " + _hours.ToString();
	}

	public void OnCancelButtonPress()
	{
		
		Hide();
	}

	public void OnRestButtonPress()
	{
		float calories = GameManager.Inst.PlayerControl.Survival.GetEatenCalories();
		if(calories <= 0)
		{
			GameManager.Inst.UIManager.SetConsoleText("Too hungry to rest. Need to eat something.");
			return;
		}
		else
		{
			//start resting
			GameManager.Inst.UIManager.FadingPanel.FadeOutAndIn(1, 1, 1);
			_isFading = true;
			_timer = 0;
		}
	}

	public void OnMouseWheelInput(float movement)
	{
		float normalizedMovement = 0;
		if(movement > 0)
		{
			normalizedMovement = 1;
		}
		else
		{
			normalizedMovement = -1;
		}

		HoursSlider.value += normalizedMovement /  6;
	}
}

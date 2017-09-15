﻿using UnityEngine;
using System.Collections;

public class CursorManager  
{
	public CursorState CurrentState;
	public GameObject ActiveCursor;

	public GameObject CursorDefault;
	public GameObject CursorAim;
	public GameObject CursorHand;
	public GameObject CursorTalk;
	public GameObject CursorPortal;

	public Texture2D Default;
	public Texture2D Hand;
	public Texture2D Talk;
	public Texture2D Portal;

	public UILabel ToolTip;
	public float ToolTipDelay;


	private float _toolTipTimer;
	private bool _isShowingToolTip;

	public void Initialize()
	{
		//UnityEngine.Cursor.visible = false;

		CursorDefault = GameObject.Find("CursorDefault");
		CursorAim = GameObject.Find("CursorAim");
		CursorHand = GameObject.Find("CursorHand");
		CursorTalk = GameObject.Find("CursorTalk");
		CursorPortal = GameObject.Find("CursorPortal");

		NGUITools.SetActive(CursorDefault.gameObject, false);
		NGUITools.SetActive(CursorHand.gameObject, false);
		NGUITools.SetActive(CursorTalk.gameObject, false);
		NGUITools.SetActive(CursorPortal.gameObject, false);

		Default = Resources.Load("HWCursorDefault") as Texture2D;
		Hand = Resources.Load("HWCursorHand") as Texture2D;
		Talk = Resources.Load("HWCursorTalk") as Texture2D;
		Portal = Resources.Load("HWCursorPortal") as Texture2D;

		CurrentState = CursorState.Default;
		RefreshCursor();

		ToolTip = GameObject.Find("ToolTip").GetComponent<UILabel>();
		ToolTipDelay = 0.01f;
		NGUITools.SetActive(ToolTip.gameObject, false);

		UIEventHandler.OnCloseWindow += OnResetCursor;
		UIEventHandler.OnLootBody += OnResetCursor;
		UIEventHandler.OnLootChest += OnResetCursor;
		UIEventHandler.OnToggleInventory += OnResetCursor;

	}

	public void SetCursorState(CursorState state)
	{
		if(CurrentState != state)
		{
			CurrentState = state;
			RefreshCursor();
		}


	}

	public void OnHitMarkerShow()
	{
		string clipName = "BulletHitMark";
		GameManager.Inst.SoundManager.UI.PlayOneShot(GameManager.Inst.SoundManager.GetClip(clipName), 0.1f);
		CursorAim.GetComponent<AimCursor>().SetCenterCursorAlpha(1);
	}

	public void ShowToolTip(string text)
	{
		ToolTip.text = text;
		UISprite background = ToolTip.transform.Find("ToolTipBackground").GetComponent<UISprite>();
		background.MakePixelPerfect();
		background.width = ToolTip.width + 6;
		background.height = ToolTip.height + 6;
		background.transform.localPosition = new Vector3(-3, -3, 0);
		_isShowingToolTip = true;
	}

	public void HideToolTip()
	{
		if(ToolTip != null)
		{
			NGUITools.SetActive(ToolTip.gameObject, false);
		}
		_isShowingToolTip = false;
		_toolTipTimer = 0;
	}

	public void OnResetCursor()
	{
		HideToolTip();
		SetCursorState(CursorState.Default);
	}

	public void PerFrameUpdate()
	{
		//run delay timer for tooltip
		if(_isShowingToolTip)
		{
			if(!NGUITools.GetActive(ToolTip))
			{
				NGUITools.SetActive(ToolTip.gameObject, true);
			}

			Vector3 pos = Input.mousePosition + new Vector3(0, 10, 0);
			pos.x = Mathf.Clamp01(pos.x / Screen.width);
			pos.y = Mathf.Clamp01(pos.y / Screen.height);
			ToolTip.transform.position = GameManager.Inst.UIManager.UICamera.ViewportToWorldPoint(pos);;
			//_toolTipTimer = 0;

			/*
			if(_toolTipTimer < ToolTipDelay)
			{
				_toolTipTimer += Time.unscaledDeltaTime;
			}
			else
			{
				
			}
			*/
		}

		if(InputEventHandler.Instance.State == UserInputState.PopupOpen || InputEventHandler.Instance.State == UserInputState.Dialogue)
		{
			SetCursorState(CursorState.Default);
			HideToolTip();
		}

		//Debug.Log(Input.mousePosition);
		if(GameManager.Inst.PlayerControl.IsGamePaused || InputEventHandler.Instance.State != UserInputState.Normal)
		{
			return;
		}

		//UnityEngine.Cursor.visible = false;
		if(CurrentState == CursorState.Aim)
		{
			UnityEngine.Cursor.visible = false;

			AimCursor aimCursor = ActiveCursor.GetComponent<AimCursor>();
			if(aimCursor != null)
			{
				float climb = GameManager.Inst.PlayerControl.SelectedPC.AimTarget.localPosition.y;
				float amount = Mathf.Clamp(climb, 0, 1);
				float baseAmount = 5;
				if(GameManager.Inst.PlayerControl.SelectedPC.IsHipAiming || GameManager.Inst.PlayerControl.SelectedPC.UpperBodyState != HumanUpperBodyStates.Aim)
				{
					baseAmount = 15;
				}
				amount = amount * 100 + baseAmount + 20 * GameManager.Inst.PlayerControl.SelectedPC.MyAI.WeaponSystem.GetTurnMoveScatter();
				aimCursor.SetExpansion(amount);
			}
		}
		else
		{
			UnityEngine.Cursor.visible = true;
		}


		float centerAlpha = CursorAim.GetComponent<AimCursor>().AimCursorCenter.alpha;
		CursorAim.GetComponent<AimCursor>().SetCenterCursorAlpha(Mathf.Lerp(centerAlpha, 0, Time.deltaTime * 3));
		//update cursor state based on what player is doing

		GameObject aimedObject = GameManager.Inst.PlayerControl.GetAimedObject();

		if(GameManager.Inst.PlayerControl.SelectedPC.MyAI.ControlType == AIControlType.Player)
		{
			
			//if(GameManager.Inst.PlayerControl.SelectedPC.GetCurrentAnimWeapon() != WeaponAnimType.Unarmed && !GameManager.Inst.UIManager.IsCursorInHUDRegion())
			if((GameManager.Inst.PlayerControl.SelectedPC.UpperBodyState == HumanUpperBodyStates.Aim 
				|| GameManager.Inst.PlayerControl.SelectedPC.UpperBodyState == HumanUpperBodyStates.HalfAim) 
				&& !GameManager.Inst.UIManager.IsCursorInHUDRegion())
			{
				//Debug.Log("setting cursor to aim");
				SetCursorState(CursorState.Aim);



				/*
				//now check if aimed object is enemy
				Character aimedChar = GameManager.Inst.PlayerControl.GetAimedObject().GetComponent<Character>();
				if(aimedChar != null && aimedChar.Faction != GameManager.Inst.PlayerControl.SelectedPC.Faction)
				{
					//mark cursor as red
					CursorAim.color = new Color(1, 0, 0);
				}
				*/
			}
			else if(aimedObject != null && aimedObject.GetComponent<PickupItem>() != null)
			{
				PickupItem pickup = aimedObject.GetComponent<PickupItem>();
				string quantity = "";
				if(pickup.Quantity > 1)
				{
					quantity = "(" + pickup.Quantity + ")";
				}
				ShowToolTip(pickup.Item.Name + quantity);
				SetCursorState(CursorState.Hand);
			}
			else if(aimedObject != null && aimedObject.GetComponent<StoryObject>() != null)
			{
				ShowToolTip(aimedObject.GetComponent<StoryObject>().Name);
				SetCursorState(CursorState.Hand);
			}
			else if(aimedObject != null && aimedObject.GetComponent<Character>() != null)
			{
				Character aimedCharacter = aimedObject.GetComponent<Character>();
				if(aimedCharacter != null && aimedCharacter.MyStatus.Health > 0 && !aimedCharacter.MyAI.IsCharacterEnemy((Character)GameManager.Inst.PlayerControl.SelectedPC)
					&& aimedCharacter.MyAI.ControlType != AIControlType.Player && !aimedCharacter.IsHidden)
				{
					SetCursorState(CursorState.Talk);
				}
				else
				{
					SetCursorState(CursorState.Default);
				}

				string name = aimedCharacter.Name;
				if(!string.IsNullOrEmpty(aimedCharacter.Title))
				{
					name = aimedCharacter.Title + " " + name;
				}

				if(!string.IsNullOrEmpty(name) && !aimedCharacter.IsHidden)
				{
					ShowToolTip(name);
				}
			}
			else if(aimedObject != null && aimedObject.tag == "SerumLab")
			{
				ShowToolTip("Serum Lab");
				SetCursorState(CursorState.Hand);
			}
			else if(aimedObject != null)
			{
				DeathCollider deathCollider = aimedObject.GetComponent<DeathCollider>();
				if(deathCollider != null)
				{
					Character aimedCharacter = deathCollider.ParentCharacter;
					if(aimedCharacter != null && aimedCharacter.MyStatus.Health <= 0)
					{
						SetCursorState(CursorState.Hand);
					}
					else
					{
						SetCursorState(CursorState.Default);
					}
				}
				else if(aimedObject.tag == "Chest" && aimedObject.GetComponent<Chest>() != null)
				{
					SetCursorState(CursorState.Hand);
				}
				else if(aimedObject.tag == "Door" || aimedObject.tag == "LightSwitch")
				{
					SetCursorState(CursorState.Hand);
				}
				else if(aimedObject.tag == "Portal")
				{

					SetCursorState(CursorState.Portal);
				}
				else
				{
					//Debug.Log("setting cursor to default");
					SetCursorState(CursorState.Default);
				}

				HideToolTip();

			}
			else
			{
				//Debug.Log("setting cursor to default");
				SetCursorState(CursorState.Default);
				HideToolTip();
			}
		}
		else
		{
			//Debug.Log("setting cursor to default");
			SetCursorState(CursorState.Default);
			HideToolTip();
		}


	}

	private void RefreshCursor()
	{
		
		switch(CurrentState)
		{
		case CursorState.None:
			NGUITools.SetActive(CursorAim.gameObject, false);
			Cursor.visible = false;
			break;
		case CursorState.Default:
			CursorAim.transform.position = new Vector3(0, 1000, 0);
			//CursorHand.transform.position = new Vector3(0, 1000, 0);;
			NGUITools.SetActive(CursorAim.gameObject, false);
			//NGUITools.SetActive(CursorHand.gameObject, false);
			//NGUITools.SetActive(CursorTalk.gameObject, false);
			//NGUITools.SetActive(CursorDefault.gameObject, true);
			Cursor.SetCursor(Default, Vector2.zero, CursorMode.Auto);
			NGUITools.SetActive(CursorPortal.gameObject, false);
			ActiveCursor = CursorDefault;
			Cursor.visible = true;
			break;
		case CursorState.Aim:
			//CursorDefault.transform.position = new Vector3(0, 1000, 0);;
			//CursorHand.transform.position = new Vector3(0, 1000, 0);;
			//NGUITools.SetActive(CursorDefault.gameObject, false);
			//NGUITools.SetActive(CursorHand.gameObject, false);
			//NGUITools.SetActive(CursorTalk.gameObject, false);
			NGUITools.SetActive(CursorAim.gameObject, true);
			NGUITools.SetActive(CursorPortal.gameObject, false);
			Cursor.visible = false;
			ActiveCursor = CursorAim;
			break;
		case CursorState.Hand:
			CursorAim.transform.position = new Vector3(0, 1000, 0);;
			//CursorDefault.transform.position = new Vector3(0, 1000, 0);;
			//NGUITools.SetActive(CursorDefault.gameObject, false);
			NGUITools.SetActive(CursorAim.gameObject, false);
			//NGUITools.SetActive(CursorTalk.gameObject, false);
			//NGUITools.SetActive(CursorHand.gameObject, true);
			Cursor.SetCursor(Hand, Vector2.zero, CursorMode.Auto);
			NGUITools.SetActive(CursorPortal.gameObject, false);
			ActiveCursor = CursorHand;
			Cursor.visible = true;
			break;
		case CursorState.Talk:
			CursorAim.transform.position = new Vector3(0, 1000, 0);;
			//CursorDefault.transform.position = new Vector3(0, 1000, 0);;
			//NGUITools.SetActive(CursorDefault.gameObject, false);
			NGUITools.SetActive(CursorAim.gameObject, false);
			//NGUITools.SetActive(CursorHand.gameObject, false);
			//NGUITools.SetActive(CursorTalk.gameObject, true);
			NGUITools.SetActive(CursorPortal.gameObject, false);
			Vector2 hotspot = new Vector2(20, 15);
			Cursor.SetCursor(Talk, hotspot, CursorMode.Auto);
			Cursor.visible = true;
			ActiveCursor = CursorTalk;
			break;
		case CursorState.Portal:
			CursorAim.transform.position = new Vector3(0, 1000, 0);;
			NGUITools.SetActive(CursorAim.gameObject, false);
			hotspot = new Vector2(25, 25);
			Cursor.SetCursor(Portal, hotspot, CursorMode.Auto);
			Cursor.visible = true;
			ActiveCursor = CursorPortal;
			break;
		}

	}
}


public enum CursorState
{
	None,
	Default,
	Aim,
	Observe,
	Hand,
	Talk,
	Portal,
}
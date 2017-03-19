﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCManager
{
	public GoapGoal DynamicGoalGuard;
	public GoapGoal DynamicGoalFollow;
	public GoapGoal DynamicGoalPatrol;

	private float _spawnTimer;

	private int _counter;

	public List<HumanCharacter> HumansInScene
	{
		get { return _humansInScene; }
	}

	public List<MutantCharacter> MutantsInScene
	{
		get { return _mutantsInScene; }
	}

	public List<Character> AllCharacters
	{
		get { return _allCharacters; }
	}

	public Dictionary<Faction, FactionData> AllFactions
	{
		get { return _allFactions; }
	}

	private List<HumanCharacter> _humansInScene;
	private List<MutantCharacter> _mutantsInScene;
	private List<Character> _allCharacters;
	private Dictionary<Faction, FactionData> _allFactions;
	private Dictionary<Character, int> _deadBodies;
	private Dictionary<Character, float> _fadeTimers;
	private int _characterIndex;

	public void Initialize()
	{
		_allCharacters = new List<Character>();
		_humansInScene = new List<HumanCharacter>();
		_mutantsInScene = new List<MutantCharacter>();
		_allFactions = new Dictionary<Faction, FactionData>();
		_deadBodies = new Dictionary<Character, int>();
		_fadeTimers = new Dictionary<Character, float>();

		DynamicGoalGuard = GameManager.Inst.DBManager.DBHandlerAI.GetGoalByID(6);
		DynamicGoalGuard.Priority = 5;
		DynamicGoalFollow = GameManager.Inst.DBManager.DBHandlerAI.GetGoalByID(8);
		DynamicGoalFollow.Priority = 5;
		DynamicGoalPatrol = GameManager.Inst.DBManager.DBHandlerAI.GetGoalByID(2);
		DynamicGoalPatrol.Priority = 5;

		FactionData newFaction1 = new FactionData();
		newFaction1.Name = "Player";
		newFaction1.FactionID = Faction.Player;

		FactionData newFaction2 = new FactionData();
		newFaction2.Name = "The Legionnaires";
		newFaction2.FactionID = Faction.Legionnaires;

		FactionData newFaction3 = new FactionData();
		newFaction3.Name = "Millitary";
		newFaction3.FactionID = Faction.Military;

		FactionData newFaction4 = new FactionData();
		newFaction4.Name = "Mutants";
		newFaction4.FactionID = Faction.Mutants;

		FactionData newFaction5 = new FactionData();
		newFaction5.Name = "Civilian";
		newFaction5.FactionID = Faction.Civilian;

		newFaction2.AddRelationshipEntry(Faction.Player, 0);
		newFaction2.AddRelationshipEntry(Faction.Mutants, 0);

		newFaction3.AddRelationshipEntry(Faction.Player, 0);
		newFaction3.AddRelationshipEntry(Faction.Mutants, 0);

		newFaction4.AddRelationshipEntry(Faction.Player, 0);
		newFaction4.AddRelationshipEntry(Faction.Legionnaires, 0);
		newFaction4.AddRelationshipEntry(Faction.Military, 0);

		newFaction5.AddRelationshipEntry(Faction.Player, 0.5f);

		_allFactions.Add(newFaction1.FactionID, newFaction1);
		_allFactions.Add(newFaction2.FactionID, newFaction2);
		_allFactions.Add(newFaction3.FactionID, newFaction3);
		_allFactions.Add(newFaction4.FactionID, newFaction4);
		_allFactions.Add(newFaction5.FactionID, newFaction5);


		//initialize preset characters
		GameObject [] characters = GameObject.FindGameObjectsWithTag("NPC");
		foreach(GameObject o in characters)
		{
			Character c = o.GetComponent<Character>();
			if(c != null)
			{
				c.Initialize();
			}
		}
	}

	public void PerSecondUpdate()
	{
		//build a dead body list
		//remove dead bodies that have been dead for some time
		List<Character> allCharactersCopy = new List<Character>(_allCharacters);
		foreach(Character c in allCharactersCopy)
		{
			if(c.MyStatus.Health <= 0)
			{
				if(_deadBodies.ContainsKey(c))
				{
					_deadBodies[c] ++;
					if(_deadBodies[c] > 45)
					{
						if(c.IsHuman)
						{
							_humansInScene.Remove((HumanCharacter)c);
						}
						else
						{
							_mutantsInScene.Remove((MutantCharacter)c);
						}
						_allCharacters.Remove(c);
						_deadBodies.Remove(c);
						GameObject.Destroy(c.gameObject);
					}
				}
				else
				{
					_deadBodies.Add(c, 0);
				}

			}
		}



	}

	public void PerFrameUpdate()
	{
		HandleCharacterHiding();
	}

	public void AddHumanCharacter(HumanCharacter character)
	{
		_humansInScene.Add(character);
		_allCharacters.Add(character);
	}

	public void AddMutantCharacter(MutantCharacter character)
	{
		_mutantsInScene.Add(character);
		_allCharacters.Add(character);
	}

	public void RemoveHumanCharacter(HumanCharacter character)
	{
		if(_humansInScene.Contains(character))
		{
			_humansInScene.Remove(character);

		}

		if(_deadBodies.ContainsKey((Character)character))
		{
			_deadBodies.Remove((Character)character);
		}
		_allCharacters.Remove((Character)character);
		if(_fadeTimers.ContainsKey((Character)character))
		{
			_fadeTimers.Remove((Character)character);
		}
		GameObject.Destroy(character.gameObject);
	}

	public void RemoveMutantCharacter(MutantCharacter character)
	{
		if(_mutantsInScene.Contains(character))
		{
			_mutantsInScene.Remove(character);

		}

		if(_deadBodies.ContainsKey((Character)character))
		{
			_deadBodies.Remove((Character)character);
		}
		_allCharacters.Remove((Character)character);
		if(_fadeTimers.ContainsKey((Character)character))
		{
			_fadeTimers.Remove((Character)character);
		}
		GameObject.Destroy(character.gameObject);
	}

	public int GetLivingHumansCount()
	{
		int i = 0;
		foreach(HumanCharacter h in _humansInScene)
		{
			if(h.MyStatus.Health > 0)
			{
				i++;
			}
		}

		return i;
	}

	public int GetLivingMutantsCount()
	{
		int i = 0;
		foreach(MutantCharacter m in _mutantsInScene)
		{
			if(m.MyStatus.Health > 0)
			{
				i++;
			}
		}

		return i;
	}

	public int GetNearbyEnemyCount()
	{
		int enemies = 0;
		Vector3 playerPos = GameManager.Inst.PlayerControl.SelectedPC.transform.position;
		foreach(MutantCharacter m in _mutantsInScene)
		{
			if(m.MyStatus.Health > 0 && Vector3.Distance(playerPos, m.transform.position) < 50)
			{
				enemies ++;
			}
		}

		foreach(HumanCharacter h in _humansInScene)
		{
			if(h.MyStatus.Health > 0 && h.MyAI.IsCharacterEnemy(GameManager.Inst.PlayerControl.SelectedPC) && Vector3.Distance(playerPos, h.transform.position) < 50)
			{
				enemies ++;
			}
		}

		return enemies;
	}

	public HumanCharacter SpawnRandomHumanCharacter(string name, AISquad squad, Vector3 loc)
	{
		HumanCharacter character = GameObject.Instantiate(Resources.Load("HumanCharacter") as GameObject).GetComponent<HumanCharacter>();



		character.CharacterID = name;

		character.Initialize();
		character.MyNavAgent.enabled = false;

		character.GoapID = 0;
		character.Faction = squad.Faction;
		GameManager.Inst.ItemManager.LoadNPCInventory(character.Inventory, squad.Faction);
		character.MyAI.WeaponSystem.LoadWeaponsFromInventory(false);

		character.MyStatus.MaxHealth = 100;
		character.MyStatus.Health = 100;

		character.transform.position = loc;
		//character.transform.position = new Vector3(70.76f, 3.296f, -23.003f);

		character.MyNavAgent.enabled = true;

		character.gameObject.name = character.gameObject.name + _counter.ToString();
		_counter ++;

		//AddHumanCharacter(character);

		return character;
	}

	public MutantCharacter SpawnRandomMutantCharacter(string name, int goapID, Vector3 loc)
	{
		MutantCharacter character = GameObject.Instantiate(Resources.Load("MutantCharacter") as GameObject).GetComponent<MutantCharacter>();

		character.CharacterID = name;

		character.Initialize();

		character.MyNavAgent.enabled = false;
		character.GoapID = goapID;
		character.Faction = Faction.Mutants;
		GameManager.Inst.ItemManager.LoadNPCInventory(character.Inventory, Faction.Mutants);
		character.MyAI.WeaponSystem.LoadWeaponsFromInventory(false);

		character.MyStatus = GetMutantStatus(character);

		character.transform.position = loc;

		character.MyNavAgent.enabled = true;

		character.gameObject.name = character.gameObject.name + _counter.ToString();
		_counter ++;

		//AddMutantCharacter(character);

		return character;
	}

	public FactionData GetFactionData(Faction id)
	{
		if(_allFactions.ContainsKey(id))
		{
			return _allFactions[id];
		}

		return null;
	}


	private CharacterStatus GetMutantStatus(MutantCharacter character)
	{
		CharacterStatus status = character.MyStatus;

		if(character.CharacterID == "Mutant1")
		{
			status.MaxHealth = 200;
			status.Health = 200;
			status.WalkSpeed = 1;
			status.RunSpeed = 4.0f;
			status.SprintSpeed = 5.0f;
			status.WalkSpeedModifier = 0.9f;
			status.RunSpeedModifier = 1.1f;
			status.SprintSpeedModifier = 1.1f;
			status.EyeSight = 1.5f;
			status.Intelligence = 2;
			status.MutantMovementBlend = 0;
		}
		else
		{
			status.MaxHealth = UnityEngine.Random.Range(80f, 150f);
			status.Health = status.MaxHealth;

			status.MutantMovementBlend = UnityEngine.Random.Range(-1, 3);
			if(status.MutantMovementBlend == 0)
			{
				status.WalkSpeed = 0.8f;
				status.RunSpeed = UnityEngine.Random.Range(0.9f, 1.5f);
				status.SprintSpeed = 5.2f;
				status.WalkSpeedModifier = 1;
				status.RunSpeedModifier = 0.8f;
				status.SprintSpeedModifier = 1.1f;
							
			}
			else if(status.MutantMovementBlend == 2)
			{
				status.WalkSpeed = 1f;
				status.RunSpeed = 2f;
				status.SprintSpeed = 5.2f;
				status.WalkSpeedModifier = 1;
				status.RunSpeedModifier = 1f;
				status.SprintSpeedModifier = 1.1f;
			}
			else if(status.MutantMovementBlend == 1)
			{
				status.WalkSpeed = 0.8f;
				status.RunSpeed = 0.9f;
				status.SprintSpeed = 5.2f;
				status.WalkSpeedModifier = 1;
				status.RunSpeedModifier = 0.8f;
				status.SprintSpeedModifier = 1.1f;
			}
			else if(status.MutantMovementBlend == -1)
			{
				status.WalkSpeed = 1f;
				status.RunSpeed = 0.8f;
				status.SprintSpeed = 5.2f;
				status.WalkSpeedModifier = 1;
				status.RunSpeedModifier = 1f;
				status.SprintSpeedModifier = 1.1f;
			}

			status.EyeSight = 1.5f;
			status.Intelligence = 2;

		}


		return status;
	}

	private void RandomizeMutantDamage(Character mutant)
	{

	}
		

	private void HandleCharacterHiding()
	{
		if(_allCharacters.Count <= 1)
		{
			return;
		}

		if(_characterIndex < _allCharacters.Count && _allCharacters[_characterIndex] != null)
		{
			RaycastHit buildingHit;
			bool isVisible = true;
			if(Physics.Raycast(_allCharacters[_characterIndex].transform.position, Vector3.down, out buildingHit, 200, (1 << 9 | 1 << 8 | 1 << 10)))
			{
				BuildingComponent component = buildingHit.collider.GetComponent<BuildingComponent>();
				if(component != null && component.Building.IsActive)
				{
					//means player is also in this building
					if(component.IsHidden)
					{
						//hide NPC as well
						isVisible = false;
					}

				}

			}




			/*
			Vector3 playerEye = GameManager.Inst.PlayerControl.SelectedPC.MyReference.Eyes.transform.position;
			//skip this character if too far away or is dead
			int iterations = 0;
			while((_allCharacters[_characterIndex].MyStatus.Health <= 0 || Vector3.Distance(playerEye, _allCharacters[_characterIndex].transform.position) > 50
				|| _allCharacters[_characterIndex].MyAI.ControlType == AIControlType.Player) && iterations <= _allCharacters.Count)
			{
				iterations ++;
				_characterIndex ++;
				if(_characterIndex >= _allCharacters.Count)
				{
					_characterIndex = 0;
				}

				if(iterations > _allCharacters.Count)
				{
					return;
				}
			}



			float colliderHeight = _allCharacters[_characterIndex].GetComponent<CapsuleCollider>().height;

			Vector3 rayTarget = _allCharacters[_characterIndex].transform.position + Vector3.up * colliderHeight * 0.7f;
			Vector3 raycastDir = rayTarget - playerEye;
			//do a raycast to see if player can see it
			RaycastHit hit;
			bool isVisible = false;
			if(Physics.Raycast(playerEye, raycastDir, out hit, 50, ~(1 << 12)))
			{
				if(hit.collider.gameObject == _allCharacters[_characterIndex].gameObject)
				{
					isVisible = true;
				}
				else
				{
					isVisible = false;
				}


			}
			else
			{
				isVisible = false;
			}
			*/


			if(isVisible && _allCharacters[_characterIndex].IsHidden)
			{
				//unfade character
				ShowCharacter(_allCharacters[_characterIndex]);
			}
			else if(!isVisible && !_allCharacters[_characterIndex].IsHidden)
			{
				//fade character
				Debug.Log("Hiding character " + _allCharacters[_characterIndex].name);
				HideCharacter(_allCharacters[_characterIndex]);
			}


		}

		_characterIndex ++;
		if(_characterIndex >= _allCharacters.Count)
		{
			_characterIndex = 0;
		}


	}

	private void HideCharacter(Character character)
	{
		/*
		if(!_fadeTimers.ContainsKey(character))
		{
			_fadeTimers.Add(character, 0);
		}

		if(_fadeTimers[character] < 3)
		{
			_fadeTimers[character] += Time.deltaTime;
			return;
		}
		*/

		//hide weapons
		Renderer [] childRenderers = character.GetComponentsInChildren<Renderer>();
		foreach(Renderer r in childRenderers)
		{
			r.enabled = false;
		}

		character.IsHidden = true;

		if(_fadeTimers.ContainsKey(character))
		{
			_fadeTimers[character] = 0;
		}
	}

	private void ShowCharacter(Character character)
	{
		Renderer [] childRenderers = character.GetComponentsInChildren<Renderer>();
		foreach(Renderer r in childRenderers)
		{
			r.enabled = true;
		}

		character.IsHidden = false;

		if(_fadeTimers.ContainsKey(character))
		{
			_fadeTimers[character] = 0;
		}
	}
}

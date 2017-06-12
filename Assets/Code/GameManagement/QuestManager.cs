﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestManager 
{
	public QuestBase CurrentQuest;

	public Dictionary<string, StoryCondition> StoryConditions;
	public Dictionary<string, StoryEventScript> Scripts;

	public void Initialize()
	{
		Scripts = new Dictionary<string, StoryEventScript>();
		StoryConditions = new Dictionary<string, StoryCondition>();

		//CurrentQuest = new WaveDefenseQuest();
		//CurrentQuest.StartQuest();

		//populate story conditions manually for now
		 
		StoryConditionItem cond1 = new StoryConditionItem();
		cond1.ID = "hastomatoseeds";
		cond1.ItemID = "mutantheart";
		StoryConditions.Add(cond1.ID, cond1);

		StoryConditionTrigger cond2 = new StoryConditionTrigger();
		cond2.ID = "zsk_village_gate_open";
		cond2.SetValue(0);
		StoryConditions.Add(cond2.ID, cond2);

		StoryConditionTrigger cond3 = new StoryConditionTrigger();
		cond3.ID = "zsk_sid_intro_done";
		cond3.SetValue(0);
		StoryConditions.Add(cond3.ID, cond3);
	

		StoryEventScript script1 = new StoryEventScript();
		script1.Script.Add("door Level1RoadBlockGate toggle");
		Scripts.Add("zsk_roadblockgate_toggle", script1);

		StoryEventScript script2 = new StoryEventScript();
		script2.Script.Add("object FarmIrrigatorHandle on");
		Scripts.Add("zsk_irrigator_on", script2);

		StoryEventScript script3 = new StoryEventScript();
		script3.Script.Add("door ZernaskayaSheetFenceDoor unlock");
		Scripts.Add("zsk_village_exit_unlock", script3);


	}

	public void PerSecondUpdate()
	{
		//CurrentQuest.PerSecondUpdate();
	}


}

/*
 * 
 * 
bool - zsk_village_gate_open
bool - zsk_sid_intro_done //whether sidorovich finished explaining
 * 
 * 
 * 
 */
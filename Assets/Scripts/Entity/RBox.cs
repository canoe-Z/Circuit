using System.Collections.Generic;
using SpiceSharp.Components;
using UnityEngine;

public class RBox : EntityBase
{
	public double R_99999 = 0;
	public double R_99 = 0;
	public double R_09 = 0;
	public MySlider[] sliders = new MySlider[6];

	public override void EntityAwake()
	{
		MySlider[] slidersDisorder = this.gameObject.GetComponentsInChildren<MySlider>();
		foreach (MySlider sld in slidersDisorder)
		{
			if (int.TryParse(sld.gameObject.name, out int id))
				sliders[id] = sld;
			else
				Debug.LogError("ErrorSliderID");
		}
	}

	void Update()
	{
		int total = 0;
		for (int i = 0; i < 6; i++)
		{
			total *= 10;
			total += sliders[i].SliderPos_int;
		}
		this.R_99999 = (float)total / (float)10;
		this.R_99 = (float)(total % 100) / (float)10;
		this.R_09 = (float)(total % 10) / (float)10;
	}

	override public bool IsConnected()//判断是否有一端连接，避免浮动节点
	{
		if (ChildPorts[0].Connected == 1 || ChildPorts[1].Connected == 1 || ChildPorts[2].Connected == 1 || ChildPorts[3].Connected == 1)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	override public void LoadElement()
	{
		int G, R999, R99, R9;
		G = ChildPorts[0].ID;
		R9 = ChildPorts[1].ID;
		R99 = ChildPorts[2].ID;
		R999 = ChildPorts[3].ID;
		
		CircuitCalculator.UF.Union(G, R9);
		CircuitCalculator.UF.Union(G, R99);
		CircuitCalculator.UF.Union(G, R999);
	}
	override public void SetElement()
	{
		int EntityID = CircuitCalculator.EntityNum;
		int G, R999, R99, R9;
		G = ChildPorts[0].ID;
		R9 = ChildPorts[1].ID;
		R99 = ChildPorts[2].ID;
		R999 = ChildPorts[3].ID;

		// 指定三个电阻的ID
		string[] ResistorID = new string[3];
		for (int i = 0; i < 3; i++)
		{
			ResistorID[i] = string.Concat(EntityID, "_", i);
		}

		CircuitCalculator.SpiceEntities.Add(new Resistor(ResistorID[0], G.ToString(), R999.ToString(), R_99999));
		CircuitCalculator.SpiceEntities.Add(new Resistor(ResistorID[1], G.ToString(), R99.ToString(), R_99));
		CircuitCalculator.SpiceEntities.Add(new Resistor(ResistorID[2], G.ToString(), R9.ToString(), R_09));
	}

	public override EntityData Save()
	{
		List<float> sliderPosList = new List<float>();
		for (int i = 0; i < 6; i++)
		{
			sliderPosList.Add(sliders[i].SliderPos);
		}
		return new RboxData(sliderPosList, transform.position, transform.rotation, ChildPortID);
	}
}

// TODO:Rbox后期可能会改用旋钮，因此暂时不更新保存方法
[System.Serializable]
public class RboxData : EntityData
{
	private readonly List<float> sliderPosList;
	public RboxData(List<float> sliderPosList, Vector3 pos, Quaternion angle, List<int> id) : base(pos, angle, id)
	{
		this.sliderPosList = sliderPosList;
	}

	override public void Load()
	{
		RBox rbox = EntityCreator.CreateEntity<RBox>(posfloat, anglefloat, IDList);
		for (int i = 0; i < 6; i++)
		{
			rbox.sliders[i].ChangeSliderPos(sliderPosList[i]);
		}
	}
}
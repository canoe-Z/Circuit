using SpiceSharp.Components;
using UnityEngine;

/// <summary>
/// 微安表，有各种量程和内阻的规格，内阻为标称
/// </summary>
public class NominaluA : EntityBase, ICalculatorUpdate
{
    private int maxuI;                  // 量程，单位微安
    private double resistance;          // 内阻真实值
    private MyPin myPin;                // 表盘指针
    private int PortID_GND, PortID_V0;

    public override void EntityAwake()
    {
        myPin = GetComponentInChildren<MyPin>();

        // 必须手动初始化Pin来保证Pin的初始化顺序
        myPin.PinAwake();
        myPin.SetPos(0);
    }

    void Start()
    {
        // CalculatorUpdate()统一在Start()中执行，保证在实例化并写入元件自身属性完毕后执行
        CircuitCalculator.CalculateEvent += CalculatorUpdate;
        CalculatorUpdate();

        PortID_GND = ChildPorts[0].ID;
        PortID_V0 = ChildPorts[1].ID;
    }

    public void CalculatorUpdate()
    {
        // 计算自身电流
        ChildPorts[1].I = (ChildPorts[1].U - ChildPorts[0].U) / resistance;

        double maxI = maxuI / 1e6;
        myPin.SetPos((float)(ChildPorts[1].I / maxI));
    }

    public override void LoadElement()
    {
        CircuitCalculator.UF.Union(PortID_GND, PortID_V0);
    }

    public override void SetElement(int entityID)
    {
        CircuitCalculator.SpiceEntities.Add(new Resistor(entityID.ToString(), PortID_GND.ToString(), PortID_V0.ToString(), resistance));
        CircuitCalculator.SpicePorts.AddRange(ChildPorts);
    }

    public static GameObject Create(int maxuI, double resistance)
    {
        NominaluA nominaluA = BaseCreate<NominaluA>().Set(maxuI, resistance);
        return nominaluA.gameObject;
    }

    private NominaluA Set(int maxuI, double resistance)
    {
        this.resistance = resistance;
        this.maxuI = maxuI;
        myPin.SetString("uA", maxuI);
        return this;
    }

    public override EntityData Save() => new NominaluAData(this);

    [System.Serializable]
    public class NominaluAData : EntityData
    {
        private readonly double resistance;
        private readonly int maxuI;

        public NominaluAData(NominaluA nominaluA)
        {
            baseData = new EntityBaseData(nominaluA);
            resistance = nominaluA.resistance;
            maxuI = nominaluA.maxuI;
        }

        public override void Load()
        {
            NominaluA nominaluA = BaseCreate<NominaluA>(baseData).Set(maxuI, resistance);
            nominaluA.resistance = resistance;
        }
    }
}
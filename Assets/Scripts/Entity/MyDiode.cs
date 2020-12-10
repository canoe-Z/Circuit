using System;
using System.Linq;
using System.Text.RegularExpressions;
using SpiceSharp.Components;
using SpiceSharp.Entities;

/// <summary>
/// 二极管
/// </summary>
public class MyDiode : EntityBase
{
    private int PortID_G, PortID_V;

    public override void EntityAwake() { }

    void Start()
    {
        PortID_G = ChildPorts[0].ID;
        PortID_V = ChildPorts[1].ID;
    }

    public override void LoadElement() => CircuitCalculator.UF.Union(PortID_G, PortID_V);

    // 构建二极管模型
    protected void ApplyParameters(Entity entity, string definition)
    {
        // Get all assignments
        definition = Regex.Replace(definition, @"\s*\=\s*", "=");
        var assignments = definition.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var assignment in assignments)
        {
            // Get the name and value
            var parts = assignment.Split('=');
            if (parts.Length != 2)
                throw new System.Exception("Invalid assignment");
            var name = parts[0].ToLower();
            var value = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);

            // Set the entity parameter
            entity.SetParameter(name, value);
        }
    }

    private DiodeModel CreateDiodeModel(string name, string parameters)
    {
        var dm = new DiodeModel(name);
        ApplyParameters(dm, parameters);
        return dm;
    }

    public override void SetElement(int entityID)
    {
        PortID_G = ChildPorts[0].ID;
        PortID_V = ChildPorts[1].ID;

        CircuitCalculator.SpiceEntities.Add(new Diode(string.Concat(entityID, "_D"),
            PortID_V.ToString(), PortID_G.ToString(), "DIO_1N4007"));

        if (CircuitCalculator.SpiceEntities.SingleOrDefault(x => x.Name == "DIO_1N4007") == null)
        {
            CreateDiodeModel("DIO_1N4007", "Is=1.09774e-8 Rs=0.0414388 N=1.78309 Cjo=2.8173e-11 M=0.318974 tt=9.85376e-6 Kf=0 Af=1");
		}
    }

    public override EntityData Save() => new SimpleEntityData<MyDiode>(this);
}
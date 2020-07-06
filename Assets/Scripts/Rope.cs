using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class Rope : MonoBehaviour
{
	private ObiRopeBlueprint blueprint;
	public static ObiSolver CreateSolver()
	{
		GameObject solverObject = new GameObject("solver", typeof(ObiSolver), typeof(ObiFixedUpdater));
		ObiSolver solver = solverObject.GetComponent<ObiSolver>();
		ObiFixedUpdater updater = solverObject.GetComponent<ObiFixedUpdater>();
		updater.solvers.Add(solver);
		return solver;
	}

	public GameObject CreateRope(GameObject obj1, GameObject obj2, ObiSolver solver)
	{
		GameObject ropeObject = new GameObject("rope", typeof(ObiRope), typeof(ObiRopeExtrudedRenderer));
		ObiRope rope = ropeObject.GetComponent<ObiRope>();
		ObiRopeExtrudedRenderer ropeRenderer = ropeObject.GetComponent<ObiRopeExtrudedRenderer>();
		ropeRenderer.section = Resources.Load<ObiRopeSection>("DefaultRopeSection");
		blueprint = ScriptableObject.CreateInstance<ObiRopeBlueprint>();

		blueprint.thickness = 0.025f;
		blueprint.resolution = 0.37f;

		//实现导线高出接线柱一段距离
		var pos1 = obj1.transform.TransformPoint(new Vector3(0, 0.14f, 0));
		var pos2 = obj2.transform.TransformPoint(new Vector3(0, 0.14f, 0));

		blueprint.path.Clear();
		blueprint.path.AddControlPoint(pos1, -Vector3.right, Vector3.right, Vector3.up, 0.1f, 0.1f, 1, 1, Color.white, "start");
		blueprint.path.AddControlPoint(pos2, -Vector3.right, Vector3.right, Vector3.up, 0.1f, 0.1f, 1, 1, Color.white, "end");
		blueprint.path.FlushEvents();
		blueprint.Generate();

		rope.ropeBlueprint = blueprint;
		rope.GetComponent<MeshRenderer>().enabled = true;

		ObiParticleAttachment ParticleAttachment1 = ropeObject.AddComponent<ObiParticleAttachment>();
		ObiParticleAttachment ParticleAttachment2 = ropeObject.AddComponent<ObiParticleAttachment>();
		ParticleAttachment1.target = obj1.transform;
		ParticleAttachment2.target = obj2.transform;
		ParticleAttachment1.particleGroup = blueprint.groups[0];
		ParticleAttachment2.particleGroup = blueprint.groups[1];

		rope.transform.parent = solver.transform;
		return ropeObject;
	}
}

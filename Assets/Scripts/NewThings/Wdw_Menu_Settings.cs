using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Wdw_Menu_Settings : MonoBehaviour
{
	public Slider sldMove;
	public Slider sldTurn;
    void Start()
    {
		sldMove.onValueChanged.AddListener(ChangeMove);
		sldTurn.onValueChanged.AddListener(ChangeTurn);
		MoveController.myMoveSpeed = sldMove.value;
		MoveController.myTurnSpeed = sldTurn.value;
	}
	
	void ChangeMove(float value)
	{
		MoveController.myMoveSpeed = value;
	}
	void ChangeTurn(float value)
	{
		MoveController.myTurnSpeed = value;
	}
}

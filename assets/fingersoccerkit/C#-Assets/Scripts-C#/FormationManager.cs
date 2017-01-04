using UnityEngine;
using System.Collections;

public class FormationManager : MonoBehaviour {


	///*************************************************************************///
	/// Main Formation manager.
	/// You can define new formations here.
	/// To define new positions and formations, please do the following:
	/*
	1. add +1 to formations counter.
	2. define a new case in "getPositionInFormation" function.
	3. for all 5 units, define an exact position on Screen. (you can copy a case and edit it's values)
	4. Note. You always set the units position, as if they are on the left side of the field. 
	The controllers automatically process the position of the units, if they belong to the right side of the field.
	*/
	///*************************************************************************///

	// Available Formations:
	/*
	1-2-2
	1-3-1
	1-2-1-1
	1-4-0
	1-1-1-1-1
	*/

	public static int formations = 5;		//total number of available formations
	public static float fixedZ = -0.5f;		//fixed Z position for all units on the selected formation
	public static float yFixer = -0.75f;	//if you ever needed to translate all units up or down a little bit, you can do it by
											//tweeking this yFixer variable.
	//*****************************************************************************
	// Every unit reads it's position from this function.
	// Units give out their index and formation and get their exact position.
	//*****************************************************************************
	public static Vector3 getPositionInFormation ( int _formationIndex ,   int _UnitIndex  ){
		Vector3 output = Vector3.zero;
		switch(_formationIndex) {
			case 0:
				if(_UnitIndex == 0) output = new Vector3(-15, 0 + yFixer, fixedZ);
				if(_UnitIndex == 1) output = new Vector3(-10, 5 + yFixer, fixedZ);
				if(_UnitIndex == 2) output = new Vector3(-10, -5 + yFixer, fixedZ);
				if(_UnitIndex == 3) output = new Vector3(-4.5f, 2 + yFixer, fixedZ);
				if(_UnitIndex == 4) output = new Vector3(-4.5f, -2 + yFixer, fixedZ);
				break;
			
			case 1:
				if(_UnitIndex == 0) output = new Vector3(-14, 0 + yFixer, fixedZ);
				if(_UnitIndex == 1) output = new Vector3(-9.5f, 0 + yFixer, fixedZ);
				if(_UnitIndex == 2) output = new Vector3(-7, 3.5f + yFixer, fixedZ);
				if(_UnitIndex == 3) output = new Vector3(-7, -3.5f + yFixer, fixedZ);
				if(_UnitIndex == 4) output = new Vector3(-3, 0 + yFixer, fixedZ);
				break;
			
			case 2:
				if(_UnitIndex == 0) output = new Vector3(-15, 0 + yFixer, fixedZ);
				if(_UnitIndex == 1) output = new Vector3(-11, 3.5f + yFixer, fixedZ);
				if(_UnitIndex == 2) output = new Vector3(-11, -3.5f + yFixer, fixedZ);
				if(_UnitIndex == 3) output = new Vector3(-6, 0 + yFixer, fixedZ);
				if(_UnitIndex == 4) output = new Vector3(-3, 0 + yFixer, fixedZ);
				break;
			
			case 3:
				if(_UnitIndex == 0) output = new Vector3(-14, 0 + yFixer, fixedZ);
				if(_UnitIndex == 1) output = new Vector3(-11, 5.5f + yFixer, fixedZ);
				if(_UnitIndex == 2) output = new Vector3(-11, 2 + yFixer, fixedZ);
				if(_UnitIndex == 3) output = new Vector3(-11, -2 + yFixer, fixedZ);
				if(_UnitIndex == 4) output = new Vector3(-11, -5.5f + yFixer, fixedZ);
				break;
			
			case 4:
				if(_UnitIndex == 0) output = new Vector3(-15, 0 + yFixer, fixedZ);
				if(_UnitIndex == 1) output = new Vector3(-12.5f, 2.5f + yFixer, fixedZ);
				if(_UnitIndex == 2) output = new Vector3(-9, 4.5f + yFixer, fixedZ);
				if(_UnitIndex == 3) output = new Vector3(-5, 5.5f + yFixer, fixedZ);
				if(_UnitIndex == 4) output = new Vector3(-1.5f, 5.5f + yFixer, fixedZ);
				break;
		}
		
		return output;
	}

}
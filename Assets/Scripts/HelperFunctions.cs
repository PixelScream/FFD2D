using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperFunctions
 {

	public static bool IsArrayEmpty<T>( this T[] test )
	{
		return test == null || test.Length <= 0;
	}

	public static bool IsCorrectLength<T>(this T[] test, int length)
	{
		return test != null && test.Length == length;
	}
}

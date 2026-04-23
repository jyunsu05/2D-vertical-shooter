using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
// adapted from http://wiki.unity3d.com/index.php/DrawArrow 


public enum ArrowType
{
  Default,
	Thin,
	Double,
	Triple,
	Solid,
	Fat,
	ThreeD,
}

public static class DrawArrow
{
	public static void ForGizmo(Vector3 pos, Vector3 direction, Color? color = null,  bool doubled = false, float arrowHeadLength = 0.2f, float arrowHeadAngle = 20.0f)
	{
		Gizmos.color = color ?? Color.white;

		//arrow shaft
		Gizmos.DrawRay(pos, direction);
 
		if (direction != Vector3.zero)
		{
			//arrow head
			Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+arrowHeadAngle,0) * new Vector3(0,0,1);
			Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-arrowHeadAngle,0) * new Vector3(0,0,1);
			Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
			Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
		}
	}
 
	public static void ForDebug(Vector3 pos, Vector3 direction, float duration = 0.5f, Color? color = null, ArrowType type = ArrowType.Default, float arrowHeadLength = 0.2f, float arrowHeadAngle = 30.0f, bool sceneCamFollows = false)
	{
		Color actualColor = color ?? Color.white;
		duration = duration/Time.timeScale;
		
		float width = 0.01f;

		Vector3 directlyRight = Vector3.zero;
		Vector3 directlyLeft = Vector3.zero;
		Vector3 directlyBack = Vector3.zero;
		Vector3 headRight = Vector3.zero;
		Vector3 headLeft = Vector3.zero;

		if (direction != Vector3.zero)
		{
			directlyRight = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+90,0) * new Vector3(0,0,1);
			directlyLeft = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-90,0) * new Vector3(0,0,1);
			directlyBack = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180,0) * new Vector3(0,0,1);
			headRight = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+arrowHeadAngle,0) * new Vector3(0,0,1);
			headLeft = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-arrowHeadAngle,0) * new Vector3(0,0,1);
		}		

		//draw arrow head
		Debug.DrawRay(pos + direction, headRight * arrowHeadLength, actualColor, duration);
		Debug.DrawRay(pos + direction, headLeft * arrowHeadLength, actualColor, duration);
		
		switch (type) {
		case ArrowType.Default:
			Debug.DrawRay(pos, direction, actualColor, duration); //draw center line
			break;
		case ArrowType.Double:
			Debug.DrawRay(pos + directlyRight * width, direction * (1-width), actualColor, duration); //draw line slightly to right
			Debug.DrawRay(pos +  directlyLeft * width, direction * (1-width), actualColor, duration); //draw line slightly to left

			//draw second arrow head
			Debug.DrawRay(pos + directlyBack * width + direction, headRight * arrowHeadLength, actualColor, duration);
			Debug.DrawRay(pos + directlyBack * width + direction, headLeft * arrowHeadLength, actualColor, duration);
			
			break;
		case ArrowType.Triple:
			Debug.DrawRay(pos, direction, actualColor, duration); //draw center line
			Debug.DrawRay(pos + directlyRight * width, direction * (1-width), actualColor, duration); //draw line slightly to right
			Debug.DrawRay(pos +  directlyLeft * width, direction * (1-width), actualColor, duration); //draw line slightly to left
			break;
		case ArrowType.Fat:
			break;
		case ArrowType.Solid:
			int increments = 20;
			for (int i=0;i<increments;i++)
			{
				float displacement = Mathf.Lerp(-width, +width, i/(float)increments);
				//draw arrow body
				Debug.DrawRay(pos + directlyRight * displacement, direction, actualColor, duration); //draw line slightly to right
				Debug.DrawRay(pos +  directlyLeft * displacement, direction, actualColor, duration); //draw line slightly to left
				//draw arrow head
				Debug.DrawRay((pos + direction) + directlyRight * displacement, headRight * arrowHeadLength, actualColor, duration);
				Debug.DrawRay((pos + direction) + directlyRight * displacement, headLeft * arrowHeadLength, actualColor, duration);
			}
			break;
		case ArrowType.Thin:
			Debug.DrawRay(pos, direction, actualColor, duration); //draw center line
			break;
		case ArrowType.ThreeD:
			break;
		}

/*#if UNITY_EDITOR
    //snap the Scene view camera to a spot where it is looking directly at this arrow.
		if (sceneCamFollows)
			SceneViewCameraFollower.activateAt(pos + direction, duration, "_arrow");
#endif*/
	}

	// 2D Arrow Methods
	public static void ForGizmo2D(Vector2 pos, Vector2 direction, Color? color = null, float arrowHeadLength = 0.2f, float arrowHeadAngle = 20.0f)
	{
		Gizmos.color = color ?? Color.white;

		Vector3 pos3 = new Vector3(pos.x, pos.y, 0);
		Vector3 dir3 = new Vector3(direction.x, direction.y, 0);

		//arrow shaft
		Gizmos.DrawRay(pos3, dir3);

		if (direction != Vector2.zero)
		{
			//arrow head - 2D calculation
			float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
			
			Vector3 headRight = new Vector3(
				Mathf.Cos((angle + 180 + arrowHeadAngle) * Mathf.Deg2Rad),
				Mathf.Sin((angle + 180 + arrowHeadAngle) * Mathf.Deg2Rad),
				0
			);
			Vector3 headLeft = new Vector3(
				Mathf.Cos((angle + 180 - arrowHeadAngle) * Mathf.Deg2Rad),
				Mathf.Sin((angle + 180 - arrowHeadAngle) * Mathf.Deg2Rad),
				0
			);
			
			Gizmos.DrawRay(pos3 + dir3, headRight * arrowHeadLength);
			Gizmos.DrawRay(pos3 + dir3, headLeft * arrowHeadLength);
		}
	}

	public static void ForDebug2D(Vector2 pos, Vector2 direction, float duration = 0.5f, Color? color = null, ArrowType type = ArrowType.Default, float arrowHeadLength = 0.2f, float arrowHeadAngle = 30.0f)
	{
		Color actualColor = color ?? Color.white;
		duration = duration / Time.timeScale;

		Vector3 pos3 = new Vector3(pos.x, pos.y, 0);
		Vector3 dir3 = new Vector3(direction.x, direction.y, 0);

		if (direction == Vector2.zero)
			return;

		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

		// Calculate arrow head directions
		Vector3 headRight = new Vector3(
			Mathf.Cos((angle + 180 + arrowHeadAngle) * Mathf.Deg2Rad),
			Mathf.Sin((angle + 180 + arrowHeadAngle) * Mathf.Deg2Rad),
			0
		);
		Vector3 headLeft = new Vector3(
			Mathf.Cos((angle + 180 - arrowHeadAngle) * Mathf.Deg2Rad),
			Mathf.Sin((angle + 180 - arrowHeadAngle) * Mathf.Deg2Rad),
			0
		);

		// Draw arrow head
		Debug.DrawRay(pos3 + dir3, headRight * arrowHeadLength, actualColor, duration);
		Debug.DrawRay(pos3 + dir3, headLeft * arrowHeadLength, actualColor, duration);

		switch (type)
		{
		case ArrowType.Default:
			Debug.DrawRay(pos3, dir3, actualColor, duration);
			break;
		case ArrowType.Thin:
			Debug.DrawRay(pos3, dir3, actualColor, duration);
			break;
		case ArrowType.Double:
		case ArrowType.Triple:
		case ArrowType.Solid:
			Debug.DrawRay(pos3, dir3, actualColor, duration);
			break;
		}
	}

	public static void randomStar(Vector3 center, Color color)
	{
		//special: refuse to draw at 0,0.
		if (center == Vector3.zero) return;
			for(int i=0;i<2;i++)
				DrawArrow.ForGizmo(center, UnityEngine.Random.onUnitSphere * 1, color, false, 0.1f, 30.0f);
	}
	
	public static void comparePositions(Transform t1, Transform t2)
	{
		//direct from one to the other:
		ForDebug(t1.position, t2.position - t1.position);

		//direction
		//Vector3 moveDirection = (t2.position-t1.position).normalized;
	}
}
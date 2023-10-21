using Unity.VisualScripting;
using UnityEngine;

#if UNITY_EDITOR
	using UnityEditor;
	using UnityEditor.EditorTools;
#endif

public class Vector3GizmoAttribute : PropertyAttribute
{

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(Vector3GizmoAttribute))]
public class Vector3GizmoDrawer : PropertyDrawer
{
	private int _stateIndex = 0;
	private readonly string[] _stateName = {"X", "L", "W"};
	private const int TotalState = 3;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);
		EditorGUI.PropertyField(position, property, label, true);
			
		if (property.propertyType is SerializedPropertyType.Vector3)
		{
			DrawButton(position, property);
		}
			
		EditorGUI.EndProperty();
	}

	private void DrawButton(Rect position, SerializedProperty property)
	{
		float buttonXpos = position.x - EditorGUIUtility.singleLineHeight * 1.1f;
		float buttonWidth = EditorGUIUtility.singleLineHeight * 1.2f;
		Rect buttonRect = new Rect(buttonXpos, position.y, buttonWidth, position.height);

		GUIStyle guiStyle = new("button")
		{
			padding = new RectOffset(4, 2, 2, 2),
		};

		if (GUI.Button(buttonRect, GetProperString(), guiStyle))
		{
			OnButtonClick(property);
		}
	}

	private void OnButtonClick(SerializedProperty property)
	{
		Selection.activeObject = property.serializedObject.targetObject;

		_stateIndex++;
		_stateIndex = _stateIndex %= TotalState;

		Vector3Handle.HideTool();
		
		if (_stateIndex == 0)
		{
			//X
			SceneView.lastActiveSceneView.LookAt(property.serializedObject.targetObject.GameObject().transform.position);
		}
		else if (_stateIndex == 1)
		{
			//local space
			SceneView.lastActiveSceneView.LookAt(property.vector3Value);
			Vector3Handle.ShowTool(property, false);
		}
		else if (_stateIndex == 2)
		{
			//world space
			SceneView.lastActiveSceneView.LookAt(property.vector3Value);
			Vector3Handle.ShowTool(property);
		}
	}

	private string GetProperString()
	{
		return _stateName[_stateIndex];
	}
}

public class Vector3Handle : EditorTool
{
	private static SerializedProperty _property;
	private static bool _isWorldPosition;

	public static void ShowTool(SerializedProperty property, bool isWorldPosition = true)
	{
		_isWorldPosition = isWorldPosition;
		_property = property;
			
		ToolManager.SetActiveTool<Vector3Handle>();

		Selection.selectionChanged -= SelectionChanged;
		Selection.selectionChanged += SelectionChanged;
	}

	public static void HideTool()
	{
		_property = null;
		_isWorldPosition = true;
		
		ToolManager.RestorePreviousPersistentTool();
	}
		
	private static void SelectionChanged()
	{
		Selection.selectionChanged -= SelectionChanged;
			
		if (Selection.activeObject == null)
		{
			HideTool();
			return;
		}

		if (ToolManager.activeToolType == typeof(Vector3Handle))
		{
			HideTool();
		}
	}
		
	public override void OnToolGUI(EditorWindow window)
	{
		if (_property == null) return;
		Vector3 spawnPoint = _property.vector3Value;

		Transform trans = _property.serializedObject.targetObject.GameObject().transform;
		bool isInLocalSpace = !_isWorldPosition && trans.parent != null;
	
		if (isInLocalSpace) spawnPoint += trans.position;

		EditorGUI.BeginChangeCheck();
		spawnPoint = Handles.PositionHandle(spawnPoint, Quaternion.identity);
		
		DrawValueText(spawnPoint);
		DrawLine(spawnPoint);

		if (EditorGUI.EndChangeCheck())
		{
			if (isInLocalSpace) spawnPoint -= trans.position;
				
			_property.vector3Value = spawnPoint;
			_property.serializedObject.ApplyModifiedProperties();
		}
	}

	private void DrawValueText(Vector3 spawnPoint)
	{
		string prefix = _isWorldPosition ? "World space : " : "Local Space : ";
		string value = " X: " + spawnPoint.x + " Y: " + spawnPoint.y + " Z: " + spawnPoint.z;
		Handles.Label(spawnPoint, prefix + value);
	}

	private void DrawLine(Vector3 spawnPoint)
	{
		Vector3 from = _isWorldPosition ? Vector3.zero : _property.serializedObject.targetObject.GameObject().transform.position;
		Handles.DrawLine(from, spawnPoint);
	}
}
#endif

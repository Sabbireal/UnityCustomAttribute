#if UNITY_EDITOR
	using UnityEditor;
#endif
	using UnityEngine;

public class SpritePreviewAttribute : PropertyAttribute
{
	public readonly char Type;
	public readonly string HeightField;
	public readonly string WidthField;
	
	public readonly float Height;
	public readonly float Width;

	public const char FLOAT_TYPE = 'f';
	public const char STRING_TYPE = 's';
	
	public SpritePreviewAttribute(float height, float width)
	{
		Type = FLOAT_TYPE;
		Height = height;
		Width = width;
	}
	
	public SpritePreviewAttribute(string heightField, string widthField)
	{
		Type = STRING_TYPE;
		HeightField = heightField;
		WidthField = widthField;
	}
	
	public SpritePreviewAttribute()
	{
		Type = FLOAT_TYPE;
		Height = Width = 70;
	}
}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(SpritePreviewAttribute))]
	public class SpritePreviewDrawer : PropertyDrawer
	{
		private const int MIN_SIZE = 50;
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
		{
			EditorGUI.BeginProperty(position, label, property);
			
			GUI.Label(position, property.displayName);

			if (property.objectReferenceValue != null && attribute is SpritePreviewAttribute spritePreviewAttribute)
			{
				position.x += 100f;
				

				switch (spritePreviewAttribute.Type)
				{
					case SpritePreviewAttribute.STRING_TYPE:
						
						SerializedProperty widthProperty = property.serializedObject.FindProperty(spritePreviewAttribute.WidthField);
						SerializedProperty heightProperty = property.serializedObject.FindProperty(spritePreviewAttribute.HeightField);

						if (widthProperty.propertyType == SerializedPropertyType.Float)
						{
							position.width = widthProperty.floatValue;
						}
						else if (widthProperty.propertyType == SerializedPropertyType.Integer)
						{
							position.width = widthProperty.intValue;
						}

						if (heightProperty.propertyType == SerializedPropertyType.Float)
						{
							position.height = heightProperty.floatValue;
						}
						else if (heightProperty.propertyType == SerializedPropertyType.Integer)
						{
							position.height = heightProperty.intValue;
						}
						break;
					
					default:
						position.width = spritePreviewAttribute.Width;
						position.height = spritePreviewAttribute.Height;
						break;
				}

				position.width = Mathf.Max(MIN_SIZE, position.width);
				position.height = Mathf.Max(MIN_SIZE, position.height);
				
				property.objectReferenceValue = EditorGUI.ObjectField(position, property.objectReferenceValue, typeof(Sprite), false);
			}
			else {
				EditorGUI.PropertyField(position, property, true);
			}

			EditorGUI.EndProperty();
		}
		
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (property.objectReferenceValue != null && attribute is SpritePreviewAttribute spritePreviewAttribute) {
				switch (spritePreviewAttribute.Type)
				{
					case SpritePreviewAttribute.STRING_TYPE:
						SerializedProperty heightProperty = property.serializedObject.FindProperty(spritePreviewAttribute.HeightField);
						
						if (heightProperty.propertyType == SerializedPropertyType.Float)
						{
							return Mathf.Max(MIN_SIZE, heightProperty.floatValue); 
						}

						if (heightProperty.propertyType == SerializedPropertyType.Integer)
						{
							return Mathf.Max(MIN_SIZE, heightProperty.intValue);
						}
						
						break;
					default:
						return spritePreviewAttribute.Height;
				}
			}

			return base.GetPropertyHeight(property, label);
		}
	}
#endif
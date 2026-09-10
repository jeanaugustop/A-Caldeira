// Test doubles ONLY. These tests do not validate Unity API availability, lifecycle, rendering or engine GC.
namespace UnityEngine
{
    public class Object { }
    public class ScriptableObject : Object { }
    public class MonoBehaviour : Object
    {
        public GameObject gameObject { get; } = new GameObject();
        public Transform transform { get; } = new Transform();
    }
    public class GameObject { public bool activeSelf; public void SetActive(bool value) { activeSelf = value; } }
    public class Transform { public void SetPositionAndRotation(Vector3 position, Quaternion rotation) { } }
    public struct Vector3 { }
    public struct Quaternion { }
    public sealed class SerializeField : Attribute { }
    public sealed class CreateAssetMenuAttribute : Attribute { public string menuName; public string fileName; }
}

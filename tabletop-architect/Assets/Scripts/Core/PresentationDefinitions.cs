using System;
using UnityEngine;

namespace TTA.Core
{
    [Serializable]
    public enum PresentationPrimitiveKind
    {
        Cube = 0,
        Sphere = 1,
        Capsule = 2,
        Cylinder = 3
    }

    [Serializable]
    public sealed class ElementPresentationDefinition
    {
        public PresentationPrimitiveKind primitive = PresentationPrimitiveKind.Cube;
        public Vector3 localScale = Vector3.one;
        public Vector3 localOffset = Vector3.zero;
        public Vector3 localEulerAngles = Vector3.zero;
        public Color color = Color.gray;
    }

    [Serializable]
    public sealed class AreaPresentationDefinition
    {
        public Vector3 anchor = Vector3.zero;
        public Vector3 itemOffset = Vector3.zero;
        public Vector3 normal = Vector3.forward;
        public bool hasBoxCollider;
        public Vector3 boxColliderCenter = Vector3.zero;
        public Vector3 boxColliderSize = Vector3.one;
    }
}

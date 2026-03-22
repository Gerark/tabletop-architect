using System.Collections.Generic;
using TTA.Core;
using UnityEngine;

namespace TTA.Presenter
{
    public sealed class Dummy3DTranscriptPresenter : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float rotationSpeed = 720f;

        private readonly Dictionary<int, AreaVisual> _visualsByAreaId = new();
        private readonly Dictionary<int, ElementVisual> _visualsByElementId = new();
        private GameDefinition _definition;
        private MatchState _match;
        private Transform _areasRoot;
        private Transform _elementsRoot;

        private sealed class AreaVisual
        {
            public int areaId = RuntimeIds.InvalidId;
            public GameObject gameObject;
            public Transform transform;
            public BoxCollider collider;
            public Dummy3DAreaObject areaObject;
        }

        private sealed class ElementVisual
        {
            public int elementId = RuntimeIds.InvalidId;
            public GameObject gameObject;
            public Transform transform;
            public MeshRenderer renderer;
            public TextMesh label;
            public Vector3 targetPosition;
            public Quaternion targetRotation = Quaternion.identity;
            public bool hasInitializedPose;
        }

        private void Awake()
        {
            EnsureRoots();
        }

        private void Update()
        {
            UpdateActiveElementTargets();

            float positionStep = moveSpeed <= 0f
                ? float.MaxValue
                : moveSpeed * Time.deltaTime;
            float rotationStep = rotationSpeed <= 0f
                ? float.MaxValue
                : rotationSpeed * Time.deltaTime;

            foreach (ElementVisual visual in _visualsByElementId.Values)
            {
                if (visual == null || visual.transform == null || visual.gameObject == null || !visual.gameObject.activeSelf)
                    continue;

                visual.transform.position = Vector3.MoveTowards(
                    visual.transform.position,
                    visual.targetPosition,
                    positionStep);

                visual.transform.rotation = Quaternion.RotateTowards(
                    visual.transform.rotation,
                    visual.targetRotation,
                    rotationStep);
            }
        }

        public void ResetPresentation(GameDefinition definition, MatchState match)
        {
            _definition = definition ?? new GameDefinition();
            _match = match;

            EnsureRoots();
            ClearAllVisuals();
            RefreshFromMatch();
        }

        public void PresentNewPublicBatches(GameDefinition definition, MatchState match, ref int nextBatchIndex)
        {
            _definition = definition ?? new GameDefinition();
            _match = match;

            EnsureRoots();

            if (_match == null || _match.transcript == null)
            {
                nextBatchIndex = 0;
                ClearAllVisuals();
                return;
            }

            int safeStart = nextBatchIndex < 0 ? 0 : nextBatchIndex;
            for (int index = safeStart; index < _match.transcript.completedBatches.Count; index++)
            {
                TranscriptBatch batch = _match.transcript.completedBatches[index];
                if (batch.observerPlayerId != RuntimeIds.InvalidId)
                    continue;

                for (int entryIndex = 0; entryIndex < batch.entries.Count; entryIndex++)
                    ApplyEntry(batch.entries[entryIndex]);

                RefreshFromMatch();
            }

            nextBatchIndex = _match.transcript.completedBatches.Count;
        }

        private void ApplyEntry(TranscriptEntry entry)
        {
            switch (entry.kind)
            {
                case TranscriptEntryKind.ElementsPlaced:
                    HandleElementsPlaced(entry);
                    break;
                case TranscriptEntryKind.ElementsUnplaced:
                    HandleElementsUnplaced(entry);
                    break;
                case TranscriptEntryKind.ElementMoved:
                    HandleElementMoved(entry);
                    break;
                case TranscriptEntryKind.RollResolved:
                    HandleRollResolved(entry);
                    break;
            }
        }

        private void HandleElementsPlaced(TranscriptEntry entry)
        {
            List<int> elementIds = GetElementIds(entry);
            for (int index = 0; index < elementIds.Count; index++)
            {
                ElementVisual visual = EnsureVisual(elementIds[index]);
                if (visual?.gameObject == null)
                    continue;

                visual.gameObject.SetActive(true);
            }
        }

        private void HandleElementsUnplaced(TranscriptEntry entry)
        {
            List<int> elementIds = GetElementIds(entry);
            for (int index = 0; index < elementIds.Count; index++)
            {
                if (!_visualsByElementId.TryGetValue(elementIds[index], out ElementVisual visual) || visual?.gameObject == null)
                    continue;

                visual.gameObject.SetActive(false);
            }
        }

        private void HandleElementMoved(TranscriptEntry entry)
        {
            if (!TryGetRuntimeId(entry, "ElementId", ValueKind.ElementId, out int elementId))
                return;

            ElementVisual visual = EnsureVisual(elementId);
            if (visual?.gameObject == null)
                return;

            visual.gameObject.SetActive(true);

            if (visual.hasInitializedPose || !TryGetRuntimeId(entry, "FromAreaId", ValueKind.AreaId, out int fromAreaId))
                return;

            RuntimeElementRecord element = _match.GetElement(elementId);
            ElementPresentationDefinition presentation = GetElementPresentation(GetElementDefinition(element));

            visual.transform.position = ResolveElementPosition(fromAreaId, presentation.localOffset);
            visual.transform.rotation = ResolveElementRotation(fromAreaId, presentation.localEulerAngles);
            visual.targetPosition = visual.transform.position;
            visual.targetRotation = visual.transform.rotation;
            visual.hasInitializedPose = true;
        }

        private void HandleRollResolved(TranscriptEntry entry)
        {
            List<int> elementIds = GetElementIds(entry);
            List<int> rolledValues = GetIntCollection(entry.fields.GetOrDefault("RolledValues"));

            for (int index = 0; index < elementIds.Count; index++)
            {
                ElementVisual visual = EnsureVisual(elementIds[index]);
                if (visual?.gameObject == null)
                    continue;

                visual.gameObject.SetActive(true);
                if (index < rolledValues.Count)
                    ApplyLabelValue(visual, rolledValues[index].ToString());
            }
        }

        private void RefreshFromMatch()
        {
            if (_match == null)
            {
                ClearAllVisuals();
                return;
            }

            RefreshAreaVisuals();
            RefreshElementVisuals();
        }

        private void RefreshAreaVisuals()
        {
            HashSet<int> runtimeAreaIds = new();
            for (int index = 0; index < _match.areas.items.Count; index++)
            {
                RuntimeAreaRecord area = _match.areas.items[index];
                runtimeAreaIds.Add(area.id);

                AreaVisual visual = EnsureAreaVisual(area.id);
                if (visual?.transform == null)
                    continue;

                AreaDefinition definition = GetAreaDefinition(area);
                AreaPresentationDefinition presentation = GetAreaPresentation(definition);
                Transform parent = ResolveAreaParent(area);

                if (visual.transform.parent != parent)
                    visual.transform.SetParent(parent, false);

                visual.transform.localPosition = presentation.anchor;
                visual.transform.localRotation = BuildAreaRotation(presentation.normal);
                visual.gameObject.name = $"{definition.key}Area#{area.id}";
                visual.gameObject.SetActive(true);

                if (visual.areaObject != null)
                {
                    visual.areaObject.areaId = area.id;
                    visual.areaObject.ownerElementId = area.ownerElementId;
                    visual.areaObject.areaKey = definition.key ?? string.Empty;
                }

                if (visual.collider != null)
                {
                    visual.collider.enabled = presentation.hasBoxCollider;
                    visual.collider.center = presentation.boxColliderCenter;
                    visual.collider.size = presentation.boxColliderSize == Vector3.zero
                        ? Vector3.one
                        : presentation.boxColliderSize;
                }
            }

            List<int> staleAreaIds = new();
            foreach (KeyValuePair<int, AreaVisual> pair in _visualsByAreaId)
            {
                if (!runtimeAreaIds.Contains(pair.Key))
                    staleAreaIds.Add(pair.Key);
            }

            for (int index = 0; index < staleAreaIds.Count; index++)
                DestroyAreaVisual(staleAreaIds[index]);
        }

        private void RefreshElementVisuals()
        {
            HashSet<int> runtimeElementIds = new();
            for (int index = 0; index < _match.elements.items.Count; index++)
            {
                RuntimeElementRecord element = _match.elements.items[index];
                runtimeElementIds.Add(element.id);

                if (element.placementState != PlacementState.Placed)
                {
                    if (_visualsByElementId.TryGetValue(element.id, out ElementVisual inactiveVisual) &&
                        inactiveVisual?.gameObject != null)
                    {
                        inactiveVisual.gameObject.SetActive(false);
                    }

                    continue;
                }

                ElementVisual visual = EnsureVisual(element.id);
                if (visual?.gameObject == null)
                    continue;

                visual.gameObject.SetActive(true);
                UpdateElementTargetPose(visual, element);

                if (!visual.hasInitializedPose)
                {
                    visual.transform.position = visual.targetPosition;
                    visual.transform.rotation = visual.targetRotation;
                    visual.hasInitializedPose = true;
                }

                UpdateLabelFromRuntime(visual, element);
            }

            List<int> staleElementIds = new();
            foreach (KeyValuePair<int, ElementVisual> pair in _visualsByElementId)
            {
                if (!runtimeElementIds.Contains(pair.Key))
                    staleElementIds.Add(pair.Key);
            }

            for (int index = 0; index < staleElementIds.Count; index++)
                DestroyElementVisual(staleElementIds[index]);
        }

        private void UpdateActiveElementTargets()
        {
            if (_match == null)
                return;

            for (int index = 0; index < _match.elements.items.Count; index++)
            {
                RuntimeElementRecord element = _match.elements.items[index];
                if (element.placementState != PlacementState.Placed)
                    continue;

                if (!_visualsByElementId.TryGetValue(element.id, out ElementVisual visual) ||
                    visual?.gameObject == null ||
                    !visual.gameObject.activeSelf)
                {
                    continue;
                }

                UpdateElementTargetPose(visual, element);
            }
        }

        private void UpdateElementTargetPose(ElementVisual visual, RuntimeElementRecord element)
        {
            RuntimeAreaRecord area = _match.GetArea(element.areaId);
            AreaPresentationDefinition areaPresentation = GetAreaPresentation(GetAreaDefinition(area));
            ElementPresentationDefinition elementPresentation = GetElementPresentation(GetElementDefinition(element));
            int orderIndex = element.orderIndex < 0 ? 0 : element.orderIndex;
            Vector3 localOffset = (areaPresentation.itemOffset * orderIndex) + elementPresentation.localOffset;

            visual.targetPosition = ResolveElementPosition(area.id, localOffset);
            visual.targetRotation = ResolveElementRotation(area.id, elementPresentation.localEulerAngles);
        }

        private AreaVisual EnsureAreaVisual(int areaId)
        {
            if (_visualsByAreaId.TryGetValue(areaId, out AreaVisual existingVisual) &&
                existingVisual?.gameObject != null)
            {
                return existingVisual;
            }

            if (_match == null || _definition == null)
                return null;

            RuntimeAreaRecord area = _match.GetArea(areaId);
            AreaDefinition definition = GetAreaDefinition(area);

            GameObject areaObject = new($"{definition.key}Area#{areaId}");
            areaObject.transform.SetParent(ResolveAreaParent(area), false);

            AreaVisual visual = new()
            {
                areaId = areaId,
                gameObject = areaObject,
                transform = areaObject.transform,
                collider = areaObject.AddComponent<BoxCollider>(),
                areaObject = areaObject.AddComponent<Dummy3DAreaObject>()
            };

            _visualsByAreaId[areaId] = visual;
            return visual;
        }

        private ElementVisual EnsureVisual(int elementId)
        {
            if (_visualsByElementId.TryGetValue(elementId, out ElementVisual existingVisual) &&
                existingVisual?.gameObject != null)
            {
                return existingVisual;
            }

            if (_match == null || _definition == null)
                return null;

            RuntimeElementRecord element = _match.GetElement(elementId);
            ElementDefinition definition = GetElementDefinition(element);
            ElementPresentationDefinition presentation = GetElementPresentation(definition);

            GameObject visualObject = GameObject.CreatePrimitive(ToPrimitiveType(presentation.primitive));
            visualObject.name = $"{definition.key}#{elementId}";
            visualObject.transform.SetParent(_elementsRoot, false);
            visualObject.transform.localScale = presentation.localScale;

            Collider collider = visualObject.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            MeshRenderer renderer = visualObject.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.material.color = presentation.color;

            ElementVisual visual = new()
            {
                elementId = elementId,
                gameObject = visualObject,
                transform = visualObject.transform,
                renderer = renderer,
                targetPosition = visualObject.transform.position,
                targetRotation = visualObject.transform.rotation
            };

            if (definition.randomDistribution != RandomDistribution.None)
                visual.label = CreateLabel(visualObject.transform, presentation.localScale);

            _visualsByElementId[elementId] = visual;
            return visual;
        }

        private TextMesh CreateLabel(Transform parent, Vector3 scale)
        {
            GameObject labelObject = new("FaceLabel");
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = new Vector3(0f, 0f, -((scale.z * 0.5f) + 0.01f));
            labelObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 64;
            label.characterSize = 0.08f;
            label.color = Color.black;
            label.text = string.Empty;
            return label;
        }

        private void UpdateLabelFromRuntime(ElementVisual visual, RuntimeElementRecord element)
        {
            if (visual?.label == null)
                return;

            ElementDefinition definition = GetElementDefinition(element);
            if (element.currentFaceIndex < 0 || element.currentFaceIndex >= definition.faces.Length)
            {
                ApplyLabelValue(visual, string.Empty);
                return;
            }

            ElementFaceDefinition face = definition.faces[element.currentFaceIndex];
            string labelValue = face.numericValue > 0
                ? face.numericValue.ToString()
                : face.id ?? string.Empty;

            ApplyLabelValue(visual, labelValue);
        }

        private void ApplyLabelValue(ElementVisual visual, string value)
        {
            if (visual?.label == null)
                return;

            visual.label.text = value ?? string.Empty;
        }

        private Vector3 ResolveElementPosition(int areaId, Vector3 localOffset)
        {
            AreaVisual areaVisual = EnsureAreaVisual(areaId);
            if (areaVisual?.transform == null)
                return localOffset;

            return areaVisual.transform.TransformPoint(localOffset);
        }

        private Quaternion ResolveElementRotation(int areaId, Vector3 localEulerAngles)
        {
            AreaVisual areaVisual = EnsureAreaVisual(areaId);
            Quaternion localRotation = Quaternion.Euler(localEulerAngles);
            if (areaVisual?.transform == null)
                return localRotation;

            return areaVisual.transform.rotation * localRotation;
        }

        private Transform ResolveAreaParent(RuntimeAreaRecord area)
        {
            if (area == null || area.ownerElementId == RuntimeIds.InvalidId)
                return _areasRoot;

            ElementVisual ownerVisual = EnsureVisual(area.ownerElementId);
            return ownerVisual?.transform ?? _areasRoot;
        }

        private ElementDefinition GetElementDefinition(RuntimeElementRecord element)
        {
            if (_definition == null ||
                _definition.elements == null ||
                element == null ||
                element.definitionIndex < 0 ||
                element.definitionIndex >= _definition.elements.Length)
            {
                return new ElementDefinition();
            }

            return _definition.elements[element.definitionIndex];
        }

        private AreaDefinition GetAreaDefinition(RuntimeAreaRecord area)
        {
            if (_definition == null || area == null)
                return new AreaDefinition();

            if (area.ownerElementId == RuntimeIds.InvalidId)
            {
                if (_definition.globalAreas == null ||
                    area.definitionIndex < 0 ||
                    area.definitionIndex >= _definition.globalAreas.Length)
                {
                    return new AreaDefinition();
                }

                return _definition.globalAreas[area.definitionIndex];
            }

            RuntimeElementRecord owner = _match.GetElement(area.ownerElementId);
            ElementDefinition ownerDefinition = GetElementDefinition(owner);
            if (ownerDefinition.ownedAreas == null ||
                area.definitionIndex < 0 ||
                area.definitionIndex >= ownerDefinition.ownedAreas.Length)
            {
                return new AreaDefinition();
            }

            return ownerDefinition.ownedAreas[area.definitionIndex];
        }

        private static ElementPresentationDefinition GetElementPresentation(ElementDefinition definition)
        {
            return definition?.presentation ?? new ElementPresentationDefinition();
        }

        private static AreaPresentationDefinition GetAreaPresentation(AreaDefinition definition)
        {
            return definition?.presentation ?? new AreaPresentationDefinition();
        }

        private static Quaternion BuildAreaRotation(Vector3 normal)
        {
            Vector3 forward = normal == Vector3.zero
                ? Vector3.forward
                : normal.normalized;

            Vector3 upHint = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(forward, upHint)) > 0.999f)
                upHint = Vector3.right;

            Vector3 up = Vector3.ProjectOnPlane(upHint, forward);
            if (up.sqrMagnitude < 0.0001f)
                up = Vector3.ProjectOnPlane(Vector3.back, forward);

            if (up.sqrMagnitude < 0.0001f)
                return Quaternion.identity;

            return Quaternion.LookRotation(forward, up.normalized);
        }

        private static PrimitiveType ToPrimitiveType(PresentationPrimitiveKind primitive)
        {
            return primitive switch
            {
                PresentationPrimitiveKind.Sphere => PrimitiveType.Sphere,
                PresentationPrimitiveKind.Capsule => PrimitiveType.Capsule,
                PresentationPrimitiveKind.Cylinder => PrimitiveType.Cylinder,
                _ => PrimitiveType.Cube
            };
        }

        private static List<int> GetElementIds(TranscriptEntry entry)
        {
            List<int> elementIds = new();
            Value value = entry.fields.GetOrDefault("ElementIds");
            if (value != null &&
                value.kind == ValueKind.Collection &&
                value.collectionItemKind == ValueKind.ElementId)
            {
                for (int index = 0; index < value.collectionItems.Count; index++)
                    elementIds.Add(value.collectionItems[index].idValue);

                return elementIds;
            }

            if (TryGetRuntimeId(entry, "ElementId", ValueKind.ElementId, out int singleElementId))
                elementIds.Add(singleElementId);

            return elementIds;
        }

        private static List<int> GetIntCollection(Value value)
        {
            List<int> results = new();
            if (value == null || value.kind != ValueKind.Collection)
                return results;

            for (int index = 0; index < value.collectionItems.Count; index++)
            {
                if (value.collectionItems[index].kind == ValueKind.Int)
                    results.Add(value.collectionItems[index].intValue);
            }

            return results;
        }

        private static bool TryGetRuntimeId(TranscriptEntry entry, string key, ValueKind expectedKind, out int runtimeId)
        {
            Value value = entry.fields.GetOrDefault(key);
            if (value != null && value.kind == expectedKind)
            {
                runtimeId = value.idValue;
                return true;
            }

            runtimeId = RuntimeIds.InvalidId;
            return false;
        }

        private void DestroyAreaVisual(int areaId)
        {
            if (!_visualsByAreaId.TryGetValue(areaId, out AreaVisual visual))
                return;

            if (visual?.gameObject != null)
                Destroy(visual.gameObject);

            _visualsByAreaId.Remove(areaId);
        }

        private void DestroyElementVisual(int elementId)
        {
            if (!_visualsByElementId.TryGetValue(elementId, out ElementVisual visual))
                return;

            if (visual?.gameObject != null)
                Destroy(visual.gameObject);

            _visualsByElementId.Remove(elementId);
        }

        private void ClearAllVisuals()
        {
            ClearAreaVisuals();
            ClearElementVisuals();
        }

        private void ClearAreaVisuals()
        {
            foreach (AreaVisual visual in _visualsByAreaId.Values)
            {
                if (visual?.gameObject != null)
                    Destroy(visual.gameObject);
            }

            _visualsByAreaId.Clear();
        }

        private void ClearElementVisuals()
        {
            foreach (ElementVisual visual in _visualsByElementId.Values)
            {
                if (visual?.gameObject != null)
                    Destroy(visual.gameObject);
            }

            _visualsByElementId.Clear();
        }

        private void EnsureRoots()
        {
            if (_areasRoot == null)
            {
                Transform existingAreasRoot = transform.Find("Areas");
                if (existingAreasRoot != null)
                {
                    _areasRoot = existingAreasRoot;
                }
                else
                {
                    GameObject areasRoot = new("Areas");
                    areasRoot.transform.SetParent(transform, false);
                    _areasRoot = areasRoot.transform;
                }
            }

            if (_elementsRoot != null)
                return;

            Transform existingElementsRoot = transform.Find("Elements");
            if (existingElementsRoot != null)
            {
                _elementsRoot = existingElementsRoot;
                return;
            }

            GameObject elementsRoot = new("Elements");
            elementsRoot.transform.SetParent(transform, false);
            _elementsRoot = elementsRoot.transform;
        }
    }
}

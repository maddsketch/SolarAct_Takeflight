import json
import uuid


def nid():
    return uuid.uuid4().hex


HLSL_GUID = "b4f8151c9e1d4db9a2d6384302c5577d"
OUT_PATH = __file__.replace("_build_flaming_circle_sg.py", "FlamingCircleUnlit.shadergraph")

PROP_SPECS = [
    ("color", "Inner Color", "_InnerColor", (1.0, 0.75, 0.22, 1.0)),
    ("color", "Outer Color", "_OuterColor", (1.0, 0.2, 0.02, 1.0)),
    ("float", "Radius", "_Radius", (0.36, 0.01, 1.0)),
    ("float", "Thickness", "_Thickness", (0.13, 0.005, 0.5)),
    ("float", "Edge Softness", "_EdgeSoftness", (0.05, 0.001, 0.2)),
    ("float", "Noise Scale", "_NoiseScale", (4.0, 0.1, 24.0)),
    ("float", "Flame Speed", "_FlameSpeed", (1.6, 0.0, 10.0)),
    ("float", "Flame Amount", "_FlameAmount", (0.07, 0.0, 0.3)),
    ("float", "Flicker Speed", "_FlickerSpeed", (6.0, 0.0, 30.0)),
    ("float", "Emission Strength", "_EmissionStrength", (3.0, 0.0, 20.0)),
]

cf_in_names = [
    "UV",
    "InnerColor",
    "OuterColor",
    "Radius",
    "Thickness",
    "EdgeSoftness",
    "NoiseScale",
    "FlameSpeed",
    "FlameAmount",
    "FlickerSpeed",
    "EmissionStrength",
]

NUM_IN = len(cf_in_names)
OUT_C = NUM_IN
OUT_A = NUM_IN + 1


def color_prop(oid, name, ref, rgba):
    return {
        "m_SGVersion": 3,
        "m_Type": "UnityEditor.ShaderGraph.Internal.ColorShaderProperty",
        "m_ObjectId": oid,
        "m_Guid": {"m_GuidSerialized": str(uuid.uuid4())},
        "m_Name": name,
        "m_DefaultRefNameVersion": 1,
        "m_RefNameGeneratedByDisplayName": name,
        "m_DefaultReferenceName": ref,
        "m_OverrideReferenceName": ref,
        "m_GeneratePropertyBlock": True,
        "m_UseCustomSlotLabel": False,
        "m_CustomSlotLabel": "",
        "m_DismissedVersion": 0,
        "m_Precision": 0,
        "overrideHLSLDeclaration": False,
        "hlslDeclarationOverride": 0,
        "m_Hidden": False,
        "m_PerRendererData": False,
        "m_customAttributes": [],
        "m_Value": {"r": rgba[0], "g": rgba[1], "b": rgba[2], "a": rgba[3]},
        "isMainColor": False,
        "m_ColorMode": 1,
    }


def float_prop(oid, name, ref, val, rmin, rmax):
    return {
        "m_SGVersion": 1,
        "m_Type": "UnityEditor.ShaderGraph.Internal.Vector1ShaderProperty",
        "m_ObjectId": oid,
        "m_Guid": {"m_GuidSerialized": str(uuid.uuid4())},
        "m_Name": name,
        "m_DefaultRefNameVersion": 1,
        "m_RefNameGeneratedByDisplayName": name,
        "m_DefaultReferenceName": ref,
        "m_OverrideReferenceName": ref,
        "m_GeneratePropertyBlock": True,
        "m_UseCustomSlotLabel": False,
        "m_CustomSlotLabel": "",
        "m_DismissedVersion": 0,
        "m_Precision": 0,
        "overrideHLSLDeclaration": False,
        "hlslDeclarationOverride": 0,
        "m_Hidden": False,
        "m_PerRendererData": False,
        "m_customAttributes": [],
        "m_Value": float(val),
        "m_FloatType": 1,
        "m_RangeValues": {"x": rmin, "y": rmax},
    }


prop_ids = []
prop_objs = []
for spec in PROP_SPECS:
    t = spec[0]
    pid = nid()
    prop_ids.append(pid)
    if t == "float":
        v, lo, hi = spec[3]
        prop_objs.append(float_prop(pid, spec[1], spec[2], v, lo, hi))
    else:
        prop_objs.append(color_prop(pid, spec[1], spec[2], spec[3]))

cat_id = nid()
graph_id = nid()
n_vpos, n_vnorm, n_vtan = nid(), nid(), nid()
n_fbase, n_femi, n_falpha = nid(), nid(), nid()
n_uv = nid()
n_cf = nid()
tgt_id = nid()
subunlit_id = nid()

s_uv_out = nid()
cf_slot_ids = [nid() for _ in range(NUM_IN)]
slot_outc = nid()
slot_outa = nid()

pn_entries = []
for i, pid in enumerate(prop_ids):
    pnid = nid()
    psid = nid()
    st = PROP_SPECS[i][0]
    pn_entries.append((pid, pnid, psid, st))

edges = [
    {"m_OutputSlot": {"m_Node": {"m_Id": n_uv}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 0}},
]
for idx, (_, pnid, _, _) in enumerate(pn_entries):
    edges.append({
        "m_OutputSlot": {"m_Node": {"m_Id": pnid}, "m_SlotId": 0},
        "m_InputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 1 + idx},
    })
edges.append({
    "m_OutputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": OUT_C},
    "m_InputSlot": {"m_Node": {"m_Id": n_fbase}, "m_SlotId": 0},
})
edges.append({
    "m_OutputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": OUT_C},
    "m_InputSlot": {"m_Node": {"m_Id": n_femi}, "m_SlotId": 0},
})
edges.append({
    "m_OutputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": OUT_A},
    "m_InputSlot": {"m_Node": {"m_Id": n_falpha}, "m_SlotId": 0},
})

parts = []
parts.append({
    "m_SGVersion": 3,
    "m_Type": "UnityEditor.ShaderGraph.GraphData",
    "m_ObjectId": graph_id,
    "m_Properties": [{"m_Id": x} for x in prop_ids],
    "m_Keywords": [],
    "m_Dropdowns": [],
    "m_CategoryData": [{"m_Id": cat_id}],
    "m_Nodes": [{"m_Id": x} for x in [n_vpos, n_vnorm, n_vtan, n_fbase, n_femi, n_falpha, n_uv, n_cf] + [e[1] for e in pn_entries]],
    "m_GroupDatas": [],
    "m_StickyNoteDatas": [],
    "m_Edges": edges,
    "m_VertexContext": {"m_Position": {"x": 0, "y": 0}, "m_Blocks": [{"m_Id": n_vpos}, {"m_Id": n_vnorm}, {"m_Id": n_vtan}]},
    "m_FragmentContext": {"m_Position": {"x": 0, "y": 200}, "m_Blocks": [{"m_Id": n_fbase}, {"m_Id": n_femi}, {"m_Id": n_falpha}]},
    "m_PreviewData": {"serializedMesh": {"m_SerializedMesh": '{"mesh":{"instanceID":0}}', "m_Guid": ""}, "preventRotation": False},
    "m_Path": "Shader Graphs",
    "m_GraphPrecision": 1,
    "m_PreviewMode": 2,
    "m_OutputNode": {"m_Id": ""},
    "m_SubDatas": [],
    "m_ActiveTargets": [{"m_Id": tgt_id}],
})
parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.CategoryData", "m_ObjectId": cat_id, "m_Name": "", "m_ChildObjectList": [{"m_Id": x} for x in prop_ids]})
parts.extend(prop_objs)

svp, svn, svt = nid(), nid(), nid()
parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.PositionMaterialSlot", "m_ObjectId": svp, "m_Id": 0, "m_DisplayName": "Position", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Position", "m_StageCapability": 1, "m_Value": {"x": 0, "y": 0, "z": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0}, "m_Labels": [], "m_Space": 0})
parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.BlockNode", "m_ObjectId": n_vpos, "m_Group": {"m_Id": ""}, "m_Name": "VertexDescription.Position", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}}, "m_Slots": [{"m_Id": svp}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_SerializedDescriptor": "VertexDescription.Position"})
parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.NormalMaterialSlot", "m_ObjectId": svn, "m_Id": 0, "m_DisplayName": "Normal", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Normal", "m_StageCapability": 1, "m_Value": {"x": 0, "y": 0, "z": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0}, "m_Labels": [], "m_Space": 0})
parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.BlockNode", "m_ObjectId": n_vnorm, "m_Group": {"m_Id": ""}, "m_Name": "VertexDescription.Normal", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}}, "m_Slots": [{"m_Id": svn}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_SerializedDescriptor": "VertexDescription.Normal"})
parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.TangentMaterialSlot", "m_ObjectId": svt, "m_Id": 0, "m_DisplayName": "Tangent", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Tangent", "m_StageCapability": 1, "m_Value": {"x": 0, "y": 0, "z": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0}, "m_Labels": [], "m_Space": 0})
parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.BlockNode", "m_ObjectId": n_vtan, "m_Group": {"m_Id": ""}, "m_Name": "VertexDescription.Tangent", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}}, "m_Slots": [{"m_Id": svt}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_SerializedDescriptor": "VertexDescription.Tangent"})

sfb, sfe, sfa = nid(), nid(), nid()
parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.ColorRGBMaterialSlot", "m_ObjectId": sfb, "m_Id": 0, "m_DisplayName": "Base Color", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "BaseColor", "m_StageCapability": 2, "m_Value": {"x": 0.5, "y": 0.5, "z": 0.5}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0}, "m_Labels": [], "m_ColorMode": 1, "m_DefaultColor": {"r": 0.5, "g": 0.5, "b": 0.5, "a": 1}})
parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.BlockNode", "m_ObjectId": n_fbase, "m_Group": {"m_Id": ""}, "m_Name": "SurfaceDescription.BaseColor", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}}, "m_Slots": [{"m_Id": sfb}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_SerializedDescriptor": "SurfaceDescription.BaseColor"})
parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.ColorRGBMaterialSlot", "m_ObjectId": sfe, "m_Id": 0, "m_DisplayName": "Emission", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Emission", "m_StageCapability": 2, "m_Value": {"x": 0, "y": 0, "z": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0}, "m_Labels": [], "m_ColorMode": 1, "m_DefaultColor": {"r": 0, "g": 0, "b": 0, "a": 1}})
parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.BlockNode", "m_ObjectId": n_femi, "m_Group": {"m_Id": ""}, "m_Name": "SurfaceDescription.Emission", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}}, "m_Slots": [{"m_Id": sfe}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_SerializedDescriptor": "SurfaceDescription.Emission"})
parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot", "m_ObjectId": sfa, "m_Id": 0, "m_DisplayName": "Alpha", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Alpha", "m_StageCapability": 2, "m_Value": 1.0, "m_DefaultValue": 1.0, "m_Labels": []})
parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.BlockNode", "m_ObjectId": n_falpha, "m_Group": {"m_Id": ""}, "m_Name": "SurfaceDescription.Alpha", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}}, "m_Slots": [{"m_Id": sfa}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_SerializedDescriptor": "SurfaceDescription.Alpha"})

parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector4MaterialSlot", "m_ObjectId": s_uv_out, "m_Id": 0, "m_DisplayName": "Out", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "Out", "m_StageCapability": 3, "m_Value": {"x": 0, "y": 0, "z": 0, "w": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0, "w": 0}, "m_Labels": []})
parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.UVNode", "m_ObjectId": n_uv, "m_Group": {"m_Id": ""}, "m_Name": "UV", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -520, "y": 40, "width": 145, "height": 128}}, "m_Slots": [{"m_Id": s_uv_out}], "synonyms": ["texcoords", "coords"], "m_Precision": 0, "m_PreviewExpanded": False, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_OutputChannel": 0})

for i, name in enumerate(cf_in_names):
    oid = cf_slot_ids[i]
    if i == 0:
        parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector4MaterialSlot", "m_ObjectId": oid, "m_Id": i, "m_DisplayName": name, "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": name, "m_StageCapability": 3, "m_Value": {"x": 0, "y": 0, "z": 0, "w": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0, "w": 0}, "m_Labels": []})
    elif i <= 2:
        parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector4MaterialSlot", "m_ObjectId": oid, "m_Id": i, "m_DisplayName": name, "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": name, "m_StageCapability": 3, "m_Value": {"x": 0, "y": 0, "z": 0, "w": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0, "w": 0}, "m_Labels": []})
    else:
        parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot", "m_ObjectId": oid, "m_Id": i, "m_DisplayName": name, "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": name, "m_StageCapability": 3, "m_Value": 0.0, "m_DefaultValue": 0.0, "m_Labels": [], "m_LiteralMode": False})

parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector3MaterialSlot", "m_ObjectId": slot_outc, "m_Id": OUT_C, "m_DisplayName": "OutColor", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "OutColor", "m_StageCapability": 3, "m_Value": {"x": 0, "y": 0, "z": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0}, "m_Labels": []})
parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot", "m_ObjectId": slot_outa, "m_Id": OUT_A, "m_DisplayName": "OutAlpha", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "OutAlpha", "m_StageCapability": 3, "m_Value": 1.0, "m_DefaultValue": 1.0, "m_Labels": [], "m_LiteralMode": False})

cf_slot_refs = cf_slot_ids + [slot_outc, slot_outa]
parts.append({
    "m_SGVersion": 1,
    "m_Type": "UnityEditor.ShaderGraph.CustomFunctionNode",
    "m_ObjectId": n_cf,
    "m_Group": {"m_Id": ""},
    "m_Name": "FlamingCircleUnlit (Custom Function)",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -200, "y": 60, "width": 320, "height": 320}},
    "m_Slots": [{"m_Id": x} for x in cf_slot_refs],
    "synonyms": ["code", "HLSL"],
    "m_Precision": 0,
    "m_PreviewExpanded": False,
    "m_DismissedVersion": 0,
    "m_PreviewMode": 0,
    "m_CustomColors": {"m_SerializableColors": []},
    "m_SourceType": 0,
    "m_FunctionName": "FlamingCircleUnlit",
    "m_FunctionSource": HLSL_GUID,
    "m_FunctionSourceUsePragmas": True,
    "m_FunctionBody": "",
})

for row, ((pid, pnid, psid, st), _) in enumerate(zip(pn_entries, PROP_SPECS)):
    parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.PropertyNode", "m_ObjectId": pnid, "m_Group": {"m_Id": ""}, "m_Name": "Property", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -460, "y": 190 + 28 * row, "width": 180, "height": 34}}, "m_Slots": [{"m_Id": psid}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_Property": {"m_Id": pid}})
    if st == "float":
        parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot", "m_ObjectId": psid, "m_Id": 0, "m_DisplayName": "Out", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "Out", "m_StageCapability": 3, "m_Value": 0.0, "m_DefaultValue": 0.0, "m_Labels": [], "m_LiteralMode": False})
    else:
        parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector4MaterialSlot", "m_ObjectId": psid, "m_Id": 0, "m_DisplayName": "Out", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "Out", "m_StageCapability": 3, "m_Value": {"x": 0, "y": 0, "z": 0, "w": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0, "w": 0}, "m_Labels": []})

parts.append({"m_SGVersion": 2, "m_Type": "UnityEditor.Rendering.Universal.ShaderGraph.UniversalUnlitSubTarget", "m_ObjectId": subunlit_id, "m_KeepLightingVariants": False, "m_DefaultDecalBlending": True, "m_DefaultSSAO": True})
parts.append({
    "m_SGVersion": 1,
    "m_Type": "UnityEditor.Rendering.Universal.ShaderGraph.UniversalTarget",
    "m_ObjectId": tgt_id,
    "m_Datas": [],
    "m_ActiveSubTarget": {"m_Id": subunlit_id},
    "m_AllowMaterialOverride": True,
    "m_SurfaceType": 1,
    "m_ZTestMode": 4,
    "m_ZWriteControl": 0,
    "m_AlphaMode": 0,
    "m_RenderFace": 0,
    "m_AlphaClip": False,
    "m_CastShadows": False,
    "m_ReceiveShadows": False,
    "m_DisableTint": False,
    "m_Sort3DAs2DCompatible": False,
    "m_AdditionalMotionVectorMode": 0,
    "m_AlembicMotionVectors": False,
    "m_SupportsLODCrossFade": False,
    "m_CustomEditorGUI": "",
    "m_SupportVFX": False,
})

with open(OUT_PATH, "w", encoding="utf-8") as f:
    f.write(json.dumps(parts[0], separators=(",", ":")))
    for obj in parts[1:]:
        f.write("\n\n")
        f.write(json.dumps(obj, separators=(",", ":")))

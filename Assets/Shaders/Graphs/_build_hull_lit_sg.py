# Generates HullProceduralLit.shadergraph — URP Lit + procedural normals + optional graph normal blend.
import json
import uuid


def nid():
    return uuid.uuid4().hex


HLSL_GUID = "c0de11a12bc34d56e7f89012ab34cd56"
OUT_PATH = __file__.replace("_build_hull_lit_sg.py", "HullProceduralLit.shadergraph")

PROP_SPECS = [
    ("color", "Base Color", "_BaseColor", (0.14, 0.15, 0.18, 1.0)),
    ("color", "Line Color", "_LineColor", (0.05, 0.05, 0.06, 1.0)),
    ("color", "Accent Color", "_AccentColor", (0.2, 0.45, 0.65, 0.9)),
    ("float", "Tiling", "_Tiling", (12.0, 1.0, 64.0)),
    ("float", "Line Width", "_LineWidth", (0.035, 0.002, 0.12)),
    ("float", "Sub Grid", "_SubGrid", (4.0, 1.0, 16.0)),
    ("float", "Sub Line Strength", "_SubLineStrength", (0.45, 0.0, 1.0)),
    ("float", "Diagonal Blend", "_DiagonalBlend", (0.35, 0.0, 1.0)),
    ("float", "Panel Variation", "_PanelVariation", (0.12, 0.0, 0.5)),
    ("float", "Bump Strength", "_BumpStrength", (0.85, 0.0, 3.0)),
    ("float", "Smoothness", "_Smoothness", (0.42, 0.0, 1.0)),
    ("float", "Metallic", "_Metallic", (0.65, 0.0, 1.0)),
]

cf_in_names = [
    "UV",
    "BaseColor",
    "LineColor",
    "AccentColor",
    "Tiling",
    "LineWidth",
    "SubGrid",
    "SubLineStrength",
    "DiagonalBlend",
    "PanelVariation",
    "BumpStrength",
]

N_CF_PROPS = 10

NUM_IN = len(cf_in_names)
OUT_C = NUM_IN
OUT_N = NUM_IN + 1
OUT_A = NUM_IN + 2

parts = []


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
        "m_ColorMode": 0,
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
n_fbase, n_fnorm, n_fsmooth, n_fmetal, n_femi, n_falpha = nid(), nid(), nid(), nid(), nid(), nid()
n_uv = nid()
n_cf = nid()
tgt_id = nid()
sublit_id = nid()

s_uv_out = nid()
cf_slot_ids = [nid() for _ in range(NUM_IN)]
slot_outc = nid()
slot_outn = nid()
slot_outa = nid()

pn_entries = []
for i in range(N_CF_PROPS):
    pid = prop_ids[i]
    pnid = nid()
    psid = nid()
    st = PROP_SPECS[i][0]
    pn_entries.append((pid, pnid, psid, st))

pn_smooth = nid()
ps_smooth = nid()
pn_metal = nid()
ps_metal = nid()

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
    "m_OutputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": OUT_N},
    "m_InputSlot": {"m_Node": {"m_Id": n_fnorm}, "m_SlotId": 0},
})
edges.append({
    "m_OutputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": OUT_A},
    "m_InputSlot": {"m_Node": {"m_Id": n_falpha}, "m_SlotId": 0},
})
edges.append({
    "m_OutputSlot": {"m_Node": {"m_Id": pn_smooth}, "m_SlotId": 0},
    "m_InputSlot": {"m_Node": {"m_Id": n_fsmooth}, "m_SlotId": 0},
})
edges.append({
    "m_OutputSlot": {"m_Node": {"m_Id": pn_metal}, "m_SlotId": 0},
    "m_InputSlot": {"m_Node": {"m_Id": n_fmetal}, "m_SlotId": 0},
})

parts.append({
    "m_SGVersion": 3,
    "m_Type": "UnityEditor.ShaderGraph.GraphData",
    "m_ObjectId": graph_id,
    "m_Properties": [{"m_Id": x} for x in prop_ids],
    "m_Keywords": [],
    "m_Dropdowns": [],
    "m_CategoryData": [{"m_Id": cat_id}],
    "m_Nodes": [{"m_Id": x} for x in [
        n_vpos, n_vnorm, n_vtan, n_fbase, n_fnorm, n_fsmooth, n_fmetal, n_femi, n_falpha,
        n_uv, n_cf,
    ] + [e[1] for e in pn_entries] + [pn_smooth, pn_metal]],
    "m_GroupDatas": [],
    "m_StickyNoteDatas": [],
    "m_Edges": edges,
    "m_VertexContext": {"m_Position": {"x": 0, "y": 0}, "m_Blocks": [{"m_Id": n_vpos}, {"m_Id": n_vnorm}, {"m_Id": n_vtan}]},
    "m_FragmentContext": {"m_Position": {"x": 0, "y": 200}, "m_Blocks": [{"m_Id": n_fbase}, {"m_Id": n_fnorm}, {"m_Id": n_fsmooth}, {"m_Id": n_fmetal}, {"m_Id": n_femi}, {"m_Id": n_falpha}]},
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
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.PositionMaterialSlot",
    "m_ObjectId": svp,
    "m_Id": 0,
    "m_DisplayName": "Position",
    "m_SlotType": 0,
    "m_Hidden": False,
    "m_ShaderOutputName": "Position",
    "m_StageCapability": 1,
    "m_Value": {"x": 0, "y": 0, "z": 0},
    "m_DefaultValue": {"x": 0, "y": 0, "z": 0},
    "m_Labels": [],
    "m_Space": 0,
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.BlockNode",
    "m_ObjectId": n_vpos,
    "m_Group": {"m_Id": ""},
    "m_Name": "VertexDescription.Position",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}},
    "m_Slots": [{"m_Id": svp}],
    "synonyms": [],
    "m_Precision": 0,
    "m_PreviewExpanded": True,
    "m_DismissedVersion": 0,
    "m_PreviewMode": 0,
    "m_CustomColors": {"m_SerializableColors": []},
    "m_SerializedDescriptor": "VertexDescription.Position",
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.NormalMaterialSlot",
    "m_ObjectId": svn,
    "m_Id": 0,
    "m_DisplayName": "Normal",
    "m_SlotType": 0,
    "m_Hidden": False,
    "m_ShaderOutputName": "Normal",
    "m_StageCapability": 1,
    "m_Value": {"x": 0, "y": 0, "z": 0},
    "m_DefaultValue": {"x": 0, "y": 0, "z": 0},
    "m_Labels": [],
    "m_Space": 0,
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.BlockNode",
    "m_ObjectId": n_vnorm,
    "m_Group": {"m_Id": ""},
    "m_Name": "VertexDescription.Normal",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}},
    "m_Slots": [{"m_Id": svn}],
    "synonyms": [],
    "m_Precision": 0,
    "m_PreviewExpanded": True,
    "m_DismissedVersion": 0,
    "m_PreviewMode": 0,
    "m_CustomColors": {"m_SerializableColors": []},
    "m_SerializedDescriptor": "VertexDescription.Normal",
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.TangentMaterialSlot",
    "m_ObjectId": svt,
    "m_Id": 0,
    "m_DisplayName": "Tangent",
    "m_SlotType": 0,
    "m_Hidden": False,
    "m_ShaderOutputName": "Tangent",
    "m_StageCapability": 1,
    "m_Value": {"x": 0, "y": 0, "z": 0},
    "m_DefaultValue": {"x": 0, "y": 0, "z": 0},
    "m_Labels": [],
    "m_Space": 0,
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.BlockNode",
    "m_ObjectId": n_vtan,
    "m_Group": {"m_Id": ""},
    "m_Name": "VertexDescription.Tangent",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}},
    "m_Slots": [{"m_Id": svt}],
    "synonyms": [],
    "m_Precision": 0,
    "m_PreviewExpanded": True,
    "m_DismissedVersion": 0,
    "m_PreviewMode": 0,
    "m_CustomColors": {"m_SerializableColors": []},
    "m_SerializedDescriptor": "VertexDescription.Tangent",
})

sfb, sfn, sfs, sfm, sfe, sfa = nid(), nid(), nid(), nid(), nid(), nid()
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.ColorRGBMaterialSlot",
    "m_ObjectId": sfb,
    "m_Id": 0,
    "m_DisplayName": "Base Color",
    "m_SlotType": 0,
    "m_Hidden": False,
    "m_ShaderOutputName": "BaseColor",
    "m_StageCapability": 2,
    "m_Value": {"x": 0.5, "y": 0.5, "z": 0.5},
    "m_DefaultValue": {"x": 0, "y": 0, "z": 0},
    "m_Labels": [],
    "m_ColorMode": 0,
    "m_DefaultColor": {"r": 0.5, "g": 0.5, "b": 0.5, "a": 1},
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.BlockNode",
    "m_ObjectId": n_fbase,
    "m_Group": {"m_Id": ""},
    "m_Name": "SurfaceDescription.BaseColor",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}},
    "m_Slots": [{"m_Id": sfb}],
    "synonyms": [],
    "m_Precision": 0,
    "m_PreviewExpanded": True,
    "m_DismissedVersion": 0,
    "m_PreviewMode": 0,
    "m_CustomColors": {"m_SerializableColors": []},
    "m_SerializedDescriptor": "SurfaceDescription.BaseColor",
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.NormalMaterialSlot",
    "m_ObjectId": sfn,
    "m_Id": 0,
    "m_DisplayName": "Normal (Tangent Space)",
    "m_SlotType": 0,
    "m_Hidden": False,
    "m_ShaderOutputName": "NormalTS",
    "m_StageCapability": 2,
    "m_Value": {"x": 0, "y": 0, "z": 1},
    "m_DefaultValue": {"x": 0, "y": 0, "z": 0},
    "m_Labels": [],
    "m_Space": 3,
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.BlockNode",
    "m_ObjectId": n_fnorm,
    "m_Group": {"m_Id": ""},
    "m_Name": "SurfaceDescription.NormalTS",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}},
    "m_Slots": [{"m_Id": sfn}],
    "synonyms": [],
    "m_Precision": 0,
    "m_PreviewExpanded": True,
    "m_DismissedVersion": 0,
    "m_PreviewMode": 0,
    "m_CustomColors": {"m_SerializableColors": []},
    "m_SerializedDescriptor": "SurfaceDescription.NormalTS",
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot",
    "m_ObjectId": sfs,
    "m_Id": 0,
    "m_DisplayName": "Smoothness",
    "m_SlotType": 0,
    "m_Hidden": False,
    "m_ShaderOutputName": "Smoothness",
    "m_StageCapability": 2,
    "m_Value": 0.5,
    "m_DefaultValue": 0.5,
    "m_Labels": [],
    "m_LiteralMode": False,
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.BlockNode",
    "m_ObjectId": n_fsmooth,
    "m_Group": {"m_Id": ""},
    "m_Name": "SurfaceDescription.Smoothness",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}},
    "m_Slots": [{"m_Id": sfs}],
    "synonyms": [],
    "m_Precision": 0,
    "m_PreviewExpanded": True,
    "m_DismissedVersion": 0,
    "m_PreviewMode": 0,
    "m_CustomColors": {"m_SerializableColors": []},
    "m_SerializedDescriptor": "SurfaceDescription.Smoothness",
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot",
    "m_ObjectId": sfm,
    "m_Id": 0,
    "m_DisplayName": "Metallic",
    "m_SlotType": 0,
    "m_Hidden": False,
    "m_ShaderOutputName": "Metallic",
    "m_StageCapability": 2,
    "m_Value": 0,
    "m_DefaultValue": 0,
    "m_Labels": [],
    "m_LiteralMode": False,
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.BlockNode",
    "m_ObjectId": n_fmetal,
    "m_Group": {"m_Id": ""},
    "m_Name": "SurfaceDescription.Metallic",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}},
    "m_Slots": [{"m_Id": sfm}],
    "synonyms": [],
    "m_Precision": 0,
    "m_PreviewExpanded": True,
    "m_DismissedVersion": 0,
    "m_PreviewMode": 0,
    "m_CustomColors": {"m_SerializableColors": []},
    "m_SerializedDescriptor": "SurfaceDescription.Metallic",
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.ColorRGBMaterialSlot",
    "m_ObjectId": sfe,
    "m_Id": 0,
    "m_DisplayName": "Emission",
    "m_SlotType": 0,
    "m_Hidden": False,
    "m_ShaderOutputName": "Emission",
    "m_StageCapability": 2,
    "m_Value": {"x": 0, "y": 0, "z": 0},
    "m_DefaultValue": {"x": 0, "y": 0, "z": 0},
    "m_Labels": [],
    "m_ColorMode": 1,
    "m_DefaultColor": {"r": 0, "g": 0, "b": 0, "a": 1},
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.BlockNode",
    "m_ObjectId": n_femi,
    "m_Group": {"m_Id": ""},
    "m_Name": "SurfaceDescription.Emission",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}},
    "m_Slots": [{"m_Id": sfe}],
    "synonyms": [],
    "m_Precision": 0,
    "m_PreviewExpanded": True,
    "m_DismissedVersion": 0,
    "m_PreviewMode": 0,
    "m_CustomColors": {"m_SerializableColors": []},
    "m_SerializedDescriptor": "SurfaceDescription.Emission",
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot",
    "m_ObjectId": sfa,
    "m_Id": 0,
    "m_DisplayName": "Alpha",
    "m_SlotType": 0,
    "m_Hidden": False,
    "m_ShaderOutputName": "Alpha",
    "m_StageCapability": 2,
    "m_Value": 1.0,
    "m_DefaultValue": 1.0,
    "m_Labels": [],
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.BlockNode",
    "m_ObjectId": n_falpha,
    "m_Group": {"m_Id": ""},
    "m_Name": "SurfaceDescription.Alpha",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}},
    "m_Slots": [{"m_Id": sfa}],
    "synonyms": [],
    "m_Precision": 0,
    "m_PreviewExpanded": True,
    "m_DismissedVersion": 0,
    "m_PreviewMode": 0,
    "m_CustomColors": {"m_SerializableColors": []},
    "m_SerializedDescriptor": "SurfaceDescription.Alpha",
})

parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.Vector4MaterialSlot",
    "m_ObjectId": s_uv_out,
    "m_Id": 0,
    "m_DisplayName": "Out",
    "m_SlotType": 1,
    "m_Hidden": False,
    "m_ShaderOutputName": "Out",
    "m_StageCapability": 3,
    "m_Value": {"x": 0, "y": 0, "z": 0, "w": 0},
    "m_DefaultValue": {"x": 0, "y": 0, "z": 0, "w": 0},
    "m_Labels": [],
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.UVNode",
    "m_ObjectId": n_uv,
    "m_Group": {"m_Id": ""},
    "m_Name": "UV",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -520, "y": 40, "width": 145, "height": 128}},
    "m_Slots": [{"m_Id": s_uv_out}],
    "synonyms": ["texcoords", "coords"],
    "m_Precision": 0,
    "m_PreviewExpanded": False,
    "m_DismissedVersion": 0,
    "m_PreviewMode": 0,
    "m_CustomColors": {"m_SerializableColors": []},
    "m_OutputChannel": 0,
})

for i, name in enumerate(cf_in_names):
    oid = cf_slot_ids[i]
    if i == 0:
        parts.append({
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.Vector4MaterialSlot",
            "m_ObjectId": oid,
            "m_Id": i,
            "m_DisplayName": name,
            "m_SlotType": 0,
            "m_Hidden": False,
            "m_ShaderOutputName": name,
            "m_StageCapability": 3,
            "m_Value": {"x": 0, "y": 0, "z": 0, "w": 0},
            "m_DefaultValue": {"x": 0, "y": 0, "z": 0, "w": 0},
            "m_Labels": [],
        })
    elif i <= 3:
        parts.append({
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.Vector4MaterialSlot",
            "m_ObjectId": oid,
            "m_Id": i,
            "m_DisplayName": name,
            "m_SlotType": 0,
            "m_Hidden": False,
            "m_ShaderOutputName": name,
            "m_StageCapability": 3,
            "m_Value": {"x": 0, "y": 0, "z": 0, "w": 0},
            "m_DefaultValue": {"x": 0, "y": 0, "z": 0, "w": 0},
            "m_Labels": [],
        })
    else:
        parts.append({
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot",
            "m_ObjectId": oid,
            "m_Id": i,
            "m_DisplayName": name,
            "m_SlotType": 0,
            "m_Hidden": False,
            "m_ShaderOutputName": name,
            "m_StageCapability": 3,
            "m_Value": 0.0,
            "m_DefaultValue": 0.0,
            "m_Labels": [],
            "m_LiteralMode": False,
        })

parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.Vector3MaterialSlot",
    "m_ObjectId": slot_outc,
    "m_Id": OUT_C,
    "m_DisplayName": "OutColor",
    "m_SlotType": 1,
    "m_Hidden": False,
    "m_ShaderOutputName": "OutColor",
    "m_StageCapability": 3,
    "m_Value": {"x": 0, "y": 0, "z": 0},
    "m_DefaultValue": {"x": 0, "y": 0, "z": 0},
    "m_Labels": [],
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.Vector3MaterialSlot",
    "m_ObjectId": slot_outn,
    "m_Id": OUT_N,
    "m_DisplayName": "OutNormalTS",
    "m_SlotType": 1,
    "m_Hidden": False,
    "m_ShaderOutputName": "OutNormalTS",
    "m_StageCapability": 3,
    "m_Value": {"x": 0, "y": 0, "z": 1},
    "m_DefaultValue": {"x": 0, "y": 0, "z": 1},
    "m_Labels": [],
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot",
    "m_ObjectId": slot_outa,
    "m_Id": OUT_A,
    "m_DisplayName": "OutAlpha",
    "m_SlotType": 1,
    "m_Hidden": False,
    "m_ShaderOutputName": "OutAlpha",
    "m_StageCapability": 3,
    "m_Value": 1.0,
    "m_DefaultValue": 1.0,
    "m_Labels": [],
    "m_LiteralMode": False,
})

cf_slot_refs = cf_slot_ids + [slot_outc, slot_outn, slot_outa]
parts.append({
    "m_SGVersion": 1,
    "m_Type": "UnityEditor.ShaderGraph.CustomFunctionNode",
    "m_ObjectId": n_cf,
    "m_Group": {"m_Id": ""},
    "m_Name": "HullProceduralLit (Custom Function)",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -200, "y": 60, "width": 320, "height": 300}},
    "m_Slots": [{"m_Id": x} for x in cf_slot_refs],
    "synonyms": ["code", "HLSL"],
    "m_Precision": 0,
    "m_PreviewExpanded": False,
    "m_DismissedVersion": 0,
    "m_PreviewMode": 0,
    "m_CustomColors": {"m_SerializableColors": []},
    "m_SourceType": 0,
    "m_FunctionName": "HullProceduralLit",
    "m_FunctionSource": HLSL_GUID,
    "m_FunctionSourceUsePragmas": True,
    "m_FunctionBody": "",
})

for row, ((pid, pnid, psid, st), spec) in enumerate(zip(pn_entries, PROP_SPECS[:N_CF_PROPS])):
    parts.append({
        "m_SGVersion": 0,
        "m_Type": "UnityEditor.ShaderGraph.PropertyNode",
        "m_ObjectId": pnid,
        "m_Group": {"m_Id": ""},
        "m_Name": "Property",
        "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -420, "y": 200 + 28 * row, "width": 160, "height": 34}},
        "m_Slots": [{"m_Id": psid}],
        "synonyms": [],
        "m_Precision": 0,
        "m_PreviewExpanded": True,
        "m_DismissedVersion": 0,
        "m_PreviewMode": 0,
        "m_CustomColors": {"m_SerializableColors": []},
        "m_Property": {"m_Id": pid},
    })
    if st == "float":
        parts.append({
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot",
            "m_ObjectId": psid,
            "m_Id": 0,
            "m_DisplayName": "Out",
            "m_SlotType": 1,
            "m_Hidden": False,
            "m_ShaderOutputName": "Out",
            "m_StageCapability": 3,
            "m_Value": 0.0,
            "m_DefaultValue": 0.0,
            "m_Labels": [],
            "m_LiteralMode": False,
        })
    else:
        parts.append({
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.Vector4MaterialSlot",
            "m_ObjectId": psid,
            "m_Id": 0,
            "m_DisplayName": "Out",
            "m_SlotType": 1,
            "m_Hidden": False,
            "m_ShaderOutputName": "Out",
            "m_StageCapability": 3,
            "m_Value": {"x": 0, "y": 0, "z": 0, "w": 0},
            "m_DefaultValue": {"x": 0, "y": 0, "z": 0, "w": 0},
            "m_Labels": [],
        })

parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.PropertyNode",
    "m_ObjectId": pn_smooth,
    "m_Group": {"m_Id": ""},
    "m_Name": "Property",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -420, "y": 520, "width": 160, "height": 34}},
    "m_Slots": [{"m_Id": ps_smooth}],
    "synonyms": [],
    "m_Precision": 0,
    "m_PreviewExpanded": True,
    "m_DismissedVersion": 0,
    "m_PreviewMode": 0,
    "m_CustomColors": {"m_SerializableColors": []},
    "m_Property": {"m_Id": prop_ids[10]},
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot",
    "m_ObjectId": ps_smooth,
    "m_Id": 0,
    "m_DisplayName": "Out",
    "m_SlotType": 1,
    "m_Hidden": False,
    "m_ShaderOutputName": "Out",
    "m_StageCapability": 3,
    "m_Value": 0.0,
    "m_DefaultValue": 0.0,
    "m_Labels": [],
    "m_LiteralMode": False,
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.PropertyNode",
    "m_ObjectId": pn_metal,
    "m_Group": {"m_Id": ""},
    "m_Name": "Property",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -420, "y": 552, "width": 160, "height": 34}},
    "m_Slots": [{"m_Id": ps_metal}],
    "synonyms": [],
    "m_Precision": 0,
    "m_PreviewExpanded": True,
    "m_DismissedVersion": 0,
    "m_PreviewMode": 0,
    "m_CustomColors": {"m_SerializableColors": []},
    "m_Property": {"m_Id": prop_ids[11]},
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot",
    "m_ObjectId": ps_metal,
    "m_Id": 0,
    "m_DisplayName": "Out",
    "m_SlotType": 1,
    "m_Hidden": False,
    "m_ShaderOutputName": "Out",
    "m_StageCapability": 3,
    "m_Value": 0.0,
    "m_DefaultValue": 0.0,
    "m_Labels": [],
    "m_LiteralMode": False,
})

parts.append({
    "m_SGVersion": 2,
    "m_Type": "UnityEditor.Rendering.Universal.ShaderGraph.UniversalLitSubTarget",
    "m_ObjectId": sublit_id,
    "m_WorkflowMode": 1,
    "m_NormalDropOffSpace": 0,
    "m_ClearCoat": False,
    "m_BlendModePreserveSpecular": True,
})
parts.append({
    "m_SGVersion": 1,
    "m_Type": "UnityEditor.Rendering.Universal.ShaderGraph.UniversalTarget",
    "m_ObjectId": tgt_id,
    "m_Datas": [],
    "m_ActiveSubTarget": {"m_Id": sublit_id},
    "m_AllowMaterialOverride": True,
    "m_SurfaceType": 0,
    "m_ZTestMode": 4,
    "m_ZWriteControl": 0,
    "m_AlphaMode": 0,
    "m_RenderFace": 0,
    "m_AlphaClip": False,
    "m_CastShadows": True,
    "m_ReceiveShadows": True,
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

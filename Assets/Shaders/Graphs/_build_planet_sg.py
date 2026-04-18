# Generates PlanetStylizedUnlit.shadergraph — run once then delete this file.
import json
import uuid


def nid():
    return uuid.uuid4().hex


HLSL_GUID = "7c3ea8912b5f4c1d9a8e7f6d5c4b3a21"
OUT_PATH = __file__.replace("_build_planet_sg.py", "PlanetStylizedUnlit.shadergraph")

# --- Property definitions: (type, display_name, ref_name, default extras)
PROP_SPECS = [
    ("float", "Use World Space", "_UseWorldSpace", (1.0, 0.0, 1.0)),
    ("float", "Noise Scale", "_NoiseScale", (2.8, 0.5, 12.0)),
    ("float", "Land Threshold", "_LandThreshold", (0.48, 0.2, 0.75)),
    ("float", "Land Blend", "_LandBlend", (0.08, 0.01, 0.25)),
    ("float", "Ice Latitude", "_IceLatitude", (0.82, 0.5, 0.98)),
    ("float", "Rim Strength", "_RimStrength", (0.55, 0.0, 1.5)),
    ("vec3", "Planet Center", "_PlanetCenter", (0.0, 0.0, 0.0)),
    ("color", "Ocean Deep", "_ColOceanDeep", (0.05, 0.12, 0.35, 1.0)),
    ("color", "Ocean Shore", "_ColOceanShore", (0.15, 0.45, 0.65, 1.0)),
    ("color", "Land", "_ColLand", (0.25, 0.55, 0.22, 1.0)),
    ("color", "Peaks", "_ColPeak", (0.45, 0.38, 0.32, 1.0)),
    ("color", "Ice", "_ColIce", (0.92, 0.95, 1.0, 1.0)),
    ("color", "Rim Color", "_RimColor", (0.4, 0.65, 1.0, 0.85)),
]

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


def vec3_prop(oid, name, ref, x, y, z):
    return {
        "m_SGVersion": 1,
        "m_Type": "UnityEditor.ShaderGraph.Internal.Vector3ShaderProperty",
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
        "m_Value": {"x": x, "y": y, "z": z, "w": 0.0},
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
    elif t == "vec3":
        x, y, z = spec[3]
        prop_objs.append(vec3_prop(pid, spec[1], spec[2], x, y, z))
    else:
        prop_objs.append(color_prop(pid, spec[1], spec[2], spec[3]))

cat_id = nid()
graph_id = nid()

# Fixed geometry / CF node ids (stable for one run)
n_vpos, n_vnorm, n_vtan = nid(), nid(), nid()
n_fbase, n_femi, n_falpha = nid(), nid(), nid()
n_pworld, n_pobj = nid(), nid()
n_norm, n_view = nid(), nid()
n_cf = nid()
tgt_id = nid()
subunlit_id = nid()

# Slots references (will define objects below)
s_pw, s_po = nid(), nid()
s_norm_out = nid()
s_view_out = nid()

# CF input slot objects (by semantic order)
cf_in_names = [
    "WorldPos", "ObjectPos", "WorldNormal", "ViewDir",
    "UseWorldSpace", "NoiseScale", "LandThreshold", "LandBlend", "IceLatitude", "RimStrength",
    "PlanetCenter",
    "ColOceanDeep", "ColOceanShore", "ColLand", "ColPeak", "ColIce", "RimColor",
]
cf_slot_ids = [nid() for _ in range(len(cf_in_names))]
slot_outc = nid()
slot_outa = nid()  # unused if opaque - we use only OutColor

# Property nodes + output slot ids
pn_entries = []
for i, pid in enumerate(prop_ids):
    pnid = nid()
    psid = nid()
    st = PROP_SPECS[i][0]
    pn_entries.append((pid, pnid, psid, st))

# Edges
edges = [
    {"m_OutputSlot": {"m_Node": {"m_Id": n_pworld}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 0}},
    {"m_OutputSlot": {"m_Node": {"m_Id": n_pobj}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 1}},
    {"m_OutputSlot": {"m_Node": {"m_Id": n_norm}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 2}},
    {"m_OutputSlot": {"m_Node": {"m_Id": n_view}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 3}},
]
for idx, (_, pnid, _, _) in enumerate(pn_entries):
    edges.append({
        "m_OutputSlot": {"m_Node": {"m_Id": pnid}, "m_SlotId": 0},
        "m_InputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 4 + idx},
    })
edges.append({
    "m_OutputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 17},
    "m_InputSlot": {"m_Node": {"m_Id": n_fbase}, "m_SlotId": 0},
})
edges.append({
    "m_OutputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 18},
    "m_InputSlot": {"m_Node": {"m_Id": n_falpha}, "m_SlotId": 0},
})

# GraphData
parts.append({
    "m_SGVersion": 3,
    "m_Type": "UnityEditor.ShaderGraph.GraphData",
    "m_ObjectId": graph_id,
    "m_Properties": [{"m_Id": x} for x in prop_ids],
    "m_Keywords": [],
    "m_Dropdowns": [],
    "m_CategoryData": [{"m_Id": cat_id}],
    "m_Nodes": [{"m_Id": x} for x in [
        n_vpos, n_vnorm, n_vtan, n_fbase, n_femi, n_falpha,
        n_pworld, n_pobj, n_norm, n_view, n_cf,
    ] + [e[1] for e in pn_entries]],
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

# Vertex blocks — separate slot ids
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

sfb, sfe, sfa = nid(), nid(), nid()
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

# Position world / object
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.Vector3MaterialSlot",
    "m_ObjectId": s_pw,
    "m_Id": 0,
    "m_DisplayName": "Out",
    "m_SlotType": 1,
    "m_Hidden": False,
    "m_ShaderOutputName": "Out",
    "m_StageCapability": 3,
    "m_Value": {"x": 0, "y": 0, "z": 0},
    "m_DefaultValue": {"x": 0, "y": 0, "z": 0},
    "m_Labels": [],
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.PositionNode",
    "m_ObjectId": n_pworld,
    "m_Group": {"m_Id": ""},
    "m_Name": "Position",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -500, "y": 0, "width": 206, "height": 130}},
    "m_Slots": [{"m_Id": s_pw}],
    "synonyms": [],
    "m_Precision": 1,
    "m_PreviewExpanded": False,
    "m_DismissedVersion": 0,
    "m_PreviewMode": 2,
    "m_CustomColors": {"m_SerializableColors": []},
    "m_Space": 2,
    "m_PositionSource": 0,
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.Vector3MaterialSlot",
    "m_ObjectId": s_po,
    "m_Id": 0,
    "m_DisplayName": "Out",
    "m_SlotType": 1,
    "m_Hidden": False,
    "m_ShaderOutputName": "Out",
    "m_StageCapability": 3,
    "m_Value": {"x": 0, "y": 0, "z": 0},
    "m_DefaultValue": {"x": 0, "y": 0, "z": 0},
    "m_Labels": [],
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.PositionNode",
    "m_ObjectId": n_pobj,
    "m_Group": {"m_Id": ""},
    "m_Name": "Position",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -500, "y": 160, "width": 206, "height": 130}},
    "m_Slots": [{"m_Id": s_po}],
    "synonyms": [],
    "m_Precision": 1,
    "m_PreviewExpanded": False,
    "m_DismissedVersion": 0,
    "m_PreviewMode": 2,
    "m_CustomColors": {"m_SerializableColors": []},
    "m_Space": 0,
    "m_PositionSource": 0,
})

parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.Vector3MaterialSlot",
    "m_ObjectId": s_norm_out,
    "m_Id": 0,
    "m_DisplayName": "Out",
    "m_SlotType": 1,
    "m_Hidden": False,
    "m_ShaderOutputName": "Out",
    "m_StageCapability": 3,
    "m_Value": {"x": 0, "y": 0, "z": 0},
    "m_DefaultValue": {"x": 0, "y": 0, "z": 0},
    "m_Labels": [],
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.NormalVectorNode",
    "m_ObjectId": n_norm,
    "m_Group": {"m_Id": ""},
    "m_Name": "Normal Vector",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -500, "y": 320, "width": 206, "height": 130}},
    "m_Slots": [{"m_Id": s_norm_out}],
    "synonyms": ["surface direction"],
    "m_Precision": 0,
    "m_PreviewExpanded": False,
    "m_DismissedVersion": 0,
    "m_PreviewMode": 2,
    "m_CustomColors": {"m_SerializableColors": []},
    "m_Space": 2,
})

parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.Vector3MaterialSlot",
    "m_ObjectId": s_view_out,
    "m_Id": 0,
    "m_DisplayName": "Out",
    "m_SlotType": 1,
    "m_Hidden": False,
    "m_ShaderOutputName": "Out",
    "m_StageCapability": 3,
    "m_Value": {"x": 0, "y": 0, "z": 0},
    "m_DefaultValue": {"x": 0, "y": 0, "z": 0},
    "m_Labels": [],
})
parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.ViewDirectionNode",
    "m_ObjectId": n_view,
    "m_Group": {"m_Id": ""},
    "m_Name": "View Direction",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -500, "y": 480, "width": 206, "height": 130}},
    "m_Slots": [{"m_Id": s_view_out}],
    "synonyms": ["eye direction"],
    "m_Precision": 0,
    "m_PreviewExpanded": False,
    "m_DismissedVersion": 0,
    "m_PreviewMode": 2,
    "m_CustomColors": {"m_SerializableColors": []},
    "m_Space": 2,
})

# CF slot definitions
cf_slot_list = []
for i, name in enumerate(cf_in_names):
    sid = i
    oid = cf_slot_ids[i]
    if i < 4 or i == 10:
        cf_slot_list.append({
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.Vector3MaterialSlot",
            "m_ObjectId": oid,
            "m_Id": sid,
            "m_DisplayName": name,
            "m_SlotType": 0,
            "m_Hidden": False,
            "m_ShaderOutputName": name,
            "m_StageCapability": 3,
            "m_Value": {"x": 0, "y": 0, "z": 0},
            "m_DefaultValue": {"x": 0, "y": 0, "z": 0},
            "m_Labels": [],
        })
    elif i < 10:
        cf_slot_list.append({
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot",
            "m_ObjectId": oid,
            "m_Id": sid,
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
    else:
        cf_slot_list.append({
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.Vector4MaterialSlot",
            "m_ObjectId": oid,
            "m_Id": sid,
            "m_DisplayName": name,
            "m_SlotType": 0,
            "m_Hidden": False,
            "m_ShaderOutputName": name,
            "m_StageCapability": 3,
            "m_Value": {"x": 0, "y": 0, "z": 0, "w": 0},
            "m_DefaultValue": {"x": 0, "y": 0, "z": 0, "w": 0},
            "m_Labels": [],
        })

parts.extend(cf_slot_list)

parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.Vector3MaterialSlot",
    "m_ObjectId": slot_outc,
    "m_Id": 17,
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
    "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot",
    "m_ObjectId": slot_outa,
    "m_Id": 18,
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

cf_slot_refs = cf_slot_ids + [slot_outc, slot_outa]
parts.append({
    "m_SGVersion": 1,
    "m_Type": "UnityEditor.ShaderGraph.CustomFunctionNode",
    "m_ObjectId": n_cf,
    "m_Group": {"m_Id": ""},
    "m_Name": "PlanetStylizedUnlit (Custom Function)",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -120, "y": 80, "width": 280, "height": 220}},
    "m_Slots": [{"m_Id": x} for x in cf_slot_refs],
    "synonyms": ["code", "HLSL"],
    "m_Precision": 0,
    "m_PreviewExpanded": False,
    "m_DismissedVersion": 0,
    "m_PreviewMode": 0,
    "m_CustomColors": {"m_SerializableColors": []},
    "m_SourceType": 0,
    "m_FunctionName": "PlanetStylizedUnlit",
    "m_FunctionSource": HLSL_GUID,
    "m_FunctionSourceUsePragmas": True,
    "m_FunctionBody": "",
})

# Property nodes
for row, ((pid, pnid, psid, st), spec) in enumerate(zip(pn_entries, PROP_SPECS)):
    parts.append({
        "m_SGVersion": 0,
        "m_Type": "UnityEditor.ShaderGraph.PropertyNode",
        "m_ObjectId": pnid,
        "m_Group": {"m_Id": ""},
        "m_Name": "Property",
        "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -420, "y": 620 + 24 * row, "width": 160, "height": 34}},
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
    elif st == "vec3":
        parts.append({
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.Vector3MaterialSlot",
            "m_ObjectId": psid,
            "m_Id": 0,
            "m_DisplayName": "Out",
            "m_SlotType": 1,
            "m_Hidden": False,
            "m_ShaderOutputName": "Out",
            "m_StageCapability": 3,
            "m_Value": {"x": 0, "y": 0, "z": 0},
            "m_DefaultValue": {"x": 0, "y": 0, "z": 0},
            "m_Labels": [],
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
    "m_SGVersion": 2,
    "m_Type": "UnityEditor.Rendering.Universal.ShaderGraph.UniversalUnlitSubTarget",
    "m_ObjectId": subunlit_id,
    "m_KeepLightingVariants": False,
    "m_DefaultDecalBlending": True,
    "m_DefaultSSAO": True,
})
parts.append({
    "m_SGVersion": 1,
    "m_Type": "UnityEditor.Rendering.Universal.ShaderGraph.UniversalTarget",
    "m_ObjectId": tgt_id,
    "m_Datas": [],
    "m_ActiveSubTarget": {"m_Id": subunlit_id},
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

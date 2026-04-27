import json
import uuid


def nid():
    return uuid.uuid4().hex


OUT_PATH = __file__.replace("_build_toon_base_front_sg.py", "ToonBaseFrontURP.shadergraph")
HLSL_GUID = "2f68a6c11d4548f89e8f9b72fb9d631a"


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


def tex_prop(oid, name, ref):
    return {
        "m_SGVersion": 0,
        "m_Type": "UnityEditor.ShaderGraph.Internal.Texture2DShaderProperty",
        "m_ObjectId": oid,
        "m_Guid": {"m_GuidSerialized": str(uuid.uuid4())},
        "promotedFromAssetID": "",
        "promotedFromCategoryName": "",
        "promotedOrdering": -1,
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
        "m_Value": {"m_SerializedTexture": '{"texture":{"fileID":0}}', "m_Guid": ""},
        "isMainTexture": True,
        "useTilingAndOffset": False,
        "useTexelSize": False,
        "isHDR": False,
        "m_Modifiable": True,
        "m_DefaultType": 0,
    }


def float_prop(oid, name, ref, default, mn, mx):
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
        "m_Value": float(default),
        "m_FloatType": 1,
        "m_RangeValues": {"x": mn, "y": mx},
    }


prop_specs = [
    ("tex", "Main Tex", "_MainTex", None),
    ("color", "Base Color", "_BaseColor", (1.0, 1.0, 1.0, 1.0)),
    ("float", "Smoothness", "_Smoothness", (0.55, 0.0, 1.0)),
    ("float", "Metallic", "_Metallic", (0.0, 0.0, 1.0)),
    ("float", "Shadow Threshold", "_ShadowThreshold", (0.5, 0.0, 1.0)),
    ("float", "Shadow Softness", "_ShadowSoftness", (0.05, 0.001, 0.5)),
    ("float", "Shadow Strength", "_ShadowStrength", (0.55, 0.0, 1.0)),
    ("float", "Highlight Threshold", "_HighlightThreshold", (0.6, 0.0, 1.0)),
    ("float", "Highlight Softness", "_HighlightSoftness", (0.04, 0.001, 0.5)),
    ("float", "Highlight Intensity", "_HighlightIntensity", (0.8, 0.0, 2.0)),
    ("color", "Shadow Tint", "_ShadowTint", (1.0, 1.0, 1.0, 1.0)),
    ("color", "Highlight Tint", "_HighlightTint", (1.0, 1.0, 1.0, 1.0)),
]

prop_ids = [nid() for _ in prop_specs]
cat_id, graph_id, target_id, sublit_id = nid(), nid(), nid(), nid()

# core nodes
n_vpos, n_vnorm, n_vtan = nid(), nid(), nid()
n_fbase, n_fsmooth, n_fmetal, n_femi, n_falpha = nid(), nid(), nid(), nid(), nid()
n_uv, n_ptex, n_pbase, n_sample, n_mul = nid(), nid(), nid(), nid(), nid()
n_norm_ws, n_view_ws, n_cf = nid(), nid(), nid()
n_psmooth, n_pmetal = nid(), nid()
n_p_shadow_t, n_p_shadow_s, n_p_shadow_k = nid(), nid(), nid()
n_p_hi_t, n_p_hi_s, n_p_hi_i = nid(), nid(), nid()
n_p_shadow_tint, n_p_highlight_tint = nid(), nid()

# block slots
s_vpos, s_vnorm, s_vtan = nid(), nid(), nid()
s_fbase, s_fsmooth, s_fmetal, s_femi, s_falpha = nid(), nid(), nid(), nid(), nid()

# utility slots
s_uv_out, s_ptex_out, s_pbase_out = nid(), nid(), nid()
s_sample_rgba, s_sample_texture, s_sample_uv, s_sample_sampler = nid(), nid(), nid(), nid()
s_sample_r, s_sample_g, s_sample_b, s_sample_a = nid(), nid(), nid(), nid()
s_mul_a, s_mul_b, s_mul_out = nid(), nid(), nid()
s_norm_out, s_view_out = nid(), nid()

# property output slots
s_psmooth_out, s_pmetal_out = nid(), nid()
s_p_shadow_t_out, s_p_shadow_s_out, s_p_shadow_k_out = nid(), nid(), nid()
s_p_hi_t_out, s_p_hi_s_out, s_p_hi_i_out = nid(), nid(), nid()
s_p_shadow_tint_out, s_p_highlight_tint_out = nid(), nid()

# custom function slots
s_cf_base, s_cf_norm, s_cf_view = nid(), nid(), nid()
s_cf_shadow_t, s_cf_shadow_s, s_cf_shadow_k = nid(), nid(), nid()
s_cf_hi_t, s_cf_hi_s, s_cf_hi_i = nid(), nid(), nid()
s_cf_shadow_tint, s_cf_highlight_tint = nid(), nid()
s_cf_out_base, s_cf_out_emi = nid(), nid()

edges = [
    {"m_OutputSlot": {"m_Node": {"m_Id": n_ptex}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": n_sample}, "m_SlotId": 1}},
    {"m_OutputSlot": {"m_Node": {"m_Id": n_uv}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": n_sample}, "m_SlotId": 2}},
    {"m_OutputSlot": {"m_Node": {"m_Id": n_sample}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": n_mul}, "m_SlotId": 0}},
    {"m_OutputSlot": {"m_Node": {"m_Id": n_pbase}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": n_mul}, "m_SlotId": 1}},

    {"m_OutputSlot": {"m_Node": {"m_Id": n_mul}, "m_SlotId": 2}, "m_InputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 0}},
    {"m_OutputSlot": {"m_Node": {"m_Id": n_norm_ws}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 1}},
    {"m_OutputSlot": {"m_Node": {"m_Id": n_view_ws}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 2}},
    {"m_OutputSlot": {"m_Node": {"m_Id": n_p_shadow_t}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 3}},
    {"m_OutputSlot": {"m_Node": {"m_Id": n_p_shadow_s}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 4}},
    {"m_OutputSlot": {"m_Node": {"m_Id": n_p_shadow_k}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 5}},
    {"m_OutputSlot": {"m_Node": {"m_Id": n_p_hi_t}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 6}},
    {"m_OutputSlot": {"m_Node": {"m_Id": n_p_hi_s}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 7}},
    {"m_OutputSlot": {"m_Node": {"m_Id": n_p_hi_i}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 8}},
    {"m_OutputSlot": {"m_Node": {"m_Id": n_p_shadow_tint}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 9}},
    {"m_OutputSlot": {"m_Node": {"m_Id": n_p_highlight_tint}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 10}},

    {"m_OutputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 11}, "m_InputSlot": {"m_Node": {"m_Id": n_fbase}, "m_SlotId": 0}},
    {"m_OutputSlot": {"m_Node": {"m_Id": n_cf}, "m_SlotId": 12}, "m_InputSlot": {"m_Node": {"m_Id": n_femi}, "m_SlotId": 0}},

    {"m_OutputSlot": {"m_Node": {"m_Id": n_psmooth}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": n_fsmooth}, "m_SlotId": 0}},
    {"m_OutputSlot": {"m_Node": {"m_Id": n_pmetal}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": n_fmetal}, "m_SlotId": 0}},
    {"m_OutputSlot": {"m_Node": {"m_Id": n_sample}, "m_SlotId": 7}, "m_InputSlot": {"m_Node": {"m_Id": n_falpha}, "m_SlotId": 0}},
]

parts = [{
    "m_SGVersion": 3,
    "m_Type": "UnityEditor.ShaderGraph.GraphData",
    "m_ObjectId": graph_id,
    "m_Properties": [{"m_Id": x} for x in prop_ids],
    "m_Keywords": [],
    "m_Dropdowns": [],
    "m_CategoryData": [{"m_Id": cat_id}],
    "m_Nodes": [{"m_Id": x} for x in [
        n_vpos, n_vnorm, n_vtan, n_fbase, n_fsmooth, n_fmetal, n_femi, n_falpha,
        n_uv, n_ptex, n_pbase, n_sample, n_mul, n_norm_ws, n_view_ws, n_cf,
        n_psmooth, n_pmetal, n_p_shadow_t, n_p_shadow_s, n_p_shadow_k, n_p_hi_t, n_p_hi_s, n_p_hi_i,
        n_p_shadow_tint, n_p_highlight_tint
    ]],
    "m_GroupDatas": [],
    "m_StickyNoteDatas": [],
    "m_Edges": edges,
    "m_VertexContext": {"m_Position": {"x": 0, "y": 0}, "m_Blocks": [{"m_Id": n_vpos}, {"m_Id": n_vnorm}, {"m_Id": n_vtan}]},
    "m_FragmentContext": {"m_Position": {"x": 0, "y": 200}, "m_Blocks": [{"m_Id": n_fbase}, {"m_Id": n_fsmooth}, {"m_Id": n_fmetal}, {"m_Id": n_femi}, {"m_Id": n_falpha}]},
    "m_PreviewData": {"serializedMesh": {"m_SerializedMesh": '{"mesh":{"instanceID":0}}', "m_Guid": ""}, "preventRotation": False},
    "m_Path": "Shader Graphs",
    "m_GraphPrecision": 1,
    "m_PreviewMode": 2,
    "m_OutputNode": {"m_Id": ""},
    "m_SubDatas": [],
    "m_ActiveTargets": [{"m_Id": target_id}],
}]

parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.CategoryData", "m_ObjectId": cat_id, "m_Name": "", "m_ChildObjectList": [{"m_Id": x} for x in prop_ids]})

for pid, spec in zip(prop_ids, prop_specs):
    t = spec[0]
    if t == "color":
        parts.append(color_prop(pid, spec[1], spec[2], spec[3]))
    elif t == "tex":
        parts.append(tex_prop(pid, spec[1], spec[2]))
    else:
        d, mn, mx = spec[3]
        parts.append(float_prop(pid, spec[1], spec[2], d, mn, mx))

# Vertex blocks
parts.extend([
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.PositionMaterialSlot", "m_ObjectId": s_vpos, "m_Id": 0, "m_DisplayName": "Position", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Position", "m_StageCapability": 1, "m_Value": {"x": 0, "y": 0, "z": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0}, "m_Labels": [], "m_Space": 0},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.BlockNode", "m_ObjectId": n_vpos, "m_Group": {"m_Id": ""}, "m_Name": "VertexDescription.Position", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}}, "m_Slots": [{"m_Id": s_vpos}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_SerializedDescriptor": "VertexDescription.Position"},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.NormalMaterialSlot", "m_ObjectId": s_vnorm, "m_Id": 0, "m_DisplayName": "Normal", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Normal", "m_StageCapability": 1, "m_Value": {"x": 0, "y": 0, "z": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0}, "m_Labels": [], "m_Space": 0},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.BlockNode", "m_ObjectId": n_vnorm, "m_Group": {"m_Id": ""}, "m_Name": "VertexDescription.Normal", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}}, "m_Slots": [{"m_Id": s_vnorm}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_SerializedDescriptor": "VertexDescription.Normal"},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.TangentMaterialSlot", "m_ObjectId": s_vtan, "m_Id": 0, "m_DisplayName": "Tangent", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Tangent", "m_StageCapability": 1, "m_Value": {"x": 0, "y": 0, "z": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0}, "m_Labels": [], "m_Space": 0},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.BlockNode", "m_ObjectId": n_vtan, "m_Group": {"m_Id": ""}, "m_Name": "VertexDescription.Tangent", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}}, "m_Slots": [{"m_Id": s_vtan}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_SerializedDescriptor": "VertexDescription.Tangent"},
])

# Fragment blocks
parts.extend([
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.ColorRGBMaterialSlot", "m_ObjectId": s_fbase, "m_Id": 0, "m_DisplayName": "Base Color", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "BaseColor", "m_StageCapability": 2, "m_Value": {"x": 0.5, "y": 0.5, "z": 0.5}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0}, "m_Labels": [], "m_ColorMode": 0, "m_DefaultColor": {"r": 0.5, "g": 0.5, "b": 0.5, "a": 1}},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.BlockNode", "m_ObjectId": n_fbase, "m_Group": {"m_Id": ""}, "m_Name": "SurfaceDescription.BaseColor", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}}, "m_Slots": [{"m_Id": s_fbase}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_SerializedDescriptor": "SurfaceDescription.BaseColor"},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot", "m_ObjectId": s_fsmooth, "m_Id": 0, "m_DisplayName": "Smoothness", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Smoothness", "m_StageCapability": 2, "m_Value": 0.5, "m_DefaultValue": 0.5, "m_Labels": [], "m_LiteralMode": False},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.BlockNode", "m_ObjectId": n_fsmooth, "m_Group": {"m_Id": ""}, "m_Name": "SurfaceDescription.Smoothness", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}}, "m_Slots": [{"m_Id": s_fsmooth}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_SerializedDescriptor": "SurfaceDescription.Smoothness"},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot", "m_ObjectId": s_fmetal, "m_Id": 0, "m_DisplayName": "Metallic", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Metallic", "m_StageCapability": 2, "m_Value": 0.0, "m_DefaultValue": 0.0, "m_Labels": [], "m_LiteralMode": False},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.BlockNode", "m_ObjectId": n_fmetal, "m_Group": {"m_Id": ""}, "m_Name": "SurfaceDescription.Metallic", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}}, "m_Slots": [{"m_Id": s_fmetal}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_SerializedDescriptor": "SurfaceDescription.Metallic"},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.ColorRGBMaterialSlot", "m_ObjectId": s_femi, "m_Id": 0, "m_DisplayName": "Emission", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Emission", "m_StageCapability": 2, "m_Value": {"x": 0, "y": 0, "z": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0}, "m_Labels": [], "m_ColorMode": 1, "m_DefaultColor": {"r": 0, "g": 0, "b": 0, "a": 1}},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.BlockNode", "m_ObjectId": n_femi, "m_Group": {"m_Id": ""}, "m_Name": "SurfaceDescription.Emission", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}}, "m_Slots": [{"m_Id": s_femi}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_SerializedDescriptor": "SurfaceDescription.Emission"},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot", "m_ObjectId": s_falpha, "m_Id": 0, "m_DisplayName": "Alpha", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Alpha", "m_StageCapability": 2, "m_Value": 1.0, "m_DefaultValue": 1.0, "m_Labels": []},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.BlockNode", "m_ObjectId": n_falpha, "m_Group": {"m_Id": ""}, "m_Name": "SurfaceDescription.Alpha", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}}, "m_Slots": [{"m_Id": s_falpha}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_SerializedDescriptor": "SurfaceDescription.Alpha"},
])

# Utility nodes
parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector4MaterialSlot", "m_ObjectId": s_uv_out, "m_Id": 0, "m_DisplayName": "Out", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "Out", "m_StageCapability": 3, "m_Value": {"x": 0, "y": 0, "z": 0, "w": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0, "w": 0}, "m_Labels": []})
parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.UVNode", "m_ObjectId": n_uv, "m_Group": {"m_Id": ""}, "m_Name": "UV", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -1040, "y": 120, "width": 145, "height": 128}}, "m_Slots": [{"m_Id": s_uv_out}], "synonyms": ["texcoords", "coords"], "m_Precision": 0, "m_PreviewExpanded": False, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_OutputChannel": 0})

prop_node_data = [
    (n_ptex, s_ptex_out, prop_ids[0], "tex", -1040, -20),
    (n_pbase, s_pbase_out, prop_ids[1], "vec4", -1040, 300),
    (n_psmooth, s_psmooth_out, prop_ids[2], "float", -1040, 520),
    (n_pmetal, s_pmetal_out, prop_ids[3], "float", -1040, 560),
    (n_p_shadow_t, s_p_shadow_t_out, prop_ids[4], "float", -1040, 620),
    (n_p_shadow_s, s_p_shadow_s_out, prop_ids[5], "float", -1040, 650),
    (n_p_shadow_k, s_p_shadow_k_out, prop_ids[6], "float", -1040, 680),
    (n_p_hi_t, s_p_hi_t_out, prop_ids[7], "float", -1040, 730),
    (n_p_hi_s, s_p_hi_s_out, prop_ids[8], "float", -1040, 760),
    (n_p_hi_i, s_p_hi_i_out, prop_ids[9], "float", -1040, 790),
    (n_p_shadow_tint, s_p_shadow_tint_out, prop_ids[10], "vec4", -1040, 830),
    (n_p_highlight_tint, s_p_highlight_tint_out, prop_ids[11], "vec4", -1040, 870),
]

for n, s, pid, kind, x, y in prop_node_data:
    if kind == "tex":
        parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Texture2DMaterialSlot", "m_ObjectId": s, "m_Id": 0, "m_DisplayName": "Out", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "Out", "m_StageCapability": 3, "m_BareResource": False})
    elif kind == "vec4":
        parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector4MaterialSlot", "m_ObjectId": s, "m_Id": 0, "m_DisplayName": "Out", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "Out", "m_StageCapability": 3, "m_Value": {"x": 1, "y": 1, "z": 1, "w": 1}, "m_DefaultValue": {"x": 1, "y": 1, "z": 1, "w": 1}, "m_Labels": []})
    else:
        parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot", "m_ObjectId": s, "m_Id": 0, "m_DisplayName": "Out", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "Out", "m_StageCapability": 3, "m_Value": 0.0, "m_DefaultValue": 0.0, "m_Labels": [], "m_LiteralMode": False})
    parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.PropertyNode", "m_ObjectId": n, "m_Group": {"m_Id": ""}, "m_Name": "Property", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": x, "y": y, "width": 160, "height": 34}}, "m_Slots": [{"m_Id": s}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_Property": {"m_Id": pid}})

parts.extend([
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector4MaterialSlot", "m_ObjectId": s_sample_rgba, "m_Id": 0, "m_DisplayName": "RGBA", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "RGBA", "m_StageCapability": 2, "m_Value": {"x": 0, "y": 0, "z": 0, "w": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0, "w": 0}, "m_Labels": []},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Texture2DInputMaterialSlot", "m_ObjectId": s_sample_texture, "m_Id": 1, "m_DisplayName": "Texture", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Texture", "m_StageCapability": 3, "m_BareResource": False, "m_Texture": {"m_SerializedTexture": '{"texture":{"fileID":0}}', "m_Guid": ""}, "m_DefaultType": 0},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.UVMaterialSlot", "m_ObjectId": s_sample_uv, "m_Id": 2, "m_DisplayName": "UV", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "UV", "m_StageCapability": 3, "m_Value": {"x": 0.0, "y": 0.0}, "m_DefaultValue": {"x": 0.0, "y": 0.0}, "m_Labels": [], "m_Channel": 0},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.SamplerStateMaterialSlot", "m_ObjectId": s_sample_sampler, "m_Id": 3, "m_DisplayName": "Sampler", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Sampler", "m_StageCapability": 3, "m_BareResource": False},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot", "m_ObjectId": s_sample_r, "m_Id": 4, "m_DisplayName": "R", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "R", "m_StageCapability": 2, "m_Value": 0.0, "m_DefaultValue": 0.0, "m_Labels": [], "m_LiteralMode": False},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot", "m_ObjectId": s_sample_g, "m_Id": 5, "m_DisplayName": "G", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "G", "m_StageCapability": 2, "m_Value": 0.0, "m_DefaultValue": 0.0, "m_Labels": [], "m_LiteralMode": False},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot", "m_ObjectId": s_sample_b, "m_Id": 6, "m_DisplayName": "B", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "B", "m_StageCapability": 2, "m_Value": 0.0, "m_DefaultValue": 0.0, "m_Labels": [], "m_LiteralMode": False},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot", "m_ObjectId": s_sample_a, "m_Id": 7, "m_DisplayName": "A", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "A", "m_StageCapability": 2, "m_Value": 0.0, "m_DefaultValue": 0.0, "m_Labels": [], "m_LiteralMode": False},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.SampleTexture2DNode", "m_ObjectId": n_sample, "m_Group": {"m_Id": ""}, "m_Name": "Sample Texture 2D", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -760, "y": 20, "width": 208.0, "height": 433.0}}, "m_Slots": [{"m_Id": s_sample_rgba}, {"m_Id": s_sample_texture}, {"m_Id": s_sample_uv}, {"m_Id": s_sample_sampler}, {"m_Id": s_sample_r}, {"m_Id": s_sample_g}, {"m_Id": s_sample_b}, {"m_Id": s_sample_a}], "synonyms": ["tex2d"], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_TextureType": 0, "m_NormalMapSpace": 0, "m_EnableGlobalMipBias": True, "m_MipSamplingMode": 0},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.DynamicValueMaterialSlot", "m_ObjectId": s_mul_a, "m_Id": 0, "m_DisplayName": "A", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "A", "m_StageCapability": 3, "m_Value": {"e00": 1.0, "e01": 0.0, "e02": 0.0, "e03": 0.0, "e10": 0.0, "e11": 1.0, "e12": 0.0, "e13": 0.0, "e20": 0.0, "e21": 0.0, "e22": 1.0, "e23": 0.0, "e30": 0.0, "e31": 0.0, "e32": 0.0, "e33": 1.0}, "m_DefaultValue": {"e00": 1.0, "e01": 0.0, "e02": 0.0, "e03": 0.0, "e10": 0.0, "e11": 1.0, "e12": 0.0, "e13": 0.0, "e20": 0.0, "e21": 0.0, "e22": 1.0, "e23": 0.0, "e30": 0.0, "e31": 0.0, "e32": 0.0, "e33": 1.0}},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.DynamicValueMaterialSlot", "m_ObjectId": s_mul_b, "m_Id": 1, "m_DisplayName": "B", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "B", "m_StageCapability": 3, "m_Value": {"e00": 1.0, "e01": 0.0, "e02": 0.0, "e03": 0.0, "e10": 0.0, "e11": 1.0, "e12": 0.0, "e13": 0.0, "e20": 0.0, "e21": 0.0, "e22": 1.0, "e23": 0.0, "e30": 0.0, "e31": 0.0, "e32": 0.0, "e33": 1.0}, "m_DefaultValue": {"e00": 1.0, "e01": 0.0, "e02": 0.0, "e03": 0.0, "e10": 0.0, "e11": 1.0, "e12": 0.0, "e13": 0.0, "e20": 0.0, "e21": 0.0, "e22": 1.0, "e23": 0.0, "e30": 0.0, "e31": 0.0, "e32": 0.0, "e33": 1.0}},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.DynamicValueMaterialSlot", "m_ObjectId": s_mul_out, "m_Id": 2, "m_DisplayName": "Out", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "Out", "m_StageCapability": 3, "m_Value": {"e00": 0.0, "e01": 0.0, "e02": 0.0, "e03": 0.0, "e10": 0.0, "e11": 0.0, "e12": 0.0, "e13": 0.0, "e20": 0.0, "e21": 0.0, "e22": 0.0, "e23": 0.0, "e30": 0.0, "e31": 0.0, "e32": 0.0, "e33": 0.0}, "m_DefaultValue": {"e00": 0.0, "e01": 0.0, "e02": 0.0, "e03": 0.0, "e10": 0.0, "e11": 0.0, "e12": 0.0, "e13": 0.0, "e20": 0.0, "e21": 0.0, "e22": 0.0, "e23": 0.0, "e30": 0.0, "e31": 0.0, "e32": 0.0, "e33": 0.0}},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.MultiplyNode", "m_ObjectId": n_mul, "m_Group": {"m_Id": ""}, "m_Name": "Multiply", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -460, "y": 160, "width": 208.0, "height": 302.0}}, "m_Slots": [{"m_Id": s_mul_a}, {"m_Id": s_mul_b}, {"m_Id": s_mul_out}], "synonyms": ["multiplication", "times", "x"], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}},
])

parts.extend([
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector3MaterialSlot", "m_ObjectId": s_norm_out, "m_Id": 0, "m_DisplayName": "Out", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "Out", "m_StageCapability": 3, "m_Value": {"x": 0, "y": 0, "z": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0}, "m_Labels": []},
    {"m_SGVersion": 1, "m_Type": "UnityEditor.ShaderGraph.NormalVectorNode", "m_ObjectId": n_norm_ws, "m_Group": {"m_Id": ""}, "m_Name": "Normal Vector", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -760, "y": 520, "width": 206, "height": 130}}, "m_Slots": [{"m_Id": s_norm_out}], "synonyms": ["surface direction"], "m_Precision": 0, "m_PreviewExpanded": False, "m_DismissedVersion": 0, "m_PreviewMode": 2, "m_CustomColors": {"m_SerializableColors": []}, "m_Space": 2},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector3MaterialSlot", "m_ObjectId": s_view_out, "m_Id": 0, "m_DisplayName": "Out", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "Out", "m_StageCapability": 3, "m_Value": {"x": 0, "y": 0, "z": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0}, "m_Labels": []},
    {"m_SGVersion": 1, "m_Type": "UnityEditor.ShaderGraph.ViewDirectionNode", "m_ObjectId": n_view_ws, "m_Group": {"m_Id": ""}, "m_Name": "View Direction", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -760, "y": 680, "width": 206, "height": 130}}, "m_Slots": [{"m_Id": s_view_out}], "synonyms": ["eye direction"], "m_Precision": 0, "m_PreviewExpanded": False, "m_DismissedVersion": 0, "m_PreviewMode": 2, "m_CustomColors": {"m_SerializableColors": []}, "m_Space": 2},
])

cf_slots = [
    (s_cf_base, 0, "BaseColor", "vec4", 0),
    (s_cf_norm, 1, "WorldNormal", "vec3", 0),
    (s_cf_view, 2, "ViewDir", "vec3", 0),
    (s_cf_shadow_t, 3, "ShadowThreshold", "float", 0),
    (s_cf_shadow_s, 4, "ShadowSoftness", "float", 0),
    (s_cf_shadow_k, 5, "ShadowStrength", "float", 0),
    (s_cf_hi_t, 6, "HighlightThreshold", "float", 0),
    (s_cf_hi_s, 7, "HighlightSoftness", "float", 0),
    (s_cf_hi_i, 8, "HighlightIntensity", "float", 0),
    (s_cf_shadow_tint, 9, "ShadowTint", "vec4", 0),
    (s_cf_highlight_tint, 10, "HighlightTint", "vec4", 0),
    (s_cf_out_base, 11, "OutBaseColor", "vec3", 1),
    (s_cf_out_emi, 12, "OutEmission", "vec3", 1),
]

for slot_id, idx, name, kind, slot_type in cf_slots:
    if kind == "vec4":
        parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector4MaterialSlot", "m_ObjectId": slot_id, "m_Id": idx, "m_DisplayName": name, "m_SlotType": slot_type, "m_Hidden": False, "m_ShaderOutputName": name, "m_StageCapability": 3, "m_Value": {"x": 0, "y": 0, "z": 0, "w": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0, "w": 0}, "m_Labels": []})
    elif kind == "vec3":
        parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector3MaterialSlot", "m_ObjectId": slot_id, "m_Id": idx, "m_DisplayName": name, "m_SlotType": slot_type, "m_Hidden": False, "m_ShaderOutputName": name, "m_StageCapability": 3, "m_Value": {"x": 0, "y": 0, "z": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0}, "m_Labels": []})
    else:
        parts.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot", "m_ObjectId": slot_id, "m_Id": idx, "m_DisplayName": name, "m_SlotType": slot_type, "m_Hidden": False, "m_ShaderOutputName": name, "m_StageCapability": 3, "m_Value": 0.0, "m_DefaultValue": 0.0, "m_Labels": [], "m_LiteralMode": False})

parts.append({
    "m_SGVersion": 1,
    "m_Type": "UnityEditor.ShaderGraph.CustomFunctionNode",
    "m_ObjectId": n_cf,
    "m_Group": {"m_Id": ""},
    "m_Name": "ToonFrontBands (Custom Function)",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -220, "y": 520, "width": 320, "height": 300}},
    "m_Slots": [{"m_Id": s[0]} for s in cf_slots],
    "synonyms": ["code", "HLSL"],
    "m_Precision": 0,
    "m_PreviewExpanded": False,
    "m_DismissedVersion": 0,
    "m_PreviewMode": 0,
    "m_CustomColors": {"m_SerializableColors": []},
    "m_SourceType": 0,
    "m_FunctionName": "ToonFrontBands",
    "m_FunctionSource": HLSL_GUID,
    "m_FunctionSourceUsePragmas": True,
    "m_FunctionBody": "",
})

# Targets
parts.append({"m_SGVersion": 2, "m_Type": "UnityEditor.Rendering.Universal.ShaderGraph.UniversalLitSubTarget", "m_ObjectId": sublit_id, "m_WorkflowMode": 1, "m_NormalDropOffSpace": 0, "m_ClearCoat": False, "m_BlendModePreserveSpecular": True})
parts.append({
    "m_SGVersion": 1,
    "m_Type": "UnityEditor.Rendering.Universal.ShaderGraph.UniversalTarget",
    "m_ObjectId": target_id,
    "m_Datas": [],
    "m_ActiveSubTarget": {"m_Id": sublit_id},
    "m_AllowMaterialOverride": True,
    "m_SurfaceType": 0,
    "m_ZTestMode": 4,
    "m_ZWriteControl": 0,
    "m_AlphaMode": 0,
    "m_RenderFace": 1,
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

import json
import uuid


def nid():
    return uuid.uuid4().hex


OUT_PATH = __file__.replace("_build_toon_base_front_sg.py", "ToonBaseFrontURP.shadergraph")

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


def tex2d_prop(oid, name, ref):
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
        "m_Value": {
            "m_SerializedTexture": '{"texture":{"fileID":0}}',
            "m_Guid": "",
        },
        "isMainTexture": True,
        "useTilingAndOffset": False,
        "useTexelSize": False,
        "isHDR": False,
        "m_Modifiable": True,
        "m_DefaultType": 0,
    }


pid_base = nid()
pid_tex = nid()
cat_id = nid()
graph_id = nid()
tgt_id = nid()
subunlit_id = nid()

n_vpos, n_vnorm, n_vtan = nid(), nid(), nid()
n_fbase, n_femi, n_falpha = nid(), nid(), nid()
n_uv = nid()
n_ptex = nid()
n_pbase = nid()
n_sample = nid()
n_mul = nid()

s_vpos, s_vnorm, s_vtan = nid(), nid(), nid()
s_fbase, s_femi, s_falpha = nid(), nid(), nid()
s_uv_out = nid()

s_ptex_out = nid()
s_pbase_out = nid()

s_sample_rgba = nid()
s_sample_texture = nid()
s_sample_sampler = nid()
s_sample_uv = nid()
s_sample_r = nid()
s_sample_g = nid()
s_sample_b = nid()
s_sample_a = nid()

s_mul_a = nid()
s_mul_b = nid()
s_mul_out = nid()

edges = [
    {
        "m_OutputSlot": {"m_Node": {"m_Id": n_ptex}, "m_SlotId": 0},
        "m_InputSlot": {"m_Node": {"m_Id": n_sample}, "m_SlotId": 1},
    },
    {
        "m_OutputSlot": {"m_Node": {"m_Id": n_uv}, "m_SlotId": 0},
        "m_InputSlot": {"m_Node": {"m_Id": n_sample}, "m_SlotId": 2},
    },
    {
        "m_OutputSlot": {"m_Node": {"m_Id": n_sample}, "m_SlotId": 0},
        "m_InputSlot": {"m_Node": {"m_Id": n_mul}, "m_SlotId": 0},
    },
    {
        "m_OutputSlot": {"m_Node": {"m_Id": n_pbase}, "m_SlotId": 0},
        "m_InputSlot": {"m_Node": {"m_Id": n_mul}, "m_SlotId": 1},
    },
    {
        "m_OutputSlot": {"m_Node": {"m_Id": n_mul}, "m_SlotId": 2},
        "m_InputSlot": {"m_Node": {"m_Id": n_fbase}, "m_SlotId": 0},
    },
    {
        "m_OutputSlot": {"m_Node": {"m_Id": n_sample}, "m_SlotId": 7},
        "m_InputSlot": {"m_Node": {"m_Id": n_falpha}, "m_SlotId": 0},
    },
]

parts.append({
    "m_SGVersion": 3,
    "m_Type": "UnityEditor.ShaderGraph.GraphData",
    "m_ObjectId": graph_id,
    "m_Properties": [{"m_Id": pid_base}, {"m_Id": pid_tex}],
    "m_Keywords": [],
    "m_Dropdowns": [],
    "m_CategoryData": [{"m_Id": cat_id}],
    "m_Nodes": [{"m_Id": x} for x in [
        n_vpos, n_vnorm, n_vtan, n_fbase, n_femi, n_falpha,
        n_uv, n_ptex, n_pbase, n_sample, n_mul
    ]],
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

parts.append({
    "m_SGVersion": 0,
    "m_Type": "UnityEditor.ShaderGraph.CategoryData",
    "m_ObjectId": cat_id,
    "m_Name": "",
    "m_ChildObjectList": [{"m_Id": pid_base}, {"m_Id": pid_tex}],
})

parts.append(color_prop(pid_base, "Base Color", "_BaseColor", (1.0, 1.0, 1.0, 1.0)))
parts.append(tex2d_prop(pid_tex, "Main Tex", "_MainTex"))

# Vertex block nodes
parts.extend([
    {
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.PositionMaterialSlot", "m_ObjectId": s_vpos,
        "m_Id": 0, "m_DisplayName": "Position", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Position",
        "m_StageCapability": 1, "m_Value": {"x": 0, "y": 0, "z": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0},
        "m_Labels": [], "m_Space": 0
    },
    {
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.BlockNode", "m_ObjectId": n_vpos, "m_Group": {"m_Id": ""},
        "m_Name": "VertexDescription.Position",
        "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}},
        "m_Slots": [{"m_Id": s_vpos}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0,
        "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_SerializedDescriptor": "VertexDescription.Position"
    },
    {
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.NormalMaterialSlot", "m_ObjectId": s_vnorm,
        "m_Id": 0, "m_DisplayName": "Normal", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Normal",
        "m_StageCapability": 1, "m_Value": {"x": 0, "y": 0, "z": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0},
        "m_Labels": [], "m_Space": 0
    },
    {
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.BlockNode", "m_ObjectId": n_vnorm, "m_Group": {"m_Id": ""},
        "m_Name": "VertexDescription.Normal",
        "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}},
        "m_Slots": [{"m_Id": s_vnorm}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0,
        "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_SerializedDescriptor": "VertexDescription.Normal"
    },
    {
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.TangentMaterialSlot", "m_ObjectId": s_vtan,
        "m_Id": 0, "m_DisplayName": "Tangent", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Tangent",
        "m_StageCapability": 1, "m_Value": {"x": 0, "y": 0, "z": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0},
        "m_Labels": [], "m_Space": 0
    },
    {
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.BlockNode", "m_ObjectId": n_vtan, "m_Group": {"m_Id": ""},
        "m_Name": "VertexDescription.Tangent",
        "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}},
        "m_Slots": [{"m_Id": s_vtan}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0,
        "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_SerializedDescriptor": "VertexDescription.Tangent"
    },
])

# Fragment block nodes
parts.extend([
    {
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.ColorRGBMaterialSlot", "m_ObjectId": s_fbase,
        "m_Id": 0, "m_DisplayName": "Base Color", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "BaseColor",
        "m_StageCapability": 2, "m_Value": {"x": 0.5, "y": 0.5, "z": 0.5}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0},
        "m_Labels": [], "m_ColorMode": 0, "m_DefaultColor": {"r": 0.5, "g": 0.5, "b": 0.5, "a": 1}
    },
    {
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.BlockNode", "m_ObjectId": n_fbase, "m_Group": {"m_Id": ""},
        "m_Name": "SurfaceDescription.BaseColor",
        "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}},
        "m_Slots": [{"m_Id": s_fbase}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0,
        "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_SerializedDescriptor": "SurfaceDescription.BaseColor"
    },
    {
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.ColorRGBMaterialSlot", "m_ObjectId": s_femi,
        "m_Id": 0, "m_DisplayName": "Emission", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Emission",
        "m_StageCapability": 2, "m_Value": {"x": 0, "y": 0, "z": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0},
        "m_Labels": [], "m_ColorMode": 1, "m_DefaultColor": {"r": 0, "g": 0, "b": 0, "a": 1}
    },
    {
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.BlockNode", "m_ObjectId": n_femi, "m_Group": {"m_Id": ""},
        "m_Name": "SurfaceDescription.Emission",
        "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}},
        "m_Slots": [{"m_Id": s_femi}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0,
        "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_SerializedDescriptor": "SurfaceDescription.Emission"
    },
    {
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot", "m_ObjectId": s_falpha,
        "m_Id": 0, "m_DisplayName": "Alpha", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Alpha",
        "m_StageCapability": 2, "m_Value": 1.0, "m_DefaultValue": 1.0, "m_Labels": []
    },
    {
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.BlockNode", "m_ObjectId": n_falpha, "m_Group": {"m_Id": ""},
        "m_Name": "SurfaceDescription.Alpha",
        "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}},
        "m_Slots": [{"m_Id": s_falpha}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0,
        "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_SerializedDescriptor": "SurfaceDescription.Alpha"
    },
])

# UV node + output slot
parts.append({
    "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector4MaterialSlot", "m_ObjectId": s_uv_out,
    "m_Id": 0, "m_DisplayName": "Out", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "Out",
    "m_StageCapability": 3, "m_Value": {"x": 0, "y": 0, "z": 0, "w": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0, "w": 0},
    "m_Labels": []
})
parts.append({
    "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.UVNode", "m_ObjectId": n_uv, "m_Group": {"m_Id": ""},
    "m_Name": "UV",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -900, "y": 200, "width": 145, "height": 128}},
    "m_Slots": [{"m_Id": s_uv_out}], "synonyms": ["texcoords", "coords"], "m_Precision": 0,
    "m_PreviewExpanded": False, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []},
    "m_OutputChannel": 0
})

# Property nodes and outputs
parts.append({
    "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Texture2DMaterialSlot", "m_ObjectId": s_ptex_out,
    "m_Id": 0, "m_DisplayName": "Main Tex", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "Out",
    "m_StageCapability": 3, "m_BareResource": False
})
parts.append({
    "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.PropertyNode", "m_ObjectId": n_ptex, "m_Group": {"m_Id": ""},
    "m_Name": "Property",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -900, "y": 0, "width": 160, "height": 34}},
    "m_Slots": [{"m_Id": s_ptex_out}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0,
    "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_Property": {"m_Id": pid_tex}
})

parts.append({
    "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector4MaterialSlot", "m_ObjectId": s_pbase_out,
    "m_Id": 0, "m_DisplayName": "Out", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "Out",
    "m_StageCapability": 3, "m_Value": {"x": 1, "y": 1, "z": 1, "w": 1}, "m_DefaultValue": {"x": 1, "y": 1, "z": 1, "w": 1},
    "m_Labels": []
})
parts.append({
    "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.PropertyNode", "m_ObjectId": n_pbase, "m_Group": {"m_Id": ""},
    "m_Name": "Property",
    "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -900, "y": 320, "width": 160, "height": 34}},
    "m_Slots": [{"m_Id": s_pbase_out}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0,
    "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_Property": {"m_Id": pid_base}
})

# Sample texture node and slots
parts.extend([
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector4MaterialSlot", "m_ObjectId": s_sample_rgba, "m_Id": 0, "m_DisplayName": "RGBA", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "RGBA", "m_StageCapability": 2, "m_Value": {"x": 0, "y": 0, "z": 0, "w": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0, "w": 0}, "m_Labels": []},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Texture2DInputMaterialSlot", "m_ObjectId": s_sample_texture, "m_Id": 1, "m_DisplayName": "Texture", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Texture", "m_StageCapability": 3, "m_BareResource": False, "m_Texture": {"m_SerializedTexture": '{"texture":{"fileID":0}}', "m_Guid": ""}, "m_DefaultType": 0},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.UVMaterialSlot", "m_ObjectId": s_sample_uv, "m_Id": 2, "m_DisplayName": "UV", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "UV", "m_StageCapability": 3, "m_Value": {"x": 0.0, "y": 0.0}, "m_DefaultValue": {"x": 0.0, "y": 0.0}, "m_Labels": [], "m_Channel": 0},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.SamplerStateMaterialSlot", "m_ObjectId": s_sample_sampler, "m_Id": 3, "m_DisplayName": "Sampler", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Sampler", "m_StageCapability": 3, "m_BareResource": False},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot", "m_ObjectId": s_sample_r, "m_Id": 4, "m_DisplayName": "R", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "R", "m_StageCapability": 2, "m_Value": 0.0, "m_DefaultValue": 0.0, "m_Labels": [], "m_LiteralMode": False},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot", "m_ObjectId": s_sample_g, "m_Id": 5, "m_DisplayName": "G", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "G", "m_StageCapability": 2, "m_Value": 0.0, "m_DefaultValue": 0.0, "m_Labels": [], "m_LiteralMode": False},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot", "m_ObjectId": s_sample_b, "m_Id": 6, "m_DisplayName": "B", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "B", "m_StageCapability": 2, "m_Value": 0.0, "m_DefaultValue": 0.0, "m_Labels": [], "m_LiteralMode": False},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot", "m_ObjectId": s_sample_a, "m_Id": 7, "m_DisplayName": "A", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "A", "m_StageCapability": 2, "m_Value": 0.0, "m_DefaultValue": 0.0, "m_Labels": [], "m_LiteralMode": False},
    {
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.SampleTexture2DNode", "m_ObjectId": n_sample, "m_Group": {"m_Id": ""},
        "m_Name": "Sample Texture 2D",
        "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -620, "y": 120, "width": 208.0, "height": 433.0}},
        "m_Slots": [{"m_Id": s_sample_rgba}, {"m_Id": s_sample_texture}, {"m_Id": s_sample_uv}, {"m_Id": s_sample_sampler}, {"m_Id": s_sample_r}, {"m_Id": s_sample_g}, {"m_Id": s_sample_b}, {"m_Id": s_sample_a}],
        "synonyms": ["tex2d"], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0, "m_PreviewMode": 0,
        "m_CustomColors": {"m_SerializableColors": []}, "m_TextureType": 0, "m_NormalMapSpace": 0, "m_EnableGlobalMipBias": True, "m_MipSamplingMode": 0
    },
])

# Multiply node + slots
parts.extend([
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.DynamicValueMaterialSlot", "m_ObjectId": s_mul_a, "m_Id": 0, "m_DisplayName": "A", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "A", "m_StageCapability": 3, "m_Value": {"e00": 1.0, "e01": 0.0, "e02": 0.0, "e03": 0.0, "e10": 0.0, "e11": 1.0, "e12": 0.0, "e13": 0.0, "e20": 0.0, "e21": 0.0, "e22": 1.0, "e23": 0.0, "e30": 0.0, "e31": 0.0, "e32": 0.0, "e33": 1.0}, "m_DefaultValue": {"e00": 1.0, "e01": 0.0, "e02": 0.0, "e03": 0.0, "e10": 0.0, "e11": 1.0, "e12": 0.0, "e13": 0.0, "e20": 0.0, "e21": 0.0, "e22": 1.0, "e23": 0.0, "e30": 0.0, "e31": 0.0, "e32": 0.0, "e33": 1.0}},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.DynamicValueMaterialSlot", "m_ObjectId": s_mul_b, "m_Id": 1, "m_DisplayName": "B", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "B", "m_StageCapability": 3, "m_Value": {"e00": 1.0, "e01": 0.0, "e02": 0.0, "e03": 0.0, "e10": 0.0, "e11": 1.0, "e12": 0.0, "e13": 0.0, "e20": 0.0, "e21": 0.0, "e22": 1.0, "e23": 0.0, "e30": 0.0, "e31": 0.0, "e32": 0.0, "e33": 1.0}, "m_DefaultValue": {"e00": 1.0, "e01": 0.0, "e02": 0.0, "e03": 0.0, "e10": 0.0, "e11": 1.0, "e12": 0.0, "e13": 0.0, "e20": 0.0, "e21": 0.0, "e22": 1.0, "e23": 0.0, "e30": 0.0, "e31": 0.0, "e32": 0.0, "e33": 1.0}},
    {"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.DynamicValueMaterialSlot", "m_ObjectId": s_mul_out, "m_Id": 2, "m_DisplayName": "Out", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "Out", "m_StageCapability": 3, "m_Value": {"e00": 0.0, "e01": 0.0, "e02": 0.0, "e03": 0.0, "e10": 0.0, "e11": 0.0, "e12": 0.0, "e13": 0.0, "e20": 0.0, "e21": 0.0, "e22": 0.0, "e23": 0.0, "e30": 0.0, "e31": 0.0, "e32": 0.0, "e33": 0.0}, "m_DefaultValue": {"e00": 0.0, "e01": 0.0, "e02": 0.0, "e03": 0.0, "e10": 0.0, "e11": 0.0, "e12": 0.0, "e13": 0.0, "e20": 0.0, "e21": 0.0, "e22": 0.0, "e23": 0.0, "e30": 0.0, "e31": 0.0, "e32": 0.0, "e33": 0.0}},
    {
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.MultiplyNode", "m_ObjectId": n_mul, "m_Group": {"m_Id": ""},
        "m_Name": "Multiply",
        "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -300, "y": 220, "width": 208.0, "height": 302.0}},
        "m_Slots": [{"m_Id": s_mul_a}, {"m_Id": s_mul_b}, {"m_Id": s_mul_out}],
        "synonyms": ["multiplication", "times", "x"], "m_Precision": 0, "m_PreviewExpanded": True, "m_DismissedVersion": 0,
        "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}
    },
])

# Target
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

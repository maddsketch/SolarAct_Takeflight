# One-off: duplicate ToonBaseFrontURPnew.shadergraph and insert scroll UV (Tiling+Offset, Time, Multiply).
# Run from repo root: python Assets/Shaders/Graphs/build_toon_front_new_scroll.py

import json
import uuid
import re
from copy import deepcopy
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
SRC = ROOT / "Assets/Shaders/Graphs/ToonBaseFrontURPnew.shadergraph"
OUT = ROOT / "Assets/Shaders/Graphs/ToonBaseFrontURPnew_Scroll.shadergraph"

UV_NODE = "c42d6eeec3ab4fa5baaa0369ad4e6411"
SAMPLE_NODE = "a619b0e0abf34e37b9dc109b0f20353e"
CATEGORY_ID = "7fd8bba17d8044019fd9e8e10d69f827"


def nid():
    return uuid.uuid4().hex


def load_objects(path: Path) -> list:
    dec = json.JSONDecoder()
    text = path.read_text(encoding="utf-8")
    idx = 0
    objs = []
    while idx < len(text):
        while idx < len(text) and text[idx].isspace():
            idx += 1
        if idx >= len(text):
            break
        obj, end = dec.raw_decode(text, idx)
        objs.append(obj)
        idx = end
    return objs


def save_objects(path: Path, objs: list) -> None:
    parts = [json.dumps(o, indent=4) for o in objs]
    path.write_text("\n\n".join(parts) + "\n", encoding="utf-8")


def zero_matrix():
    return {f"e{i}{j}": 0.0 for i in range(4) for j in range(4)}


def id_matrix():
    m = zero_matrix()
    m["e00"] = m["e11"] = m["e22"] = m["e33"] = 1.0
    return m


def main():
    objs = load_objects(SRC)
    # New IDs
    p_speed, p_tiling = nid(), nid()
    n_time, n_mul, n_til = nid(), nid(), nid()
    pn_sp, pn_tl = nid(), nid()
    # Time slots (5)
    t_s0, t_s1, t_s2, t_s3, t_s4 = nid(), nid(), nid(), nid(), nid()
    # Multiply slots
    m_a, m_b, m_out = nid(), nid(), nid()
    # TilingAndOffset slots
    z_uv, z_tile, z_off, z_out = nid(), nid(), nid(), nid()
    # Property node out slots
    s_sp, s_tl = nid(), nid()

    new_edges = [
        {
            "m_OutputSlot": {"m_Node": {"m_Id": UV_NODE}, "m_SlotId": 0},
            "m_InputSlot": {"m_Node": {"m_Id": n_til}, "m_SlotId": 0},
        },
        {
            "m_OutputSlot": {"m_Node": {"m_Id": pn_tl}, "m_SlotId": 0},
            "m_InputSlot": {"m_Node": {"m_Id": n_til}, "m_SlotId": 1},
        },
        {
            "m_OutputSlot": {"m_Node": {"m_Id": n_mul}, "m_SlotId": 2},
            "m_InputSlot": {"m_Node": {"m_Id": n_til}, "m_SlotId": 2},
        },
        {
            "m_OutputSlot": {"m_Node": {"m_Id": n_til}, "m_SlotId": 3},
            "m_InputSlot": {"m_Node": {"m_Id": SAMPLE_NODE}, "m_SlotId": 2},
        },
        {
            "m_OutputSlot": {"m_Node": {"m_Id": pn_sp}, "m_SlotId": 0},
            "m_InputSlot": {"m_Node": {"m_Id": n_mul}, "m_SlotId": 0},
        },
        {
            "m_OutputSlot": {"m_Node": {"m_Id": n_time}, "m_SlotId": 0},
            "m_InputSlot": {"m_Node": {"m_Id": n_mul}, "m_SlotId": 1},
        },
    ]

    block = {
        "m_OutputSlot": {"m_Node": {"m_Id": UV_NODE}, "m_SlotId": 0},
        "m_InputSlot": {"m_Node": {"m_Id": SAMPLE_NODE}, "m_SlotId": 2},
    }

    for o in objs:
        if o.get("m_Type") == "UnityEditor.ShaderGraph.GraphData":
            o["m_Properties"].append({"m_Id": p_speed})
            o["m_Properties"].append({"m_Id": p_tiling})
            o["m_Nodes"].extend(
                [
                    {"m_Id": n_time},
                    {"m_Id": n_mul},
                    {"m_Id": n_til},
                    {"m_Id": pn_sp},
                    {"m_Id": pn_tl},
                ]
            )
            new_e = []
            for e in o["m_Edges"]:
                if e == block or (
                    e.get("m_OutputSlot", {}).get("m_Node", {}).get("m_Id") == UV_NODE
                    and e.get("m_OutputSlot", {}).get("m_SlotId") == 0
                    and e.get("m_InputSlot", {}).get("m_Node", {}).get("m_Id") == SAMPLE_NODE
                    and e.get("m_InputSlot", {}).get("m_SlotId") == 2
                ):
                    continue
                new_e.append(e)
            o["m_Edges"] = new_e + new_edges
        elif o.get("m_Type") == "UnityEditor.ShaderGraph.CategoryData" and o.get("m_ObjectId") == CATEGORY_ID:
            o["m_ChildObjectList"].append({"m_Id": p_speed})
            o["m_ChildObjectList"].append({"m_Id": p_tiling})

    def guid_s():
        return str(uuid.uuid4())

    new_blocks = [
        {
            "m_SGVersion": 1,
            "m_Type": "UnityEditor.ShaderGraph.Internal.Vector2ShaderProperty",
            "m_ObjectId": p_speed,
            "m_Guid": {"m_GuidSerialized": guid_s()},
            "m_Name": "Scroll Speed",
            "m_DefaultRefNameVersion": 1,
            "m_RefNameGeneratedByDisplayName": "Scroll Speed",
            "m_DefaultReferenceName": "_ScrollSpeed",
            "m_OverrideReferenceName": "_ScrollSpeed",
            "m_GeneratePropertyBlock": True,
            "m_UseCustomSlotLabel": False,
            "m_CustomSlotLabel": "",
            "m_DismissedVersion": 0,
            "m_Precision": 0,
            "overrideHLSLDeclaration": False,
            "hlslDeclarationOverride": 0,
            "m_Hidden": False,
            "m_Value": {"x": 0.05, "y": 0.0, "z": 0.0, "w": 0.0},
        },
        {
            "m_SGVersion": 1,
            "m_Type": "UnityEditor.ShaderGraph.Internal.Vector2ShaderProperty",
            "m_ObjectId": p_tiling,
            "m_Guid": {"m_GuidSerialized": guid_s()},
            "m_Name": "Scroll Tiling",
            "m_DefaultRefNameVersion": 1,
            "m_RefNameGeneratedByDisplayName": "Scroll Tiling",
            "m_DefaultReferenceName": "_ScrollTiling",
            "m_OverrideReferenceName": "_ScrollTiling",
            "m_GeneratePropertyBlock": True,
            "m_UseCustomSlotLabel": False,
            "m_CustomSlotLabel": "",
            "m_DismissedVersion": 0,
            "m_Precision": 0,
            "overrideHLSLDeclaration": False,
            "hlslDeclarationOverride": 0,
            "m_Hidden": False,
            "m_Value": {"x": 1.0, "y": 1.0, "z": 0.0, "w": 0.0},
        },
        {
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.TimeNode",
            "m_ObjectId": n_time,
            "m_Group": {"m_Id": ""},
            "m_Name": "Time",
            "m_DrawState": {
                "m_Expanded": False,
                "m_Position": {
                    "serializedVersion": "2",
                    "x": -1180.0,
                    "y": -80.0,
                    "width": 79.0,
                    "height": 76.0,
                },
            },
            "m_Slots": [
                {"m_Id": t_s0},
                {"m_Id": t_s1},
                {"m_Id": t_s2},
                {"m_Id": t_s3},
                {"m_Id": t_s4},
            ],
            "synonyms": [],
            "m_Precision": 0,
            "m_PreviewExpanded": True,
            "m_DismissedVersion": 0,
            "m_PreviewMode": 0,
            "m_CustomColors": {"m_SerializableColors": []},
        },
        {
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot",
            "m_ObjectId": t_s0,
            "m_Id": 0,
            "m_DisplayName": "Time",
            "m_SlotType": 1,
            "m_Hidden": False,
            "m_ShaderOutputName": "Time",
            "m_StageCapability": 3,
            "m_Value": 0.0,
            "m_DefaultValue": 0.0,
            "m_Labels": [],
            "m_LiteralMode": False,
        },
        {
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot",
            "m_ObjectId": t_s1,
            "m_Id": 1,
            "m_DisplayName": "Sine Time",
            "m_SlotType": 1,
            "m_Hidden": False,
            "m_ShaderOutputName": "Sine Time",
            "m_StageCapability": 3,
            "m_Value": 0.0,
            "m_DefaultValue": 0.0,
            "m_Labels": [],
            "m_LiteralMode": False,
        },
        {
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot",
            "m_ObjectId": t_s2,
            "m_Id": 2,
            "m_DisplayName": "Cosine Time",
            "m_SlotType": 1,
            "m_Hidden": False,
            "m_ShaderOutputName": "Cosine Time",
            "m_StageCapability": 3,
            "m_Value": 0.0,
            "m_DefaultValue": 0.0,
            "m_Labels": [],
            "m_LiteralMode": False,
        },
        {
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot",
            "m_ObjectId": t_s3,
            "m_Id": 3,
            "m_DisplayName": "Delta Time",
            "m_SlotType": 1,
            "m_Hidden": False,
            "m_ShaderOutputName": "Delta Time",
            "m_StageCapability": 3,
            "m_Value": 0.0,
            "m_DefaultValue": 0.0,
            "m_Labels": [],
            "m_LiteralMode": False,
        },
        {
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot",
            "m_ObjectId": t_s4,
            "m_Id": 4,
            "m_DisplayName": "Smooth Delta",
            "m_SlotType": 1,
            "m_Hidden": False,
            "m_ShaderOutputName": "Smooth Delta",
            "m_StageCapability": 3,
            "m_Value": 0.0,
            "m_DefaultValue": 0.0,
            "m_Labels": [],
            "m_LiteralMode": False,
        },
        {
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.MultiplyNode",
            "m_ObjectId": n_mul,
            "m_Group": {"m_Id": ""},
            "m_Name": "Multiply",
            "m_DrawState": {
                "m_Expanded": True,
                "m_Position": {
                    "serializedVersion": "2",
                    "x": -1000.0,
                    "y": -100.0,
                    "width": 208.0,
                    "height": 302.0,
                },
            },
            "m_Slots": [{"m_Id": m_a}, {"m_Id": m_b}, {"m_Id": m_out}],
            "synonyms": ["multiplication", "times", "x"],
            "m_Precision": 0,
            "m_PreviewExpanded": True,
            "m_DismissedVersion": 0,
            "m_PreviewMode": 0,
            "m_CustomColors": {"m_SerializableColors": []},
        },
        {
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.DynamicValueMaterialSlot",
            "m_ObjectId": m_a,
            "m_Id": 0,
            "m_DisplayName": "A",
            "m_SlotType": 0,
            "m_Hidden": False,
            "m_ShaderOutputName": "A",
            "m_StageCapability": 3,
            "m_Value": zero_matrix(),
            "m_DefaultValue": id_matrix(),
        },
        {
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.DynamicValueMaterialSlot",
            "m_ObjectId": m_b,
            "m_Id": 1,
            "m_DisplayName": "B",
            "m_SlotType": 0,
            "m_Hidden": False,
            "m_ShaderOutputName": "B",
            "m_StageCapability": 3,
            "m_Value": zero_matrix(),
            "m_DefaultValue": id_matrix(),
        },
        {
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.DynamicValueMaterialSlot",
            "m_ObjectId": m_out,
            "m_Id": 2,
            "m_DisplayName": "Out",
            "m_SlotType": 1,
            "m_Hidden": False,
            "m_ShaderOutputName": "Out",
            "m_StageCapability": 3,
            "m_Value": zero_matrix(),
            "m_DefaultValue": id_matrix(),
        },
        {
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.TilingAndOffsetNode",
            "m_ObjectId": n_til,
            "m_Group": {"m_Id": ""},
            "m_Name": "Tiling And Offset",
            "m_DrawState": {
                "m_Expanded": True,
                "m_Position": {
                    "serializedVersion": "2",
                    "x": -900.0,
                    "y": 40.0,
                    "width": 154.0,
                    "height": 142.0,
                },
            },
            "m_Slots": [{"m_Id": z_uv}, {"m_Id": z_tile}, {"m_Id": z_off}, {"m_Id": z_out}],
            "synonyms": ["pan", "scale"],
            "m_Precision": 0,
            "m_PreviewExpanded": False,
            "m_DismissedVersion": 0,
            "m_PreviewMode": 0,
            "m_CustomColors": {"m_SerializableColors": []},
        },
        {
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.UVMaterialSlot",
            "m_ObjectId": z_uv,
            "m_Id": 0,
            "m_DisplayName": "UV",
            "m_SlotType": 0,
            "m_Hidden": False,
            "m_ShaderOutputName": "UV",
            "m_StageCapability": 3,
            "m_Value": {"x": 0.0, "y": 0.0},
            "m_DefaultValue": {"x": 0.0, "y": 0.0},
            "m_Labels": [],
            "m_Channel": 0,
        },
        {
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.Vector2MaterialSlot",
            "m_ObjectId": z_tile,
            "m_Id": 1,
            "m_DisplayName": "Tiling",
            "m_SlotType": 0,
            "m_Hidden": False,
            "m_ShaderOutputName": "Tiling",
            "m_StageCapability": 3,
            "m_Value": {"x": 1.0, "y": 1.0},
            "m_DefaultValue": {"x": 0.0, "y": 0.0},
            "m_Labels": [],
        },
        {
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.Vector2MaterialSlot",
            "m_ObjectId": z_off,
            "m_Id": 2,
            "m_DisplayName": "Offset",
            "m_SlotType": 0,
            "m_Hidden": False,
            "m_ShaderOutputName": "Offset",
            "m_StageCapability": 3,
            "m_Value": {"x": 0.0, "y": 0.0},
            "m_DefaultValue": {"x": 0.0, "y": 0.0},
            "m_Labels": [],
        },
        {
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.Vector2MaterialSlot",
            "m_ObjectId": z_out,
            "m_Id": 3,
            "m_DisplayName": "Out",
            "m_SlotType": 1,
            "m_Hidden": False,
            "m_ShaderOutputName": "Out",
            "m_StageCapability": 3,
            "m_Value": {"x": 0.0, "y": 0.0},
            "m_DefaultValue": {"x": 0.0, "y": 0.0},
            "m_Labels": [],
        },
        {
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.PropertyNode",
            "m_ObjectId": pn_sp,
            "m_Group": {"m_Id": ""},
            "m_Name": "Property",
            "m_DrawState": {
                "m_Expanded": True,
                "m_Position": {
                    "serializedVersion": "2",
                    "x": -1280.0,
                    "y": -160.0,
                    "width": 160.0,
                    "height": 34.0,
                },
            },
            "m_Slots": [{"m_Id": s_sp}],
            "synonyms": [],
            "m_Precision": 0,
            "m_PreviewExpanded": True,
            "m_DismissedVersion": 0,
            "m_PreviewMode": 0,
            "m_CustomColors": {"m_SerializableColors": []},
            "m_Property": {"m_Id": p_speed},
        },
        {
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.Vector2MaterialSlot",
            "m_ObjectId": s_sp,
            "m_Id": 0,
            "m_DisplayName": "Scroll Speed",
            "m_SlotType": 1,
            "m_Hidden": False,
            "m_ShaderOutputName": "Out",
            "m_StageCapability": 3,
            "m_Value": {"x": 0.0, "y": 0.0},
            "m_DefaultValue": {"x": 0.0, "y": 0.0},
            "m_Labels": [],
        },
        {
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.PropertyNode",
            "m_ObjectId": pn_tl,
            "m_Group": {"m_Id": ""},
            "m_Name": "Property",
            "m_DrawState": {
                "m_Expanded": True,
                "m_Position": {
                    "serializedVersion": "2",
                    "x": -1280.0,
                    "y": 80.0,
                    "width": 160.0,
                    "height": 34.0,
                },
            },
            "m_Slots": [{"m_Id": s_tl}],
            "synonyms": [],
            "m_Precision": 0,
            "m_PreviewExpanded": True,
            "m_DismissedVersion": 0,
            "m_PreviewMode": 0,
            "m_CustomColors": {"m_SerializableColors": []},
            "m_Property": {"m_Id": p_tiling},
        },
        {
            "m_SGVersion": 0,
            "m_Type": "UnityEditor.ShaderGraph.Vector2MaterialSlot",
            "m_ObjectId": s_tl,
            "m_Id": 0,
            "m_DisplayName": "Scroll Tiling",
            "m_SlotType": 1,
            "m_Hidden": False,
            "m_ShaderOutputName": "Out",
            "m_StageCapability": 3,
            "m_Value": {"x": 0.0, "y": 0.0},
            "m_DefaultValue": {"x": 0.0, "y": 0.0},
            "m_Labels": [],
        },
    ]

    objs.extend(new_blocks)
    save_objects(OUT, objs)
    print("Wrote", OUT)


if __name__ == "__main__":
    main()

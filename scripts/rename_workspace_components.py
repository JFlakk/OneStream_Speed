#!/usr/bin/env python3
import re
import csv
import shutil
import sys
from pathlib import Path
import xml.etree.ElementTree as ET

# Config
NAME_ATTRS = ["Name","name","DisplayName","displayName","Caption","caption","Label","label","ComponentName","componentName","UniqueName","uniqueName"]
TYPE_ATTRS = ["Type","type","ControlType","controlType","WidgetType","widgetType"]

ABBREV_MAP = {
    'CHART': 'CH', 'GRID': 'TB', 'TABLE': 'TB', 'TABLEVIEW': 'TB', 'LIST': 'LB', 'FORMULA': 'FX',
    'INDICATOR': 'IN', 'TEXT': 'TX', 'IMAGE': 'IM', 'KPI': 'KP', 'DASHBOARD': 'DB'
}

def abbrev_type(t):
    if not t:
        return 'UNK'
    s = re.sub(r'[^A-Za-z0-9]+','',str(t)).upper()
    for k,v in ABBREV_MAP.items():
        if k in s:
            return v
    return (s[:2] if len(s)>=2 else (s + 'X'))

def build_new_name(ab, counter):
    return f"DC_{ab}_{counter:03d}"

def collect_candidates(root):
    candidates = []  # list of (elem, attr_name, old_value)
    for elem in root.iter():
        for a in NAME_ATTRS:
            if a in elem.attrib:
                val = elem.attrib.get(a)
                if val and val.strip():
                    candidates.append((elem, a, val.strip()))
                    break
    return candidates

def find_type_hint(elem):
    for a in TYPE_ATTRS:
        if a in elem.attrib and elem.attrib.get(a):
            return elem.attrib.get(a)
    # fallback to tag local name
    tag = elem.tag
    if '}' in tag:
        tag = tag.split('}',1)[1]
    return tag


def main():
    if len(sys.argv) < 3:
        print('Usage: rename_workspace_components.py input.xml output.xml')
        sys.exit(2)
    inp = Path(sys.argv[1])
    outp = Path(sys.argv[2])
    if not inp.exists():
        print('Input not found:', inp)
        sys.exit(1)

    # backup
    bak = inp.with_suffix(inp.suffix + '.bak')
    shutil.copyfile(inp, bak)
    print('Backup written to', bak)

    # parse
    parser = ET.XMLParser(encoding='utf-8')
    tree = ET.parse(inp, parser=parser)
    root = tree.getroot()

    candidates = collect_candidates(root)
    print('Found', len(candidates), 'named elements')

    # build mapping
    type_counters = {}
    mappings = {}  # old -> new
    elem_attr_map = {}  # old -> (elem, attr)

    for elem, attr, old in candidates:
        if old in mappings:
            # already mapped
            continue
        t = find_type_hint(elem)
        ab = abbrev_type(t)
        type_counters.setdefault(ab, 0)
        type_counters[ab] += 1
        new = build_new_name(ab, type_counters[ab])
        # ensure uniqueness globally
        while new in mappings.values():
            type_counters[ab] += 1
            new = build_new_name(ab, type_counters[ab])
        mappings[old] = new
        elem_attr_map[old] = (elem, attr)

    print('Prepared', len(mappings), 'renames')

    # preview print first 20
    i=0
    for o,n in mappings.items():
        print(o,'->',n)
        i+=1
        if i>=20:
            break

    # apply primary attribute changes
    for old, new in mappings.items():
        elem, attr = elem_attr_map[old]
        elem.set(attr, new)

    # token replacement pattern
    def token_re(old):
        return re.compile(r'(?<![A-Za-z0-9_])' + re.escape(old) + r'(?![A-Za-z0-9_])')

    # replace in all attribute values
    for elem in root.iter():
        for a in list(elem.attrib.keys()):
            v = elem.attrib.get(a)
            if not v:
                continue
            newv = v
            # exact match
            if v in mappings:
                newv = mappings[v]
            else:
                for old, new in mappings.items():
                    if token_re(old).search(newv):
                        newv = token_re(old).sub(new, newv)
            if newv != v:
                elem.set(a, newv)

    # replace in text nodes if they match exactly or token
    for elem in root.iter():
        if elem.text and elem.text.strip():
            t = elem.text.strip()
            newt = t
            if t in mappings:
                newt = mappings[t]
            else:
                for old,new in mappings.items():
                    if token_re(old).search(newt):
                        newt = token_re(old).sub(new, newt)
            if newt != t:
                elem.text = newt

    # write output
    tree.write(outp, encoding='utf-8', xml_declaration=True)
    print('Wrote', outp)

    # write CSV mapping
    csvp = outp.with_suffix('.mapping.csv')
    with open(csvp, 'w', newline='', encoding='utf-8') as f:
        w = csv.writer(f)
        w.writerow(['OldName','NewName'])
        for o,n in mappings.items():
            w.writerow([o,n])
    print('Mapping CSV written to', csvp)

if __name__ == '__main__':
    main()

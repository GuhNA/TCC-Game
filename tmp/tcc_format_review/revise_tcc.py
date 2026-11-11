from pathlib import Path
from zipfile import ZipFile
from copy import deepcopy
from lxml import etree as E
import re, json, hashlib

ROOT = Path(r'G:\Unity projects\TCC-Game')
SOURCE = ROOT / 'Docs' / 'Processos de desenvolvimento de jogos indies.docx'
DEST = ROOT / 'Docs' / 'Processos de desenvolvimento de jogos indies - revisado.docx'
QA = ROOT / 'tmp' / 'tcc_format_review'
NS = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}
W = '{' + NS['w'] + '}'
XML = '{http://www.w3.org/XML/1998/namespace}'

def child(parent, tag):
    el = parent.find('w:' + tag, NS)
    if el is None:
        el = E.SubElement(parent, W + tag)
    return el

def prop(parent, tag, **attrs):
    el = child(parent, tag)
    for k, v in attrs.items():
        el.set(W + k, str(v))
    return el

def props(el, kind):
    p = el.find('w:' + kind, NS)
    if p is None:
        p = E.Element(W + kind)
        el.insert(0, p)
    return p

def txt(p):
    return ''.join(p.xpath('.//w:t/text()', namespaces=NS))

def run_format(rpr, size):
    fonts = child(rpr, 'rFonts')
    fonts.attrib.clear()
    for k in ('ascii', 'hAnsi', 'eastAsia', 'cs'):
        fonts.set(W + k, 'Times New Roman')
    prop(rpr, 'sz', val=size * 2)
    prop(rpr, 'szCs', val=size * 2)
    color = child(rpr, 'color')
    color.attrib.clear()
    color.set(W + 'val', '000000')

def spacing(pp, before=0, after=120, line=360):
    sp = child(pp, 'spacing')
    sp.attrib.clear()
    for k, v in dict(before=before, after=after, line=line, lineRule='auto', beforeAutospacing=0, afterAutospacing=0).items():
        sp.set(W + k, str(v))

def italic_ranges(p, ranges):
    offset = 0
    for r in list(p.xpath('.//w:r', namespaces=NS)):
        texts = r.findall('w:t', NS)
        t = ''.join(x.text or '' for x in texts)
        if not t:
            continue
        start, end = offset, offset + len(t)
        offset = end
        cuts = {0, len(t)}
        for a,b in ranges:
            if a < end and b > start:
                cuts.update([max(0,a-start),min(len(t),b-start)])
        cuts = sorted(cuts)
        if not any(a < end and b > start for a,b in ranges):
            continue
        assert all(E.QName(c).localname in ('rPr','t') for c in r), 'Complex run cannot be split'
        parent = r.getparent(); pos = parent.index(r)
        for a,b in zip(cuts,cuts[1:]):
            clone = deepcopy(r)
            for c in list(clone):
                if c.tag != W + 'rPr': clone.remove(c)
            tnode = E.SubElement(clone,W+'t');tnode.text=t[a:b];tnode.set(XML+'space','preserve')
            if any(ra <= start+a and rb >= start+b for ra,rb in ranges):
                rp = props(clone,'rPr');prop(rp,'i',val=1);prop(rp,'iCs',val=1)
            parent.insert(pos,clone);pos+=1
        parent.remove(r)

with ZipFile(SOURCE) as z:
    entries={i.filename:(i,z.read(i.filename)) for i in z.infolist()}
document=E.fromstring(entries['word/document.xml'][1])
styles=E.fromstring(entries['word/styles.xml'][1])
body=document.find('w:body',NS)
original_paras=body.findall('w:p',NS)
original_texts=[txt(p) for p in original_paras]

# One bounded content addition, after the existing discussion of balancing rules.
addition=('No desenvolvimento do protótipo, o balanceamento foi a atividade mais trabalhosa para a equipe. '
          'A dificuldade esteve em conciliar o ritmo de geração de ouro, os custos de melhorias e contratações '
          'e o avanço da força das ondas. Como esses elementos são interdependentes, o ajuste de um parâmetro '
          'exige considerar seus efeitos sobre a economia, a defesa e a exploração ao longo da partida.')
newp=E.Element(W+'p');newp.append(deepcopy(original_paras[46].find('w:pPr',NS)))
r=E.SubElement(newp,W+'r');rp=E.SubElement(r,W+'rPr');run_format(rp,12)
E.SubElement(r,W+'t').text=addition
body.insert(body.index(original_paras[46])+1,newp)

# Only punctuation required by the supplied model changes outside the addition.
for p in original_paras[25:29]:
    texts=p.xpath('.//w:t',namespaces=NS)
    last=next(t for t in reversed(texts) if t.text)
    assert last.text.endswith('.')
    last.text=last.text[:-1]+';'

# Fix the underlying styles as well as direct overrides so future edits retain the model.
style_sizes={'Normal':12,'Ttulo2':12,'Corpodetexto':12,'Lista':12,'figura':12,'Legenda':10,'bibliografia':10,'CorpodetextoChar':12}
for st in styles.findall('w:style',NS):
    sid=st.get(W+'styleId')
    if sid not in style_sizes: continue
    run_format(child(st,'rPr'),style_sizes[sid])
    if st.get(W+'type')=='paragraph' and sid != 'Normal':
        pp=child(st,'pPr')
        spacing(pp,line=240 if sid in ('Legenda','bibliografia') else 360)
        prop(pp,'jc',val='center' if sid in ('figura','Legenda') else 'left' if sid in ('Ttulo2','bibliografia') else 'both')
        if sid=='Ttulo2':
            spacing(pp,before=360)
            prop(child(st,'rPr'),'b',val=1)

title=next((s for s in styles.findall('w:style',NS) if s.find('w:name',NS) is not None and s.find('w:name',NS).get(W+'val','').lower()=='title'),None)
if title is None:
    title=E.SubElement(styles,W+'style',{W+'type':'paragraph',W+'styleId':'TCCTitle'})
    prop(title,'name',val='Title');prop(title,'basedOn',val='Normal');child(title,'qFormat')
titleid=title.get(W+'styleId')
tr=child(title,'rPr');run_format(tr,12);prop(tr,'b',val=1)
tp=child(title,'pPr');spacing(tp,after=240,line=240);prop(tp,'jc',val='center')
for border in list(tp.findall('w:pBdr',NS)):tp.remove(border)

italics=[]
for p in body.findall('w:p',NS):
    text=txt(p);pp=props(p,'pPr')
    sid=p.find('w:pPr/w:pStyle',NS)
    sid=sid.get(W+'val') if sid is not None else ''
    is_reference=sid=='bibliografia'
    is_source=text.startswith('Fonte:')
    is_caption=bool(re.match(r'^Figura \d+\s',text))
    is_heading=sid=='Ttulo2' or text=='REFERÊNCIAS'
    size=10 if is_reference or is_source or (not text and p.xpath('.//w:drawing',namespaces=NS)) else 12
    run_format(child(pp,'rPr'),size)
    for r in p.xpath('.//w:r',namespaces=NS):
        run_format(props(r,'rPr'),size)
    if is_heading:
        prop(pp,'jc',val='left');prop(pp,'keepNext',val=1);prop(pp,'keepLines',val=1)
        spacing(pp,before=360)
        for r in p.xpath('.//w:r',namespaces=NS):prop(props(r,'rPr'),'b',val=1)
    elif is_reference:
        prop(pp,'jc',val='left');spacing(pp,line=240)
    elif is_source:
        prop(pp,'jc',val='center');spacing(pp,before=60,after=240,line=240)
    elif is_caption:
        prop(pp,'jc',val='center');spacing(pp,before=120,after=0)
    elif sid in ('Corpodetexto','Lista'):
        prop(pp,'jc',val='both');spacing(pp,after=240 if p is original_paras[29] else 120)
    if p is original_paras[0]:
        prop(pp,'pStyle',val=titleid);prop(pp,'jc',val='center')
    # Citation expressions receive italics; foreign terms are made consistent with
    # their existing treatment. Bibliographic title emphasis is preserved.
    patterns=[r'\bet al\.',r'\bapud\b']
    if is_reference: patterns += [r'\bIn(?=:)']
    else: patterns += [r'\bidle\b',r'\bindies?\b',r'\bdesign\b',r'\bcommits?\b',r'\bbranch(?:es)?\b',r'\bsprites?\b']
    ranges=[]
    citation_spans=[m.span() for m in re.finditer(r'\([^)]*(?:\d{4}|\[s\.d\.\])[^)]*\)',text)]
    for pattern in patterns:
        for m in re.finditer(pattern,text,re.IGNORECASE):
            if pattern not in (r'\bet al\.',r'\bapud\b') and any(a <= m.start() and m.end() <= b for a,b in citation_spans):
                continue
            ranges.append(m.span());italics.append({'paragraph':text[:55],'term':m.group()})
    italic_ranges(p,ranges)

changes={'addition':addition,'italics':italics,'original_sha256':hashlib.sha256(SOURCE.read_bytes()).hexdigest()}
updated_texts=[txt(p) for p in body.findall('w:p',NS)]
expected=original_texts[:47]+[addition]+original_texts[47:]
for i in range(25,29):expected[i]=expected[i][:-1]+';'
assert updated_texts==expected, 'Unexpected content modification'
assert len(document.xpath('//*[local-name()="inline"]'))==8
assert len(document.xpath('//*[local-name()="anchor"]'))==0
for name,node in [('word/document.xml',document),('word/styles.xml',styles)]:
    info,_=entries[name]
    entries[name]=(info,E.tostring(node,xml_declaration=True,encoding='UTF-8',standalone=True))
with ZipFile(DEST,'w') as out:
    for info,data in entries.values():out.writestr(info,data)
changes['output']=str(DEST)
changes['abstract_words']=len(original_texts[8].split())
(QA/'changes.json').write_text(json.dumps(changes,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'output':str(DEST),'abstract_words':changes['abstract_words'],'paragraphs':len(updated_texts),'content_additions':1,'punctuation_adjustments':4},ensure_ascii=False))

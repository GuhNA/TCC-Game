from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
from copy import deepcopy
from lxml import etree as E
from PIL import Image
import re, json, shutil, hashlib, sys

sys.stdout.reconfigure(encoding='utf-8')
ROOT=Path(r'G:\Unity projects\TCC-Game')
QA=ROOT/'tmp'/'tcc_format_review'/'exploration'
QA.mkdir(parents=True,exist_ok=True)
SOURCE=ROOT/'Docs'/'Processos de desenvolvimento de jogos indies - revisado.docx'
DEST=ROOT/'Docs'/'Processos de desenvolvimento de jogos indies - revisado com exploração.docx'
BASE=QA/'before-exploration.docx'
if not BASE.exists(): shutil.copy2(SOURCE,BASE)
NS={
 'w':'http://schemas.openxmlformats.org/wordprocessingml/2006/main',
 'wp':'http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing',
 'a':'http://schemas.openxmlformats.org/drawingml/2006/main',
 'pic':'http://schemas.openxmlformats.org/drawingml/2006/picture',
 'r':'http://schemas.openxmlformats.org/officeDocument/2006/relationships'
}
W='{'+NS['w']+'}'
def txt(p):return ''.join(p.xpath('.//w:t/text()',namespaces=NS))
with ZipFile(BASE) as z:entries={i.filename:(i,z.read(i.filename)) for i in z.infolist()}
doc=E.fromstring(entries['word/document.xml'][1]);body=doc.find('w:body',NS)
paras=body.findall('w:p',NS);before_texts=[txt(p) for p in paras]
heading_template=next(p for p in paras if txt(p)=='4.3. Retomada da partida')
body_template=next(p for p in paras if txt(p).startswith('Ao retornar, o jogador pode consultar'))
caption_template=next(p for p in paras if txt(p).startswith('Figura 6 —'))
image_template=caption_template.getnext()
source_template=image_template.getnext()

def paragraph(template,text):
 p=E.Element(W+'p');p.append(deepcopy(template.find('w:pPr',NS)))
 r=E.SubElement(p,W+'r');r.append(deepcopy(template.find('w:r/w:rPr',NS)))
 E.SubElement(r,W+'t').text=text
 return p

def replace_plain(p,old,new):
 # Replace only the matched text nodes and preserve all surrounding run formatting.
 ts=p.xpath('.//w:t',namespaces=NS);full=''.join(t.text or '' for t in ts)
 assert old in full,(old,full)
 start=full.index(old);end=start+len(old);pos=0;inserted=False
 for t in ts:
  value=t.text or '';a=pos;b=pos+len(value);pos=b
  if a<end and b>start:
   prefix=value[:max(0,start-a)];suffix=value[min(len(value),end-a):]
   t.text=prefix+(new if not inserted else '')+suffix;inserted=True
 assert txt(p)==full.replace(old,new,1)

# Renumber the existing subsections and figures following the insertion.
replacements={
 '4.3. Retomada da partida':'4.4. Retomada da partida',
 '4.4. Verificação dos comportamentos de retorno':'4.5. Verificação dos comportamentos de retorno',
 '4.5. Discussão e limites da avaliação':'4.6. Discussão e limites da avaliação',
 'As Figuras 6 a 8 apresentam':'As Figuras 9 a 11 apresentam',
 'Figura 6 —':'Figura 9 —',
 'Figura 7 —':'Figura 10 —',
 'Figura 8 —':'Figura 11 —',
 'A Figura 7 mostra':'A Figura 10 mostra',
 'A Figura 8 mostra':'A Figura 11 mostra'
}
expected=[]
for p in paras:
 text=txt(p)
 for old,new in replacements.items():
  if text.startswith(old):replace_plain(p,old,new);text=txt(p);break
 expected.append(text)

intro=('A exploração amplia a obtenção de materiais além da economia da vila. O jogador seleciona '
       'de um a três aventureiros contratados e disponíveis, escolhe uma região liberada e aguarda '
       'o deslocamento. Aventureiros em missão ou na guarda ficam indisponíveis para outro envio, '
       'o que exige distribuir a equipe entre novas conquistas e a proteção das áreas já ocupadas.')
combat=('A Figura 6 apresenta o combate por turnos temporizados. Após o comando Batalhar, as ações '
        'ocorrem automaticamente, com intervalo de 0,65 segundo entre elas. A velocidade alimenta '
        'a iniciativa e permite que personagens mais rápidos atuem com maior frequência. O dano '
        'varia entre os limites de cada combatente e é reduzido pela armadura ou pela resistência '
        'mágica do alvo, conforme o tipo de ataque. O jogador acompanha a vida, as rodadas e o '
        'histórico de golpes, podendo recuar.')
balance=('O balanceamento de poder considera a composição das equipes e os atributos dos combatentes. '
         'Cada inimigo possui um valor de poder, e cada região define uma faixa para a soma desses '
         'valores na equipe sorteada, limitada a três integrantes. As batalhas de defesa usam uma '
         'redução configurável dessa faixa. Esses parâmetros permitem ajustar o desafio por região; '
         'o resultado também depende da vida, da velocidade, do dano e das resistências dos participantes.')
map_text=('O mapa da Figura 7 reúne os destinos e permite acompanhar o envio, a chegada e os relatórios '
          'das regiões. A vitória conquista a área e libera novos destinos conforme suas dependências. '
          'A equipe inicia o retorno, e as recompensas da batalha entram no inventário ao final desse '
          'percurso. A região conquistada passa a exigir decisões de coleta e defesa.')
workers=('Os exploradores são trabalhadores dedicados à coleta, enquanto os aventureiros participam '
         'dos combates e da guarda. Cada nível da Construção de Exploradores amplia em uma unidade '
         'a capacidade compartilhada entre as regiões. Os trabalhadores alocados realizam coletas '
         'em ciclos, com recursos e quantidades sorteados conforme o local. Os materiais ficam '
         'acumulados na região até o comando Transportar transferi-los para o inventário da vila.')
report=('O relatório da Figura 8 mostra os exploradores alocados e disponíveis, a guarda e o tempo '
        'até o próximo ataque. Nos recursos, D indica o acumulado para transporte e T, o máximo '
        'possível por ciclo, sem representar um limite de armazenamento. O painel permite enviar '
        'ou retirar exploradores e definir a guarda. A coleta pausa durante ataques pendentes; '
        'uma região atacada sem guarda é perdida, juntamente com os exploradores nela alocados.')

images=[
 ('C:/Users/Gustavo Nascimento/AppData/Local/Temp/codex-clipboard-02e4d64d-96c5-47f9-b1c1-c62f20e4041a.png',
  'Figura 6 — Combate por turnos temporizados nas Colinas do Vento',
  'Combate nas Colinas do Vento, com aventureiros, inimigos, atributos, barras de vida e comandos Batalhar e Recuar'),
 ('C:/Users/Gustavo Nascimento/AppData/Local/Temp/codex-clipboard-a7a0e8ba-1264-406c-982e-b3b59e433d7f.png',
  'Figura 7 — Mapa de exploração e seleção de destinos',
  'Mapa com regiões de exploração e botões para envio de aventureiros ou consulta de relatório'),
 ('C:/Users/Gustavo Nascimento/AppData/Local/Temp/codex-clipboard-90852160-9b0f-40fb-9ec9-e607019cde59.png',
  'Figura 8 — Relatório da região e gestão de exploradores',
  'Relatório das Colinas do Vento com guarda, exploradores, recursos acumulados, próxima onda e comandos de gestão')
]
rels=E.fromstring(entries['word/_rels/document.xml.rels'][1])
rel_ns='{http://schemas.openxmlformats.org/package/2006/relationships}'
rid=max(int(el.get('Id')[3:]) for el in rels if re.match(r'rId\d+$',el.get('Id','')))
doc_id=max(int(el.get('id')) for el in doc.xpath('//wp:docPr',namespaces=NS))
new_files={};image_info=[]

def figure(index):
 global rid,doc_id
 filename,title,description=images[index];path=Path(filename)
 rid+=1;doc_id+=1;relationship='rId'+str(rid)
 media=f'word/media/exploration-tcc-{index+1}.png'
 assert media not in entries
 new_files[media]=path.read_bytes()
 E.SubElement(rels,rel_ns+'Relationship',Id=relationship,Type=NS['r']+'/image',Target=media.removeprefix('word/'))
 pic=deepcopy(image_template)
 with Image.open(path) as im:w,h=im.size
 cx=round(13.5*360000);cy=round(cx*h/w)
 for extent in pic.xpath('.//wp:extent|.//a:xfrm/a:ext',namespaces=NS):
  extent.set('cx',str(cx));extent.set('cy',str(cy))
 for node in pic.xpath('.//wp:inline',namespaces=NS):
  for key in list(node.attrib):
   if key.startswith('{'):del node.attrib[key]
 prop=pic.find('.//wp:docPr',NS);prop.set('id',str(doc_id));prop.set('name',f'exploration-tcc-{index+1}');prop.set('descr',description)
 pic.find('.//pic:cNvPr',NS).set('name',Path(media).name)
 pic.find('.//a:blip',NS).set('{'+NS['r']+'}embed',relationship)
 image_info.append({'caption':title,'width_cm':13.5,'height_cm':cy/360000,'source_sha256':hashlib.sha256(new_files[media]).hexdigest()})
 return [paragraph(caption_template,title),pic,paragraph(source_template,'Fonte: captura do protótipo, fornecida pelos autores')]

new=[paragraph(heading_template,'4.3. Exploração, combate temporizado e gestão de recursos'),
     paragraph(body_template,intro),paragraph(body_template,combat),paragraph(body_template,balance),*figure(0),
     paragraph(body_template,map_text),*figure(1),
     paragraph(body_template,workers),paragraph(body_template,report),*figure(2)]
insertion=body.index(heading_template)
for offset,p in enumerate(new):body.insert(insertion+offset,p)

actual=body.findall('w:p',NS)
expected[96:96]=[txt(p) for p in new]
assert [txt(p) for p in actual]==expected
assert len(doc.xpath('//wp:inline',namespaces=NS))==11
assert len(doc.xpath('//wp:anchor',namespaces=NS))==0
assert len({e.get('id') for e in doc.xpath('//wp:docPr',namespaces=NS)})==11
assert [int(re.match(r'Figura (\d+)',txt(p)).group(1)) for p in actual if re.match(r'Figura \d+ —',txt(p))]==list(range(1,12))
for name,node in [('word/document.xml',doc),('word/_rels/document.xml.rels',rels)]:
 info,_=entries[name];entries[name]=(info,E.tostring(node,xml_declaration=True,encoding='UTF-8',standalone=True))
with ZipFile(DEST,'w') as z:
 for info,data in entries.values():z.writestr(info,data)
 for name,data in new_files.items():z.writestr(name,data,compress_type=ZIP_DEFLATED)
log={'new_section':'4.3. Exploração, combate temporizado e gestão de recursos',
     'added_words':sum(len(p.split()) for p in [intro,combat,balance,map_text,workers,report]),
     'figures':image_info,'old_sections_and_figures_renumbered':replacements,
     'base_sha256':hashlib.sha256(BASE.read_bytes()).hexdigest(),'output':str(DEST)}
(QA/'changes.json').write_text(json.dumps(log,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(log,ensure_ascii=False,indent=2))

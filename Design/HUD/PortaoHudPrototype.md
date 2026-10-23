# Protótipo do conjunto de HUDs — mapa Portão

O recrutamento aprovado é a referência de linguagem visual: pixel art, madeira,
metal escuro, pergaminho, destaques dourados e ícones ilustrados. Este protótipo
aplica o mesmo kit aos demais pontos de interface sem alterar a lógica do jogo.

## Mapa de interfaces

| Camada | Interface | Função | Direção no protótipo |
|---|---|---|---|
| Persistente | Ouro | Leitura rápida de recurso | Placa metálica compacta no canto superior direito |
| Persistente | Estado do portão | Integridade e próxima onda | Placa de madeira no canto superior esquerdo |
| Transitória | Alerta de onda | Urgência estratégica | Faixa metálica no topo, abaixo da área de segurança |
| Transitória | Prompt de interação | Próxima ação | Barra curta no rodapé com tecla destacada |
| Mundo | Indicador de alerta | NPC ou ponto relevante | Emblema vermelho e dourado, legível sobre o cenário |
| Modal | Viagem | Confirmar mudança de mapa | Pergaminho central compacto, decisão primária clara |
| Modal | Guarnição | Redistribuir soldados | Relatório, reserva, guarnição, destino, trânsito e ações de 1/10 por classe |
| Modal | Alerta genérico | Comunicar bloqueio ou consequência | Modal curto com uma única ação |
| Modal | Recrutamento | Contratar soldados | Mantém a baseline já aprovada |
| Sistema | Fade de cena | Transição técnica | Sem ornamentação; permanece neutro |

## Regras de composição

- Apenas um modal pode ocupar a tela por vez.
- Informações persistentes ficam nos cantos e preservam o centro do jogo.
- Alertas temporários não ficam atrás de modais; dentro de um fluxo modal, o
  feedback pertence ao próprio painel.
- Dourado indica ação principal ou valor; vermelho indica risco; bronze indica
  ação secundária.
- Textos e ícones essenciais continuam legíveis em 1366 × 768.

## Validação

O cenário visual `portao-hud` captura o estado de jogo em duas resoluções e os
três modais. As imagens são candidatas à aprovação humana; não viram baseline
automaticamente.

Os botões desta vitrine representam as ações e seus estados visuais. A ligação
com a lógica de compra, viagem e transferência pertence à etapa de integração,
depois da avaliação das capturas.

Para inspecionar no Unity, abra o prefab
`Assets/Resources/UI/Prototypes/PortaoHudPrototypeCanvas.prefab`. Seus grupos
`Gameplay HUD`, `Travel Modal`, `Garrison Modal` e `Alert Modal` representam
os estados independentes; mantenha apenas um modal ativo.

As capturas ficam em `Artifacts/VisualTests/portao-hud/captures` e o relatório em
`Artifacts/VisualTests/portao-hud/report.md`. A imagem de recrutamento usada na
prancha comparativa serve como referência de estilo, não como correspondência
de composição: cada tela terá sua própria baseline após aprovação.

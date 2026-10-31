# Centro — propostas de UX/UI

**Design aprovado pelo usuário em 27/10/2026.** As 11 capturas foram preservadas
em `Assets/Tests/Visual/Baselines/centro-proposals/`. Esta é a baseline do
protótipo aprovado; os modais dos edifícios já foram integrados na etapa 1.
Impostos e interações também já foram integrados na etapa 2; a revisão do conjunto é a próxima etapa.
Plano: [implementação por etapas](CentroHudImplementation.md).

Análise da cena salva em 27/10/2026. A referência visual é o kit medieval pixelado
validado no Portão. As imagens desta rodada são propostas isoladas, com dados
controlados: não alteram a cena Centro, seus prefabs de produção ou o save.

## Escopo encontrado

| Interface | Situação | Direção |
|---|---|---|
| Modal de edifícios | Legado; 7 textos TMP, 2 botões e fonte Liberation Sans | Redesenhar um template que serve aos cinco edifícios |
| Aviso de impostos | Texto legado; dura 1 segundo e não usa o tema | Substituir por feedback contextual breve e legível |
| Ouro | Já consome o prefab compartilhado | Preservar, sem criar um segundo saldo |
| Prompt de interação | Criado em runtime; visual já compartilhado | Melhorar a descrição da ação no contexto do Centro |
| Alertas de onda | Criados em runtime; visual já compartilhado | Preservar fila, prioridade e retomada após menus |
| Viagem pelos quatro sentinelas | Modal já redesenhado | Reutilizar e conferir os nomes dos destinos |
| Entrada da Taverna | Interação separada da melhoria do edifício | Distinguir “Entrar” de “Gerenciar” |

O recrutamento de aventureiros e os painéis de exploração pertencem à Taverna,
não à cena Centro levantada. O tipo ExplorersLodge existe no código, mas não foi
encontrado entre os cinco edifícios desta cena; não foi incluído como tela ativa.

## 1. Modal de construção e melhoria — prioridade alta

### Problemas observados

- O modal atual concentra nome, descrição, nível, produção e custo em texto,
  sem a hierarquia visual e os materiais já aprovados para o jogo.
- Falta comparar o que o jogador possui com o que ganhará ao investir.
- “Produção” é uma descrição insuficiente para três mecanismos diferentes.
  Nos edifícios militares, o painel calcula produção pela definição, mas o
  recrutamento usa outra fórmula para a capacidade do reestoque.
- Ouro insuficiente desabilita o botão; falta mostrar claramente quanto falta.
- O controlador suporta exigências de recursos, mas as cinco definições desta
  cena não configuram requisitos extras nesta revisão. Não devemos inventá-los.
- O bloqueio atual usa diretamente `moviment.SetMovementBlocked` e UiMenuState;
  o controlador não trata Esc. A implementação deve usar o mesmo contrato de
  bloqueio, fechamento e retorno de foco dos outros modais.

### Composição proposta

1. Cabeçalho metálico com nome do edifício e fechamento separado.
2. Faixa própria de subtítulo, afastada de molduras e do conteúdo.
3. Ilustração do edifício à esquerda, usando seus sprites reais.
4. Comparação “Atual / Próximo nível” à direita, com benefício em linguagem de jogo.
5. Custo, saldo disponível e saldo após melhoria na mesma área de decisão.
6. Feedback dentro do modal, seguido de uma ação dourada e uma saída secundária.

O botão terá área clicável confortável e margem interna de pelo menos 16 unidades
em relação às molduras, seguindo o ajuste aprovado no recrutamento.

### Benefícios que a UI deve comunicar

| Edifício | Indicador correto | Exemplo real de nível 2 → 3 |
|---|---|---|
| Quartel | Guerreiros disponíveis por reestoque | 16 → 19; custo 225 ouro |
| Estande de Arqueiros | Arqueiros disponíveis por reestoque | 16 → 19; custo 225 ouro |
| Escola de Magia | Magos disponíveis por reestoque | 16 → 19; custo 225 ouro |
| Mercado Local | Ouro acumulado por minuto | 15 → 20; custo 225 ouro |
| Taverna | Aventureiros oferecidos por onda | 3 → 4; custo 450 ouro |

Os números vêm dos calculadores atuais. O reestoque militar ocorre a cada 6 horas
e a melhoria não entrega soldados imediatamente. A Taverna tem limite 5; os
outros quatro edifícios, limite 10. Valores das capturas são exemplos controlados,
não uma leitura da partida do usuário.

### Estados necessários

| Estado | Apresentação e ação |
|---|---|
| Não construído | Mostrar benefício inicial e “Construir por X ouro” |
| Melhoria disponível | Comparar níveis; mostrar custo e saldo após; “Melhorar para nível X” |
| Ouro insuficiente | Mostrar “Faltam X ouro” e manter o motivo perto do botão desabilitado |
| Recursos insuficientes | Quando houver requisitos configurados, mostrar cada item possuído/exigido |
| Nível máximo | Explicar que o benefício permanece ativo; retirar a ação de melhoria e oferecer saída |
| Melhoria concluída | Atualizar nível, comparação e saldo; feedback local de sucesso |

Não adicionar outra confirmação para toda melhoria: o custo explícito e a ação
nomeada já formam a decisão. Impedir compra dupla e atualizar o estado logo após
a transação são responsabilidades da integração, não de uma segunda tela.

## 2. Impostos e feedback — prioridade alta

A interação atual coleta diretamente os impostos e retorna três resultados:
coleta concluída, Mercado não construído ou nenhum valor disponível.

Proposta principal: manter essa coleta em uma ação, com prompt específico e um
aviso compacto de 3–4 segundos. Exibir “+X ouro coletado” e o saldo atualizado.
Não usar modal com fundo escuro para uma coleta rotineira.

Quando o Mercado não existir, dizer o que fazer: “Construa o Mercado Local para
gerar impostos”. Sem saldo acumulado, usar “Ainda não há impostos disponíveis”.
Os avisos devem aguardar menus que ocupem a tela, preservando sua oportunidade
de leitura; erros da melhoria ficam no próprio modal.

Como opção, a vitrine mostra uma placa compacta de impostos no canto superior
esquerdo. Ela torna o acúmulo visível antes de caminhar até o coletor. Pode aparecer
somente na proximidade do NPC para manter o HUD enxuto. É uma adição de interface,
não uma função existente: a integração precisará de uma consulta de impostos
somente leitura. Nunca usar `CollectTaxes()` para preencher esse indicador.

## 3. Interação e coesão — prioridade média

- Edifício: “Gerenciar Quartel”, “Gerenciar Mercado Local” etc.
- Porta da Taverna: preservar “Entrar na Taverna”.
- Coletor: “Coletar impostos” ou “Consultar impostos”, conforme disponibilidade.
- Uma única janela por vez; Esc e botão de saída liberam o mesmo bloqueio.
- Reaproveitar o saldo e os alertas já redesenhados; evitar outro cabeçalho fixo
  grande ou um painel estratégico global sem necessidade demonstrada.

## Direção visual comum

Madeira delimita o painel; ferro organiza cabeçalhos e avisos; pergaminho suporta
dados e comparação; dourado marca ação principal e valores. Bronze é secundário;
vermelho indica risco ou fechamento. Pixelify Sans, ícones e molduras vêm do kit
existente. Reaproveitar os sprites dos edifícios antes de pedir novas artes.

Cor sempre acompanha uma explicação textual. Estados desabilitados devem manter
o rótulo legível. Textos longos devem quebrar ou ganhar área, em vez de serem
reduzidos até ficarem ilegíveis. Verificar tudo também em 1366 × 768.

## Sequência sugerida após aprovação

1. Aprovar o template do modal e a forma de comunicar benefícios/custos.
2. Integrar construção/melhoria, fechamento e seus estados aos cinco edifícios.
3. Integrar feedback de impostos e prompts; decidir se a placa de impostos será
   permanente, contextual ou ficará para depois.
4. Conferir viagens e alertas compartilhados no Centro e capturar a revisão final.

A validação funcional futura deve cobrir construir, melhorar, custo descontado
uma vez, ouro insuficiente, nível máximo e coleta. A validação visual deve cobrir
os cinco edifícios, estados vazios/indisponíveis e duas resoluções. Nenhuma baseline
nova é aprovada automaticamente nesta fase de proposta.

## Evidências do projeto

- Cena e controles: [inventário gerado](CentroHudInventory.md).
- Modal e conteúdo atuais: `Assets/Scripts/Canvas/BuildingPanel.cs`.
- Custo e produção: `Assets/Scripts/City/BuildingCalculator.cs`.
- Capacidade e intervalo militar: `Assets/Scripts/Portao/RecruitmentService.cs`.
- Coleta e acúmulo: `Assets/Scripts/City/TaxCollectorNpc.cs`,
  `TaxAccrualService.cs` e `TaxCalculator.cs`.
- Porta da Taverna: `Assets/Scripts/Navigation/TavernScenePortal.cs`.
- Proposta isolada: `Assets/Resources/UI/Prototypes/CentroHudProposalCanvas.prefab`.

As capturas do modal atual são reproduções com dados de exemplo sobre a cena salva,
sem executar transações ou modificar o save. As propostas também usam dados fixos.

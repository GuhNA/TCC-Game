# Relatório de fidelidade visual — Recrutamento do Portão

Data da revisão: 23/10/2026

## Resultado da rodada

A implementação foi reconstruída a partir da direção visual aprovada e passou
pela cena-vitrine em cinco estados reproduzíveis. O teste funcional de fumaça
também passou (1 de 1).

A versão atual foi aprovada como baseline visual em 20/10/2026.

## Alterações realizadas

- moldura externa refeita com travessas de madeira, postes de ferro, cantos
  forjados e rebites;
- molduras individuais adicionadas aos três cartões, com divisórias estruturais;
- estandartes coloridos e emblemas próprios para guerreiro, arqueiro e mago;
- estandartes, tecido do cabeçalho, elmo, moedas, ampulheta e confirmação
  substituídos por sprites transparentes autorais derivados da referência;
- nomes das classes reposicionados em faixas abaixo dos retratos;
- retratos recortados novamente para remover fragmentos laterais e melhorar o
  enquadramento;
- botões enriquecidos com moldura dupla, relevo, sombra e rebites;
- ícones de moeda, estoque/reserva, tempo e confirmação adicionados;
- cabeçalho recebeu tecido, metal e hierarquia mais próxima do conceito;
- feedback de compra permanece dentro do modal e acima do HUD da cena;
- atalho de quantidade 5 incluído para reproduzir a composição aprovada.
- Pixelify Sans incorporada localmente e aplicada ao HUD, com suporte aos
  acentos do português;
- estoque e reserva reorganizados verticalmente, com elmo ampliado;
- atalhos receberam acabamento em bronze, controles de quantidade permanecem
  em ferro e a ação principal usa ouro com rótulo escuro.

## Estados verificados

- normal em 1920 × 1080;
- normal compacto em 1366 × 768;
- estoque esgotado;
- ouro insuficiente;
- compra concluída.

## Diferenças residuais conhecidas

- os emblemas são sprites reutilizáveis no mesmo estilo da ilustração conceitual;
- a cena real preserva textos e valores dinâmicos, portanto pequenos ajustes de
  espaçamento diferem da imagem estática de referência.

## Decisão de baseline

Status: **baseline aprovada**.

Os cinco estados aprovados estão em
`Assets/Tests/Visual/Baselines/recruitment/`. Uma execução posterior confirmou
erro médio 0 e nenhuma divergência de pixels em todos os estados.
Após a extração do kit visual compartilhado, a regressão foi executada novamente
e manteve os mesmos resultados; o teste funcional também permaneceu aprovado.

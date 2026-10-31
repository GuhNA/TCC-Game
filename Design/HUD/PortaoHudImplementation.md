# Aplicação por etapas — HUDs do Portão

O conjunto de protótipos foi aprovado pelo usuário em 23/10/2026.

1. **HUD base:** ouro, integridade/estado do portão e interação. Conectar valores
   reais e preservar atualização, ocultação dos prompts e compra.
2. **Alertas:** aviso de onda, indicador sobre o capitão e feedback genérico.
   Preservar duração, prioridade visual e condições de exibição.
3. **Modais:** viagem e guarnição. Conectar destino, relatório, estoque,
   transferências de 1/10, confirmação e fechamento com bloqueio do jogador.

Cada etapa recebe capturas dos prefabs de produção e uma verificação funcional
focada. A aprovação do protótipo autoriza a aplicação dessas etapas; alterações
de composição além do aprovado devem ser apresentadas na revisão.

O HUD de ouro é um prefab compartilhado com o Centro; sua atualização visual
também se reflete nessa cena. O saldo e a lógica econômica continuam únicos.

## Entrega da etapa 1 — 23/10/2026

- Prefabs de ouro, contexto do portão e interação atualizados com as peças do
  protótipo aprovado.
- Saldo conectado ao evento econômico; integridade, título, estado, tempo e
  ameaça conectados ao portão ativo.
- Barra de integridade proporcional e vermelha em estado crítico.
- Prompt com tecla independente, descrição dinâmica e ocultação durante modais.
- Teste funcional de recrutamento passou (1/1), incluindo o saldo pixelado e a
  ocultação do prompt durante a compra.
- Capturas dos prefabs de produção em 1920 × 1080 e 1366 × 768, incluindo
  integridade crítica e saldo máximo.

Revisão: [galeria da etapa 1](PortaoHudStageOneReview.html).
As etapas 2 e 3 são descritas abaixo.

## Entrega da etapa 2 — 23/10/2026

- Faixa de onda migrada para o metal, elmo e tipografia aprovados. O título
  acompanha a urgência e o texto vem dos eventos reais da campanha.
- Um aviso interrompido por menu permanece na fila/estado ativo e retoma sua
  exibição e contagem quando o menu fecha.
- Indicador do capitão em canvas de mundo, dimensionado independentemente da
  escala do NPC e ancorado acima do personagem. Oculto enquanto menus estão abertos.
- Feedback genérico do Portão usa o painel aprovado e oferece fechamento pelo
  botão, preservando o tempo automático configurado na cena (1 segundo).
- O recrutamento mantém seu feedback interno, evitando repetir o aviso genérico.
- Quatro capturas dos prefabs de produção: aviso, aviso crítico, feedback e
  feedback compacto, em 1920 × 1080 e 1366 × 768.
- Verificação funcional: compra/saldo/fechamento e fluxo dos alertas (retomada,
  expiração, posição do indicador e fechamento manual/automático do feedback).
- Resultado final: 2/2 testes passaram; o Unity encerrou normalmente na execução
  de confirmação. As quatro capturas foram inspecionadas visualmente.

Revisão: [galeria da etapa 2](PortaoHudStageTwoReview.html).
Os avisos de onda são compartilhados com os demais mapas; o feedback genérico
novo foi configurado apenas na cena do Portão.

## Entrega da etapa 3 — 23/10/2026

- Modais de viagem e guarnição migrados para as peças do protótipo aprovado.
- Viagem apresenta o destino real, integridade e estado da ameaça quando o
  destino é um portão; o retorno ao Centro não usa o tempo fictício do protótipo.
- Botões de confirmar/cancelar mantêm a navegação existente e o bloqueio do
  jogador; apenas um menu pode ser aberto por vez.
- A guarnição exibe as contagens reais por classe e preserva os comandos de
  receber/enviar lotes de 1/10, troca de destino, trânsito e confirmação de relatório.
- Comandos ficam indisponíveis conforme o estoque da origem e o estado da campanha.
- Relatórios completos e múltiplas transferências têm áreas de rolagem com barras
  visíveis quando necessário. O tempo estratégico continua avançando neste painel.
- Seis capturas: retorno ao Centro, destino portão, viagem compacta, guarnição,
  ausência de tropas/relatório e conteúdo extenso na resolução compacta.
- Teste funcional passou junto aos anteriores: 3/3. Cobriu receber 10, enviar 1,
  atualização das contagens, destino, confirmação do relatório, comandos sem tropas,
  fechamento, cancelamento de viagem e transição real até a cena Centro.

Revisão: [galeria da etapa 3](PortaoHudStageThreeReview.html).
Os dois prefabs de modal são compartilhados; o novo visual também aparece nos
NPCs de viagem do Centro. As três etapas de integração estão aplicadas.

### Conferência final — 27/10/2026

Revisão retomada após a interrupção da entrega. O botão de fechar da guarnição
foi afastado da área de rolagem de transferências. As capturas de revisão
recalculam o layout dos textos antes de determinar se a barra de rolagem é
necessária. Esta revisão de composição preserva a lógica funcional já validada
pelos três testes da execução de 23/10/2026.

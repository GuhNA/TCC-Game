# Centro — conjunto de exteriores e terreno

> Registro histórico: esta etapa já foi integrada às cenas finais. As cenas de protótipo e seus geradores foram retirados; veja o [registro da limpeza](ApprovedPrototypeCleanup.md). As imagens e aprovações foram preservadas.

Etapa 4A entregue para revisão do conjunto. O usuário testou e aprovou o piloto da Taverna e autorizou o avanço. A baseline desse piloto foi registrada com `Approve-VisualBaseline.ps1 -Scenario centro-tavern-facade`; as novas artes continuam candidatas.

## Entrega

- [Galeria de revisão](CentroExteriorSetReview.html), com dez capturas reais do Unity.
- Quatro exteriores novos, fundação reutilizável e grama. Taverna e calçamento reaproveitam as artes aprovadas.
- Aplicação na cena `Assets/Scenes/Prototypes/CentroLayoutPrototype.unity`, sem substituir o Centro de produção.
- Os cinco lotes usam um apresentador comum para alternar fundação/fachada conforme o nível. A camada de apresentação não altera preços, níveis, compras ou saves.
- Escala uniforme nas fachadas, importação Point sem mipmaps e sem compressão. Pivôs na soleira ou na abertura norte do estande.
- Ruas e pequenos acessos usam textura repetida com coordenadas do mundo: os cruzamentos mantêm o mesmo alinhamento das pedras. As bordas têm uma faixa curta de transição para a grama.
- Terreno extra cobre a câmera e a torre alta; a área de movimento permanece em **1.040 u²**, com o mesmo jogador e bases físicas.
- Oclusão por pés e transparência de fachadas altas reaproveitam o comportamento do piloto. O estande mantém seu serviço na aproximação norte; esta etapa não adiciona uma atividade dentro do pátio.

## Sprites finais

Todos os seis novos sprites foram criados com a ferramenta **imagegen integrada**. Os prompts e as revisões estão em [CentroExteriorSetPrompts.md](CentroExteriorSetPrompts.md).

| Uso | Arquivo consumido | Importação |
|---|---|---|
| Quartel | [BarracksExteriorV2.png](../../Assets/Images/Buildings/CentroSetV1/BarracksExteriorV2.png) | 134 px/u; soleira central |
| Mercado | [MarketExteriorV1.png](../../Assets/Images/Buildings/CentroSetV1/MarketExteriorV1.png) | 120 px/u; acesso entre os toldos |
| Arqueiros | [ArcheryCourtV1.png](../../Assets/Images/Buildings/CentroSetV1/ArcheryCourtV1.png) | 151 px/u; abertura norte |
| Escola | [MagicSchoolExteriorV2.png](../../Assets/Images/Buildings/CentroSetV1/MagicSchoolExteriorV2.png) | 121 px/u; soleira da torre |
| Fundação | [UnbuiltFoundationV1.png](../../Assets/Images/Buildings/CentroSetV1/UnbuiltFoundationV1.png) | Borda dividida em nove partes, adaptável ao lote |
| Grama | [VillageGrassV1.png](../../Assets/Images/Buildings/CentroSetV1/VillageGrassV1.png) | 192 px/u; repetida no terreno |

As imagens V1 do Quartel e da Escola na pasta são rascunhos preservados; a cena usa os arquivos V2 da tabela. A Escola tem uma silhueta visível de cerca de 12,3 u, um pouco acima da régua inicial de 11,3 u, para manter a porta próxima de 2,5 u sem deformar a arte. A revisão completa mostra a torre ao lado do jogador. A base física e o acesso não foram ampliados.

## Validação

- Montagem no Unity concluída sem erro de compilação.
- **1/1 teste funcional passou**, conferindo área, tamanho do jogador, cinco vistas/fundações, acesso aos serviços, impostos, limites da câmera, colisão, bloqueio da Taverna no nível 0, construção, alternância de fundação e fachada, ida/volta e preservação da partida salva.
- Cenário visual próprio `centro-exterior-set`, separado dos testes funcionais. Dez capturas: conjunto, praça, quatro serviços, aproximações compactas da Escola e Taverna, fundações e oclusão do Quartel.
- Inspeção das capturas: acesso central do Mercado desimpedido; torre e Quartel se sobrepõem parcialmente conforme a profundidade, com a porta da Escola visível; abertura norte do estande alinhada à aproximação. O halo aparente na prévia gerada da Escola não aparece no render com alpha sobre o terreno.
- A primeira vista geral recortava o ouro devido ao formato da prancha. O HUD foi ocultado apenas nas vistas gerais. Enquadramentos de revisão foram ajustados para afastar os personagens dos avisos; o teste funcional não foi repetido para essas mudanças de apresentação.
- Capturas e relatório em `Artifacts/VisualTests/centro-exterior-set/`. Status esperado: **NeedsHumanApproval**, que indica revisão artística pendente e não falha funcional.
- Conferência final SHA-256: `Centro.unity`, `Taverna.unity` e `EditorBuildSettings.asset` permanecem iguais aos registros anteriores ao protótipo. Os links e imagens locais da galeria foram verificados.

A baseline anterior representa o contexto do piloto, com terreno provisório. Mudanças deliberadas no mundo são avaliadas pelo novo cenário do conjunto; uma diferença no fundo antigo não substitui a avaliação dos componentes de HUD ou da arte aprovada.

## Como reproduzir

No Unity, **TCC Game → World → Build Centro Exterior Set** reconstrói o protótipo completo. **Refresh Centro Exterior Set** reaplica somente o conjunto. O gerador normal do protótipo também instala as artes quando estão disponíveis.

```powershell
.\tools\Test-CentroLayoutPrototype.ps1
.\tools\Test-Visual.ps1 -Scenario centro-exterior-set
```

Para avaliar no jogo, abra a cena CentroLayoutPrototype e dê Play. Os dados são temporários, com os edifícios construídos. A captura de fundações usa uma prévia visual no editor, sem alterar níveis salvos.

## Próximo marco

Revisar as quatro novas artes e o conjunto nesta galeria. Em seguida, a etapa **4B** integra a composição validada ao Centro principal e aos portais reais. A etapa **5** encerra com HUDs e modais no novo cenário, quatro destinos, retorno da Taverna, limites e capturas finais.

O terreno desta entrega é a base contínua; marcos de praça, vegetação decorativa, acabamento dos limites e variações de edifício por nível podem receber um passe próprio de polimento. Esses itens não são apresentados como artes já finalizadas nesta rodada.

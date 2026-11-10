# Centro — caminhos e terreno vivo V2

> Registro histórico: esta etapa já foi integrada às cenas finais. As cenas de protótipo e seus geradores foram retirados; veja o [registro da limpeza](ApprovedPrototypeCleanup.md). As imagens e aprovações foram preservadas.

Revisão solicitada após a entrega dos exteriores: o terreno anterior tinha pouca identidade, ruas uniformes e textura granulada. Esta rodada modifica a composição do chão e acrescenta detalhes de vila, mantendo as construções e a planta aprovadas.

## Resultado

- [Galeria com comparação antes/depois](CentroTerrainV2Review.html).
- Três materiais novos: grama com tufos legíveis, terra compactada e pedras maiores, com blocos de cor mais definidos.
- Ruas principais de pedra com contorno irregular, pequenas variações de largura e faixa de terra nas bordas.
- Percursos de terra entre jardins e ao redor do estande, além dos acessos de pedra às cinco construções.
- Praça com área pavimentada arredondada e poço lateral, preservando o eixo central de passagem.
- Três bancos com lanternas, barris e caixas; sete canteiros; árvores de tamanhos variados enquadrando as bordas da vila.
- Poço e bancos têm colisores apenas na base. A cobertura do poço fica transparente quando encobre o jogador por trás. Canteiros baixos são decoração; as árvores têm raízes fora da área caminhável.
- A área do mapa continua em **1.040 u²**, com os mesmos cinco lotes, jogador, serviços, portais e dados temporários do protótipo. O Centro de produção não recebe esta revisão antes da avaliação visual.

## Pixel art

As imagens foram geradas com instruções de pixels grandes, agrupamentos de cor e paleta reduzida, sem a granulação fotográfica dos materiais anteriores. A importação usa Point e não gera mipmaps. O material de chão amostra uma grade lógica de 128 pixels por repetição para manter arestas definidas; isso é uma configuração de renderização, sem alterar os arquivos de arte gerados.

As ruas não usam mais o preenchimento de grandes retângulos com bordas suavizadas. Uma malha estática com recorte de 0,125 u desenha os contornos em degraus; as faixas contíguas são agrupadas para evitar um objeto por quadrado. As texturas mantêm coordenadas contínuas nos cruzamentos. Na inspeção inicial, as pedras ficaram grandes demais; sua repetição foi reduzida de 3,5 para 1,75 u antes das capturas finais.

## Arquivos finais

Sete imagens novas, criadas com a ferramenta **imagegen integrada**, salvas no projeto:

- [Grass.png](../../Assets/Images/World/CentroTerrainV2/Grass.png)
- [Paving.png](../../Assets/Images/World/CentroTerrainV2/Paving.png)
- [Dirt.png](../../Assets/Images/World/CentroTerrainV2/Dirt.png)
- [Well.png](../../Assets/Images/World/CentroTerrainV2/Well.png)
- [StreetFurniture.png](../../Assets/Images/World/CentroTerrainV2/StreetFurniture.png)
- [Flowerbed.png](../../Assets/Images/World/CentroTerrainV2/Flowerbed.png)
- [Oak.png](../../Assets/Images/World/CentroTerrainV2/Oak.png)

[Prompts completos](CentroTerrainV2Prompts.md). Os materiais e as malhas gerados ficam na mesma pasta. As artes anteriores permanecem disponíveis, assim como as capturas da primeira versão em `Artifacts/VisualTests/centro-exterior-set/`.

## Verificação

- Montagem no Unity sem erro de compilação.
- **1/1 teste PlayMode passou**, executado uma vez nesta rodada, incluindo uma aproximação real do poço e tentativa de atravessar sua base. Também cobre serviços, colisões, tamanho do mapa/jogador, quatro saídas, construção e ida/volta da Taverna, sem modificar a partida salva.
- A montagem rejeita a colocação de objetos sólidos sobre os corredores centrais, lotes, rotas de serviço, coletor, gestão e retorno da Taverna.
- Cenário visual separado: `centro-terrain-v2`, com treze capturas. A prancha automática compara a visão geral anterior com a atual; a galeria compara a praça com o mesmo enquadramento nas duas versões.
- A transparência do poço foi acrescentada depois do teste de circulação. É uma mudança de apresentação: colisores, rotas e economia não foram alterados; a revisão visual inclui uma captura específica atrás da cobertura.
- As capturas são candidatas à avaliação humana. Nenhuma baseline nova foi promovida automaticamente.
- Conferência final: treze capturas geradas, links locais da galeria válidos e hashes de Centro.unity, Taverna.unity e EditorBuildSettings.asset iguais aos registros anteriores ao protótipo.

## Como abrir

Abra `Assets/Scenes/Prototypes/CentroLayoutPrototype.unity` e dê Play. Para reaplicar somente o terreno, use **TCC Game → World → Refresh Centro Living Terrain**. O gerador do conjunto também instala esta revisão quando os sete sprites estão presentes.

```powershell
.\tools\Test-CentroLayoutPrototype.ps1
.\tools\Test-Visual.ps1 -Scenario centro-terrain-v2
```

Após revisar esta composição, retomar a integração ao Centro principal e a conferência final dos HUDs e transições. Esta revisão não adiciona novos serviços, ações de banco/poço ou atividades dentro do estande.

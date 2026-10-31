# Revisao de caminhada — 28/10/2026

## Backup anterior a qualquer alteracao

G:/Unity projects/TCC-Game-backup-20261028-before-walk

Assets, Packages, ProjectSettings e UserSettings: 975 arquivos conferidos por SHA-256 contra o projeto original antes das alteracoes. Historico .git e demais arquivos do projeto tambem copiados. A primeira passagem incluiu caches por engano e quatro arquivos temporarios bloqueados falharam; a segunda passagem excluiu Library/Temp/Logs/obj e concluiu sem falhas. Esses caches excedentes no backup nao fazem parte da verificacao e podem ser regenerados. Nao foi executada restauracao nem exclusao de arquivos.

As cenas abertas estavam sem indicador de alteracoes nao salvas. O backup representa o estado em disco. Para restaurar, prefira abrir uma copia dos arquivos-fonte deste backup em uma pasta nova com Unity 6000.3.11f1, sem reutilizar Library e Temp. Nao sobreponha trabalho posterior sem uma nova copia de seguranca.

## Resultado de arte: NAO APROVADO

Tres tentativas pelo gerador de imagens integrado foram inspecionadas e rejeitadas. Nao foram aplicadas ao Player:

- REJECTED_12frame_attempt.png: segunda metade nao alterna claramente a perna dominante; origem/escala de geracao nao atende a celula final.
- REJECTED_8pose_attempt.png: repeticao das poses, mao abandona o pomo, cabelo encosta na borda.
- REJECTED_8pose_revision.png: melhora parcial da passagem, mas apoio/oclusao das pernas e contato da mao ainda inconsistentes.

Nenhuma dessas imagens e spritesheet final pronta para slice. Nao foram declaradas dimensoes de entrega nem aprovacao de animacao. DOWN/UP novos nao foram produzidos porque RIGHT continua reprovado.

## Teste tecnico na Unity

Nova ferramenta: Assets/animation/WalkRightReview/Editor/WalkCadenceAudit.cs.

Menu TCC Game > Animation Tests > Audit All Player Clips - Create Timing Study.

Cria copias independentes dos seis clips em Assets/animation/WalkRightReview/CadenceStudy. As copias de WALK usam 11 FPS, 12 sprites, ciclo de 1.090909 s; IDLE mantem 6 FPS e 2 s. O controller de estudo nao substitui o controller de producao.

Auditoria executada via AnimationClipPlayable -> Animator -> SpriteRenderer no editor: os seis clips retornaram 12 sprites distintos, raiz sem deslocamento, sem curvas de Transform e geometria de sprite consistente dentro de cada clip. Isso comprova integridade tecnica, nao qualidade das poses. O tempo foi amostrado por frame durante tres passagens; nao e teste de deslocamento em gameplay nem medicao de escorregamento dos pes.

Cadencias originais encontradas: walk_right 6 FPS; walk_down 7 FPS; walk_up 12 FPS. Os tres idles 6 FPS. A diferenca de cadencia merece padronizacao, mas elevar FPS nao conserta perna errada.

Menu TCC Game > Animation Tests > Compare Original and 11 FPS abre comparacao com seletor de direcao, original/estudo, reproducao e passo a passo. Usa Animator/SpriteRenderer temporarios, sem salvar cenas existentes.

## Referencias consultadas

- https://www.slynyrd.com/blog/2024/5/24/pixelblog-50-human-walk-cycle — poses de contato, carga, passagem e avanco; consistencia de roupa.
- https://manningkrull.com/__pixel-art/walking.php — manter identidade das pernas por sobreposicao/sombreamento; trajetoria do pe e defasagem de meio ciclo entre pernas.

## Pendencias reais

Reconstruir pernas com identidade anatomica estavel; fixar mao no pomo; estabilizar cabelo/capa; depois completar 12 poses uteis e direcoes DOWN/UP. Validar deslocamento no chao antes de substituir clips de producao. Nao ha entrega artistica final nesta revisao.

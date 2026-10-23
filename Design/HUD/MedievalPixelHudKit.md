# Kit visual medieval pixelado

O kit concentra a linguagem visual aprovada do HUD sem definir o layout de uma
tela específica.

## Tema compartilhado

O asset `MedievalPixelHudTheme` é a fonte única para:

- fonte Pixelify Sans;
- superfícies de madeira, pergaminho e metal;
- botões dourado, bronze e vermelho;
- estandartes das classes;
- ícones de moeda, ampulheta, confirmação e elmo;
- ornamento do cabeçalho;
- cores semânticas principais.

## Primitivas reutilizáveis

`MedievalHudKit` cria painéis fixos ou responsivos, textos, botões com relevo e
rebites, ícones e peças de moldura. Cada tela continua responsável por sua
composição, conteúdo e regras de negócio.

O menu de recrutamento é a primeira tela consumidora e também a referência de
regressão. Alterações no kit só são aceitas quando suas cinco capturas continuam
iguais ao baseline aprovado. Estados adicionais de interação pertencem à etapa 2.

## Estado da etapa 1

A extração está implementada: o menu de recrutamento consome o tema e as
primitivas compartilhadas. A verificação rápida `tools/Test-MedievalHudKit.ps1`
confere, em uma única execução, que os 14 sprites continuam binariamente iguais
ao visual aprovado, que o tema está completo e que o prefab usa os novos assets.
O teste visual completo permanece separado e pode ser executado com
`tools/Test-Visual.ps1 -Scenario recruitment`.

Validação final da etapa em 23/10/2026:

- cinco capturas visuais aprovadas, com erro médio 0 e 0% de pixels alterados;
- teste funcional de recrutamento aprovado (1 de 1);
- verificação estrutural do kit aprovada.
